using UnityEngine;

public class HS_CameraController : MonoBehaviour
{
    //camera holder
    public Transform Holder;
    public Vector3 cameraPos = new Vector3(0, 0, 0);
    public float currDistance = 5.0f;
    public float xRotate = 250.0f;
    public float yRotate = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float prevDistance;
    private float x = 0.0f;
    private float y = 0.0f;

    //For camera colliding
    private RaycastHit hit;
    public LayerMask collidingLayers = ~0; //Target marker can only collide with scene layer
    private float distanceHit;

    private void Start()
    {
        Vector3 angles = this.transform.eulerAngles;
        this.x = angles.y;
        this.y = angles.x;
    }

    private void LateUpdate()
    {
        if (this.currDistance < 2)
        {
            this.currDistance = 2;
        }

        // (currDistance - 2) / 3.5f - constant for far camera position
        Vector3 targetPos = this.Holder.position + new Vector3(0, (this.distanceHit - 2) / 3f + this.cameraPos[1], 0);

        this.currDistance -= Input.GetAxis("Mouse ScrollWheel") * 2;
        if (this.Holder)
        {
            Vector3 pos = Input.mousePosition;
            float dpiScale = 1;
            if (Screen.dpi < 1)
            {
                dpiScale = 1;
            }

            if (Screen.dpi < 200)
            {
                dpiScale = 1;
            }
            else
            {
                dpiScale = Screen.dpi / 200f;
            }

            if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale)
            {
                return;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            this.x += (float)(Input.GetAxis("Mouse X") * this.xRotate * 0.02);
            this.y -= (float)(Input.GetAxis("Mouse Y") * this.yRotate * 0.02);
            this.y = ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
            Quaternion rotation = Quaternion.Euler(this.y, this.x, 0);
            Vector3 position = rotation * new Vector3(0, 0, -this.currDistance) + targetPos;
            //If camera collide with collidingLayers move it to this point.
            if (Physics.Raycast(targetPos, position - targetPos, out this.hit, (position - targetPos).magnitude, this.collidingLayers))
            {
                this.transform.position = this.hit.point;
                //Min(4) distance from ground for camera target point
                this.distanceHit = Mathf.Clamp(Vector3.Distance(targetPos, this.hit.point), 4, 600);

            }
            else
            {
                this.transform.position = position;
                this.distanceHit = this.currDistance;
            }
            this.transform.rotation = rotation;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (this.prevDistance != this.currDistance)
        {
            this.prevDistance = this.currDistance;
            Quaternion rot = Quaternion.Euler(this.y, this.x, 0);
            // (currDistance - 2) / 3.5f - constant for far camera position
            Vector3 po = rot * new Vector3(0, 0, -this.currDistance) + targetPos;
            this.transform.rotation = rot;
            this.transform.position = po;
        }
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }
}