using UnityEngine;

public class PauseState : IState
{
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
    }
}