#define STATE_DEBUG

using Irehon;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum PlayerStateType { Idle, Fall, Jump, Walk, Run, Death, Dance }

public class PlayerStateMachine : NetworkBehaviour
{
    public PlayerState CurrentState => this.currentState;
    public PlayerState PreviousState => this.previousState;
    [HideInInspector]
    public UnityEvent OnPlayerChangeState = new UnityEvent();

    public Dictionary<PlayerStateType, PlayerState> PlayerStates { get; private set; }

    private PlayerState currentState;
    private PlayerState previousState;
    private Player player;

    private void Awake()
    {
        this.player = this.GetComponent<Player>();

        this.PlayerStates = new Dictionary<PlayerStateType, PlayerState>();

        this.PlayerStates.Add(PlayerStateType.Idle, new PlayerIdleState(this.player));
        this.PlayerStates.Add(PlayerStateType.Fall, new PlayerFallState(this.player));
        this.PlayerStates.Add(PlayerStateType.Death, new PlayerDeathState(this.player));
        this.PlayerStates.Add(PlayerStateType.Jump, new PlayerJumpingState(this.player));
        this.PlayerStates.Add(PlayerStateType.Walk, new PlayerWalkState(this.player));
        this.PlayerStates.Add(PlayerStateType.Run, new PlayerRunState(this.player));
        this.PlayerStates.Add(PlayerStateType.Dance, new PlayerDanceState(this.player));
    }

    private void FixedUpdate()
    {
        this.currentState?.Update();
    }

    private void SetNewState(PlayerStateType type, bool isResimulating = false)
    {
        this.previousState = this.currentState;
        this.currentState = this.PlayerStates[type];

        if (this.previousState == null)
        {
            this.currentState.Enter(isResimulating);

            this.OnPlayerChangeState.Invoke();
            return;
        }

        if (this.previousState.Type != this.currentState.Type)
        {
            this.previousState?.Exit(isResimulating);
            this.currentState.Enter(isResimulating);

            this.OnPlayerChangeState.Invoke();
        }
    }

    public void InputInState(InputInfo input, bool isResimulating = false)
    {
        this.SetNewState(this.CurrentState.HandleInput(input, this.isServer), isResimulating);
    }

    public void ChangePlayerState(PlayerStateType state)
    {
        this.SetNewState(state);
    }

    [TargetRpc]
    private void ChangePlayerStateRpc(PlayerStateType stateType)
    {
        this.SetNewState(stateType);
    }
}
