using System;
using UnityEngine;

namespace Shashki
{
    /// <summary>
    /// Компонент на каждой клетке (runtime).
    /// Хранит координаты клетки и её цвет (тёмная или светлая).
    /// </summary>
    public class BoardCell : MonoBehaviour
    {
        [Min(0)] [SerializeField] private int _row;
        [Min(0)] [SerializeField] private int _col;
        [SerializeField] private Material _lightMaterial;
        [SerializeField] private Material _darkMaterial;
        [SerializeField] private bool _isDark;
        [SerializeField] private SpriteRenderer _renderer; // Для подсветки
        [SerializeField] private bool _isHighlight; // Для подсветки
        [SerializeField] private ParticleSystem _highlight;
        
        [Min(0)] public int Row => _row;
        [Min(0)] public int Col => _col;
        public bool IsDark => _isDark;

        private Color _originalColor;

        private void Awake()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null)
                _originalColor = _renderer.color;
        }

        public void SetData(int row, int col, bool isDark)
        {
            _row = row;
            _col = col;
            _isDark = isDark;
            
            _renderer.material = _isDark ? _darkMaterial : _lightMaterial;
            
        }

        /// <summary>
        /// Включить/выключить подсветку клетки.
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            _isHighlight = highlight;
            if (_isDark)
            {
                _highlight.gameObject.SetActive(_isHighlight);
                _highlight.Play();
            }
           
        }

        private void OnValidate()
        {
            SetHighlight(_isHighlight);
        }
    }
}