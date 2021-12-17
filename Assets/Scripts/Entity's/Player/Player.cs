using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Server;
using Client;

public class CharacterInfoEvent : UnityEvent<CharacterInfo> { }

public struct SpawnPointInfo
{
	public Vector3 spawnPosition;
	public string spawnSceneName;
}

public class Player : Entity
{
	public delegate void PlayerEventHandler(Player player);
	public static event PlayerEventHandler OnPlayerIntializeEvent;
	public CharacterInfoEvent OnCharacterDataUpdateEvent { get; private set; } = new CharacterInfoEvent();
	public PlayerBonesLinks PlayerBonesLinks { get; private set; }
	public bool isDataAlreadyRecieved { get; private set; } = false;
	public PlayerContainerController ContainerController => containerController;

	[SyncVar(hook = nameof(GetName)), HideInInspector]
	public SteamId Id;

	[SerializeField]
	private FractionBehaviourData northData;
	[SerializeField]
	private FractionBehaviourData southData;

	[SerializeField]
	private GameObject deathContainerPrefab;

	private PlayerStateMachine stateMachine;
	private PlayerContainerController containerController;
	private CharacterController controller;

	private CharacterInfo characterInfo;

	private List<PlayerCollider> playerColliders = new List<PlayerCollider>();

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

		foreach (Collider collider in HitboxColliders)
			playerColliders.Add(collider.GetComponent<PlayerCollider>());
		
		if (isLocalPlayer)
			LocalPlayerIntialize();

		if (isServer)
		{
			IntializeServerEvents();
			InvokeRepeating(nameof(PassiveRegenerateHealth), 1, 1);
		}
	}

	private void LocalPlayerIntialize()
    {
		OnPlayerIntializeEvent?.Invoke(this);

		OnDoDamageEvent += x => UIController.i.ShowHitMarker();

		gameObject.layer = 1 << 1;

		OnHealthChangeEvent += (oldHealth, newHealth) => UIController.i.SetHealthBarValue(1f * newHealth / maxHealth);

		OnTakeDamageEvent += x => CameraController.CreateShake(5f, .3f);

		if (fraction == Fraction.North)
			FractionBehaviourData = northData;
		else
			FractionBehaviourData = southData;
	}

	[Server]
	private void IntializeServerEvents()
    {
		OnDoDamageEvent += OnDoDamageRpc;
		OnTakeDamageEvent += OnTakeDamageRpc;

		OnDeathEvent += () => stateMachine.ChangePlayerState(PlayerStateType.Death);
		OnDeathEvent += () => SpawnDeathContainer();
		OnDeathEvent += InitiateRespawn;

		OnRespawnEvent += TeleportToSpawnPoint;
		OnRespawnEvent += SetDefaultState;

		OnGetKilledEvent += murder =>
		{
			if (murder != null && murder is Player)
			{
				ServerMessage killMessage = new ServerMessage()
				{
					message = (murder as Player).Id.ToString(),
					subMessage = Id.ToString(),
					messageType = MessageType.KillLog
				};
				NetworkServer.SendToAll(killMessage);
			}
		};

		containerController.OnEquipmentUpdate += equipment =>
		{
			foreach (PlayerCollider playerCollider in playerColliders)
				playerCollider.UpdateModifier(equipment);
		};
	}

	private void PassiveRegenerateHealth()
    {
		if (isAlive)
			SetHealth(health + 10);
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

		Friend friend = new Friend(newId);
		await friend.RequestInfoAsync();

		name = friend.Name;
	}

	[Server]
	public void SetCharacterInfo(CharacterInfo data)
	{
		isDataAlreadyRecieved = true;

		characterInfo = data;
		fraction = characterInfo.fraction;
		if (fraction == Fraction.North)
			FractionBehaviourData = northData;
		else
			FractionBehaviourData = southData;
		UpdateCharacterData(characterInfo);
	}  

	[ClientRpc]
	public void ShowModel() => PlayerBonesLinks.Model.gameObject.SetActive(true);

	[ClientRpc]
	public void HideModel() => PlayerBonesLinks.Model.gameObject.SetActive(false);

	[TargetRpc]
	private void UpdateCharacterData(CharacterInfo data)
	{
		isDataAlreadyRecieved = true;
		characterInfo = data;
		OnCharacterDataUpdateEvent?.Invoke(characterInfo);
	}

	public CharacterInfo GetCharacterInfo() => characterInfo;

	public override void SetDefaultState()
	{
		isAlive = true;
		SetHealth(maxHealth);
		stateMachine.ChangePlayerState(PlayerStateType.Idle);
	}

	[Server]
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

	[Server]
	private void SpawnDeathContainer()
	{
		GameObject deadBody = Instantiate(deathContainerPrefab);
		NetworkServer.Spawn(deadBody);

		deadBody.transform.position = transform.position + Vector3.up;
		deadBody.transform.rotation = transform.rotation;

		deadBody.GetComponent<DeathContainer>().AttachMultipleContainers(new List<Container> { containerController.inventory, containerController.equipment });
	}
}
