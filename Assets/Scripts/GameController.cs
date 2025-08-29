using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Shashki
{
    public enum GameState
    {
        NormalTurn,          // Обычный ход шашками
        SelectingAbilityPiece, // Выбор шашки для способности (общий)
        SelectingSwapFirstPiece,  // Выбор своей шашки для SwapSides
        SelectingSwapSecondPiece  // Выбор чужой шашки для SwapSides
    }
    
    public enum Winner
    {
        Player,
        Opponent,
        Draw
    }

    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardRoot _board;
        [SerializeField] private PieceHolder _pieceHolder;
        [SerializeField] private PowerUpManager _powerUpManager;
        [SerializeField] private LayerMask _piecesLayer;
        [SerializeField] private LayerMask _cellsLayer;
        [SerializeField] private float _turnDelay = 15f;
        [SerializeField] private TimerProgress _timerProgress;
        [SerializeField] private TMPro.TextMeshProUGUI _currentPlayerText;
        
        
        private PieceView _selectedPiece;
        private PieceOwner _currentPlayer = PieceOwner.Player;
        private GameState _currentState = GameState.NormalTurn;
        private PieceView _firstSwapPiece; 
        
       
        
        private float _turnTime;
        private bool _canTick;
        private Dictionary<PieceOwner, int> _playerSkipMovesDic;
        private bool _isGameOver;
        public PieceOwner Owner => _currentPlayer;
        public event Action OnTurnEnd;
        public event Action<Winner> OnGameOver;

        private void Awake()
        {
            _turnTime = _turnDelay;
            OnTurnEnd += OnPlayerChanged;
            _canTick = true;
            _currentPlayerText.text = $"Текущий ишгрок {_currentPlayer}";

            _playerSkipMovesDic = new Dictionary<PieceOwner, int>()
            {
                { PieceOwner.Player, 0 },
                { PieceOwner.Opponent, 0 },
            };
        }

        private void Update()
        {
            if (!_canTick)
                return;
                
            _turnTime -= Time.deltaTime;
            if (_turnTime < 0)
            {
                _playerSkipMovesDic[_currentPlayer]++;
                EndTurn();
            }
            else
            {
                _timerProgress.SetProgress(_turnTime/_turnDelay);
            }
            
            HandleInput();
        }

        private void EndTurn()
        {
            _currentPlayer = (_currentPlayer == PieceOwner.Player) ? PieceOwner.Opponent : PieceOwner.Player;
            
            _timerProgress.SetOwner(_currentPlayer == PieceOwner.Player);
            _timerProgress.ResetProgress();
            _turnTime = _turnDelay;
            OnTurnEnd?.Invoke();
            _canTick = false;
            _currentPlayerText.text = $"Текущий ишгрок {_currentPlayer}";
            StartCoroutine(SwitchTurn());
        }

        private void EndGame(Winner winner)
        {
            _isGameOver = true;
            Debug.Log($"[GameController] Игра окончена! Победитель: {winner}");
            OnGameOver?.Invoke(winner);
        }
        
        private IEnumerator SwitchTurn()
        {
            yield return new WaitForSeconds(3);
            _canTick = true;
        }

        public void SetAbilitySelectionMode(bool isSelecting, AbilityType abilityType = AbilityType.None)
        {
            if (isSelecting)
            {
                if (abilityType == AbilityType.SwapSides)
                {
                    _currentState = GameState.SelectingSwapFirstPiece;
                }
                else
                {
                    _currentState = GameState.SelectingAbilityPiece;
                }
                HighlightPlayerPieces(true);
            }
            else
            {
                _currentState = GameState.NormalTurn;
                HighlightPlayerPieces(false);
                HighlightOpponentPieces(false); // Убираем подсветку чужих шашек
                _firstSwapPiece = null;
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

        private void HighlightOpponentPieces(bool highlight)
        {
            var opponent = _currentPlayer == PieceOwner.Player ? PieceOwner.Opponent : PieceOwner.Player;
            var pieces = _pieceHolder.GetPieces().Values.Where(p => p.Owner == opponent);
            foreach (var piece in pieces)
            {
                if (highlight && _firstSwapPiece != null && IsAdjacent(_firstSwapPiece, piece))
                {
                    piece.SetHighlight();
                }
                else
                {
                    piece.SetBaseColor();
                }
            }
        }

        private bool IsAdjacent(PieceView piece1, PieceView piece2)
        {
            int dr = Mathf.Abs(piece1.Row - piece2.Row);
            int dc = Mathf.Abs(piece1.Col - piece2.Col);
            return (dr == 1 && dc == 0) || (dr == 0 && dc == 1) || (dr == 1 && dc == 1); // Соседние клетки, включая диагонали
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

                PieceView clickedPiece = hitObject.GetComponent<PieceView>();
                BoardCell targetCell = hitObject.GetComponent<BoardCell>();

                switch (_currentState)
                {
                    case GameState.SelectingAbilityPiece:
                        if (clickedPiece != null && clickedPiece.Owner == _currentPlayer)
                        {
                            _powerUpManager.ApplyToPiece(clickedPiece);
                            SetAbilitySelectionMode(false);
                            Debug.Log($"[GameController] Способность применена к шашке ({clickedPiece.Row}, {clickedPiece.Col})");
                        }
                        break;

                    case GameState.SelectingSwapFirstPiece:
                        if (clickedPiece != null && clickedPiece.Owner == _currentPlayer)
                        {
                            _firstSwapPiece = clickedPiece;
                            _powerUpManager.ApplyToPiece(clickedPiece); // Применяем способность к первой шашке
                            _currentState = GameState.SelectingSwapSecondPiece;
                            HighlightOpponentPieces(true); // Подсвечиваем соседние чужие шашки
                            Debug.Log($"[GameController] Выбрана своя шашка для обмена ({clickedPiece.Row}, {clickedPiece.Col}). Теперь выберите соседнюю чужую.");
                        }
                        break;

                    case GameState.SelectingSwapSecondPiece:
                        if (clickedPiece != null && clickedPiece.Owner != _currentPlayer && IsAdjacent(_firstSwapPiece, clickedPiece))
                        {
                            // Находим способность SwapSides
                            var swapAbility = _powerUpManager.GetAbilityInstance(AbilityType.SwapSides) as SwapSidesAbility;
                            if (swapAbility != null)
                            {
                                swapAbility.PerformSwap(_firstSwapPiece, clickedPiece, _board, _pieceHolder);
                                _powerUpManager.ConsumeAbility(_currentPlayer, AbilityType.SwapSides); // Уменьшаем счетчик
                                // Проверяем, стала ли шашка дамкой
                                CheckForKingPromotion(_firstSwapPiece);
                                CheckForKingPromotion(clickedPiece);
                                // Выбираем свою шашку для продолжения хода
                                SelectPiece(_firstSwapPiece);
                            }
                            SetAbilitySelectionMode(false); // Возвращаемся в нормальный режим
                            Debug.Log($"[GameController] Обмен выполнен с чужой шашкой ({clickedPiece.Row}, {clickedPiece.Col})");
                        }
                        else
                        {
                            Debug.Log($"[GameController] Invalid selection for second piece.");
                        }
                        break;

                    case GameState.NormalTurn:
                        if (_selectedPiece != null)
                        {
                            if (targetCell != null && _selectedPiece.Owner == _currentPlayer)
                            {
                                if (_pieceHolder.TryMove(_selectedPiece, targetCell, out bool continueCapturing))
                                {
                                    var newMoves = _selectedPiece.GetPossibleMoves(_board);
                                    if (!continueCapturing || !newMoves.Exists(m => m.IsCapture))
                                    {
                                        _powerUpManager.ExecuteBombExplosion(_currentPlayer, _board, _pieceHolder);
                                        DeselectPiece();
                                        _playerSkipMovesDic[_currentPlayer] = 0;
                                        EndTurn();
                                    }
                                    else
                                    {
                                        _selectedPiece.HighlightPossibleMoves(_board, false);
                                        _selectedPiece.HighlightPossibleMoves(_board, true);
                                    }
                                }
                            }
                        }
                        else if (clickedPiece != null && clickedPiece.Owner == _currentPlayer)
                        {
                            SelectPiece(clickedPiece);
                        }
                        break;
                }
            }
        }

        private void CheckForKingPromotion(PieceView piece)
        {
            if (!piece.IsKing &&
                ((piece.Owner == PieceOwner.Player && piece.Row == 0) ||
                 (piece.Owner == PieceOwner.Opponent && piece.Row == _board.Rows - 1)))
            {
                piece.PromoteToKing();
                Debug.Log($"[GameController] Шашка ({piece.Row}, {piece.Col}) стала дамкой после обмена!");
            }
        }

        private void OnPlayerChanged()
        {
            var pieces = _pieceHolder.GetPieces().Values.Where(p => p.Owner == _currentPlayer).ToList();
            bool hasMove = false;

            foreach (var p in pieces)
            {
                var moves = p.GetPossibleMoves(_board);
                if (moves.Any())
                    hasMove = true;
            }

            if (!hasMove && pieces.Count > 0)
            {
                Debug.Log($"Оставшиеся шашки заблокированы {pieces.Count}");
                for (var index = 0; index < pieces.Count; index++)
                {
                    var p = pieces[index];
                    _pieceHolder.PieceDestory(p);
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_timerProgress == null)
                _timerProgress = FindFirstObjectByType<TimerProgress>();
        }


#endif
    }

}