using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> _availableAbilities; // Список доступных способностей
        private Dictionary<AbilityType, int> _abilityCounts = new Dictionary<AbilityType, int>(); // Количество каждой способности
        private AbilityBase _selectedAbility; // Текущая выбранная способность
        private Dictionary<AbilityType, AbilityBase> _availableAbilitiesDic;
        private PieceView _bombPiece; // Добавлено: назначенная шашка-бомба

        public event Action<AbilityBase> OnAbilityAdded;
        private void Awake()
        {
            // Инициализация на старте сцены: 1 единица для каждой способности
            foreach (var ability in _availableAbilities)
            {
                _abilityCounts[ability.Id] = 20; // 1 единица на старте
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName}: {_abilityCounts[ability.Id]}");
            }
            
            _availableAbilitiesDic = _availableAbilities.ToDictionary(a=> a.Id, a => a);
        }

        // Покупка способности (пока просто увеличивает количество)
        public void BuyAbility(AbilityType abilityId)
        {
            if (_availableAbilities.Exists(a => a.Id == abilityId))
            {
                _abilityCounts.TryGetValue(abilityId, out int count);
                _abilityCounts[abilityId] = count + 1;
                Debug.Log($"[PowerUpManager] Куплена способность {abilityId}, теперь: {_abilityCounts[abilityId]}");
                OnAbilityAdded?.Invoke(_availableAbilitiesDic[abilityId]);
            }
            else
            {
                Debug.LogWarning($"[PowerUpManager] Способность {abilityId} не найдена");
            }
        }

        // Активация способности (например, после клика на UI кнопку)
        public bool ActivateAbility(AbilityType abilityId, GameController gameController)
        {
            if (!_abilityCounts.ContainsKey(abilityId) || _abilityCounts[abilityId] <= 0)
            {
                Debug.Log($"[PowerUpManager] Способность {abilityId} недоступна (количество: {_abilityCounts.GetValueOrDefault(abilityId)})");
                return false;
            }

            var ability = _availableAbilities.Find(a => a.Id == abilityId);
            if (ability == null)
            {
                Debug.LogWarning($"[PowerUpManager] Способность {abilityId} не найдена");
                return false;
            }

            _selectedAbility = ability;
            gameController.SetAbilitySelectionMode(true); // Включаем режим выбора шашки
            Debug.Log($"[PowerUpManager] Активирована способность {ability.DisplayName}, ждём выбора шашки");
            return true;
        }

        // Применение способности к выбранной шашке
        public void ApplyToPiece(PieceView piece)
        {
            if (_selectedAbility == null || piece == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нельзя применить способность: нет выбранной способности или шашки");
                return;
            }

            _selectedAbility.Apply(piece, this);
            _abilityCounts[_selectedAbility.Id]--;
            Debug.Log($"[PowerUpManager] Способность {_selectedAbility.DisplayName} применена к шашке ({piece.Row}, {piece.Col}), осталось: {_abilityCounts[_selectedAbility.Id]}");
            _selectedAbility = null;
        }

        // Получить количество способности
        public int GetAbilityCount(AbilityType abilityId)
        {
            return _abilityCounts.GetValueOrDefault(abilityId, 0);
        }

        public void SetBombPiece(PieceView piece)
        {
            _bombPiece = piece;
            Debug.Log($"[PowerUpManager] Назначена бомба-каикадзе на шашку ({piece.Row}, {piece.Col})");
        }

        public void ExecuteBombExplosion(BoardRoot board, PieceHolder pieceHolder)
        {
            if (_bombPiece == null)
            {
                Debug.LogWarning("[PowerUpManager] Нет назначенной бомбы для взрыва");
                return;
            }

            _bombPiece.ExecuteAbility(board, pieceHolder);
            _bombPiece = null; // Очищаем после взрыва
            Debug.Log("[PowerUpManager] Взрыв бомбы-каикадзе выполнен");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad1))
                BuyAbility(AbilityType.BombKaikaze);
        }
    }
}