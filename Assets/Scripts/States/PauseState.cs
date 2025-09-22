using UnityEngine;

public class PauseState : IState, IStateMachineBehavior, ITickable
{
    private StateMachine _stateMachine;
    private readonly IUIService _uiService;

    public PauseState(IUIService uiService)
    {
        _uiService = uiService;
    }
    
    public void SetStateMachine(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    public void Exit()
    {
        Debug.Log($"Exit {GetType()}");
        _uiService.ShowWindow(WindowId.Pause);
        Time.timeScale = 1f;
    }

    public void Enter()
    {
        Debug.Log($"Enter {GetType()}");
        
        _uiService.ShowWindow(WindowId.Pause);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
       
        _stateMachine.Enter<GameplayState>();
    }

    public void OpenSettings()
    {
        // TODO: Переключиться в SettingsState
    }

    public void ExitToLobby()
    {
        // Перед выходом в лобби нужно обязательно вернуть Time.timeScale к 1!
        // Метод Exit() этого стейта сделает это автоматически при переключении.
        _stateMachine.Enter<LobbyState>();
    }

    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _stateMachine.Enter<PauseState>();
        }
    }
}