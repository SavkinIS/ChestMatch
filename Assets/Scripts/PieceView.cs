using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shashki
{
     public class PieceView : MonoBehaviour
    {
        [SerializeField] private int _row;
        [SerializeField] private int _col;
        [SerializeField] private PieceOwner _owner;
        [SerializeField] private Color _color;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private bool _isKing;
        [SerializeField] private AbilityBase _ability; // Добавлено: способность, привязанная к шашке
        
        public int Row => _row;
        public int Col => _col;
        public PieceOwner Owner => _owner;
        public bool IsKing => _isKing;
        public AbilityBase Ability => _ability;

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
        
        public void SetData(int row, int col, PieceOwner owner)
        {
            _row = row;
            _col = col;
            _owner = owner;
            
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
            // TODO: визуалка (например, смена спрайта или добавление короны)
        }

        public void SetAbility(AbilityBase ability)
        {
            _ability = ability;
            Debug.Log($"[PieceView] Шашке ({_row}, {_col}) присвоена способность {ability?.DisplayName}");
            // TODO: визуалка способности (например, иконка или цвет)
        }

        public void ExecuteAbility(BoardRoot board, PieceHolder pieceHolder)
        {
            if (_ability != null)
            {
                _ability.Execute(this, board, pieceHolder);
                _ability = null; // Очищаем способность после выполнения
            }
        }

        /// <summary>
        /// Получить список возможных ходов (обычные шаги или цепочка поеданий).
        /// Если есть поедания, обычные ходы исключаются.
        /// </summary>
        public List<Move> GetPossibleMoves(BoardRoot board)
        {
            List<Move> moves = new List<Move>();
            List<Move> captureMoves = new List<Move>();

            // Собираем ходы с поеданием (включая цепочки)
            AddCaptureChainMoves(board, Row, Col, new List<PieceView>(), captureMoves);

            if (captureMoves.Count > 0)
            {
                Debug.Log($"[PieceView] Найдено {captureMoves.Count} ходов с поеданием для шашки ({Row}, {Col})");
                return captureMoves;
            }

            // Если нет поеданий, добавляем обычные шаги
            int dir = (Owner == PieceOwner.Player) ? -1 : 1;
            AddStep(board, moves, Row + dir, Col + 1);
            AddStep(board, moves, Row + dir, Col - 1);

            // Дамка: обычные ходы на любое расстояние
            if (IsKing)
            {
                AddKingMoves(board, moves);
            }

            Debug.Log($"[PieceView] Найдено {moves.Count} обычных ходов для шашки ({Row}, {Col})");
            return moves;
        }

        private void AddStep(BoardRoot board, List<Move> moves, int row, int col)
        {
            if (board.IsInside(row, col))
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
        }

        private void AddKingMoves(BoardRoot board, List<Move> moves)
        {
            int[] dirs = { -1, 1 };
            foreach (int dr in dirs)
            {
                foreach (int dc in dirs)
                {
                    for (int i = 1; i < board.Rows; i++)
                    {
                        int row = Row + dr * i;
                        int col = Col + dc * i;
                        if (!board.IsInside(row, col)) break;
                        
                        var cell = board.GetCell(row, col);
                        if (cell == null) break;
                        if (board.GetPieceAt(row, col) != null) break;

                        moves.Add(new Move
                        {
                            From = board.GetCell(Row, Col),
                            To = cell,
                            IsCapture = false
                        });
                    }
                }
            }
        }

        private void AddCaptureChainMoves(BoardRoot board, int currentRow, int currentCol, List<PieceView> captured, List<Move> moves)
        {
            int[] dirs = { -1, 1 };

            foreach (int dr in dirs)
            {
                foreach (int dc in dirs)
                {
                    // Поедание для обычной шашки или дамки (на соседней клетке)
                    int midRow = currentRow + dr;
                    int midCol = currentCol + dc;
                    int landRow = currentRow + dr * 2;
                    int landCol = currentCol + dc * 2;

                    if (board.IsInside(midRow, midCol) && board.IsInside(landRow, landCol))
                    {
                        var midCell = board.GetCell(midRow, midCol);
                        var landCell = board.GetCell(landRow, landCol);
                        var midPiece = board.GetPieceAt(midRow, midCol);

                        // Проверяем, можно ли съесть шашку
                        bool canCapture = midCell != null && landCell != null && midPiece != null && midPiece.Owner != Owner &&
                                         board.GetPieceAt(landRow, landCol) == null && !captured.Contains(midPiece);

                        // Запрещаем поедание шашек на границах доски
                        if (canCapture && (midRow == 0 || midRow == board.Rows - 1 || midCol == 0 || midCol == board.Cols - 1))
                        {
                            Debug.Log($"[PieceView] Поедание шашки на ({midRow}, {midCol}) запрещено (на границе доски)");
                            canCapture = false;
                        }

                        if (canCapture)
                        {
                            var newCaptured = new List<PieceView>(captured) { midPiece };
                            moves.Add(new Move
                            {
                                From = board.GetCell(currentRow, currentCol),
                                To = landCell,
                                IsCapture = true,
                                CapturedPiece = midPiece
                            });
                            Debug.Log($"[PieceView] Добавлен ход с поеданием: ({currentRow}, {currentCol}) -> ({landRow}, {landCol}), съедена шашка на ({midRow}, {midCol})");

                            // Временно удаляем съеденную шашку из карты для рекурсии
                            board.UnregisterPiece(midRow, midCol);
                            AddCaptureChainMoves(board, landRow, landCol, newCaptured, moves);
                            // Восстанавливаем карту
                            board.RegisterPiece(midPiece, midRow, midCol);
                        }
                    }
                    else
                    {
                        Debug.Log($"[PieceView] Поедание с ({currentRow}, {currentCol}) в направлении ({dr}, {dc}) невозможно: вне доски");
                    }

                    // Дамка: проверяем поедание на любом расстоянии
                    if (IsKing)
                    {
                        for (int i = 1; i < board.Rows; i++)
                        {
                             midRow = currentRow + dr * i;
                             midCol = currentCol + dc * i;
                             landRow = currentRow + dr * (i + 1);
                             landCol = currentCol + dc * (i + 1);

                            if (!board.IsInside(midRow, midCol)) break;
                            if (!board.IsInside(landRow, landCol)) continue;

                            var midCell = board.GetCell(midRow, midCol);
                            var landCell = board.GetCell(landRow, landCol);
                            var midPiece = board.GetPieceAt(midRow, midCol);

                            bool canCapture = midCell != null && landCell != null && midPiece != null && midPiece.Owner != Owner &&
                                             board.GetPieceAt(landRow, landCol) == null && !captured.Contains(midPiece);

                            // Запрещаем поедание шашек на границах доски
                            if (canCapture && (midRow == 0 || midRow == board.Rows - 1 || midCol == 0 || midCol == board.Cols - 1))
                            {
                                Debug.Log($"[PieceView] Поедание шашки на ({midRow}, {midCol}) запрещено (на границе доски)");
                                canCapture = false;
                            }

                            if (canCapture)
                            {
                                var newCaptured = new List<PieceView>(captured) { midPiece };
                                moves.Add(new Move
                                {
                                    From = board.GetCell(currentRow, currentCol),
                                    To = landCell,
                                    IsCapture = true,
                                    CapturedPiece = midPiece
                                });
                                Debug.Log($"[PieceView] Добавлен ход дамки с поеданием: ({currentRow}, {currentCol}) -> ({landRow}, {landCol}), съедена шашка на ({midRow}, {midCol})");

                                // Временно удаляем съеденную шашку из карты для рекурсии
                                board.UnregisterPiece(midRow, midCol);
                                AddCaptureChainMoves(board, landRow, landCol, newCaptured, moves);
                                // Восстанавливаем карту
                                board.RegisterPiece(midPiece, midRow, midCol);
                                break; // Только одно поедание за раз по этой диагонали
                            }
                            if (midPiece != null) break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Подсветить возможные ходы для этой шашки.
        /// </summary>
        public void HighlightPossibleMoves(BoardRoot board, bool highlight)
        {
            var moves = GetPossibleMoves(board);
            foreach (var move in moves)
            {
                move.To.SetHighlight(highlight);
            }
        }

        public void SetDataAfterMove(int targetRow, int targetCol)
        {
            _row = targetRow;
            _col = targetCol;
        }
    }
}