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
    private bool isClient = false;

    private void Start()
    {
        isClient = GetComponent<Entity>().isClient;
    }

    private void FixedUpdate()
    {
        if (isClient)
            return;
        var newState = currentState?.Update(currentTimeInState);
        if (newState != currentState)
            SetNewState(newState);

        currentTimeInState += Time.fixedDeltaTime;
    }

    public void SetNewState(MobState state)
    {
        if (isClient)
            return;
        previousState = currentState;
        currentState = state;

        previousState?.Exit();
        currentState.Enter();

        currentTimeInState = 0;

        OnChangeState.Invoke();
    }
}
