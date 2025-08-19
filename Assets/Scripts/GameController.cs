using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardRoot _board;
        [SerializeField] private PieceHolder _pieceHolder;
        [SerializeField] private LayerMask _piecesLayer; // Слой для шашек (например, "Pieces")
        [SerializeField] private LayerMask _cellsLayer;  // Слой для клеток (например, "Cells")
        
        private PieceView _selectedPiece;
        private PieceOwner _currentPlayer = PieceOwner.Player;

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Получаем позицию клика в мировых координатах
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                Debug.Log($"[GameController] Клик по: {mousePos}");

                // Raycast2D для поиска объекта под кликом
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero, 20f);
                RaycastHit2D hit = hits.Length > 0 ? hits[0] : new RaycastHit2D();
                
                if (hit.collider == null)
                {
                    Debug.Log("[GameController] Raycast не попал");
                    return;
                }

                // Проверяем все попадания для отладки
                foreach (var h in hits)
                {
                    Debug.Log($"[GameController] Попал по: {h.collider.gameObject.name}, слой: {LayerMask.LayerToName(h.collider.gameObject.layer)}");
                }

                // Проверяем, что попали
                GameObject hitObject = hit.collider.gameObject;

                // Если шашка уже выбрана, проверяем клик по клетке для хода
                if (_selectedPiece != null)
                {
                    BoardCell targetCell = hitObject.GetComponent<BoardCell>();
                    if (targetCell != null && _selectedPiece.Owner == _currentPlayer)
                    {
                        if (_pieceHolder.TryMove(_selectedPiece, targetCell, out bool continueCapturing))
                        {
                            var newMoves = _selectedPiece.GetPossibleMoves(_board);
                            var count = newMoves.Where(m => m.IsCapture).Count();
                            Debug.Log($"[GameController] Новые ходы для шашки ({_selectedPiece.Row}, {_selectedPiece.Col}): {newMoves.Count}, поедания: {count}");

                            if (!continueCapturing || !newMoves.Exists(m => m.IsCapture))
                            {
                                // Ход завершён, переключаем игрока
                                _currentPlayer = (_currentPlayer == PieceOwner.Player) ? PieceOwner.Opponent : PieceOwner.Player;
                                Debug.Log($"[GameController] Ход сделан! Теперь ходит: {_currentPlayer}");
                                DeselectPiece();
                            }
                            else
                            {
                                // Продолжаем поедание, обновляем подсветку
                                _selectedPiece.HighlightPossibleMoves(_board, false);
                                Debug.Log($"[GameController] Продолжаем поедание для шашки на ({_selectedPiece.Row}, {_selectedPiece.Col}), ходов: {newMoves.Count}");
                                _selectedPiece.HighlightPossibleMoves(_board, true);
                            }
                        }
                    }
                }
                else
                {
                    // Проверяем клик по шашке
                    PieceView piece = hitObject.GetComponent<PieceView>();
                    if (piece != null && piece.Owner == _currentPlayer)
                    {
                        SelectPiece(piece);
                    }
                }
            }
        }

        private void SelectPiece(PieceView piece)
        {
            if (_selectedPiece != null)
            {
                DeselectPiece();
            }

            _selectedPiece = piece;
            // Подсвечиваем возможные ходы
            var moves = _selectedPiece.GetPossibleMoves(_board);
            Debug.Log($"[GameController] Выбрана шашка на ({piece.Row}, {piece.Col}), ходов: {moves.Count}");
            _selectedPiece.HighlightPossibleMoves(_board, true);
        }

        private void DeselectPiece()
        {
            if (_selectedPiece != null)
            {
                // Убираем подсветку
                _selectedPiece.HighlightPossibleMoves(_board, false);
                _selectedPiece = null;
            }
        }
    }
}