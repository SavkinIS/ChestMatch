using UnityEngine;
using Zenject;

public class LobbyState : IState, IStateMachineBehavior
{
    private StateMachine _stateMachine;
    private IGameFlowModel _gameFlowModel;

    public LobbyState( IGameFlowModel gameFlowModel) 
    {
        _gameFlowModel = gameFlowModel;
    }
    
    public void SetStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
    }
    
    public void OnLevelSelected(string sceneName)
    {
        _gameFlowModel.SceneToLoad = sceneName;
        _gameFlowModel.NextState = typeof(GameplayState);
        _stateMachine.Enter<LoadingState>();
    }
}