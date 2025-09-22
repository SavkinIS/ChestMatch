using System;
using UnityEngine;
using Zenject;

public class LoadingState : IState, IStateMachineBehavior
{
    private StateMachine _stateMachine;
    private readonly IGameFlowModel _gameFlowModel;
    private readonly ISceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;

    public LoadingState(IGameFlowModel gameFlowModel, ISceneLoader sceneLoader, LoadingCurtain loadingCurtain)
    {
        _loadingCurtain = loadingCurtain;
        _gameFlowModel = gameFlowModel;
        _sceneLoader = sceneLoader;
    }
    
    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
        string sceneToLoad = _gameFlowModel.SceneToLoad;
        _loadingCurtain.Show(() =>
        {
            _sceneLoader.LoadScene(sceneToLoad, OnLoadCompleted);
        });
        
    }
    
    private void OnLoadCompleted()
    {
        if (_gameFlowModel.NextState == typeof(LobbyState))
            _stateMachine.Enter<LobbyState>();
        else
            _stateMachine.Enter<GameplayState>();
        
        
        _loadingCurtain.Hide();
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