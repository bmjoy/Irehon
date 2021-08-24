using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerStateType { Idle, Fall, Jump, Walk, Run, Death}

public class PlayerStateMachine : NetworkBehaviour
{
    public PlayerState CurrentState => currentState;
    public PlayerState PreviousState => previousState;
    public UnityEvent OnPlayerChangeState = new UnityEvent();

    private PlayerState currentState;
    private PlayerState previousState;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        currentState.Update();
    }

    private void SetNewState(PlayerState state)
    {
        previousState = currentState;
        currentState = state;

        if (previousState == null)
        {
            currentState.Enter();

            OnPlayerChangeState.Invoke();
            return;
        }

        if (previousState.Type != currentState.Type)
        {
            previousState?.Exit();
            currentState.Enter();

            OnPlayerChangeState.Invoke();
        }
    }

    public void InputInState(InputInfo input)
    {
        SetNewState(CurrentState.HandleInput(input, isServer));
    }

    public void ChangePlayerState(PlayerState state)
    {
        SetNewState(state);

        if (isServer)
            ChangePlayerStateRpc(currentState.Type);
    }

    public PlayerState GetPlayerState(PlayerStateType state)
    {
        switch (state)
        {
            case PlayerStateType.Death: return new PlayerDeathState(player);
            case PlayerStateType.Idle: return new PlayerIdleState(player);
            case PlayerStateType.Fall: return new PlayerFallState(player);
            case PlayerStateType.Jump: return new PlayerJumpingState(player);
            case PlayerStateType.Walk: return new PlayerWalkState(player);
            case PlayerStateType.Run: return new PlayerRunState(player);
            default: return new PlayerIdleState(player);
        }
    }

    [TargetRpc]
    private void ChangePlayerStateRpc(PlayerStateType stateType)
    {
        PlayerState state = GetPlayerState(stateType);

        SetNewState(state);
    }
}
