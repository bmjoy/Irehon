using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public delegate void ChangeLookingTargetEventHandler(Vector3 target);
    public delegate void TargetingOnEntityEventHandler(Entity entity, Player player);
    public delegate void ChangeCursorStateEventHandler(bool state);
    public static CameraController i;
    public static bool IsCursosLocked => i.cursorAiming;
    public TargetingOnEntityEventHandler OnLookingOnEntityEvent;
    public static event ChangeCursorStateEventHandler OnChangeCursorStateEvent;
    public static GameObject interactableTarget;

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
    private CharacterController characterController;
    private bool cursorAiming;
    private float xRotation = 0f;

    private void Awake()
    {
        if (i != null && i != this)
            Destroy(this);
        else
            i = this;

        cameraComponent = GetComponent<Camera>();
        Player.OnPlayerIntializeEvent += Intialize;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorAiming = true;
        currentShakeHandler = mainCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        OnLookingOnEntityEvent += (entity, player) => entity?.InvokePlayerLookingEvent();
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
        characterController = player.GetComponent<CharacterController>();
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

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 20, 1 << 11 | 1 << 10 | 1 << 13))
        {
            oldPosition.z = hit.distance;
        }
        else
        {
            oldPosition.z = 20;
        }
        targetTransform.localPosition = oldPosition;
    }

    private void RotateCamera()
    {
        float xMouse = Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY_HORIZONTAL * Time.deltaTime;
        float yMouse = Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY_VERTICAL * Time.deltaTime;

        xRotation -= yMouse;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        shoulderTransform.localRotation = Quaternion.Euler(xRotation, 0, 0f);

        characterController.Rotate(Vector3.up * xMouse);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (cursorAiming)
                EnableCursor();
            else
                DisableCursor();
        }

        if (!cursorAiming || playerStateMachine == null || !playerStateMachine.CurrentState.CanRotateCamera)
            return;

        RotateCamera();
    }

    //TODO: перевести в UI controller
    public static void EnableCursor()
    {
        if (i != null)
        {
            i.cursorAiming = false;
            UIController.i.HideHint();
            UIController.i.DisableDefaultCrosshair();
            OnChangeCursorStateEvent.Invoke(true);
        }
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void DisableCursor()
    {
        OnChangeCursorStateEvent.Invoke(false);
        i.cursorAiming = true;
        Cursor.visible = false;
        UIController.i.EnableDefaultCrosshair();
        Cursor.lockState = CursorLockMode.Locked;
        TooltipWindowController.HideTooltip();
    }

    private void InvokeEntityTargetLookEvent()
    {
        RaycastHit hit;
        Entity entity = null;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50f, 1 << 10))
        {
            entity = hit.collider.GetComponent<EntityCollider>()?.GetParentEntityComponent();
        }
        OnLookingOnEntityEvent.Invoke(entity, player); 
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
        {
            OnLookingOnEntityEvent.Invoke(null, player);
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 7, 1 << 12))
        {
            interactableTarget = hit.collider.gameObject;
        }
        else
            interactableTarget = null;

        if (!interacter.isInteracting && interactableTarget != null)
            UIController.i.ShowHint("Interact", "Press E to interract with this object");
        else
            UIController.i.HideHint();

        UpdateLookingPoint();
        InvokeEntityTargetLookEvent();
    }
}
