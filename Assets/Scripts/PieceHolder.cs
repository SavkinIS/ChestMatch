// PieceSpawner.cs
// Генерация шашек для BoardRoot (работает и в редакторе, и в плеймоде)

using System;
using System.Collections.Generic;
using Shashki;
using UnityEngine;

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
        [SerializeField] private Color _player1Color = Color.white;
        [SerializeField] private Color _player2Color = Color.black;

        private readonly List<PieceView> _pieces = new List<PieceView>();

        public void SpawnPieces()
        {
            if (_piecePrefab == null || _board == null) return;

            // удаляем старые
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

                // верхние ряды
                if (cell.Row < _rowsWithPieces)
                {
                    CreatePiece(cell, _player1Color, PieceOwner.Opponent);
                }
                // нижние ряды
                else if (cell.Row >= rows - _rowsWithPieces)
                {
                    CreatePiece(cell, _player2Color, PieceOwner.Player);
                }
            }

            Debug.Log($"[PieceHolder] Расставлено {_pieces.Count} шашек");
        }

        private void CreatePiece(BoardCell cell, Color color, PieceOwner owner)
        {
            var piece = Instantiate(_piecePrefab, cell.transform.position + Vector3.back * 0.01f,
                Quaternion.identity, transform);
            piece.SetData(cell.Row, cell.Col, owner, color);
            _pieces.Add(piece);

            // регистрируем в карте BoardRoot
            _board.RegisterPiece(piece, cell.Row, cell.Col);
        }

        /// <summary>
        /// Попробовать сходить шашкой в target-клетку.
        /// </summary>
        public bool TryMove(PieceView piece, BoardCell target)
        {
            var moves = piece.GetPossibleMoves(_board);
            foreach (var move in moves)
            {
                if (move.To == target)
                {
                    // обновляем карту фигур
                    _board.MovePieceInMap(piece, piece.Row, piece.Col, target.Row, target.Col);

                    // перемещаем
                    piece.transform.position = target.transform.position + Vector3.back * 0.01f;
                    piece.SetData(target.Row, target.Col, piece.Owner,
                        piece.GetComponentInChildren<Renderer>().material.color);

                    // поедание
                    if (move.IsCapture && move.CapturedPiece != null)
                    {
                        _pieces.Remove(move.CapturedPiece);
                        _board.UnregisterPiece(move.CapturedPiece.Row, move.CapturedPiece.Col);
                        Destroy(move.CapturedPiece.gameObject);
                    }

                    // превращение в дамку
                    if (!piece.IsKing &&
                        ((piece.Owner == PieceOwner.Player && target.Row == 0) ||
                         (piece.Owner == PieceOwner.Opponent && target.Row == _board.Rows - 1)))
                    {
                        piece.PromoteToKing();
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
