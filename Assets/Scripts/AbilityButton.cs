using UnityEngine;
using UnityEngine.UI;

namespace Shashki
{
    [RequireComponent(typeof(Button))]
    public class AbilityButton : MonoBehaviour
    {
        [SerializeField] private AbilityType _abilityType; // Тип способности
        [SerializeField] private PowerUpManager _powerUpManager; // Ссылка на менеджер
        [SerializeField] private GameController _gameController; // Ссылка на контроллер
        [SerializeField] private TMPro.TextMeshProUGUI _countText; // UI текст для количества

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        private void Start()
        {
            UpdateButtonState();
            _powerUpManager.OnAbilityAdded += OnAbilityAdded;
        }

        private void OnAbilityAdded(AbilityBase ability)
        {
            if (ability.Id == _abilityType)
            {
                UpdateButtonState();
            }
        }

        private void OnButtonClick()
        {
            if (_powerUpManager.ActivateAbility(_abilityType, _gameController))
            {
                Debug.Log($"[AbilityButton] Кнопка активировала способность {_abilityType}");
                UpdateButtonState();
            }
            else
            {
                Debug.Log($"[AbilityButton] Нельзя активировать {_abilityType}: недостаточно единиц");
            }
        }

        private void UpdateButtonState()
        {
            int count = _powerUpManager.GetAbilityCount(_abilityType);
            _button.interactable = count > 0;
            if (_countText != null)
                _countText.text = $"{_abilityType} {count}";
            Debug.Log($"[AbilityButton] Состояние кнопки {_abilityType}: доступно = {count > 0}, количество = {count}");
        }
    }
}