using System;
using UnityEngine;

public class BootstrapState : IState
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