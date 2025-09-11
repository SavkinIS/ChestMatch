using System.Collections.Generic;
using UnityEngine;

namespace Shashki
{
    [DisallowMultipleComponent]
    public class BoardRoot : MonoBehaviour
    {
        [Header("Pieces")]
        [SerializeField] private PieceHolder _pieceHolder;
        
        [Header("Grid")]
        [SerializeField] private int _rows = 8;
        [SerializeField] private int _cols = 8;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _spacing = 0f;
        [SerializeField] private bool _darkSquaresOnly = false;
        [SerializeField] private bool _checkerPattern = true;

        [Space(2)]
        [Header("Cells")]
        [SerializeField] private List<BoardCell> _cells;

        public int Rows => _rows;
        public int Cols => _cols;
        public float CellSize => _cellSize;
        public float Spacing => _spacing;
        public bool DarkSquaresOnly => _darkSquaresOnly;
        public bool CheckerPattern => _checkerPattern;
        public List<BoardCell> Cells => _cells;

        private Dictionary<(int row, int col), BoardCell> _cellLookup = new Dictionary<(int, int), BoardCell>();
        private Dictionary<(int row, int col), PieceView> _pieceMap = new Dictionary<(int, int), PieceView>();

        private void Awake()
        {
            RebuildCells();
            _pieceMap = _pieceHolder.GetPieces();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RebuildCells();
            if (_pieceHolder == null)
                _pieceHolder = FindFirstObjectByType<PieceHolder>();
        }
#endif

        public void SetData(int rows, int cols, float cellSize, float spacing, bool darkSquaresOnly, bool checkerPattern)
        {
            _rows = rows;
            _cols = cols;
            _cellSize = cellSize;
            _spacing = spacing;
            _darkSquaresOnly = darkSquaresOnly;
            _checkerPattern = checkerPattern;
        }


        public void SetCells(List<BoardCell> cells)
        {
            _cells = cells;
            RebuildCells();
        }

        [ContextMenu("Rebuild Cells")]
        public void RebuildCells()
        {
            if (_cells == null || _cells.Count == 0)
            {
                var found = GetComponentsInChildren<BoardCell>(true);
                _cells = new List<BoardCell>(found);
            }

            _cellLookup.Clear();
            if (_cells != null)
            {
                foreach (var c in _cells)
                {
                    if (c == null) continue;
                    _cellLookup[(c.Row, c.Col)] = c;
                }
            }
        }

        public BoardCell GetCell(int row, int col)
        {
            _cellLookup.TryGetValue((row, col), out var cell);
            return cell;
        }

        public bool IsInside(int row, int col)
        {
            return row >= 0 && row < _rows && col >= 0 && col < _cols;
        }

        public bool IsInside(Vector2Int pos) => IsInside(pos.x, pos.y);

        public PieceView GetPieceAt(int row, int col)
        {
            if (_pieceMap.TryGetValue((row, col), out var p))
            {
                Debug.Log($"[BoardRoot] Найдена шашка на ({row}, {col}): {p.name}");
                return p;
            }

            Debug.Log($"[BoardRoot] Шашка на ({row}, {col}) не найдена");
            return null;
        }

        public PieceView GetPieceAt(Vector2Int pos) => GetPieceAt(pos.x, pos.y);

        public void RegisterPiece(PieceView piece, int row, int col)
        {
            if (piece == null) return;
            _pieceMap[(row, col)] = piece;
            Debug.Log($"[BoardRoot] Зарегистрирована шашка {piece.name} на ({row}, {col})");
        }

        public void UnregisterPiece(int row, int col)
        {
            _pieceMap.Remove((row, col));
            Debug.Log($"[BoardRoot] Удалена шашка с ({row}, {col})");
        }

        public void MovePieceInMap(PieceView piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            if (piece == null) return;
            if (_pieceMap.ContainsKey((fromRow, fromCol)))
                _pieceMap.Remove((fromRow, fromCol));
            _pieceMap[(toRow, toCol)] = piece;
            Debug.Log($"[BoardRoot] Шашка {piece.name} перемещена с ({fromRow}, {fromCol}) на ({toRow}, {toCol})");
        }

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

        public void OnTurnEnd()
        {
            foreach (var c in _cells)
            {
                c.SetHighlight(false);
            }
        }
    }
}