using System;
using Shashki;
using UnityEngine;
using UnityEngine.UI;

public class SelectAbilityBtn : MonoBehaviour
{
    [field: SerializeField] public AbilityType Abilities { get; private set; }
    [SerializeField] private Button _button;
    [SerializeField] private TMPro.TextMeshProUGUI _abilityText;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Image _image;
    [field: SerializeField] public bool IsSelected { get; private set; }

    public void Init(Action<AbilityType> onAbilitySelected)
    {
        IsSelected = false;
        _button.onClick.AddListener(() =>
        {
            IsSelected = !IsSelected;
            CheckColors();
            onAbilitySelected(Abilities);
        });
    }

    private void CheckColors()
    {
       _image.color = IsSelected ? _selectedColor : _normalColor;
    }

    private void OnValidate()
    {
        gameObject.name = Abilities.ToString();
        _abilityText = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _abilityText.text = Abilities.ToString();
        _image = GetComponent<Image>();
    }
}