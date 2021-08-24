using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class PlayerStateMachine : NetworkBehaviour
{
    private enum PlayerStateType { Idle, Death}
    public PlayerState CurrentState => currentState;
    public UnityEvent OnPlayerChangeState = new UnityEvent();

    private PlayerState currentState;
    private Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
    }

    public void InputInState(InputInfo input)
    {
        PlayerState previousState = currentState;

        currentState = currentState.HandleInput(input, isServer);

        if (previousState != currentState)
        {
            if (previousState != null)
                previousState.Exit();
            
            currentState.Enter();

            OnPlayerChangeState.Invoke();
        }
    }

    public void ChangePlayerState(PlayerState state)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = state;

        state.Enter();

        OnPlayerChangeState.Invoke();
    }

    private PlayerState GetPlayerState(PlayerStateType state)
    {
        switch (state)
        {
            case PlayerStateType.Death: return new PlayerDeathState(player);
            case PlayerStateType.Idle: return new PlayerIdleState(player);
            default: return new PlayerIdleState(player);
        }
    }

    [TargetRpc]
    private void ChangePlayerStateRpc(PlayerStateType stateType)
    {
        PlayerState state = GetPlayerState(stateType);

        if (currentState != null)
            currentState.Exit();

        currentState = state;

        state.Enter();

        OnPlayerChangeState.Invoke();
    }
}
