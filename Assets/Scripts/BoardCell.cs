using UnityEngine;

namespace Shashki
{
    /// <summary>
    /// Компонент на каждой клетке (runtime).
    /// Хранит координаты клетки и её цвет (тёмная или светлая).
    /// </summary>
    public class BoardCell : MonoBehaviour
    {
        [Min(0)] [SerializeField] private int _row;   // Номер строки
        [Min(0)] [SerializeField] private int _col;  
        [SerializeField] private bool _isDark; 
        
        
        [Min(0)] public int Row => _row;
        [Min(0)] public int Col => _col;
        public bool IsDark => _isDark;

        public void SetData(int row, int col, bool isDark)
        {
            _row = row;
            _col = col;
            _isDark = isDark;
        }
    }
}