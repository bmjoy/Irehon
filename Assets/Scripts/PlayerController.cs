using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerController : NetworkBehaviour
{
    public struct SendInputState
    {
        public InputState input;
        public int frame;
        public Vector3 position;
        public System.DateTime inputSendedTime;
    };

    public struct InputState
    {
        public bool ForwardKeyDown;
        public bool BackKeyDown;
        public bool RightKeyDown;
        public bool LeftKeyDown;
        public bool SprintKeyDown;
        public bool JumpKeyDown;

        public float currentRotation;

        public int frame;

        public static bool operator !=(InputState c1, InputState c2)
        {
            if (c1.ForwardKeyDown != c2.ForwardKeyDown || c1.BackKeyDown != c2.BackKeyDown
                || c1.RightKeyDown != c2.RightKeyDown || c1.LeftKeyDown != c2.LeftKeyDown || 
                c1.JumpKeyDown != c2.JumpKeyDown || c1.SprintKeyDown != c2.SprintKeyDown)
                return true;
            return false;
        }

        public static bool operator ==(InputState c1, InputState c2)
        {
            if (c1 != c2)
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
    protected enum MouseClickType { Down = 0, Up};

    [SyncVar]
    protected float lastXSpineAxis = 0f;
    [SerializeField]
    protected Transform shoulder;
    [SerializeField]
    protected ClassData currentClass;
    protected PlayerMovement movement;
    protected Rigidbody rigidBody;
    protected Queue<SendInputState> sendedInputs = new Queue<SendInputState>();
    protected InputState previousInput;
    protected int currentFrame;
    protected int gettedInputs;
    protected bool isControllAllow;
    protected Player player;
    protected PlayerAnimatorController animator;
    protected bool isGrounded;

    protected virtual void Start()
    {
        if (isLocalPlayer)
        {
            previousInput = GetInput();
            CameraController.instance.SetTarget(shoulder, transform);
        }
        rigidBody = GetComponent<Rigidbody>();
        isGrounded = true;
        player = GetComponent<Player>();
        currentClass = GetComponent<ClassData>();
        animator = GetComponent<PlayerAnimatorController>();
        movement = GetComponent<PlayerMovement>();
    }

    public virtual void InvokeAttackEvent()
    {
        currentClass.AttackEvent();
    }

    protected virtual void Update()
    {
        if (!isGrounded)
            movement.IncreaseJumpGravityForce();
        if (!isLocalPlayer)
            return;
        if (!isControllAllow) //нужно защитить серверные вызовы
            return;
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

    protected void FixedUpdate()
    {
        currentFrame++;

        if (isServer)
        {
            if (transform.position.y < -5)
                player.Kill();
        }

        if (!isLocalPlayer)
        {
            float newX = Mathf.Lerp(shoulder.localRotation.eulerAngles.x, lastXSpineAxis, 10);
            shoulder.localRotation = Quaternion.Euler(newX, -3.5f, 0f);
            return;
        }
        if (isControllAllow)
        {
            InputState currentInput = GetInput();
            if (currentInput.GetMoveVector() != Vector2.zero || previousInput != currentInput) //Вернуть, только для теста
            {
                sendedInputs.Enqueue(PackageSendedInput(currentInput));
                Move(currentInput);
            }
            previousInput = currentInput;
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetFallingState(true);
            isGrounded = false;
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetFallingState(false);
            isGrounded = true;
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Walkable"))
        {
            animator.SetFallingState(false);
            isGrounded = true;
        }
    }

    #region

    [Command]
    protected void SendMove(InputState input)
    {
        if (!isControllAllow || gettedInputs > currentFrame)
            return;
        ProcessInputState(input);
        CheckPrediction(connectionToClient, input, transform.position);
    }

    [Command]
    public void UpdateXRotation(float newX)
    {
        if (isLocalPlayer || !isControllAllow)
            return;
        lastXSpineAxis = newX;
    }

    [Command]
    public void UpdateYRotation(float newY)
    {
        if (isLocalPlayer || !isControllAllow)
            return;
        transform.rotation = Quaternion.Euler(0f, newY, 0f);
    }

    [Command]
    protected void MouseEvent(KeyCode mouseButton, MouseClickType type)
    {
        if (isClient || !isControllAllow || !player.IsAlive())
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
    protected void CheckPrediction(NetworkConnection con, InputState input, Vector3 pos)
    {
        TimeSpan ping = DateTime.Now - sendedInputs.Dequeue().inputSendedTime;
        DebugNetwork.instance.ShowPing(Convert.ToInt32(ping.TotalMilliseconds));

        float yPosition = transform.position.y;
    
        Quaternion rot = transform.rotation;
        transform.position = pos;
        
        foreach (SendInputState sendedInput in sendedInputs)
            movement.ApplyTransformInput(sendedInput.input);
        transform.rotation = rot;
        if (sendedInputs.Count > 0)
        {
            float delta = pos.y - sendedInputs.Peek().position.y;
            if (delta > 1f)
                yPosition += delta;
            if (delta > .00001f)
                yPosition = Mathf.Lerp(yPosition, yPosition + (delta), 0.1f);
        }
        transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);
    }

    [TargetRpc]
    public void HitConfirmed(NetworkConnection con)
    {
        UIController.instance.ShowHitMarker();
    }
    #endregion

    protected virtual void Jump()
    {
        movement.Jump();
        isGrounded = false;
        animator.PlayJump();
    }

    protected virtual void JumpInput()
    {
        if (isGrounded && rigidBody.velocity.y < 1)
        {
            animator.PlayJump();
            movement.Jump();
            if (isServer && !isLocalPlayer)
                JumpRPC();
        }
    }

    [ClientRpc(excludeOwner = true)]
    protected virtual void JumpRPC()
    {
        Jump();
    }

    public void BlockControll() => isControllAllow = false;
    
    public void AllowControll() => isControllAllow = true;

    public bool IsControllAllowed() => isControllAllow;

    protected InputState GetInput()
    {
        InputState currentInput = new InputState();
        currentInput.currentRotation = transform.rotation.eulerAngles.y;
        currentInput.ForwardKeyDown = Input.GetKey(KeyCode.W);
        currentInput.BackKeyDown = Input.GetKey(KeyCode.S);
        currentInput.RightKeyDown = Input.GetKey(KeyCode.D);
        currentInput.LeftKeyDown = Input.GetKey(KeyCode.A);
        currentInput.SprintKeyDown = Input.GetKey(KeyCode.LeftShift);
        currentInput.JumpKeyDown = Input.GetKey(KeyCode.Space);
        return currentInput;
    }

    protected SendInputState PackageSendedInput(InputState input)
    {
        SendInputState sendInputState = new SendInputState();
        sendInputState.input = input;
        sendInputState.frame = currentFrame;
        sendInputState.position = transform.position;
        sendInputState.inputSendedTime = DateTime.Now;
        return sendInputState;
    }

    public void ProcessInputState(InputState input)
    {
        movement.ApplyTransformInput(input);
        animator.ApplyAnimatorInput(input);
        if (input.JumpKeyDown)
            JumpInput();
    }

    public void Move(InputState input)
    {
        ProcessInputState(input);
        if (isClientOnly)
            SendMove(input);
    }
}
