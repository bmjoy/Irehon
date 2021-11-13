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
    public static CameraController i;
    public static bool IsCursosLocked => i.cursorAiming;
    public static OnChangeCursorHidingState OnChangeCursorStateEvent => i.OnChangeCursorState;

    public OnChangeCursorHidingState OnChangeCursorState = new OnChangeCursorHidingState();
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
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private PlayerInteracter interacter;
    private Transform playerTransform;
    private Transform shoulderTransform;
    private bool cursorAiming;
    private float xRotation = 0f;
    private bool isLookingAtFloor;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(this);
        else
            i = this;

        cameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorAiming = false;
        currentShakeHandler = mainCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
    }

    public static void CreateShake(float power, float time)
    {
        i.shakeTimer = time;
        i.shakeTimerTotal = time;
        i.currentIntensity = power;
        i.currentShakeHandler.m_AmplitudeGain = i.currentIntensity;
    }
    public static Vector2 GetRotation()
    {
        return new Vector2(i.xRotation, i.playerTransform.rotation.eulerAngles.y);
    }

    public bool IsTargetOnFloor()
    {
        return isLookingAtFloor;
    }
    public static Vector2 GetCurrentRotation()
    {
        Vector2 rotation = Vector2.zero;
        rotation.x = i.shoulderTransform.rotation.eulerAngles.x;
        rotation.y = i.playerTransform.eulerAngles.y;
        return rotation;
    }

    public static Vector3 GetLookingTargetPosition()
    {
        return i.targetTransform.position;
    }

    public static float GetPlayerYAxis()
    {
        float angle = i.shoulderTransform.eulerAngles.x;
        angle = (angle > 180) ? angle - 360 : angle;
        return angle;
    }

    public static void EnableAimCamera()
    {
        i.mainCamera.gameObject.SetActive(false);
        i.aimCamera.gameObject.SetActive(true);
        float currentAmplitude = i.currentShakeHandler.m_AmplitudeGain;
        i.currentShakeHandler = i.aimCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        i.currentShakeHandler.m_AmplitudeGain = currentAmplitude;
    }

    public static void DisableAimCamera()
    {
        i.mainCamera.gameObject.SetActive(true);
        i.aimCamera.gameObject.SetActive(false);
        float currentAmplitude = i.currentShakeHandler.m_AmplitudeGain;
        i.currentShakeHandler = i.mainCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        i.currentShakeHandler.m_AmplitudeGain = currentAmplitude;
    }

    public void Intialize(Player player)
    {
        PlayerBonesLinks links = player.GetComponent<PlayerBonesLinks>();
        shoulderTransform = links.Shoulder;
        playerTransform = player.transform;
        mainCamera.Follow = links.Shoulder;
        aimCamera.Follow = links.Shoulder;
        this.player = player;
        interacter = player.GetComponent<PlayerInteracter>();
        playerStateMachine = player.GetComponent<PlayerStateMachine>();
    }

    private void UpdateLookingPoint()
    {
        RaycastHit hit;
        Vector3 oldPosition = targetTransform.localPosition;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, 1 << 11 | 1 << 10 | 1 << 12))
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
        targetTransform.localPosition = oldPosition;
    }

    private void RotateCamera()
    {
        float xMouse = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY_HORIZONTAL * Time.fixedDeltaTime;
        float yMouse = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY_VERTICAL * Time.fixedDeltaTime;

        xRotation -= yMouse;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        shoulderTransform.localRotation = Quaternion.Euler(xRotation, -3.5f, 0f);

        player.Rotate(Vector3.up * xMouse);
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
    }

    //TODO: перевести в UI controller
    public static void EnableCursor()
    {
        if (i != null)
        {
            i.cursorAiming = false;
            UIController.i.HideHint();
            i.OnChangeCursorState.Invoke(true);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void DisableCursor()
    {
        i.OnChangeCursorState.Invoke(false);
        i.cursorAiming = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        TooltipWindowController.HideTooltip();
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
            currentShakeHandler.m_AmplitudeGain = 0;

        if (interacter.isInteracting)
            UIController.i.HideHint();

        if (!cursorAiming || !playerStateMachine.CurrentState.CanRotateCamera)
            return;

        if (!interacter.isInteracting && Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), 4, 1 << 12))
            UIController.i.ShowHint("Interact", "Press E to interract with this object");
        else
            UIController.i.HideHint();

        RotateCamera();

        UpdateLookingPoint();
    }
}
