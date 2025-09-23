using Shashki;
using UnityEngine;
using Zenject;

public class GameplayState : IState, ITickable, IStateMachineBehavior
{
    private StateMachine _stateMachine;
    private readonly IUIService _uiService;
    private readonly DiContainer _container;
    private GameObject _gameSessionInstance;
    private GameCore _gameCore;
    private readonly GameFlowModel _gameFlowModel;
    private readonly AbilityConteiner _abilityConteiner;

    public GameplayState(IUIService uiService, DiContainer container, GameFlowModel gameFlowModel, AbilityConteiner abilityConteiner)
    {
        _uiService = uiService;
        _container = container;
        _gameFlowModel = gameFlowModel;
        _abilityConteiner = abilityConteiner;
    }
    
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
        
        if (_gameCore != null)
        {
            _gameCore.OnGameOver -= HandleGameOver;
        }

        if (_gameSessionInstance != null)
        {
            Object.Destroy(_gameSessionInstance);
        }
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
        
        var gamePrefab = _container.ResolveId<GameCore>("GameSessionPrefab");
        _gameSessionInstance = _container.InstantiatePrefab(gamePrefab);
        _gameCore = _gameSessionInstance.GetComponent<GameCore>();
        
        if (_gameCore != null)
        {
            var gameplayWindow = _uiService.GetWindow<GameplayWindow>(WindowId.Gameplay);
            _gameCore.Init(gameplayWindow, _gameFlowModel, _abilityConteiner);
            _gameCore.OnGameOver += HandleGameOver;
        }

        _uiService.ShowWindow(WindowId.Gameplay);
    }
    
    private void HandleGameOver(Winner winner)
    {
        _stateMachine.Enter<LobbyState>();
    }

    
    
    public void Tick()
    {
        
    }

    public void SetStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
}