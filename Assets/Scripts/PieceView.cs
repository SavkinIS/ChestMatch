using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shashki
{
    /// <summary>
    /// Мета-инфо шашки
    /// </summary>
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private int _row;
        [SerializeField] private int _col;
        [SerializeField] private PieceOwner _owner;
        [SerializeField] private Color _color;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private bool _isKing;
        
        public int Row => _row;
        public int Col => _col;
        public PieceOwner Owner => _owner;
        public bool IsKing => _isKing;

        public void SetData(int row, int col, PieceOwner owner, Color color)
        {
            _row = row;
            _col = col;
            _owner = owner;
            _color = color;
            
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            
            if (_renderer != null)
            {
                _renderer.color = _color;
            }
        }
        
        public void PromoteToKing()
        {
            _isKing = true;
            Debug.Log($"{name} стал дамкой!");
            // TODO: визуалка (например, смена материала/добавление короны)
        }

        /// <summary>
        /// Получить список возможных ходов (обычные и поедания).
        /// </summary>
        public List<Move> GetPossibleMoves(BoardRoot board)
        {
            List<Move> moves = new List<Move>();

            int dir = (Owner == PieceOwner.Player) ? -1 : 1;

            // обычный шаг
            AddStep(board, moves, Row + dir, Col + 1);
            AddStep(board, moves, Row + dir, Col - 1);

            // дамка может назад
            if (IsKing)
            {
                AddStep(board, moves, Row - dir, Col + 1);
                AddStep(board, moves, Row - dir, Col - 1);
            }

            // поедание
            AddCaptureMoves(board, moves);

            return moves;
        }

        private void AddStep(BoardRoot board, List<Move> moves, int row, int col)
        {
            var cell = board.GetCell(row, col);
            if (cell != null && board.GetPieceAt(row, col) == null)
            {
                moves.Add(new Move
                {
                    From = board.GetCell(Row, Col),
                    To = cell,
                    IsCapture = false
                });
            }
        }

        private void AddCaptureMoves(BoardRoot board, List<Move> moves)
        {
            int[] dirs = { -1, 1 };
            foreach (int dr in dirs)
            {
                foreach (int dc in dirs)
                {
                    int midRow = Row + dr;
                    int midCol = Col + dc;
                    int landRow = Row + dr * 2;
                    int landCol = Col + dc * 2;

                    var midCell = board.GetCell(midRow, midCol);
                    var landCell = board.GetCell(landRow, landCol);

                    if (midCell == null || landCell == null) continue;

                    var midPiece = board.GetPieceAt(midRow, midCol);
                    if (midPiece != null && midPiece.Owner != Owner &&
                        board.GetPieceAt(landRow, landCol) == null)
                    {
                        moves.Add(new Move
                        {
                            From = board.GetCell(Row, Col),
                            To = landCell,
                            IsCapture = true,
                            CapturedPiece = midPiece
                        });
                    }
                }
            }
        }
    }
}