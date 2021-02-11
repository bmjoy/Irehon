using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class PlayerController : NetworkBehaviour
{
    protected enum MouseClickType { Down = 0, Up};

    [SyncVar]
    protected float lastXSpineAxis = 0f;
    [SerializeField]
    protected Transform shoulder;
    [SerializeField]
    protected ClassData currentClass;
    protected AudioSource audioSource;
    protected PlayerMovement movement;
    protected RagdollController ragdoll;
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
        ragdoll = GetComponent<RagdollController>();
        audioSource = GetComponent<AudioSource>();
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
        if (Input.GetKeyDown(KeyCode.F))
            ragdoll.ActivateRagdoll();
        if (Input.GetKeyDown(KeyCode.H))
            ragdoll.DisableRagdoll();
        if (!isLocalPlayer)
            return;
        if (!isControllAllow) //нужно защитить серверные вызовы
            return;
        if (Input.GetKey(KeyCode.Mouse0))
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

        if (!isGrounded)
            movement.IncreaseJumpGravityForce();

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
        CheckPrediction(connectionToClient, transform.position);
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
    protected void CheckPrediction(NetworkConnection con, Vector3 pos)
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
        InputState currentInput = new InputState
        {
            currentRotation = transform.rotation.eulerAngles.y,
            ForwardKeyDown = Input.GetKey(KeyCode.W),
            BackKeyDown = Input.GetKey(KeyCode.S),
            RightKeyDown = Input.GetKey(KeyCode.D),
            LeftKeyDown = Input.GetKey(KeyCode.A),
            SprintKeyDown = Input.GetKey(KeyCode.LeftShift),
            JumpKeyDown = Input.GetKey(KeyCode.Space)
        };
        return currentInput;
    }

    protected SendInputState PackageSendedInput(InputState input)
    {
        SendInputState sendInputState = new SendInputState
        {
            input = input,
            frame = currentFrame,
            position = transform.position,
            inputSendedTime = DateTime.Now
        };
        return sendInputState;
    }

    protected void ProcessInputState(InputState input)
    {
        movement.ApplyTransformInput(input);
        animator.ApplyAnimatorInput(input);
        if (input.JumpKeyDown)
            JumpInput();
    }

    protected void Move(InputState input)
    {
        ProcessInputState(input);
        if (isClientOnly)
            SendMove(input);
    }

    public void SetVelocity(Vector3 velocity)
    {
        rigidBody.velocity = transform.TransformDirection(velocity);
        if (isServer)
            SetVelocityOnOthers(velocity);
    }

    public void ResetHorizontalVelocty()
    {
        rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
        if (isServer)
            ResetHorizontalVeloctyOnOthers();
    }
    [ClientRpc(excludeOwner = true)]
    private void ResetHorizontalVeloctyOnOthers()
    {
        rigidBody.velocity = new Vector3(0, rigidBody.velocity.y, 0);
    }

    public void ResetState()
    {
        animator.ResetTriggers();
        currentClass.ResetClassState();
    }

    public void ResetControll()
    {
        AllowControll();
        animator.ResetAnimator();
        currentClass.ResetClassState();
    }

    [ClientRpc(excludeOwner = true)]
    private void SetVelocityOnOthers(Vector3 velocity)
    {
        rigidBody.velocity = velocity;
    }

        public void AddForce(Vector3 force)
    {
        rigidBody.AddRelativeForce(force, ForceMode.Impulse);
        if (isServer)
            AddForceOnOthers(force);
    }

    [ClientRpc(excludeOwner = true)]
    private void AddForceOnOthers(Vector3 force)
    {
        rigidBody.AddRelativeForce(force, ForceMode.VelocityChange);
    }
}
