using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : NetworkBehaviour
{
    private int frame;

    private Queue<InputInfo> sendedInputs = new Queue<InputInfo>();

    private PlayerStateMachine playerStateMachine;

    private void Start()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    private void FixedUpdate()
    {
        InputInfo currentInput = new InputInfo();
        currentInput.PressedKeys = new List<KeyCode>();

        FillMovementKeysInput(ref currentInput);
        FillCameraInput(ref currentInput);
    }

    private void FillMovementKeysInput(ref InputInfo input)
    {
        CheckInputKey(KeyCode.W, ref input);
        CheckInputKey(KeyCode.A, ref input);
        CheckInputKey(KeyCode.S, ref input);
        CheckInputKey(KeyCode.D, ref input);
        CheckInputKey(KeyCode.LeftShift, ref input);
    }

    private void FillCameraInput(ref InputInfo input)
    {
        input.TargetPoint = CameraController.GetLookingTargetPosition();
        input.CameraRotation = CameraController.GetCurrentRotation();
    }

    private void CheckInputKey(KeyCode key, ref InputInfo input)
    {
        if (Input.GetKeyDown(key) && !input.PressedKeys.Contains(key))
            input.PressedKeys.Add(key);
    }

    [Command]
    private void SendInputOnServer(InputInfo input)
    {

    }

    [TargetRpc]
    private void RecieveInputResponse(InputInfo input)
    {
        sendedInputs.Dequeue();
        
        transform.position = input.Position;

        Quaternion rot = transform.rotation;

        foreach (var sendedInput in sendedInputs)
            playerStateMachine.CurrentState.ClientHandleInput(sendedInput);

        transform.rotation = rot;
    }

    [TargetRpc]
    private void DropWaitingInputResponse()
    {
        sendedInputs.Dequeue();
    }
}
