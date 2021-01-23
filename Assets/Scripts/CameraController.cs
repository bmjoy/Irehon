using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float MOUSE_SENSITIVITY_HORIZONTAL = 100f;
    private const float MOUSE_SENSITIVITY_VERTICAL = 70f;
    
    private Transform playerTransform;
    private Transform shoulderTransform;

    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera aimCamera;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera mainCamera;

    public static CameraController instance;

    private bool needToSendY = false;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InvokeRepeating("SendRotationPacket", 3, 0.05f);
    }

    public void SendRotationPacket()
    {
        if (playerTransform == null)
            return;
        if (needToSendY)
            PlayerController.instance.UpdateYRotation(playerTransform.rotation.eulerAngles.y);
        if (PlayerController.instance.IsAiming())
            PlayerController.instance.UpdateXRotation(xRotation);
    }

    public float GetPlayerYAxis()
    {
        return shoulderTransform.eulerAngles.x;
    }

    float xRotation = 0f;

    public void EnableAimCamera()
    {
        mainCamera.gameObject.SetActive(false);
        aimCamera.gameObject.SetActive(true);
    }

    public void DisableAimCamera()
    {
        mainCamera.gameObject.SetActive(true);
        aimCamera.gameObject.SetActive(false);
    }

    public void SetTarget(Transform shoulderTarget, Transform playerTransform)
    {
        shoulderTransform = shoulderTarget;
        this.playerTransform = playerTransform;
        mainCamera.Follow = shoulderTarget;
        aimCamera.Follow = shoulderTarget;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
            return;

        float xMouse = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY_HORIZONTAL * Time.deltaTime;
        float yMouse = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY_VERTICAL* Time.deltaTime;

        xRotation -= yMouse;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);

        shoulderTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerTransform.Rotate(Vector3.up * xMouse);
        if (xMouse != 0)
            needToSendY = true;
    }
}
