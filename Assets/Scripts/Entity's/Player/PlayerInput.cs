using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    private long avaliablePackets;

    private Queue<InputInfo> sendedInputs = new Queue<InputInfo>();

    private PlayerStateMachine playerStateMachine;

    private void Start()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void FixedUpdate()
    {
        avaliablePackets++;

        if (!isLocalPlayer)
            return;
        
        InputInfo currentInput = new InputInfo();
        currentInput.PressedKeys = new List<KeyCode>();

        FillMovementKeysInput(ref currentInput);
        FillCameraInput(ref currentInput);

        SendInputOnServer(currentInput);
        sendedInputs.Enqueue(currentInput);
    }

    private void FillMovementKeysInput(ref InputInfo input)
    {
        CheckInputKey(KeyCode.W, ref input);
        CheckInputKey(KeyCode.A, ref input);
        CheckInputKey(KeyCode.S, ref input);
        CheckInputKey(KeyCode.D, ref input);
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
        if (Input.GetKey(key) && !input.PressedKeys.Contains(key))
            input.PressedKeys.Add(key);
    }

    [Command]
    private void SendInputOnServer(InputInfo input)
    {
        if (avaliablePackets <= 0)
            return;

        avaliablePackets--;
        playerStateMachine.InputInState(input);
        input.Position = transform.position;
        RecieveInputResponse(input);
    }

    [TargetRpc]
    private void RecieveInputResponse(InputInfo input)
    {
        sendedInputs.Dequeue();

        PlayerState currentState = playerStateMachine.CurrentState;
        transform.position = input.Position;

        Quaternion rot = transform.rotation;

        foreach (var sendedInput in sendedInputs)
            playerStateMachine.InputInState(input);

        transform.rotation = rot;
        playerStateMachine.ChangePlayerState(currentState);
    }
}
