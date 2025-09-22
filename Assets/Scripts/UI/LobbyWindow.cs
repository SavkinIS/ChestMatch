using System.Collections.Generic;
using System.Linq;
using Shashki;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyWindow : BaseWindow
{
    [SerializeField] private Button _startButton;
    [SerializeField] private List<SelectAbilityBtn> _abilityBtns;
    
    private ILobbyState _lobbyState;
    [Inject]
    public void Construct(ILobbyState lobbyState)
    {
        _lobbyState = lobbyState;
        _startButton.onClick.AddListener(() => _lobbyState.OnLevelSelected("Gamplay"));
        
        for (int i = 0; i < _abilityBtns.Count; i++)
        {
            _abilityBtns[i].Init(OnSelectAbility);
        }
    }

    private void OnSelectAbility(AbilityType obj)
    {
        List<AbilityType> ability = new List<AbilityType>();
        
        for (int i = 0; i < _abilityBtns.Count; i++)
        {
            var abilityBtn = _abilityBtns[i];
            if (abilityBtn.IsSelected)
                ability.Add(_abilityBtns[i].Abilities);
        }

        _lobbyState.OnAbilitySelected(ability);
    }
}