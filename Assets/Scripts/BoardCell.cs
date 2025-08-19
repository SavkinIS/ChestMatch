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
        [SerializeField] private bool _isDark;
        [SerializeField] private SpriteRenderer _renderer; // Для подсветки
        
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
        }

        /// <summary>
        /// Включить/выключить подсветку клетки.
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            if (_renderer == null) return;
            _renderer.color = highlight ? new Color(0.5f, 1f, 0.5f, 1f) : _originalColor; // Зелёная подсветка
        }

        public void SetColor(Material material)
        {
            _renderer.material = material;
        }
    }
}