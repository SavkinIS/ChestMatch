using UnityEngine;

public class SettingsState : IState
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