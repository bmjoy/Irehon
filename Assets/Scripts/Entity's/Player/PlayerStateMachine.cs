using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStateMachine : NetworkBehaviour
{
    public PlayerState CurrentState => currentState;
    public UnityEvent OnPlayerChangeState = new UnityEvent();

    private PlayerState currentState;
    private void FixedUpdate()
    {
        currentState.Update();
    }

    public void ChangePlayerState(PlayerState state)
    {
        if (state != null)
            state.Exit();
        currentState = state;
        state.Enter();
        OnPlayerChangeState.Invoke();
    }
}
