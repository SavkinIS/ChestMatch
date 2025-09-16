using UnityEngine;

public class GameplayState : IState, ITickable
{
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
    }

    public void Tick()
    {
        
    }
    
}