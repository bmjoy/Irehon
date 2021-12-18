using UnityEngine;

namespace Irehon.Camera
{
    public class PlayerCamera : MonoBehaviour
    {
        public static PlayerCamera Instance { get; private set; }
        public static bool CanRotateCamera;

        public UnityEngine.Camera Camera { get; private set; }

        [SerializeField]
        private Transform forwardCameraPoint;

        private Transform shoulderTransform;
        private Transform playerTransform;
        private CharacterController characterController;

        private float horizontalRotation = 0f;

        private void Awake()
        {
            Instance = this;

            this.Camera = this.GetComponent<UnityEngine.Camera>();
            Player.OnPlayerIntializeEvent += this.Intialize;
        }

        private void Update()
        {
            this.UpdateLookPosition();
        }

        public void RotateCamera(Vector2 rotate)
        {
            this.horizontalRotation -= rotate.y;
            this.horizontalRotation = Mathf.Clamp(this.horizontalRotation, -85f, 85f);

            this.shoulderTransform.localRotation = Quaternion.Euler(this.horizontalRotation, 0, 0f);

            this.characterController.Rotate(Vector3.up * rotate.x);
        }

        public Vector2 GetRotation()
        {
            Vector2 rotation = Vector2.zero;
            rotation.x = this.shoulderTransform.rotation.eulerAngles.x;
            rotation.y = this.playerTransform.eulerAngles.y;
            return rotation;
        }

        public Vector3 GetLookPosition()
        {
            return this.forwardCameraPoint.position;
        }

        private void UpdateLookPosition()
        {
            RaycastHit hit;
            Vector3 oldPosition = this.forwardCameraPoint.localPosition;

            if (Physics.Raycast(this.transform.position, this.transform.TransformDirection(Vector3.forward), out hit, 20, 1 << 11 | 1 << 10 | 1 << 13))
            {
                oldPosition.z = hit.distance;
            }
            else
            {
                oldPosition.z = 20;
            }
            this.forwardCameraPoint.localPosition = oldPosition;
        }

        private void Intialize(Player player)
        {
            PlayerBonesLinks links = player.GetComponent<PlayerBonesLinks>();
            this.shoulderTransform = links.Shoulder;
            this.characterController = player.GetComponent<CharacterController>();
            this.playerTransform = player.transform;
        }
    }
}