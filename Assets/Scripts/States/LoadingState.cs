using System;
using UnityEngine;
using Zenject;

public class LoadingState : IState, IStateMachineBehavior
{
    private StateMachine _stateMachine;
    private IGameFlowModel _gameFlowModel;
    private ISceneLoader _sceneLoader;

    public LoadingState(IGameFlowModel gameFlowModel, ISceneLoader sceneLoader)
    {
        
        _gameFlowModel = gameFlowModel;
        _sceneLoader = sceneLoader;
    }
    
    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
        string sceneToLoad = _gameFlowModel.SceneToLoad;
        _sceneLoader.LoadScene(sceneToLoad, OnLoadCompleted);
    }
    
    private void OnLoadCompleted()
    {
        if (_gameFlowModel.NextState == typeof(LobbyState))
            _stateMachine.Enter<LobbyState>();
        else
            _stateMachine.Enter<GameplayState>();
    }
    
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
    }

    public void SetStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
}