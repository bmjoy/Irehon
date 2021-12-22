using Irehon.Entitys;
using Irehon.UI;
using UnityEngine;

namespace Irehon.Camera
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
        public GameObject InteractTarget { get; private set; }

        public delegate void TargetingOnEntityEventHandler(Entity entity, Player player);


        private const float MouseHorizontalSensivity = 100f;
        private const float MouseVerticalSensivity = 70f;

        public event TargetingOnEntityEventHandler OnLookingOnEntityEvent;


        private Player player;
        private PlayerStateMachine playerStateMachine;
        private PlayerInteracter interacter;


        private void Awake()
        {
            Instance = this;

            Player.LocalPlayerIntialized += this.Intialize;
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
        }


        public void Update()
        {
            if (Mouse.IsCursorEnabled || this.playerStateMachine == null || !this.playerStateMachine.CurrentState.CanRotateCamera)
            {
                return;
            }

            float xMouse = Input.GetAxis("Mouse X") * MouseHorizontalSensivity * Time.deltaTime;
            float yMouse = Input.GetAxis("Mouse Y") * MouseVerticalSensivity * Time.deltaTime;
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
            if (this.interacter == null)
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
                this.InteractTarget = hit.collider.gameObject;
            }
            else
            {
                this.InteractTarget = null;
            }

            if (!this.interacter.isInteracting && this.InteractTarget != null)
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