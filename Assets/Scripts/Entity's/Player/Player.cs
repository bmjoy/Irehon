using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class OnCharacterDataUpdate : UnityEvent<CharacterInfo> { }

public struct SpawnPointInfo
{
	public Vector3 spawnPosition;
	public string spawnSceneName;
}

public class Player : Entity
{
	public OnContainerUpdate OnPublicEquipmentUpdate { get; private set; } = new OnContainerUpdate();
	public OnCharacterDataUpdate OnCharacterDataUpdateEvent { get; private set; } = new OnCharacterDataUpdate();
	public PlayerBonesLinks PlayerBonesLinks { get; private set; }
	public bool isDataAlreadyRecieved { get; private set; } = false;
	public PlayerContainerController ContainerController => containerController;

	[SyncVar(hook = "GetName"), HideInInspector]
	public SteamId Id;

	[SerializeField]
	private GameObject deathContainerPrefab;

	private PlayerStateMachine stateMachine;
	private PlayerContainerController containerController;
	private CharacterController controller;

	private CharacterInfo characterData;

	private List<PlayerCollider> playerColliders = new List<PlayerCollider>();

	[SyncVar(hook ="EquipmentHook")]
	private string equipmentJson;

	protected override void Awake()
	{
		base.Awake();
		stateMachine = GetComponent<PlayerStateMachine>();
		containerController = GetComponent<PlayerContainerController>();
		PlayerBonesLinks = GetComponent<PlayerBonesLinks>();
		controller = GetComponent<CharacterController>();
	}

	protected override void Start()
	{
		SetDefaultState();

		if (isLocalPlayer)
		{
			OnDoDamageEvent.AddListener(x => UIController.i.ShowHitMarker());

			ContainerWindowManager.i.PlayerIntialize(this);
			CameraController.i.Intialize(this);
			CraftWindowManager.Intialize(this);

			OnHealthChangeEvent.AddListener((oldHealth, newHealth) => UIController.i.SetHealthBarValue(1f * newHealth / maxHealth));

			OnTakeDamageEvent.AddListener(x => CameraController.CreateShake(5f, .3f));
		}

		foreach (Collider collider in HitboxColliders)
			playerColliders.Add(collider.GetComponent<PlayerCollider>());

		if (!isServer)
		{
			OnPublicEquipmentUpdate.AddListener(GetComponent<PlayerModelManager>().UpdateEquipmentContainer);
			if (equipmentJson != null)
				OnPublicEquipmentUpdate.Invoke(new Container(SimpleJSON.JSON.Parse(equipmentJson)));
		}
		if (isServer)
		{
			OnDeathEvent.AddListener(InitiateRespawn);
			OnRespawnEvent.AddListener(TeleportToSpawnPoint);
			OnRespawnEvent.AddListener(SetDefaultState);
		}
	}

	public void SelfKill()
    {
		Death();
    }

	[TargetRpc]
	public void SetPositionRpc(Vector3 position) => controller.SetPosition(position);

	private async void GetName(SteamId oldId, SteamId newId)
	{
		if (isServer)
			return;

#if UNITY_EDITOR
		name = "Test";
		return;
#endif

		Friend friend = new Friend(newId);
		await friend.RequestInfoAsync();

		name = friend.Name;
	}

	[Server]
	private void ServerIntialize(CharacterInfo data)
	{
		ContainerData.ContainerUpdateNotifier.Subscribe(data.inventoryId, containerController.SendContainerData);
		ContainerData.ContainerUpdateNotifier.Subscribe(data.equipmentId, containerController.SendContainerData);
		ContainerData.ContainerUpdateNotifier.Subscribe(data.equipmentId, SendEquipmentInfo);
	}

	[Server]
	public void SetCharacterInfo(CharacterInfo data)
	{
		if (!isDataAlreadyRecieved)
			ServerIntialize(data);

		isDataAlreadyRecieved = true;

		Container equipment = ContainerData.LoadedContainers[data.equipmentId];
		Container inventory = ContainerData.LoadedContainers[data.inventoryId];

		characterData = data;
		UpdateCharacterData(characterData);

		containerController.SendContainerData(data.inventoryId, inventory);
		containerController.SendContainerData(data.equipmentId, equipment);

		SendEquipmentInfo(data.equipmentId, equipment);
	}

