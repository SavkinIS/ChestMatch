using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> _availableAbilities;
        [SerializeField] private GameController _gameController;
        
        private Dictionary<PieceOwner, Dictionary<AbilityType, int>> _abilityCounts = new Dictionary<PieceOwner, Dictionary<AbilityType, int>>();
        private Dictionary<PieceOwner, AbilityBase> _selectedAbilities;
        private Dictionary<AbilityType, AbilityBase> _availableAbilitiesDic;
        private Dictionary<PieceOwner, PieceView> _bombPieces;

        public event Action<PieceOwner, AbilityBase> OnAbilityAdded;

        private void Awake()
        {
            _gameController = FindObjectOfType<GameController>();
            
            _abilityCounts[PieceOwner.Player] = new Dictionary<AbilityType, int>();
            _abilityCounts[PieceOwner.Opponent] = new Dictionary<AbilityType, int>();
            _selectedAbilities = new Dictionary<PieceOwner, AbilityBase>();
            _bombPieces = new Dictionary<PieceOwner, PieceView>();
            _availableAbilitiesDic = _availableAbilities.ToDictionary(a => a.Id, a => a);

            foreach (var ability in _availableAbilities)
            {
                _abilityCounts[PieceOwner.Player][ability.Id] = 20;
                _abilityCounts[PieceOwner.Opponent][ability.Id] = 20;
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Player}: {_abilityCounts[PieceOwner.Player][ability.Id]}");
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Opponent}: {_abilityCounts[PieceOwner.Opponent][ability.Id]}");
            }
        }

        public void BuyAbility(PieceOwner owner, AbilityType abilityId)
        {
            if (_availableAbilities.Exists(a => a.Id == abilityId))
            {
                _abilityCounts[owner].TryGetValue(abilityId, out int count);
                _abilityCounts[owner][abilityId] = count + 1;
                Debug.Log($"[PowerUpManager] Куплена способность {abilityId} для {owner}, теперь: {_abilityCounts[owner][abilityId]}");
                OnAbilityAdded?.Invoke(owner, _availableAbilitiesDic[abilityId]);
            }
            else
            {
                Debug.LogWarning($"[PowerUpManager] Способность {abilityId} не найдена для {owner}");
            }
        }

        public bool ActivateAbility(AbilityType abilityId, GameController gameController)
        {
            PieceOwner owner = _gameController.Owner;
            
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

            _selectedAbilities[owner] = ability;
            gameController.SetAbilitySelectionMode(true, abilityId);
            Debug.Log($"[PowerUpManager] Активирована способность {ability.DisplayName} для {owner}, ждём выбора шашки");
            return true;
        }

        public void ApplyToPiece(PieceView piece)
        {
            if (piece == null || !_selectedAbilities.ContainsKey(piece.Owner) || _selectedAbilities[piece.Owner] == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нельзя применить способность: нет выбранной способности или шашки для {piece?.Owner}");
                return;
            }

            var ability = _selectedAbilities[piece.Owner];
            ability.Apply(piece, this);
            if (ability.Id != AbilityType.SwapSides)
            {
                _abilityCounts[piece.Owner][ability.Id]--;
                _selectedAbilities[piece.Owner] = null;
            }
            Debug.Log($"[PowerUpManager] Способность {ability.DisplayName} применена к шашке ({piece.Row}, {piece.Col}) для {piece.Owner}, осталось: {_abilityCounts[piece.Owner][ability.Id]}");
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
                _abilityCounts[owner][abilityId]--;
                _selectedAbilities[owner] = null;
                Debug.Log($"[PowerUpManager] Способность {abilityId} для {owner} потреблена, осталось: {_abilityCounts[owner][abilityId]}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                BuyAbility(PieceOwner.Player, AbilityType.BombKaikaze);
            if (Input.GetKeyDown(KeyCode.Keypad2))
                BuyAbility(PieceOwner.Opponent, AbilityType.BombKaikaze);
            if (Input.GetKeyDown(KeyCode.Keypad3))
                BuyAbility(PieceOwner.Player, AbilityType.SwapSides);
            if (Input.GetKeyDown(KeyCode.Keypad4))
                BuyAbility(PieceOwner.Opponent, AbilityType.SwapSides);
            if (Input.GetKeyDown(KeyCode.Keypad5))
                ActivateAbility(AbilityType.SwapSides, _gameController);
            if (Input.GetKeyDown(KeyCode.Keypad6))  // НОВОЕ: для Щита
                ActivateAbility(AbilityType.Shield, _gameController);
        }
    }
}