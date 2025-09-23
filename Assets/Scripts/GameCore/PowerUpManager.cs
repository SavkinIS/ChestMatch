using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shashki
{
    public class PowerUpManager 
    {
        private List<AbilityBase> _availableAbilities;
        private GameCore _gameCore;
        
        private Dictionary<PieceOwner, Dictionary<AbilityType, int>> _abilityCounts = new Dictionary<PieceOwner, Dictionary<AbilityType, int>>();
        private Dictionary<PieceOwner, AbilityBase> _selectedAbilities;
        private Dictionary<AbilityType, AbilityBase> _availableAbilitiesDic;
        private Dictionary<PieceOwner, PieceView> _bombPieces;

        public event Action<PieceOwner, AbilityBase> OnAbilityAdded;
        public event Action OnAbilityChanged;
        public GameCore GameCore => _gameCore;

        public void Init(List<AbilityBase> availableAbilities, GameCore gameCore)
        {
            _availableAbilities = availableAbilities;
            _gameCore = gameCore;
            _abilityCounts[PieceOwner.Player] = new Dictionary<AbilityType, int>();
            _abilityCounts[PieceOwner.Opponent] = new Dictionary<AbilityType, int>();
            _selectedAbilities = new Dictionary<PieceOwner, AbilityBase>();
            _bombPieces = new Dictionary<PieceOwner, PieceView>();
            _availableAbilitiesDic = _availableAbilities.ToDictionary(a => a.Id, a => a);

            foreach (var ability in _availableAbilities)
            {
                _abilityCounts[PieceOwner.Player][ability.Id] = 1;
                _abilityCounts[PieceOwner.Opponent][ability.Id] = 1;
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Player}: {_abilityCounts[PieceOwner.Player][ability.Id]}");
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Opponent}: {_abilityCounts[PieceOwner.Opponent][ability.Id]}");
            }
        }

        public bool ActivateAbility(AbilityType abilityId, GameCore gameCore)
        {
            PieceOwner owner = this._gameCore.Owner;

            if (!_abilityCounts[owner].ContainsKey(abilityId) || _abilityCounts[owner][abilityId] <= 0)
            {
                Debug.Log($"[PowerUpManager] Способность {abilityId} недоступна для {owner} (количество: {_abilityCounts[owner].GetValueOrDefault(abilityId)})");
                return false;
            }

            var ability = _availableAbilities.Find(a => a.Id == abilityId);
            if (ability == null)
            {
                Debug.LogWarning($"[PowerUpManager] Способность {abilityId} не найдена для {owner}");
                return false;
            }

            if (_selectedAbilities.TryGetValue(owner, out var selectedAbility) && selectedAbility == ability)
            {
                _selectedAbilities[owner] = null;
                return false;
            }
            
            _selectedAbilities[owner] = ability;
            gameCore.SetAbilitySelectionMode(true, abilityId);
            Debug.Log($"[PowerUpManager] Активирована способность {ability.DisplayName} для {owner}, ждём выбора шашки");
            return true;
        }

        public void ApplyToPiece(PieceView piece)
        {
            PieceOwner owner = _gameCore.Owner;
            
            if (piece == null || !_selectedAbilities.ContainsKey(owner) || _selectedAbilities[owner] == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нельзя применить способность: нет выбранной способности или шашки для {owner}");
                return;
            }

            var ability = _selectedAbilities[owner];
            ability.Apply(piece, this);
            if (ability.Id != AbilityType.SwapSides)
            {
                DecreaseAbility(owner, ability.Id);
            }
            Debug.Log($"[PowerUpManager] Способность {ability.DisplayName} применена к шашке ({piece.Row}, {piece.Col}) для {piece.Owner}, осталось: {_abilityCounts[piece.Owner][ability.Id]}");
        }

        private void DecreaseAbility(PieceOwner owner, AbilityType ability)
        {
            _abilityCounts[owner][ability]--;
            _selectedAbilities[owner] = null;
            OnAbilityChanged?.Invoke();
        }

        public AbilityBase GetAbilityInstance(AbilityType abilityId)
        {
            return _availableAbilities.Find(a => a.Id == abilityId);
        }

        public int GetAbilityCount(PieceOwner owner, AbilityType abilityId)
        {
            return _abilityCounts[owner].GetValueOrDefault(abilityId, 0);
        }

        public void SetBombPiece(PieceView piece)
        {
            _bombPieces[piece.Owner] = piece;
            Debug.Log($"[PowerUpManager] Назначена бомба-каикадзе на шашку ({piece.Row}, {piece.Col}) для {piece.Owner}");
        }

        public void ExecuteBombExplosion(PieceOwner owner, BoardRoot board, PieceHolder pieceHolder)
        {
            if (!_bombPieces.ContainsKey(owner) || _bombPieces[owner] == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нет назначенной бомбы для {owner}");
                return;
            }

            _bombPieces[owner].ExecuteAbility(board, pieceHolder);
            _bombPieces[owner] = null;
            Debug.Log($"[PowerUpManager] Взрыв бомбы-каикадзе выполнен для {owner}");
        }

        public void ConsumeAbility(PieceOwner owner, AbilityType abilityId)
        {
            if (_abilityCounts[owner].ContainsKey(abilityId))
            {
                DecreaseAbility(owner, abilityId);
                Debug.Log($"[PowerUpManager] Способность {abilityId} для {owner} потреблена, осталось: {_abilityCounts[owner][abilityId]}");
            }
        }

        public AbilityType GetCurrentAbility()
        {
            return _selectedAbilities[_gameCore.Owner].Id;
        }
    }
}