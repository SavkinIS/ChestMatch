using Shashki;
using UnityEngine;

public class GameplayWindow : BaseWindow
{
    [SerializeField] private TimerProgress _timerProgress;
    [SerializeField] private TMPro.TextMeshProUGUI _currentPlayerText;
    [SerializeField] private EndGamePanel _endGamePanel;
    [SerializeField] private TurnTransitionPanel _transitionPanel; 
    [SerializeField] private AbilityButton _abilityButton;
    [SerializeField] private TurnTransitionPanel _turnPanel;
    [SerializeField] private GameObject _playPanel;
    
    public TimerProgress TimerProgress => _timerProgress;
    public TurnTransitionPanel TransitionPanel => _transitionPanel;
    
    public AbilityButton AbilityButton => _abilityButton;


    public void Initialize(PieceOwner pieceOwner)
    {
        _currentPlayerText.text = $"Текущий игрок {pieceOwner}";
        _endGamePanel.ClosePanel();
        _playPanel.SetActiveOptimize(false);
    }

    public void SetPlayerTxt(string value)
    {
        _currentPlayerText.text = value;
    }

    public void ActivateEndGamePanel(Winner winner)
    {
        _endGamePanel.Activate(winner);
        _playPanel.SetActiveOptimize(false);
        if (_transitionPanel != null)
            _transitionPanel.Hide();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_timerProgress == null)
            _timerProgress = FindFirstObjectByType<TimerProgress>();
        if (_transitionPanel == null)
            _transitionPanel = FindFirstObjectByType<TurnTransitionPanel>();
    }
#endif
}