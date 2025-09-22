using System.Collections.Generic;
using Shashki;
using UnityEngine;
using Zenject;

public interface ILobbyState
{
    void OnLevelSelected(string sceneName);
    void OnAbilitySelected(List<AbilityType> abilities);
}

public class LobbyState : IState, IStateMachineBehavior,ILobbyState
{
    private StateMachine _stateMachine;
    private readonly IGameFlowModel _gameFlowModel;
    private readonly IUIService _uiService;

    public LobbyState( IGameFlowModel gameFlowModel, IUIService uiService) 
    {
        _gameFlowModel = gameFlowModel;
        _uiService = uiService;
    }
    
    public void SetStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
        _uiService.HideWindow(WindowId.MainMenu);
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
        _uiService.ShowWindow(WindowId.MainMenu);
    }
    
    public void OnLevelSelected(string sceneName)
    {
        _gameFlowModel.SceneToLoad = sceneName;
        _gameFlowModel.NextState = typeof(GameplayState);
        _stateMachine.Enter<LoadingState>();
    }

    public void OnAbilitySelected(List<AbilityType> abilities)
    {
        _gameFlowModel.AvailableAbilities = abilities;
    }
}