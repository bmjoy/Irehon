using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public Camera cameraComponent { get; private set; }
    private const float MOUSE_SENSITIVITY_HORIZONTAL = 100f;
    private const float MOUSE_SENSITIVITY_VERTICAL = 70f;
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera aimCamera;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera mainCamera;
    private PlayerController player;
    private Transform playerTransform;
    private Transform shoulderTransform;
    private bool cursorAiming;
    private bool needToSendY = false;
    private bool needToSendX = false;

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void Awake()
    {
        instance = this;
        cameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorAiming = true;
        InvokeRepeating("SendRotationPacket", 3, 0.05f);
    }

    public void SendRotationPacket()
    {
        if (playerTransform == null)
            return;
        if (needToSendY)
            player.UpdateYRotation(playerTransform.rotation.eulerAngles.y);
        if (needToSendX)
            player.UpdateXRotation(xRotation);
        needToSendX = false;
        needToSendY = false;
    }

    public Vector2 GetCurrentRotation()
    {
        Vector2 rotation = Vector2.zero;
        rotation.x = shoulderTransform.rotation.eulerAngles.x;
        rotation.y = playerTransform.eulerAngles.y;
        return rotation;
    }

    public Vector3 GetLookingTargetPosition()
    {
        return targetTransform.position;
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
        player = playerTransform.GetComponent<PlayerController>();
    }

    public void UpdateTarget()
    {
        RaycastHit hit = new RaycastHit();
        Vector3 oldPosition = targetTransform.localPosition;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, 1 << 11))
            oldPosition.z = hit.distance;
        else
            oldPosition.z = 20;

        targetTransform.localPosition = oldPosition;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            EnableCursor();
        else if (Input.GetKeyDown(KeyCode.Mouse1)) 
        {
            DisableCursor();
        }
    }

    private void EnableCursor()
    {
        cursorAiming = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisableCursor()
    {
        cursorAiming = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
            return;

        if (!cursorAiming || !player.IsControllAllowed())
            return;

        UpdateTarget();

        float xMouse = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY_HORIZONTAL * Time.deltaTime;
        float yMouse = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY_VERTICAL* Time.deltaTime;

        xRotation -= yMouse;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f);

        shoulderTransform.localRotation = Quaternion.Euler(xRotation,-3.5f, 0f);
        if (yMouse != 0)
            needToSendX = true;

        playerTransform.Rotate(Vector3.up * xMouse);
        if (xMouse != 0)
            needToSendY = true;
    }
}
