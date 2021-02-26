/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class PlayerController : NetworkBehaviour
{
    protected enum MouseClickType { Down = 0, Up };

    [SyncVar]
    protected float lastXSpineAxis = 0f;
    [SerializeField]
    protected Transform shoulder;
    protected List<KeyCode> pressedKeys = new List<KeyCode>();
    protected AudioSource audioSource;
    protected IMovementBehaviour movement;
    protected Rigidbody rigidBody;
    protected IAbilitySystem abilitySystem;
    protected Queue<SendInputState> sendedInputs = new Queue<SendInputState>();
    protected InputState previousInput;
    protected int currentFrame;
    protected int gettedInputs;
    protected bool isControllAllow;
    protected Player player;

    protected virtual void Start()
    {
        if (isLocalPlayer)
        {
            previousInput = GetInput();
            CameraController.instance.SetTarget(shoulder, transform);
        }
        abilitySystem = GetComponent<AbilitySystem>();
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer)
            return;
        if (!isControllAllow) //нужно защитить серверные вызовы
            return;
        CheckAbilityTriggerKey(KeyCode.Mouse0);
        CheckAbilityTriggerKey(KeyCode.Mouse1);
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
                ProcessInput(currentInput);
            }
            previousInput = currentInput;
        }
    }

    #region

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
    protected void KeyPressed(KeyCode button, Vector3 target)
    {
        if (isClient || !isControllAllow || !player.IsAlive())
            return;
        if (!pressedKeys.Contains(button))
        {
            abilitySystem.AbilityKeyDown(button, target);
            pressedKeys.Add(button);
        }
    }

    [Command]
    protected void KeyUnpressed(KeyCode button, Vector3 target)
    {
        if (isClient || !isControllAllow || !player.IsAlive())
            return;
        if (pressedKeys.Contains(button))
        {
            abilitySystem.AbilityKeyUp(button, target);
            pressedKeys.Remove(button);
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
            movement.ProcessMovementInput(sendedInput.input);
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

    protected virtual void JumpInput()
    {
        if (movement.IsCanJump())
        {
            movement.Jump();
            if (isServer && !isLocalPlayer)
                JumpRPC();
        }
    }

    [ClientRpc(excludeOwner = true)]
    protected virtual void JumpRPC()
    {
        movement.Jump();
    }

    protected void CheckAbilityTriggerKey(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            abilitySystem.AbilityKeyDown(key, CameraController.instance.GetLookingTargetPosition());
            KeyPressed(key, CameraController.instance.GetLookingTargetPosition());
        }
        if (Input.GetKeyUp(key))
        {
            abilitySystem.AbilityKeyUp(key, CameraController.instance.GetLookingTargetPosition());
            KeyUnpressed(key, CameraController.instance.GetLookingTargetPosition());
        }
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

    [Command]
    protected void SendMove(InputState input)
    {
        if (!isControllAllow || gettedInputs > currentFrame || !player.IsAlive())
            return;
        ProcessInputState(input);
        CheckPrediction(connectionToClient, transform.position);
    }

    protected void ProcessInputState(InputState input)
    {
        movement.ProcessMovementInput(input);
        if (input.JumpKeyDown)
            JumpInput();
    }

    protected void ProcessInput(InputState input)
    {
        ProcessInputState(input);
        if (isClientOnly)
            SendMove(input);
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public partial class PlayerController : NetworkBehaviour
{
    protected enum MouseClickType { Down = 0, Up };

    [SyncVar]
    protected float lastXSpineAxis = 0f;
    [SerializeField]
    protected Transform shoulder;
    protected List<KeyCode> pressedKeys = new List<KeyCode>();
    protected AudioSource audioSource;
    protected IMovementBehaviour movement;
    protected Rigidbody rigidBody;
    protected IAbilitySystem abilitySystem;
    protected Queue<SendInputState> sendedInputs = new Queue<SendInputState>();
    protected InputState previousInput;
    protected int currentFrame;
    protected int gettedInputs;
    protected bool isControllAllow;
    protected Player player;

    protected virtual void Start()
    {
        if (isLocalPlayer)
        {
            previousInput = GetInput();
            CameraController.instance.SetTarget(shoulder, transform);
        }
        abilitySystem = GetComponent<AbilitySystem>();
        audioSource = GetComponent<AudioSource>();
        rigidBody = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
    }

    protected virtual void Update()
    {
        if (!isLocalPlayer)
            return;
        if (!isControllAllow) //нужно защитить серверные вызовы
            return;
        CheckAbilityTriggerKey(KeyCode.Mouse0);
        CheckAbilityTriggerKey(KeyCode.Mouse1);
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
                ProcessInput(currentInput);
            }
            previousInput = currentInput;
        }
    }

    #region

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
    protected void KeyPressed(KeyCode button, Vector3 target)
    {
        if (isClient || !isControllAllow || !player.IsAlive())
            return;
        if (!pressedKeys.Contains(button))
        {
            abilitySystem.AbilityKeyDown(button, target);
            pressedKeys.Add(button);
        }
    }

    [Command]
    protected void KeyUnpressed(KeyCode button, Vector3 target)
    {
        if (isClient || !isControllAllow || !player.IsAlive())
            return;
        if (pressedKeys.Contains(button))
        {
            abilitySystem.AbilityKeyUp(button, target);
            pressedKeys.Remove(button);
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
            movement.ProcessMovementInput(sendedInput.input);
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
    protected void DropPrediction(NetworkConnection con)
    {
        SendInputState droppedInput = sendedInputs.Dequeue();
    }

    public void HitConfirmed(int damage)
    {
        UIController.instance.ShowHitMarker();
    }

    public void TakeDamageEffect(int damage)
    {
        CameraController.instance.CreateShake(1f, .2f);
    }
    #endregion

    protected virtual void JumpInput()
    {
        if (movement.IsCanJump())
        {
            movement.Jump();
            if (isServer && !isLocalPlayer)
                JumpRPC();
        }
    }

    [ClientRpc(excludeOwner = true)]
    protected virtual void JumpRPC()
    {
        movement.Jump();
    }

    protected void CheckAbilityTriggerKey(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            abilitySystem.AbilityKeyDown(key, CameraController.instance.GetLookingTargetPosition());
            KeyPressed(key, CameraController.instance.GetLookingTargetPosition());
        }
        if (Input.GetKeyUp(key))
        {
            abilitySystem.AbilityKeyUp(key, CameraController.instance.GetLookingTargetPosition());
            KeyUnpressed(key, CameraController.instance.GetLookingTargetPosition());
        }
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

    [Command]
    protected void SendMove(InputState input)
    {
        if (!isControllAllow || gettedInputs > currentFrame || !player.IsAlive())
        {
            DropPrediction(connectionToClient);
            return;
        }
        ProcessInputState(input);
        CheckPrediction(connectionToClient, transform.position);
    }

    protected void ProcessInputState(InputState input)
    {
        movement.ProcessMovementInput(input);
        if (input.JumpKeyDown)
            JumpInput();
    }

    protected void ProcessInput(InputState input)
    {
        ProcessInputState(input);
        if (isClientOnly)
            SendMove(input);
    }
}
