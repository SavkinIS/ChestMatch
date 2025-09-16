using UnityEngine;

public class LoadingState : IPayloadedState<string>
{
    public void Enter(string payload)
    {
        Debug.Log($"Enter {GetType()} {payload}");
    }
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
    }
}