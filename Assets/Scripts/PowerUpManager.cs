using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class PowerUpManager : MonoBehaviour
    {
        [SerializeField] private List<AbilityBase> _availableAbilities; // Список доступных способностей
        [SerializeField] private GameController _gameController;
        private Dictionary<PieceOwner, Dictionary<AbilityType, int>> _abilityCounts = new Dictionary<PieceOwner, Dictionary<AbilityType, int>>(); // Количество способностей для каждого игрока
        private Dictionary<PieceOwner, AbilityBase> _selectedAbilities; // Текущие выбранные способности для каждого игрока
        private Dictionary<AbilityType, AbilityBase> _availableAbilitiesDic;
        private Dictionary<PieceOwner, PieceView> _bombPieces; // Бомбы для каждого игрока
       

        public event Action<PieceOwner, AbilityBase> OnAbilityAdded; // Обновлено: добавлен PieceOwner

        private void Awake()
        {
            _gameController = FindObjectOfType<GameController>();
            
            // Инициализация коллекций для каждого игрока
            _abilityCounts[PieceOwner.Player] = new Dictionary<AbilityType, int>();
            _abilityCounts[PieceOwner.Opponent] = new Dictionary<AbilityType, int>();
            _selectedAbilities = new Dictionary<PieceOwner, AbilityBase>();
            _bombPieces = new Dictionary<PieceOwner, PieceView>();
            _availableAbilitiesDic = _availableAbilities.ToDictionary(a => a.Id, a => a);

            // Инициализация способностей: 1 единица для каждой способности для каждого игрока
            foreach (var ability in _availableAbilities)
            {
                _abilityCounts[PieceOwner.Player][ability.Id] = 20;
                _abilityCounts[PieceOwner.Opponent][ability.Id] = 20;
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Player}: {_abilityCounts[PieceOwner.Player][ability.Id]}");
                Debug.Log($"[PowerUpManager] Инициализирована способность {ability.DisplayName} для {PieceOwner.Opponent}: {_abilityCounts[PieceOwner.Opponent][ability.Id]}");
            }
        }

        // Покупка способности для указанного игрока
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

        // Активация способности для указанного игрока
        public bool ActivateAbility( AbilityType abilityId, GameController gameController)
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
            gameController.SetAbilitySelectionMode(true); // Включаем режим выбора шашки
            Debug.Log($"[PowerUpManager] Активирована способность {ability.DisplayName} для {owner}, ждём выбора шашки");
            return true;
        }

        // Применение способности к выбранной шашке
        public void ApplyToPiece(PieceView piece)
        {
            if (piece == null || !_selectedAbilities.ContainsKey(piece.Owner) || _selectedAbilities[piece.Owner] == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нельзя применить способность: нет выбранной способности или шашки для {piece?.Owner}");
                return;
            }

            var ability = _selectedAbilities[piece.Owner];
            ability.Apply(piece, this);
            _abilityCounts[piece.Owner][ability.Id]--;
            Debug.Log($"[PowerUpManager] Способность {ability.DisplayName} применена к шашке ({piece.Row}, {piece.Col}) для {piece.Owner}, осталось: {_abilityCounts[piece.Owner][ability.Id]}");
            _selectedAbilities[piece.Owner] = null;
        }

        // Получить количество способности для указанного игрока
        public int GetAbilityCount(PieceOwner owner, AbilityType abilityId)
        {
            return _abilityCounts[owner].GetValueOrDefault(abilityId, 0);
        }

        // Назначение бомбы для указанного игрока
        public void SetBombPiece(PieceView piece)
        {
            _bombPieces[piece.Owner] = piece;
            Debug.Log($"[PowerUpManager] Назначена бомба-каикадзе на шашку ({piece.Row}, {piece.Col}) для {piece.Owner}");
        }

        // Выполнение взрыва бомбы для указанного игрока
        public void ExecuteBombExplosion(PieceOwner owner, BoardRoot board, PieceHolder pieceHolder)
        {
            if (!_bombPieces.ContainsKey(owner) || _bombPieces[owner] == null)
            {
                Debug.LogWarning($"[PowerUpManager] Нет назначенной бомбы для {owner}");
                return;
            }

            _bombPieces[owner].ExecuteAbility(board, pieceHolder);
            _bombPieces[owner] = null; // Очищаем после взрыва
            Debug.Log($"[PowerUpManager] Взрыв бомбы-каикадзе выполнен для {owner}");
        }

        private void Update()
        {
            // Тестовые клавиши для покупки способностей для каждого игрока
            if (Input.GetKeyDown(KeyCode.Keypad1))
                BuyAbility(PieceOwner.Player, AbilityType.BombKaikaze);
            if (Input.GetKeyDown(KeyCode.Keypad2))
                BuyAbility(PieceOwner.Opponent, AbilityType.BombKaikaze);
        }
    }
}