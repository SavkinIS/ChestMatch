using System.Collections.Generic;
using Shashki;
using UnityEngine;

namespace Shashki
{
    /// <summary>
    /// Root-объект доски. Хранит сериализуемые параметры и список клеток (_cells).
    /// Добавляет рантайм-кэш для быстрой работы с клетками и простые методы для работы с фигурами.
    /// Совместим с существующим API: SetData, SetMaterials, SetCells и публичными геттерами.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoardRoot : MonoBehaviour
    {
        [Header("Grid")]
        [SerializeField] private int _rows = 8;       // Количество строк
        [SerializeField] private int _cols = 8;       // Количество столбцов
        [SerializeField] private float _cellSize = 1f;// Размер клетки в юнитах
        [SerializeField] private float _spacing = 0f; // Расстояние между клетками
        [SerializeField] private bool _darkSquaresOnly = false;  // Генерировать только тёмные клетки (шашки)
        [SerializeField] private bool _checkerPattern = true;    // Использовать ли шахматный паттерн

        [Header("Materials (saved paths)")]
        [Tooltip("GUID материалов, созданных автоматически. Не трогать вручную.")]
        [SerializeField] private string _lightMaterialGuid;
        [SerializeField] private string _darkMaterialGuid;
        [Space(2)]
        [Header("Cells")]
        [SerializeField] private List<BoardCell> _cells;

        // Публичные свойства (как в твоём коде)
        public int Rows => _rows;
        public int Cols => _cols;
        public float CellSize => _cellSize;
        public float Spacing => _spacing;
        public bool DarkSquaresOnly => _darkSquaresOnly;
        public bool CheckerPattern => _checkerPattern;
        public string LightMaterialGuid => _lightMaterialGuid;
        public string DarkMaterialGuid => _darkMaterialGuid;
        public List<BoardCell> Cells => _cells;

        // --- runtime caches ---
        // Быстрый словарь для поиска клеток по (row,col)
        private readonly Dictionary<(int row, int col), BoardCell> _cellLookup = new Dictionary<(int, int), BoardCell>();

        // Карта фигур: (row,col) -> PieceView (регистрируется при создании/перемещении фигуры)
        private readonly Dictionary<(int row, int col), PieceView> _pieceMap = new Dictionary<(int, int), PieceView>();

        // --- Unity lifecycle ---
        private void Awake()
        {
            RebuildCells();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // в редакторе актуализируем кэши, чтобы инспектор и сцена синхронизировались
            RebuildCells();
        }
#endif

        /// <summary>
        /// Установить параметры доски (как у тебя было).
        /// </summary>
        public void SetData(int rows, int cols, float cellSize, float spacing, bool darkSquaresOnly, bool checkerPattern)
        {
            _rows = rows;
            _cols = cols;
            _cellSize = cellSize;
            _spacing = spacing;
            _darkSquaresOnly = darkSquaresOnly;
            _checkerPattern = checkerPattern;
        }

        /// <summary>
        /// Установить GUID материалов (как было).
        /// </summary>
        public void SetMaterials(string lightMaterialGuid, string darkMaterialGuid)
        {
            _lightMaterialGuid = lightMaterialGuid;
            _darkMaterialGuid = darkMaterialGuid;
        }

        /// <summary>
        /// Установить список клеток напрямую (как было).
        /// Вызывать если клетки создаются/меняются извне.
        /// </summary>
        public void SetCells(List<BoardCell> cells)
        {
            _cells = cells;
            RebuildCells();
        }

        /// <summary>
        /// (ContextMenu) Собрать и обновить локальные кэши клеток.
        /// Если _cells пустой — попытается найти дочерние BoardCell автоматически.
        /// </summary>
        [ContextMenu("Rebuild Cells")]
        public void RebuildCells()
        {
            // Если список клеток в инспекторе пуст, попробуем автоматически собрать дочерние BoardCell
            if (_cells == null || _cells.Count == 0)
            {
                var found = GetComponentsInChildren<BoardCell>(true);
                _cells = new List<BoardCell>(found);
            }

            // Обновляем lookup
            _cellLookup.Clear();
            if (_cells != null)
            {
                foreach (var c in _cells)
                {
                    if (c == null) continue;
                    // Защита от повторной записи — последний пишет
                    _cellLookup[(c.Row, c.Col)] = c;
                }
            }

            // При перестройке клеток можно очистить карту фигур — но оставим карту, если фигуры уже зарегистрированы
            // _pieceMap.Clear();
        }

        /// <summary>
        /// Возвращает BoardCell по координатам (или null).
        /// </summary>
        public BoardCell GetCell(int row, int col)
        {
            _cellLookup.TryGetValue((row, col), out var cell);
            return cell;
        }

        /// <summary>
        /// Проверяет, попадает ли пара координат внутрь доски.
        /// </summary>
        public bool IsInside(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        public bool IsInside(Vector2Int pos) => IsInside(pos.x, pos.y);

        /// <summary>
        /// Возвращает PieceView на клетке, если есть.
        /// Сначала проверяем _pieceMap (O(1)), затем делаем fallback по сцене (медленнее).
        /// </summary>
        public PieceView GetPieceAt(int row, int col)
        {
            if (_pieceMap.TryGetValue((row, col), out var p))
                return p;

            // fallback (на время разработки / если map не используется)
            var all = FindObjectsOfType<PieceView>();
            foreach (var piece in all)
            {
                if (piece.Row == row && piece.Col == col)
                    return piece;
            }

            return null;
        }

        public PieceView GetPieceAt(Vector2Int pos) => GetPieceAt(pos.x, pos.y);

        /// <summary>
        /// Регистрирует фигуру в карте (использовать при создании / спавне).
        /// Если на целевой ячейке уже была зарегистрирована другая фигура — она будет перезаписана.
        /// </summary>
        public void RegisterPiece(PieceView piece, int row, int col)
        {
            if (piece == null) return;
            _pieceMap[(row, col)] = piece;
        }

        /// <summary>
        /// Удаляет запись о фигуре (например, при поедании).
        /// </summary>
        public void UnregisterPiece(int row, int col)
        {
            _pieceMap.Remove((row, col));
        }

        /// <summary>
        /// Обновляет позицию фигуры в карте (при перемещении).
        /// </summary>
        public void MovePieceInMap(PieceView piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (piece == null) return;

            // если в карте есть запись старой позиции — удаляем
            if (_pieceMap.ContainsKey((fromRow, fromCol)))
                _pieceMap.Remove((fromRow, fromCol));

            // ставим новую
            _pieceMap[(toRow, toCol)] = piece;
        }

        /// <summary>
        /// Полезный метод: найти ближайшую клетку по мировым координатам.
        /// </summary>
        public BoardCell GetNearestCellFromWorld(Vector3 worldPos)
        {
            BoardCell best = null;
            float bestSqr = float.MaxValue;

            if (_cells == null || _cells.Count == 0) return null;

            foreach (var c in _cells)
            {
                if (c == null) continue;
                float s = (c.transform.position - worldPos).sqrMagnitude;
                if (s < bestSqr)
                {
                    bestSqr = s;
                    best = c;
                }
            }

            return best;
        }
    }
}