using System;
using System.Collections.Generic;
using System.Linq;
using Shashki;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shashki
{
    public enum PieceOwner
    {
        Player, Opponent
    }
    
    public class PieceHolder : MonoBehaviour
    {
        [SerializeField] private BoardRoot _board;
        [SerializeField] private PieceView _piecePrefab;
        [SerializeField] private int _rowsWithPieces = 2;
        [SerializeField] private Color _playerColor = Color.white;
        [SerializeField] private Color _playerColorBlock;
        [SerializeField] private Material _playerHighlightMaterial;
        [SerializeField] private Color _opponentColor = Color.black;
        [SerializeField] private Color _opponentColorBlock;
        [SerializeField] private Material _opponentHighlightMaterial; 
        [SerializeField] private List<PieceView> _pieces = new List<PieceView>();

        public void SpawnPieces()
        {
            if (_piecePrefab == null || _board == null) return;

            // Удаляем старые
            foreach (var piece in _pieces)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(piece.gameObject);
                else
                    Destroy(piece.gameObject);
#else
                Destroy(piece.gameObject);
#endif
            }
            _pieces.Clear();

            int rows = _board.Rows;

            foreach (BoardCell cell in _board.Cells)
            {
                if (cell == null) continue;
                if (!cell.IsDark) continue;

                // Верхние ряды
                if (cell.Row < _rowsWithPieces)
                {
                    CreatePiece(cell, _opponentColor, PieceOwner.Opponent, _opponentColorBlock, _opponentHighlightMaterial);
                }
                // Нижние ряды
                else if (cell.Row >= rows - _rowsWithPieces)
                {
                    CreatePiece(cell, _playerColor, PieceOwner.Player, _playerColorBlock, _playerHighlightMaterial);
                }
            }

            Debug.Log($"[PieceHolder] Расставлено {_pieces.Count} шашек");
        }

        private void CreatePiece(BoardCell cell, Color color, PieceOwner owner, Color colorBlock, Material highlightMaterial)
        {
            var piece = (PieceView)PrefabUtility.InstantiatePrefab(_piecePrefab);
            
            piece.transform.position = cell.transform.position + Vector3.back * 0.01f;
            piece.transform.parent = transform;
            piece.SetData(cell.Row, cell.Col, owner, color, colorBlock, highlightMaterial);
            piece.name = $"Piece_{owner}";
            _pieces.Add(piece);

            // Регистрируем в карте BoardRoot
            _board.RegisterPiece(piece, cell.Row, cell.Col);
        }

        /// <summary>
        /// Попробовать сходить шашкой в target-клетку.
        /// Возвращает true, если ход выполнен, и out continueCapturing указывает, можно ли продолжить поедание.
        /// </summary>
        public bool TryMove(PieceView piece, BoardCell target, out bool continueCapturing)
        {
            continueCapturing = false;
            var moves = piece.GetPossibleMoves(_board);
            foreach (var move in moves)
            {
                if (move.To == target)
                {
                    // Обновляем карту фигур
                    _board.MovePieceInMap(piece, piece.Row, piece.Col, target.Row, target.Col);

                    // Перемещаем и обновляем данные
                    piece.SetDataAfterMove(target.Row, target.Col);
                    piece.transform.position = target.transform.position + Vector3.back * 0.01f;

                    // Обработка поедания
                    if (move.IsCapture && move.CapturedPiece != null)
                    {
                        PieceDestory(move.CapturedPiece);
                        Debug.Log($"[PieceHolder] Съедена шашка на ({move.CapturedPiece.Row}, {move.CapturedPiece.Col})");
                    }

                    // Проверяем дальнейшие поедания
                    var newMoves = piece.GetPossibleMoves(_board);
                    Debug.Log($"[PieceHolder] Новые ходы после поедания для ({piece.Row}, {piece.Col}): {newMoves.Count}, поедания: {newMoves.Count(m => m.IsCapture)}");
                    if (move.IsCapture && newMoves.Exists(m => m.IsCapture))
                    {
                        continueCapturing = true;
                        Debug.Log($"[PieceHolder] Возможные поедания: {string.Join(", ", newMoves.Where(m => m.IsCapture).Select(m => $"({m.To.Row}, {m.To.Col})"))}");
                    }

                    // Превращение в дамку
                    if (!piece.IsKing &&
                        ((piece.Owner == PieceOwner.Player && target.Row == 0) ||
                         (piece.Owner == PieceOwner.Opponent && target.Row == _board.Rows - 1)))
                    {
                        piece.PromoteToKing();
                        continueCapturing = false; // Дамка завершает ход
                    }

                    // Выполняем способность, если она есть и нет дальнейших поеданий
                    if (!continueCapturing && piece.Ability != null)
                    {
                        piece.ExecuteAbility(_board, this);
                    }

                    return true;
                }
            }

            Debug.Log($"[PieceHolder] Невалидный ход для шашки ({piece.Row}, {piece.Col}) в ({target.Row}, {target.Col})");
            return false;
        }

        public void PieceDestory(PieceView piece)
        {
            _pieces.Remove(piece);
            _board.UnregisterPiece(piece.Row, piece.Col);
            piece.DestroyPiece();
            // Destroy(piece.gameObject);
        }

        public Dictionary<(int row, int col), PieceView> GetPieces()
        {
            return _pieces.ToDictionary(p => (p.Row, p.Col), p => p);
        }
    }
}