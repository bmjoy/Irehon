using UnityEngine;

namespace PolyPerfect
{
    public class Common_SurfaceRotation : MonoBehaviour
    {
        private string terrainLayer = "Terrain";
        private int layer;
        private bool rotate = true;
        private Quaternion targetRotation;
        private float rotationSpeed = 2f;

        private void Awake()
        {
            this.layer = LayerMask.GetMask(this.terrainLayer);
        }

        private void Start()
        {
            RaycastHit hit;
            Vector3 direction = this.transform.parent.TransformDirection(Vector3.down);

            if (Physics.Raycast(this.transform.parent.position, direction, out hit, 50f, this.layer))
            {
                float distance = hit.distance;
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                this.transform.rotation = surfaceRotation * this.transform.parent.rotation;
            }
        }

        private void Update()
        {
            if (!this.rotate)
            {
                return;
            }

            RaycastHit hit;
            Vector3 direction = this.transform.parent.TransformDirection(Vector3.down);

            if (Physics.Raycast(this.transform.parent.position, direction, out hit, 50f, this.layer))
            {
                float distance = hit.distance;
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                this.targetRotation = surfaceRotation * this.transform.parent.rotation;
            }

            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.targetRotation, Time.deltaTime * this.rotationSpeed);
        }

        public void SetRotationSpeed(float speed)
        {
            if (speed > 0f)
            {
                this.rotationSpeed = speed;
            }
        }

        private void OnBecameVisible()
        {
            this.rotate = true;
        }

        private void OnBecameInvisible()
        {
            this.rotate = false;
        }
    }
}