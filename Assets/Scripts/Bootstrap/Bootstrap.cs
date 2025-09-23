using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Bootstrap : MonoBehaviour
{
    private StateMachine _stateMachine;
    private GameFlowModel _gameFlowModel;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _stateMachine.Enter<BootstrapState>();
        OnSwitchStateClicked();
    }
    
    
    [Inject]
    public void Construct(StateMachine stateMachine, GameFlowModel gameFlowModel)
    {
        _stateMachine = stateMachine;
        _gameFlowModel = gameFlowModel;
    }
    
    
    private void OnSwitchStateClicked()
    {
        _gameFlowModel.SceneToLoad = "Lobby";
        _gameFlowModel.NextState = typeof(LobbyState);
        
        _stateMachine.Enter<LoadingState>(); 
    }
    
}