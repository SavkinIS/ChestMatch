using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardRoot _board;
        [SerializeField] private PieceHolder _pieceHolder;
        [SerializeField] private PowerUpManager _powerUpManager;
        [SerializeField] private LayerMask _piecesLayer;
        [SerializeField] private LayerMask _cellsLayer;
        
        private PieceView _selectedPiece;
        private PieceOwner _currentPlayer = PieceOwner.Player;
        private bool _isSelectingAbilityPiece;

        private void Update()
        {
            HandleInput();
        }

        public void SetAbilitySelectionMode(bool isSelecting)
        {
            _isSelectingAbilityPiece = isSelecting;
            if (isSelecting)
            {
                HighlightPlayerPieces(true);
            }
            else
            {
                HighlightPlayerPieces(false);
                // Не вызываем DeselectPiece(), чтобы не сбросить выбор шашки, если она уже выбрана
            }
        }

        private void HighlightPlayerPieces(bool highlight)
        {
            var pieces = _pieceHolder.GetPieces().Values.Where(p => p.Owner == _currentPlayer);
            foreach (var piece in pieces)
            {
                if (highlight)
                {
                    piece.SetHighlight();
                }
                else
                {
                    piece.SetBaseColor();
                    piece.SetData(piece.Row, piece.Col, piece.Owner);
                }
            }
        }

        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                Debug.Log($"[GameController] Клик по: {mousePos}");

                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero, 20f);
                if (hits.Length == 0)
                {
                    Debug.Log("[GameController] Raycast не попал");
                    return;
                }

                foreach (var h in hits)
                {
                    Debug.Log($"[GameController] Попал по: {h.collider.gameObject.name}, слой: {LayerMask.LayerToName(h.collider.gameObject.layer)}");
                }

                // Выбираем первое попадание по шашке или клетке
                GameObject hitObject = hits.FirstOrDefault(h => h.collider != null).collider.gameObject;

                // Режим выбора шашки для способности
                if (_isSelectingAbilityPiece)
                {
                    PieceView piece = hitObject.GetComponent<PieceView>();
                    if (piece != null && piece.Owner == _currentPlayer)
                    {
                        _powerUpManager.ApplyToPiece(piece);
                        SetAbilitySelectionMode(false); // Выключаем режим выбора
                        Debug.Log($"[GameController] Способность применена к шашке ({piece.Row}, {piece.Col}), выбор шашки для хода не произведён");
                    }
                    else
                    {
                        Debug.Log($"[GameController] Клик в режиме способности игнорируется: не попали по шашке игрока (hit: {hitObject.name})");
                    }
                    return;
                }

                // Если шашка уже выбрана, проверяем клик по клетке для хода
                if (_selectedPiece != null)
                {
                    BoardCell targetCell = hitObject.GetComponent<BoardCell>();
                    if (targetCell != null && _selectedPiece.Owner == _currentPlayer)
                    {
                        Debug.Log($"[GameController] Пытаемся сделать ход шашкой ({_selectedPiece.Row}, {_selectedPiece.Col}) в клетку ({targetCell.Row}, {targetCell.Col})");
                        if (_pieceHolder.TryMove(_selectedPiece, targetCell, out bool continueCapturing))
                        {
                            var newMoves = _selectedPiece.GetPossibleMoves(_board);
                            var count = newMoves.Where(m => m.IsCapture).Count();
                            Debug.Log($"[GameController] Новые ходы для шашки ({_selectedPiece.Row}, {_selectedPiece.Col}): {newMoves.Count}, поедания: {count}");

                            if (!continueCapturing || !newMoves.Exists(m => m.IsCapture))
                            {
                                _currentPlayer = (_currentPlayer == PieceOwner.Player) ? PieceOwner.Opponent : PieceOwner.Player;
                                Debug.Log($"[GameController] Ход сделан! Теперь ходит: {_currentPlayer}");
                                _powerUpManager.ExecuteBombExplosion(_board, _pieceHolder); // Взрыв в конце хода
                                DeselectPiece();
                            }
                            else
                            {
                                _selectedPiece.HighlightPossibleMoves(_board, false);
                                Debug.Log($"[GameController] Продолжаем поедание для шашки на ({_selectedPiece.Row}, {_selectedPiece.Col}), ходов: {newMoves.Count}");
                                _selectedPiece.HighlightPossibleMoves(_board, true);
                            }
                        }
                        else
                        {
                            Debug.Log($"[GameController] Ход шашкой ({_selectedPiece.Row}, {_selectedPiece.Col}) в ({targetCell.Row}, {targetCell.Col}) не удался");
                        }
                    }
                    else
                    {
                        Debug.Log($"[GameController] Клик по {hitObject.name} игнорируется: не клетка или шашка не принадлежит игроку");
                    }
                    return;
                }

                // Проверяем клик по шашке
                PieceView clickedPiece = hitObject.GetComponent<PieceView>();
                if (clickedPiece != null && clickedPiece.Owner == _currentPlayer)
                {
                    SelectPiece(clickedPiece);
                }
                else
                {
                    Debug.Log($"[GameController] Клик по {hitObject.name} игнорируется: не шашка или не принадлежит текущему игроку ({_currentPlayer})");
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
            var moves = _selectedPiece.GetPossibleMoves(_board);
            Debug.Log($"[GameController] Выбрана шашка на ({piece.Row}, {piece.Col}), ходов: {moves.Count}");
            _selectedPiece.HighlightPossibleMoves(_board, true);
        }

        private void DeselectPiece()
        {
            if (_selectedPiece != null)
            {
                _selectedPiece.HighlightPossibleMoves(_board, false);
                _selectedPiece = null;
            }
        }
    }
}