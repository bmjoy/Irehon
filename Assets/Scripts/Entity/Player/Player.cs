using Irehon.Camera;
using Irehon.Client;
using Irehon.Entitys;
using Irehon.UI;
using Mirror;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Irehon
{
    public class Player : Entity
    {
        public delegate void PlayerEventHandler(Player player);
        public delegate void CharacterInfoUpdateEventHandler(CharacterInfo characterInfo);
        
        public static event PlayerEventHandler LocalPlayerIntialized;
        public static Player LocalPlayer { get; private set; }

        public event CharacterInfoUpdateEventHandler CharacterInfoUpdated;
        public PlayerBonesLinks PlayerBonesLinks { get; private set; }
        public bool isDataAlreadyRecieved { get; private set; } = false;
        
        [SyncVar(hook = nameof(GetName))]
        public SteamId Id;

        [SerializeField]
        private FractionBehaviourData northData;
        [SerializeField]
        private FractionBehaviourData southData;
        [SerializeField]
        private GameObject deathContainerPrefab;

        private PlayerStateMachine stateMachine;
        private CharacterController controller;
        private PlayerContainers playerContainers;
        private CharacterInfo characterInfo;

        private List<PlayerCollider> playerColliders = new List<PlayerCollider>();

        protected override void Awake()
        { 
            base.Awake();
            playerContainers = GetComponent<PlayerContainers>();
            stateMachine = GetComponent<PlayerStateMachine>();
            this.PlayerBonesLinks = GetComponent<PlayerBonesLinks>();
            controller = GetComponent<CharacterController>();
        }

        protected override async void Start()
        {
            SetDefaultState();

            foreach (Collider collider in HitboxColliders)
            {
                playerColliders.Add(collider.GetComponent<PlayerCollider>());
            }

            if (this.isLocalPlayer)
            {
                LocalPlayerIntialize();
            }

            if (this.isServer)
            {
                IntializeServerEvents();

                InvokeRepeating(nameof(PassiveRegenerateHealth), 1, 1);

                SetHealth(characterInfo.health);
            }
        }

        private void LocalPlayerIntialize()
        {
            LocalPlayer = this;
            LocalPlayerIntialized?.Invoke(this);
            //—бросить всех после первого вызова
            LocalPlayerIntialized = x => { };

            DidDamage += x => Hitmarker.Instance.ShowHitMarker();

            this.gameObject.layer = 1 << 1;

            HealthChanged += (oldHealth, newHealth) => PlayerHealthbar.Instance.SetHealthBarValue(1f * newHealth / maxHealth);

            GotDamage += x => CameraShake.Instance.CreateShake(5f, .3f);

            if (fraction == Fraction.North)
            {
                FractionBehaviourData = northData;
            }
            else
            {
                FractionBehaviourData = southData;
            }
        }

        [Server]
        private void IntializeServerEvents()
        {
            DidDamage += OnDoDamageRpc;
            GotDamage += OnTakeDamageRpc;

            Dead += () => stateMachine.ChangePlayerState(PlayerStateType.Death);
            Dead += () => SpawnDeathContainer();
            Dead += InitiateRespawn;

            Respawned += TeleportToSpawnPoint;
            Respawned += SetDefaultState;

            KilledByEntity += SendAllKillMesagge;
        }

        private void SendAllKillMesagge(Entity murder)
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
        }

        public void UpdateArmorModifiers(Container equipment)
        {
            foreach (PlayerCollider playerCollider in playerColliders)
            {
                playerCollider.UpdateModifier(equipment);
            }
        }

        private void PassiveRegenerateHealth()
        {
            if (isAlive)
            {
                SetHealth(health + 10);
            }
        }

        public void SelfKill()
        {
            Death();
        }

        [TargetRpc]
        public void SetPositionRpc(Vector3 position)
        {
            controller.SetPosition(position);
        }

        private async void GetName(SteamId oldId, SteamId newId)
        {
            if (this.isServer)
            {
                return;
            }

            Friend friend = new Friend(newId);
            await friend.RequestInfoAsync();

            name = friend.Name;
        }

        [Server]
        public void SetCharacterInfo(CharacterInfo data)
        {
            this.isDataAlreadyRecieved = true;

            characterInfo = data;
            fraction = characterInfo.fraction;
            if (fraction == Fraction.North)
            {
                FractionBehaviourData = northData;
            }
            else
            {
                FractionBehaviourData = southData;
            }

            UpdateCharacterData(characterInfo);
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
            characterInfo = data;
            CharacterInfoUpdated?.Invoke(characterInfo);
        }

        public CharacterInfo GetCharacterInfo()
        {
            return characterInfo;
        }

        public override void SetDefaultState()
        {
            isAlive = true;
            SetHealth(maxHealth);
            stateMachine.ChangePlayerState(PlayerStateType.Idle);
        }

        [Server]
        private void TeleportToSpawnPoint()
        {
            CharacterInfo currentCharacterInfo = ((PlayerSession)this.connectionToClient.authenticationData).character;
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

            deadBody.transform.position = this.transform.position + Vector3.up;
            deadBody.transform.rotation = this.transform.rotation;

            deadBody.GetComponent<DeathChest>().AttachMultipleContainers(new List<Container> { playerContainers.inventory, playerContainers.equipment});
        }
    }
}