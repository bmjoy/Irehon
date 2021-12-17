using Mirror;
using System.Collections;
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
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        abilitySystem = GetComponent<AbilitySystem>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void FixedUpdate()
    {
        avaliablePackets++;

        if (!isLocalPlayer)
            return;
        
        InputInfo currentInput = new InputInfo();
        currentInput.PressedKeys = new List<KeyCode>();

        if (GameSession.IsListeningGameKeys)
        {
            FillMovementKeysInput(ref currentInput);
        }
        FillCameraInput(ref currentInput);

        playerStateMachine.InputInState(currentInput);
        SendInputOnServer(currentInput);
        currentInput.Position = transform.position;
        sendedInputs.Enqueue(currentInput);
    }

    private void FillMovementKeysInput(ref InputInfo input)
    {
        CheckInteractionAttemp(ref input);
        CheckInputKey(abilitySystem.ListeningKey, ref input);
        CheckInputKey(KeyCode.W, ref input);
        CheckInputKey(KeyCode.A, ref input);
        CheckInputKey(KeyCode.S, ref input);
        CheckInputKey(KeyCode.D, ref input);
        CheckInputKey(KeyCode.V, ref input);
        CheckInputKey(KeyCode.LeftShift, ref input);
        CheckInputKey(KeyCode.Space, ref input);
    }

    private void FillCameraInput(ref InputInfo input)
    {
        input.TargetPoint = CameraController.GetLookingTargetPosition();
        input.CameraRotation = CameraController.GetCurrentRotation();
    }

    private void CheckInputKey(KeyCode key, ref InputInfo input)
    {
        if ((key == KeyCode.Mouse1 || key == KeyCode.Mouse0) && !CameraController.IsCursosLocked)
            return;
        if (Input.GetKey(key) && !input.PressedKeys.Contains(key))
            input.PressedKeys.Add(key);
    }

    private bool IsKeySinglePressed(KeyCode key)
    {
        if (Input.GetKey(key) && !pressedButtons.Contains(key))
        {
            pressedButtons.Add(key);
            return true;
        }

        if (!Input.GetKey(key) && pressedButtons.Contains(key))
            pressedButtons.Remove(key);
        return false;
    }

    private void CheckSinglePressKey(KeyCode key, ref InputInfo input)
    {
        if (IsKeySinglePressed(key))
            input.PressedKeys.Add(key);
    }

    private void CheckInteractionAttemp(ref InputInfo input)
    {
        var key = KeyCode.E;
        if (IsKeySinglePressed(key) && CameraController.interactableTarget != null) 
        {
            var identity = CameraController.interactableTarget.GetComponent<NetworkIdentity>();
            if (identity != null)
                input.interactionTarget = identity;
        }
    }

    [Command]
    private void SendInputOnServer(InputInfo input)
    {
        if (avaliablePackets <= 0)
        {
            DropInput();
            return;
        }

        avaliablePackets--;

        playerStateMachine.InputInState(input);

        input.Position = transform.position;
        input.PlayerStateType = playerStateMachine.CurrentState.Type;

        RecieveInputResponse(input);
    }

    [TargetRpc]
    private void DropInput()
    {
        sendedInputs.Dequeue();
    }

    [TargetRpc]
    private void RecieveInputResponse(InputInfo input)
    {
        sendedInputs.Dequeue();

        float yPosition = transform.position.y;

        playerStateMachine.ChangePlayerState(input.PlayerStateType);

        characterController.SetPosition(input.Position);

        Quaternion rot = transform.rotation;

        Physics.autoSimulation = false;
        foreach (var sendedInput in sendedInputs)
        {
            playerStateMachine.InputInState(sendedInput);
            Physics.Simulate(.0000001f);
        }
        Physics.autoSimulation = true;

        if (sendedInputs.Count > 0)
        {
            float delta = input.Position.y - sendedInputs.Peek().Position.y;

            if (delta > 15f || delta < -15f)
            {
                yPosition = input.Position.y;
            }
        }

        characterController.SetPosition(new Vector3(transform.position.x, yPosition, transform.position.z));

        characterController.SetRotation(rot);
    }
}
