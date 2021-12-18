using Irehon;
using Irehon.Client;
using Irehon.Entitys;
using Irehon.UI;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
    public PlayerContainerController ContainerController => this.containerController;

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
        this.stateMachine = this.GetComponent<PlayerStateMachine>();
        this.containerController = this.GetComponent<PlayerContainerController>();
        this.PlayerBonesLinks = this.GetComponent<PlayerBonesLinks>();
        this.controller = this.GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        this.SetDefaultState();

        foreach (Collider collider in this.HitboxColliders)
        {
            this.playerColliders.Add(collider.GetComponent<PlayerCollider>());
        }

        if (this.isLocalPlayer)
        {
            this.LocalPlayerIntialize();
        }

        if (this.isServer)
        {
            this.IntializeServerEvents();
            this.InvokeRepeating(nameof(PassiveRegenerateHealth), 1, 1);
        }
    }

    private void LocalPlayerIntialize()
    {
        OnPlayerIntializeEvent?.Invoke(this);

        OnDoDamageEvent += x => Hitmarker.Instance.ShowHitMarker();

        this.gameObject.layer = 1 << 1;

        OnHealthChangeEvent += (oldHealth, newHealth) => PlayerHealthbar.Instance.SetHealthBarValue(1f * newHealth / this.maxHealth);

        OnTakeDamageEvent += x => CameraShake.Instance.CreateShake(5f, .3f);

        if (this.fraction == Fraction.North)
        {
            this.FractionBehaviourData = this.northData;
        }
        else
        {
            this.FractionBehaviourData = this.southData;
        }
    }

    [Server]
    private void IntializeServerEvents()
    {
        OnDoDamageEvent += this.OnDoDamageRpc;
        OnTakeDamageEvent += this.OnTakeDamageRpc;

        OnDeathEvent += () => this.stateMachine.ChangePlayerState(PlayerStateType.Death);
        OnDeathEvent += () => this.SpawnDeathContainer();
        OnDeathEvent += this.InitiateRespawn;

        OnRespawnEvent += this.TeleportToSpawnPoint;
        OnRespawnEvent += this.SetDefaultState;

        OnGetKilledEvent += murder =>
        {
            if (murder != null && murder is Player)
            {
                ServerMessage killMessage = new ServerMessage()
                {
                    message = (murder as Player).Id.ToString(),
                    subMessage = this.Id.ToString(),
                    messageType = MessageType.KillLog
                };
                NetworkServer.SendToAll(killMessage);
            }
        };

        this.containerController.OnEquipmentUpdate += equipment =>
        {
            foreach (PlayerCollider playerCollider in this.playerColliders)
            {
                playerCollider.UpdateModifier(equipment);
            }
        };
    }

    private void PassiveRegenerateHealth()
    {
        if (this.isAlive)
        {
            this.SetHealth(this.health + 10);
        }
    }

    public void SelfKill()
    {
        this.Death();
    }

    [TargetRpc]
    public void SetPositionRpc(Vector3 position)
    {
        this.controller.SetPosition(position);
    }

    private async void GetName(SteamId oldId, SteamId newId)
    {
        if (this.isServer)
        {
            return;
        }

        Friend friend = new Friend(newId);
        await friend.RequestInfoAsync();

        this.name = friend.Name;
    }

    [Server]
    public void SetCharacterInfo(CharacterInfo data)
    {
        this.isDataAlreadyRecieved = true;

        this.characterInfo = data;
        this.fraction = this.characterInfo.fraction;
        if (this.fraction == Fraction.North)
        {
            this.FractionBehaviourData = this.northData;
        }
        else
        {
            this.FractionBehaviourData = this.southData;
        }

        this.UpdateCharacterData(this.characterInfo);
    }

    [ClientRpc]
    public void ShowModel()
    {
        this.PlayerBonesLinks.Model.gameObject.SetActive(true);
    }

    [ClientRpc]
    public void HideModel()
    {
        this.PlayerBonesLinks.Model.gameObject.SetActive(false);
    }

    [TargetRpc]
    private void UpdateCharacterData(CharacterInfo data)
    {
        this.isDataAlreadyRecieved = true;
        this.characterInfo = data;
        this.OnCharacterDataUpdateEvent?.Invoke(this.characterInfo);
    }

    public CharacterInfo GetCharacterInfo()
    {
        return this.characterInfo;
    }

    public override void SetDefaultState()
    {
        this.isAlive = true;
        this.SetHealth(this.maxHealth);
        this.stateMachine.ChangePlayerState(PlayerStateType.Idle);
    }

    [Server]
    private void TeleportToSpawnPoint()
    {
        CharacterInfo currentCharacterInfo = ((PlayerConnectionInfo)this.connectionToClient.authenticationData).character;
        if (SceneManager.GetActiveScene().name != currentCharacterInfo.spawnSceneName)
        {
            SceneChanger.ChangeCharacterScene(this, currentCharacterInfo.spawnSceneName, currentCharacterInfo.spawnPoint);
        }
        else
        {
            this.controller.SetPosition(currentCharacterInfo.spawnPoint);
            this.SetPositionRpc(currentCharacterInfo.spawnPoint);
        }
    }

    [Server]
    private void SpawnDeathContainer()
    {
        GameObject deadBody = Instantiate(this.deathContainerPrefab);
        NetworkServer.Spawn(deadBody);

        deadBody.transform.position = this.transform.position + Vector3.up;
        deadBody.transform.rotation = this.transform.rotation;

        deadBody.GetComponent<DeathContainer>().AttachMultipleContainers(new List<Container> { this.containerController.inventory, this.containerController.equipment });
    }
}
