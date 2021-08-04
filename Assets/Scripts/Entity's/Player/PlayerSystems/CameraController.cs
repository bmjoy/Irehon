using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnChangingTarget : UnityEvent<Vector3>
{
}

public class OnChangeCursorHidingState : UnityEvent<bool>
{
}

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public OnChangingTarget OnChangingTarget;
    public OnChangeCursorHidingState OnChangeCursorState;
    public Camera cameraComponent { get; private set; } 
    private const float MOUSE_SENSITIVITY_HORIZONTAL = 100f;
    private const float MOUSE_SENSITIVITY_VERTICAL = 70f;
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera aimCamera;
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera mainCamera;
    private Cinemachine.CinemachineBasicMultiChannelPerlin currentShakeHandler;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float currentIntensity;
    private PlayerController playerController;
    private Player player;
    private Transform playerTransform;
    private Transform shoulderTransform;
    private bool cursorAiming;
    private bool needToSendY = false;
    private bool needToSendX = false;
    private bool isLookingAtFloor;

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
        OnChangingTarget = new OnChangingTarget();
        OnChangeCursorState = new OnChangeCursorHidingState();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorAiming = false;
        currentShakeHandler = mainCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        InvokeRepeating("SendRotationPacket", 3, 0.05f);
    }

    public void CreateShake(float power, float time)
    {
        shakeTimer = time;
        shakeTimerTotal = time;
        currentIntensity = power;
        currentShakeHandler.m_AmplitudeGain = currentIntensity;
    }

    public void SendRotationPacket()
    {
        if (playerTransform == null)
            return;
        if (needToSendY)
            playerController.UpdateYRotation(playerTransform.rotation.eulerAngles.y);
        if (needToSendX)
            playerController.UpdateXRotation(xRotation);
        needToSendX = false;
        needToSendY = false;
    }

    public bool IsTargetOnFloor()
    {
        return isLookingAtFloor;
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
        float angle = shoulderTransform.eulerAngles.x;
        angle = (angle > 180) ? angle - 360 : angle;
        return angle;
    }

    float xRotation = 0f;

    public void EnableAimCamera()
    {
        mainCamera.gameObject.SetActive(false);
        aimCamera.gameObject.SetActive(true);
        float currentAmplitude = currentShakeHandler.m_AmplitudeGain;
        currentShakeHandler = aimCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        currentShakeHandler.m_AmplitudeGain = currentAmplitude;
    }

    public void DisableAimCamera()
    {
        mainCamera.gameObject.SetActive(true);
        aimCamera.gameObject.SetActive(false);
        float currentAmplitude = currentShakeHandler.m_AmplitudeGain;
        currentShakeHandler = mainCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        currentShakeHandler.m_AmplitudeGain = currentAmplitude;
    }

    public void SetTarget(Transform shoulderTarget, Transform playerTransform)
    {
        shoulderTransform = shoulderTarget;
        this.playerTransform = playerTransform;
        mainCamera.Follow = shoulderTarget;
        aimCamera.Follow = shoulderTarget;
        playerController = playerTransform.GetComponent<PlayerController>();
        player = playerTransform.GetComponent<Player>();
    }

    public void UpdateTarget()
    {
        RaycastHit hit;
        Vector3 oldPosition = targetTransform.localPosition;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, 1 << 11 | 1 << 10))
        {
            oldPosition.z = hit.distance;
            if (hit.collider.CompareTag("Floor") || hit.collider.CompareTag("Walkable"))
                isLookingAtFloor = true;
            else
                isLookingAtFloor = false;
        }
        else
        {
            isLookingAtFloor = false;
            oldPosition.z = 20;
        }
        OnChangingTarget.Invoke(targetTransform.position);
        targetTransform.localPosition = oldPosition;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            if (cursorAiming)
                EnableCursor();
            else
                DisableCursor();
        }
        if (Input.GetKeyDown(KeyCode.E))
            InteractAttemp();
    }

    private void InteractAttemp()
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 4, 1 << 12))
            return;
        player.InterractAttempToServer(hit.point);
    }

    private void EnableCursor()
    {
        OnChangeCursorState.Invoke(false);
        cursorAiming = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisableCursor()
    {
        OnChangeCursorState.Invoke(true);
        cursorAiming = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
            return;

        if (shakeTimer >= 0)
        {
            shakeTimer -= Time.fixedDeltaTime;
            currentShakeHandler.m_AmplitudeGain = Mathf.Lerp(currentIntensity, 0, (1 - shakeTimer / shakeTimerTotal));
        }
        else
        {
            currentShakeHandler.m_AmplitudeGain = 0;
        }

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 4, 1 << 12))
            UIController.instance.ShowInteractableHint();
        else
            UIController.instance.HideInteractableHint();

        if (!cursorAiming || !playerController.IsControllAllowed())
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
