using System;
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
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _buttonColorBase;
        [SerializeField] private Color _buttonColorActive;

        private Button _button;
        private PieceOwner _currentOwner;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
            _buttonImage = gameObject.GetComponent<Image>();
            _buttonImage.color = _buttonColorBase;
        }

        private void Start()
        {
            UpdateButtonState();
            _powerUpManager.OnAbilityAdded += OnAbilityAdded;
            _powerUpManager.OnAbilityChanged += OnAbilityChanged;
            _gameController.OnTurnEnd += OnTurnEnd;
        }

        private void OnAbilityChanged()
        {
            _buttonImage.color = _buttonColorBase;
            UpdateButtonState();
        }

        private void OnTurnEnd()
        {
            _currentOwner = _gameController.Owner;
            UpdateButtonState();
        }

        private void OnAbilityAdded(PieceOwner owner, AbilityBase ability)
        {
            _currentOwner = owner;
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
                _buttonImage.color = _buttonColorActive;
                UpdateButtonState();
            }
            else
            {
                _buttonImage.color = _buttonColorBase;
                Debug.Log($"[AbilityButton] Нельзя активировать {_abilityType}: недостаточно единиц");
            }
        }

        private void UpdateButtonState()
        {
            int count = _powerUpManager.GetAbilityCount(_currentOwner, _abilityType);
            _button.interactable = count > 0;
            if (_countText != null)
                _countText.text = $"{_abilityType}<br>{count}";
            Debug.Log($"[AbilityButton] Состояние кнопки {_abilityType}: доступно = {count > 0}, количество = {count}");
        }

#if UNITY_EDITOR
        
        private void OnValidate()
        {
            gameObject.name = $"AbilityButton_{_abilityType}";
            if (_countText != null)
                _countText.text = $"{_abilityType}";
        }
        
#endif
     
    }
}