	[ClientRpc]
	public void ShowModel() => PlayerBonesLinks.Model.gameObject.SetActive(true);

	[ClientRpc]
	public void HideModel() => PlayerBonesLinks.Model.gameObject.SetActive(false);

	[TargetRpc]
	private void UpdateCharacterData(CharacterInfo data)
	{
		isDataAlreadyRecieved = true;
		characterData = data;
		OnCharacterDataUpdateEvent.Invoke(characterData);
	}


	[Server]
	private void SendEquipmentInfo(int id, Container equip)
	{
		GetComponent<PlayerWeaponEquipment>().UpdateWeapon(equip);

		foreach (PlayerCollider playerCollider in playerColliders)
			playerCollider.UpdateModifier(equip);

		equipmentJson = equip.ToJson().ToString();
		SendEquipmentInfoRpc(equip);
	}

	[ClientRpc]
	private void SendEquipmentInfoRpc(Container equipment) => OnPublicEquipmentUpdate.Invoke(equipment);
	private void EquipmentHook(string oldJson, string newJson)
	{
		OnPublicEquipmentUpdate.Invoke(new Container(SimpleJSON.JSON.Parse(newJson)));
	}

	public CharacterInfo GetCharacterInfo() => characterData;

	protected override void SetDefaultState()
	{
		isAlive = true;
		SetHealth(maxHealth);
		stateMachine.ChangePlayerState(PlayerStateType.Idle);
	}

	private void TeleportToSpawnPoint()
    {
		CharacterInfo currentCharacterInfo = ((PlayerConnectionInfo)connectionToClient.authenticationData).character;
		if (SceneManager.GetActiveScene().name != currentCharacterInfo.spawnSceneName)
		{
			SceneChanger.ChangeCharacterScene(this, currentCharacterInfo.spawnSceneName, currentCharacterInfo.spawnPoint);
		}
		else
		{
			controller.SetPosition(currentCharacterInfo.spawnPoint);
			SetPositionRpc(currentCharacterInfo.spawnPoint);
		}
	}

	protected override void Death()
	{
		base.Death();

		if (isServer)
		{
			stateMachine.ChangePlayerState(PlayerStateType.Death);
			SpawnDeathContainer();
			DeathClientRpc();
		}
	}

	private async void SpawnDeathContainer()
	{
		GameObject deadBody = Instantiate(deathContainerPrefab);
		NetworkServer.Spawn(deadBody);

		deadBody.transform.position = transform.position + Vector3.up;
		deadBody.transform.rotation = transform.rotation;

		Container equipment = ContainerData.LoadedContainers[characterData.equipmentId];

		deadBody.GetComponent<DeathContainer>().SetEquipment(equipment);

		var www = Api.Request("/containers/?quantity=1", ApiMethod.POST);
		await www.SendWebRequest();
		int newContainerId = Api.GetResult(www)["id"].AsInt;

		List<int> characterContainersId = new List<int>();

		characterContainersId.Add(characterData.equipmentId);
		characterContainersId.Add(characterData.inventoryId);

		ContainerData.MoveAllItemsInNewContainer(characterContainersId, newContainerId);

		deadBody.GetComponent<Chest>().SetChestId(newContainerId);
		deadBody.GetComponent<DeathContainer>().CheckIsContainerEmpty(ContainerData.LoadedContainers[newContainerId]);
	}

	public override void TakeDamage(DamageMessage damageMessage)
	{
		if (damageMessage.source as Player != null &&
				(damageMessage.source as Player).GetCharacterInfo().fraction == characterData.fraction)
			return;
		Debug.Log(damageMessage.damage);
		base.TakeDamage(damageMessage);
		TakeDamageEventTargetRpc(damageMessage.damage);
	}

	private void OnDestroy()
	{
		if (isServer)
		{
			ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.inventoryId, containerController.SendContainerData);
			ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipmentId, containerController.SendContainerData);
			ContainerData.ContainerUpdateNotifier.UnSubscribe(characterData.equipmentId, SendEquipmentInfo);
		}
	}
}
