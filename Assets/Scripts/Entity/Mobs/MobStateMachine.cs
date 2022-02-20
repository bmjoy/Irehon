using Irehon.Entitys;
using UnityEngine;
using UnityEngine.Events;

public class MobStateMachine : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent OnChangeState = new UnityEvent();
    public MobState CurrentState => currentState;
    private MobState currentState;
    private MobState previousState;

    private float currentTimeInState;
    private bool isClient = false;

    private void Start()
    {
        this.isClient = this.GetComponent<Entity>().isClient;
    }

    private void FixedUpdate()
    {
        if (this.isClient)
        {
            return;
        }

        MobState newState = this.currentState?.Update(this.currentTimeInState);
        Debug.Log(nameof(newState));
        if (newState != this.currentState)
        {
            this.SetNewState(newState);
        }

        this.currentTimeInState += Time.fixedDeltaTime;
    }

    public void SetNewState(MobState state)
    {
        if (this.isClient)
        {
            return;
        }

        this.previousState = this.currentState;
        this.currentState = state;

        this.previousState?.Exit();
        this.currentState.Enter();

        this.currentTimeInState = 0;

        this.OnChangeState.Invoke();
    }
}
