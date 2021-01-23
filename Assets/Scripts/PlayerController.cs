using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private List<InputState> sendedInputs = new List<InputState>();

    private const float MOVEMENT_SPEED = 0.05f;

    private InputState previousInput;

    private int currentFrame;

    public struct InputState
    {
        public bool ForwardKeyDown;
        public bool BackKeyDown;
        public bool RightKeyDown;
        public bool LeftKeyDown;

        public float currentRotation;

        public int frame;

        public static bool operator !=(InputState c1, InputState c2)
        {
            if (c1.ForwardKeyDown != c2.ForwardKeyDown || c1.BackKeyDown != c2.BackKeyDown
                || c1.RightKeyDown != c2.RightKeyDown || c1.LeftKeyDown != c2.LeftKeyDown)
                return true;
            return false;
        }

        public static bool operator ==(InputState c1, InputState c2)
        {
            if (c1.ForwardKeyDown != c2.ForwardKeyDown || c1.BackKeyDown != c2.BackKeyDown
                || c1.RightKeyDown != c2.RightKeyDown || c1.LeftKeyDown != c2.LeftKeyDown)
                return false;
            return true;
        }

        public Vector2 GetMoveVector()
        {
            Vector2 moveVector = Vector2.zero;
            moveVector.x += RightKeyDown ? 1 : 0;
            moveVector.x += LeftKeyDown ? -1 : 0;
            moveVector.y += ForwardKeyDown ? 1 : 0;
            moveVector.y += BackKeyDown ? -1 : 0;
            return moveVector;
        }
    };

    private InputState GetInput()
    {
        InputState currentInput = new InputState();
        currentInput.currentRotation = transform.rotation.eulerAngles.y;
        currentInput.ForwardKeyDown = Input.GetKey(KeyCode.W);
        currentInput.BackKeyDown = Input.GetKey(KeyCode.S);
        currentInput.RightKeyDown = Input.GetKey(KeyCode.D);
        currentInput.LeftKeyDown = Input.GetKey(KeyCode.A);
        currentInput.frame = currentFrame;
        return currentInput;
    }

    #region
    public static PlayerController instance;

    private enum MouseClickType { Down = 0, Up};

    [SerializeField]
    private Transform shoulder;

    [SerializeField]
    private Transform spine;

    [SerializeField]
    private PlayerClass[] classPool;

    private PlayerClass currentClass;
    private Animator animator;
    private PlayerMovement movement;


    [SyncVar]
    private float lastXSpineAxis = 0f;

    [SyncVar]
    private bool aiming;

    private Vector3 previousMovementVector;




    [Command]
    public void UpdateXRotation(float newX)
    {
        if (isLocalPlayer)
            return;
        lastXSpineAxis = newX;
    }

    [Command]
    public void UpdateYRotation(float newY)
    {
        if (isLocalPlayer)
            return;
        transform.rotation = Quaternion.Euler(0f, newY, 0f);
    }

    public bool IsAiming()
    {
        return aiming;
    }

    public void StartAiming()
    {
        aiming = true;
        if (!isServerOnly)
            CameraController.instance.EnableAimCamera();
        animator.SetBool("Aiming", true);
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
        {
            if (aiming)
                spine.Rotate(0f, -lastXSpineAxis, 0f);
            return;
        }
        if (aiming && isLocalPlayer)
        {
            spine.Rotate(0f, -CameraController.instance.GetPlayerYAxis(), 0f);
        }
    }

    public void StopAiming()
    {
        aiming = false;
        if (!isServerOnly)
            CameraController.instance.DisableAimCamera();
        animator.SetBool("Aiming", false);
    }

    public void ResetMovementAnimation()
    {
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
        animator.SetBool("Walking", false);

        aiming = false;
        animator.SetBool("Aiming", false);

        SendedResetMovementAnimation();
    }

    [Command]
    private void SendedResetMovementAnimation()
    {
        if (isLocalPlayer)
            return;
        animator.SetFloat("xMove", 0);
        animator.SetFloat("zMove", 0);
        animator.SetBool("Walking", false);

        aiming = false;
        animator.SetBool("Aiming", false);
    }

    public void Init()
    {
        currentClass = null;
        if (isLocalPlayer)
        {
            instance = this;
            CameraController.instance.SetTarget(shoulder, transform);
        }
        previousInput = GetInput();
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    public void EnableClass(string className)
    {
        PlayerClass temp = FindClassByName(className);
        if (temp == null)
        {
            Debug.Log(className + " class not founded");
            return;
        }
        if (currentClass != null)
            currentClass.DisableArmorParts();
        currentClass = temp;
        currentClass.EnableArmorParts(transform);
        currentClass.SetAnimationOverride(animator);
    }

    private PlayerClass FindClassByName(string className)
    {
        foreach (PlayerClass playerClass in classPool)
            if (className == playerClass.GetClassName())
                return playerClass;
        return null;
    }

    [Command]
    private void MouseEvent(KeyCode mouseButton, MouseClickType type)
    {
        if (mouseButton == KeyCode.Mouse0)
        {
            if (type == MouseClickType.Down)
                currentClass.LMBDown();
            else if (type == MouseClickType.Up)
                currentClass.LMBUp();
        }
        else if (mouseButton == KeyCode.Mouse1)
        {
            if (type == MouseClickType.Down)
                currentClass.RMBDown();
            else if (type == MouseClickType.Up)
                currentClass.RMBUp();
        }
    }

    #endregion

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.Space) && !aiming)
            movement.Jump();
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            MouseEvent(KeyCode.Mouse0, MouseClickType.Down);
            currentClass.LMBDown();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            MouseEvent(KeyCode.Mouse0, MouseClickType.Up);
            currentClass.LMBUp();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            MouseEvent(KeyCode.Mouse1, MouseClickType.Down);
            currentClass.RMBDown();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1)) 
        {
            MouseEvent(KeyCode.Mouse1, MouseClickType.Up);
            currentClass.RMBUp();
        }
    }

    public void Move(InputState input)
    {
        movement.ApplyTransformInput(input);
        movement.ApplyAnimatorInput(input);
        if (isClientOnly)
            SendMove(input);
    }

    [Command]
    private void SendMove(InputState input)
    {
        movement.ApplyTransformInput(input);
        movement.ApplyAnimatorInput(input);
        CheckPrediction(connectionToClient, input, transform.position);
    }

    [TargetRpc]
    private void CheckPrediction(NetworkConnection con, InputState input, Vector3 pos)
    {
        Vector3 temp = transform.position;
        transform.position = pos;
        sendedInputs.Remove(input);
        sendedInputs.ForEach(movement.ApplyTransformInput);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (aiming)
        {
            animator.SetFloat("XAim", CameraController.instance.GetPlayerYAxis());
            return;
        }
        InputState currentInput = GetInput();
        if (currentInput.GetMoveVector() != Vector2.zero || previousInput != currentInput)
        {
            Move(currentInput);
        }
        previousInput = currentInput;
        currentFrame++;
    }
}
