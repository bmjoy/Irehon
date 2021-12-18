using UnityEngine;

public class CameraDepth : MonoBehaviour
{
    public DepthTextureMode Mode;
    private Camera cam;

    private void OnEnable()
    {
        this.cam = this.GetComponent<Camera>();
        this.cam.depthTextureMode = this.Mode;
    }
}
