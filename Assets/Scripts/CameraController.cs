using Irehon.Entitys;
using Irehon.UI;
using UnityEngine;

namespace Irehon
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
        
        public delegate void ChangeLookingTargetEventHandler(Vector3 target);
        public delegate void TargetingOnEntityEventHandler(Entity entity, Player player);
        public delegate void ChangeCursorStateEventHandler(bool state);
        
        public GameObject InteractTarget { get; private set; }

        private const float MOUSE_SENSITIVITY_HORIZONTAL = 100f;
        private const float MOUSE_SENSITIVITY_VERTICAL = 70f;

        public event TargetingOnEntityEventHandler OnLookingOnEntityEvent;


        private Player player;
        private PlayerStateMachine playerStateMachine;
        private PlayerInteracter interacter;

        private bool isPlayerIntialized = false;


        private void Awake()
        {
            Instance = this;

            Player.OnPlayerIntializeEvent += this.Intialize;
        }

        private void Start()
        {
            this.OnLookingOnEntityEvent += (entity, player) => entity?.InvokePlayerLookingEvent();
        }

        public void Intialize(Player player)
        {
            this.player = player;
            this.interacter = player.GetComponent<PlayerInteracter>();
            this.playerStateMachine = player.GetComponent<PlayerStateMachine>();
            this.isPlayerIntialized = true;
        }


        public void Update()
        {
            if (Mouse.IsCursorEnabled || this.playerStateMachine == null || !this.playerStateMachine.CurrentState.CanRotateCamera)
            {
                return;
            }

            float xMouse = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY_HORIZONTAL * Time.deltaTime;
            float yMouse = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY_VERTICAL * Time.deltaTime;
            PlayerCamera.Instance.RotateCamera(new Vector2(xMouse, yMouse));
        }

        private void InvokeEntityTargetLookEvent()
        {
            RaycastHit hit;
            Entity entity = null;

            if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, 50f, 1 << 10))
            {
                entity = hit.collider.GetComponent<EntityCollider>()?.GetParentEntityComponent();
            }
            this.OnLookingOnEntityEvent.Invoke(entity, this.player);
        }

        private void FixedUpdate()
        {
            if (!this.isPlayerIntialized)
            {
                return;
            }

            if (this.interacter.isInteracting)
            {
                Hint.Instance.HideHint();
            }

            if (Mouse.IsCursorEnabled || !this.playerStateMachine.CurrentState.CanRotateCamera)
            {
                this.OnLookingOnEntityEvent.Invoke(null, this.player);
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, 7, 1 << 12))
            {
                InteractTarget = hit.collider.gameObject;
            }
            else
            {
                InteractTarget = null;
            }

            if (!this.interacter.isInteracting && InteractTarget != null)
            {
                Hint.Instance.ShowHint("Interact", "Press E to interract with this object");
            }
            else
            {
                Hint.Instance.HideHint();
            }

            this.InvokeEntityTargetLookEvent();
        }
    }
}