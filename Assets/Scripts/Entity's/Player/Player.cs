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
	[SerializeField]
	private GameObject deathContainerPrefab;

	[SyncVar(hook = "GetName")]
	public SteamId Id;
	public PlayerBonesLinks PlayerBonesLinks { get; private set; }
	public bool isDataAlreadyRecieved { get; private set; } = false;
	public OnCharacterDataUpdate OnCharacterDataUpdateEvent = new OnCharacterDataUpdate();
	public PlayerContainerController ContainerController => containerController;
	public OnContainerUpdate OnPublicEquipmentUpdate { get; private set; } = new OnContainerUpdate();
	private PlayerStateMachine stateMachine;
	private PlayerContainerController containerController;
	private CharacterInfo characterData;


	protected override void Awake()
	{
		base.Awake();
		stateMachine = GetComponent<PlayerStateMachine>();
		containerController = GetComponent<PlayerContainerController>();
		PlayerBonesLinks = GetComponent<PlayerBonesLinks>();
	}

	protected override void Start()
	{
		isAlive = true;
		SetHealth(maxHealth);

		if (isLocalPlayer)
		{
			HitConfirmEvent.AddListener(x => UIController.i.ShowHitMarker());

			ContainerWindowManager.i.PlayerIntialize(this);
			CameraController.i.Intialize(this);
			CraftWindowManager.Intialize(this);

			OnHealthChanged.AddListener((oldHealth, newHealth) => UIController.i.SetHealthBarValue(1f * newHealth / maxHealth));

			OnTakeDamageEvent.AddListener(x => CameraController.CreateShake(5f, .3f));
		}

		OnPublicEquipmentUpdate.AddListener(x => print($"Got equip info {x.ToJson()}"));

		if (!isServer)
			OnPublicEquipmentUpdate.AddListener(GetComponent<PlayerModelManager>().UpdateEquipmentContainer);
	}

	private async void GetName(SteamId oldId, SteamId newId)
	{
		if (isServer)
			return;

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
	public void SendCharacterInfo(CharacterInfo data)
	{
		StartCoroutine(SendInfo());

		if (!isDataAlreadyRecieved)
			ServerIntialize(data);

		isDataAlreadyRecieved = true;

		IEnumerator SendInfo()
		{
			yield return ContainerData.LoadContainer(data.equipmentId);
			Container equipment = ContainerData.LoadedContainers[data.equipmentId];

			yield return ContainerData.LoadContainer(data.inventoryId);
			Container inventory = ContainerData.LoadedContainers[data.inventoryId];

			characterData = data;
			UpdateCharacterData(characterData);

			containerController.SendContainerData(data.inventoryId, inventory);
			containerController.SendContainerData(data.equipmentId, equipment);

			SendEquipmentInfo(data.equipmentId, equipment);
		}
	}

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
		SendEquipmentInfoRpc(equip);
	}

	[ClientRpc]
	private void SendEquipmentInfoRpc(Container equipment) => OnPublicEquipmentUpdate.Invoke(equipment);


	public CharacterInfo GetCharacterData() => characterData;

	protected override void SetDefaultState()
	{
		isAlive = true;
		SetHealth(maxHealth);
		stateMachine.ChangePlayerState(PlayerStateType.Idle);

		if (isServer)
		{
			CharacterInfo currentCharacterInfo = ((PlayerConnectionInfo)connectionToClient.authenticationData).character;
			if (SceneManager.GetActiveScene().name != currentCharacterInfo.spawnSceneName)
			{
				SceneChanger.ChangeCharacterScene(this, currentCharacterInfo.spawnSceneName, currentCharacterInfo.spawnPoint);
			}
			else
			{
				transform.position = currentCharacterInfo.spawnPoint;
			}
		}
	}

	protected override void Death()
	{
		base.Death();


		if (isServer)
		{
			stateMachine.ChangePlayerState(PlayerStateType.Death);
			StartCoroutine(SpawnDeathContainer());
		}

		if (isServerOnly)
			DeathOnClient();
	}

	IEnumerator SpawnDeathContainer()
	{
		GameObject deadBody = Instantiate(deathContainerPrefab);
		NetworkServer.Spawn(deadBody);

		deadBody.transform.position = transform.position;
		deadBody.transform.rotation = transform.rotation;

		yield return ContainerData.LoadContainer(characterData.equipmentId);
		Container equipment = ContainerData.LoadedContainers[characterData.equipmentId];

		deadBody.GetComponent<DeathContainer>().SetEquipment(equipment);

		var www = Api.Request("/containers/?quantity=1", ApiMethod.POST);
		yield return www.SendWebRequest();
		int newContainerId = Api.GetResult(www)["id"].AsInt;

		List<int> characterContainersId = new List<int>();

		characterContainersId.Add(characterData.equipmentId);
		characterContainersId.Add(characterData.inventoryId);

		yield return ContainerData.MoveAllItemsInNewContainer(characterContainersId, newContainerId);

		deadBody.GetComponent<Chest>().SetChestId(newContainerId);
	}

	protected override void Respawn()
	{
		base.Respawn();

		if (isServer)
			RespawnOnClient();
	}

	[ClientRpc]
	protected virtual void DeathOnClient() => Death();

	[ClientRpc]
	protected virtual void RespawnOnClient() => Respawn();

	[Server]
	public void DoDamage(Entity target, int damage)
	{
		DamageMessage damageMessage = new DamageMessage
		{
			damage = damage,
			source = this,
			target = target
		};
		target.TakeDamage(damageMessage);
	}

	public override void TakeDamage(DamageMessage damageMessage)
	{
		if (damageMessage.source as Player != null &&
				(damageMessage.source as Player).GetCharacterData().fraction == characterData.fraction)
			return;

		base.TakeDamage(damageMessage);
		TakeDamageEvent(damageMessage.damage);
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
