using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class MobStateMachine : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent OnChangeState = new UnityEvent();

    private MobState currentState;
    private MobState previousState;

    private float currentTimeInState;

    private void FixedUpdate()
    {
        var newState = currentState?.Update(currentTimeInState);
        if (newState != currentState)
            SetNewState(newState);

        currentTimeInState += Time.fixedDeltaTime;
    }

    public void SetNewState(MobState state)
    {
        previousState = currentState;
        currentState = state;

        previousState?.Exit();
        currentState.Enter();

        currentTimeInState = 0;

        OnChangeState.Invoke();
    }
}
