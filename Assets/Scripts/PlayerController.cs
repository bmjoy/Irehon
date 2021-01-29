using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{

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
    private enum MouseClickType { Down = 0, Up};

    [SyncVar]
    private float lastXSpineAxis = 0f;
    [SerializeField]
    private Transform shoulder;
    [SerializeField]
    private GameObject currentWeapon;
    [SerializeField]
    private ClassData currentClass;
    private PlayerMovement movement;
    private List<InputState> sendedInputs = new List<InputState>();
    private InputState previousInput;
    private int currentFrame;
    public PlayerAnimatorController animatorController { get; private set; }
    public static PlayerController instance;

    #region

    [Command]
    private void SendMove(InputState input)
    {
        movement.ApplyTransformInput(input);
        animatorController.ApplyAnimatorInput(input);
        CheckPrediction(connectionToClient, input, transform.position);
    }

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

    [Command]
    private void MouseEvent(KeyCode mouseButton, MouseClickType type)
    {
        if (isClient)
            return;
        if (mouseButton == KeyCode.Mouse0)
        {
            if (type == MouseClickType.Down)
                currentClass.LeftMouseButtonDown();
            else if (type == MouseClickType.Up)
                currentClass.LeftMouseButtonUp();
        }
        else if (mouseButton == KeyCode.Mouse1)
        {
            if (type == MouseClickType.Down)
                currentClass.RightMouseButtonDown();
            else if (type == MouseClickType.Up)
                currentClass.RightMouseButtonUp();
        }
    }

    [TargetRpc]
    private void CheckPrediction(NetworkConnection con, InputState input, Vector3 pos)
    {
        Quaternion rot = transform.rotation;
        transform.position = pos;
        sendedInputs.Remove(input);
        sendedInputs.ForEach(movement.ApplyTransformInput);
        transform.rotation = rot;
    }

    [TargetRpc]
    public void HitConfirmed(NetworkConnection con)
    {
        print("confirmed hit");
    }
    #endregion

    public void SetWeapon(GameObject weapon) => currentWeapon = weapon;

    public bool isOwner()
    {
        return isLocalPlayer;
    }

    public GameObject GetWeapon() => currentWeapon;

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
    public void Move(InputState input)
    {
        movement.ApplyTransformInput(input);
        animatorController.ApplyAnimatorInput(input);
        if (isClientOnly)
            SendMove(input);
    }

    public void Start()
    {
        currentClass = null;
        if (isLocalPlayer)
        {
            instance = this;
            previousInput = GetInput();
            CameraController.instance.SetTarget(shoulder, transform);
        }
        currentClass = GetComponent<ClassData>();
        animatorController = GetComponent<PlayerAnimatorController>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.Space))
            movement.Jump();
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            MouseEvent(KeyCode.Mouse0, MouseClickType.Down);
            currentClass.LeftMouseButtonDown();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            MouseEvent(KeyCode.Mouse0, MouseClickType.Up);
            currentClass.LeftMouseButtonUp();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            MouseEvent(KeyCode.Mouse1, MouseClickType.Down);
            currentClass.RightMouseButtonDown();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            MouseEvent(KeyCode.Mouse1, MouseClickType.Up);
            currentClass.RightMouseButtonUp();
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            shoulder.localRotation = Quaternion.Euler(lastXSpineAxis, -3.5f, 0f); ;
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
