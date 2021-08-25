#define STATE_DEBUG

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

    public Dictionary<PlayerStateType, PlayerState> PlayerStates { get; private set; }

    private PlayerState currentState;
    private PlayerState previousState;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();

        PlayerStates = new Dictionary<PlayerStateType, PlayerState>();

        PlayerStates.Add(PlayerStateType.Idle, new PlayerIdleState(player));
        PlayerStates.Add(PlayerStateType.Fall, new PlayerFallState(player));
        PlayerStates.Add(PlayerStateType.Death, new PlayerDeathState(player));
        PlayerStates.Add(PlayerStateType.Jump, new PlayerJumpingState(player));
        PlayerStates.Add(PlayerStateType.Walk, new PlayerWalkState(player));
        PlayerStates.Add(PlayerStateType.Run, new PlayerRunState(player));
    }

    private void FixedUpdate()
    {
        currentState.Update();
    }

    private void SetNewState(PlayerStateType type)
    {
        previousState = currentState;
        currentState = PlayerStates[type];

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

    public void ChangePlayerState(PlayerStateType state)
    {
        SetNewState(state);

        if (isServer)
            ChangePlayerStateRpc(state);
    }

    [TargetRpc]
    private void ChangePlayerStateRpc(PlayerStateType stateType) => SetNewState(stateType);
}
