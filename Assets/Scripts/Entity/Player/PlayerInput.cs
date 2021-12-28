using Irehon;
using Irehon.Camera;
using Irehon.Interactable;
using Irehon.UI;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    private long avaliablePackets;

    private Queue<InputInfo> sendedInputs = new Queue<InputInfo>();

    private Player player;
    private PlayerStateMachine playerStateMachine;
    private AbilitySystem abilitySystem;
    private CharacterController characterController;

    private List<KeyCode> pressedButtons = new List<KeyCode>();

    private void Start()
    {
        this.player = this.GetComponent<Player>();
        this.characterController = this.GetComponent<CharacterController>();
        this.abilitySystem = this.GetComponent<AbilitySystem>();
        this.playerStateMachine = this.GetComponent<PlayerStateMachine>();
    }

    private void FixedUpdate()
    {
        this.avaliablePackets++;

        if (!this.isLocalPlayer)
        {
            return;
        }

        InputInfo currentInput = new InputInfo();
        currentInput.PressedKeys = new List<KeyCode>();

        if (GameSession.IsListeningGameKeys)
        {
            this.FillMovementKeysInput(ref currentInput);
        }
        this.FillCameraInput(ref currentInput);

        this.playerStateMachine.InputInState(currentInput);
        this.SendInputOnServer(currentInput);
        currentInput.Position = this.transform.position;
        this.sendedInputs.Enqueue(currentInput);
    }

    private void FillMovementKeysInput(ref InputInfo input)
    {
        this.CheckInteractionAttemp(ref input);
        this.CheckInputKey(this.abilitySystem.ListeningKey, ref input);
        this.CheckInputKey(KeyCode.W, ref input);
        this.CheckInputKey(KeyCode.A, ref input);
        this.CheckInputKey(KeyCode.S, ref input);
        this.CheckInputKey(KeyCode.D, ref input);
        this.CheckInputKey(KeyCode.V, ref input);
        this.CheckInputKey(KeyCode.LeftShift, ref input);
        this.CheckInputKey(KeyCode.Space, ref input);
    }

    private void FillCameraInput(ref InputInfo input)
    {
        input.TargetPoint = PlayerCamera.Instance.GetLookPosition();
        input.CameraRotation = PlayerCamera.Instance.GetRotation();
    }

    private void CheckInputKey(KeyCode key, ref InputInfo input)
    {
        if ((key == KeyCode.Mouse1 || key == KeyCode.Mouse0) && Mouse.IsCursorEnabled)
        {
            return;
        }

        if (Input.GetKey(key) && !input.PressedKeys.Contains(key))
        {
            input.PressedKeys.Add(key);
        }
    }

    private bool IsKeySinglePressed(KeyCode key)
    {
        if (Input.GetKey(key) && !this.pressedButtons.Contains(key))
        {
            this.pressedButtons.Add(key);
            return true;
        }

        if (!Input.GetKey(key) && this.pressedButtons.Contains(key))
        {
            this.pressedButtons.Remove(key);
        }

        return false;
    }

    private void CheckSinglePressKey(KeyCode key, ref InputInfo input)
    {
        if (this.IsKeySinglePressed(key))
        {
            input.PressedKeys.Add(key);
        }
    }

    private void CheckInteractionAttemp(ref InputInfo input)
    {
        KeyCode key = KeyCode.E;
        if (this.IsKeySinglePressed(key) && CameraController.Instance.InteractTarget != null)
        {
            NetworkIdentity identity;
            if (CameraController.Instance.InteractTarget.GetComponent<InteractLink>() != null)
            {
                identity = CameraController.Instance.InteractTarget.GetComponent<InteractLink>().interactableOrigin.netIdentity;
            }
            else
                identity = CameraController.Instance.InteractTarget.GetComponent<NetworkIdentity>();

            if (identity != null)
            {
                input.interactionTarget = identity;
            }
        }
    }

    [Command]
    private void SendInputOnServer(InputInfo input)
    {
        
    }

    [TargetRpc]
    private void DropInput()
    {
        this.sendedInputs.Dequeue();
    }

    [TargetRpc]
    private void RecieveInputResponse(InputInfo input)
    {
        this.sendedInputs.Dequeue();

        float yPosition = this.transform.position.y;

        this.playerStateMachine.ChangePlayerState(input.PlayerStateType);

        this.characterController.SetPosition(input.Position);

        Quaternion rot = this.transform.rotation;

        Physics.autoSimulation = false;
        int simulationCount = 0;
        foreach (InputInfo sendedInput in this.sendedInputs)
        {
            this.playerStateMachine.InputInState(sendedInput);
            simulationCount++;
            if (simulationCount % 7 == 0)
                Physics.Simulate(.0000001f);
        }
        Physics.autoSimulation = true;

        if (this.sendedInputs.Count > 0)
        {
            float delta = input.Position.y - this.sendedInputs.Peek().Position.y;

            if (delta > 15f || delta < -15f)
            {
                yPosition = input.Position.y;
            }
        }

        this.characterController.SetPosition(new Vector3(this.transform.position.x, yPosition, this.transform.position.z));

        this.characterController.SetRotation(rot);
    }
}
