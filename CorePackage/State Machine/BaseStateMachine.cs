using UnityEngine;

public class BaseStateMachine : MonoBehaviour
{
    public State CurrentState { get; private set; }

    public void Initialize(State initialState)
    {
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void ChangeState(State newState)
    {
        CurrentState?.Exit();
        newState.Enter();
        CurrentState = newState;
    }

    public void Update()
    {
        CurrentState?.Update();
    }
}
