using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public enum GameState
    {
        NormalTurn,
        SelectingAbilityPiece,
        SelectingSwapFirstPiece,
        SelectingSwapSecondPiece
    }
    
    public enum Winner
    {
        Player,
        Opponent,
    }

    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardRoot _board;
        [SerializeField] private PieceHolder _pieceHolder;
        [SerializeField] private PowerUpManager _powerUpManager;
        [SerializeField] private LayerMask _piecesLayer;
        [SerializeField] private LayerMask _cellsLayer;
        [SerializeField] private float _turnDelay = 600f;
        [SerializeField] private TimerProgress _timerProgress;
        [SerializeField] private TMPro.TextMeshProUGUI _currentPlayerText;
        [SerializeField] private EndGamePanel _endGamePanel;
        [SerializeField] private TurnTransitionPanel _transitionPanel; 
        [SerializeField] private GameObject _gamePanel;
        
        private Dictionary<PieceOwner, int> _playersPoints = new Dictionary<PieceOwner, int>();
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
            _currentPlayerText.text = $"Текущий игрок {_currentPlayer}";
            _endGamePanel.ClosePanel();
            _gamePanel.SetActive(true);

            _playerSkipMovesDic = new Dictionary<PieceOwner, int>()
            {
                { PieceOwner.Player, 0 },
                { PieceOwner.Opponent, 0 },
            };
            _playersPoints = new Dictionary<PieceOwner, int>()
            {
                { PieceOwner.Player, 0 },
                { PieceOwner.Opponent, 0 },
            };
        }

        private void Update()
        {
            if (_isGameOver) return; // Останавливаем ввод и таймер при окончании игры
            
            if (!_canTick) return;
                
            _turnTime -= Time.deltaTime;
            if (_turnTime < 0)
            {
               
                _playerSkipMovesDic[_currentPlayer]++;
                if (_playerSkipMovesDic[_currentPlayer] >= 2)
                {
                    // Игрок пропустил 2 хода подряд — проигрыш
                    EndGame(_currentPlayer == PieceOwner.Player ? Winner.Opponent : Winner.Player);
                    return;
                }
                EndTurn();
            }
            else
            {
                _timerProgress.SetProgress(_turnTime / _turnDelay);
                _timerProgress.SetTimeTxt(_turnTime);
            }
            
            HandleInput();
        }

        private void EndTurn()
        {
            
            _currentPlayer = (_currentPlayer == PieceOwner.Player) ? PieceOwner.Opponent : PieceOwner.Player;
            _board.OnTurnEnd();
            _timerProgress.SetTimeTxt(-1);
            _timerProgress.SetOwner(_currentPlayer == PieceOwner.Player);
            _timerProgress.ResetProgress();
            _turnTime = _turnDelay;
            _selectedPiece = null;
            OnTurnEnd?.Invoke();
            _canTick = false;
            _currentPlayerText.text = $"Текущий игрок {_currentPlayer}";
            StartCoroutine(SwitchTurn());
            
            CheckGameOver(); // Проверяем условия после смены хода
        }

        private void CheckGameOver()
        {
            // Получаем шашки для обоих игроков
            var playerPieces = _pieceHolder.GetPieces().Values.Where(p => p.Owner == PieceOwner.Player).ToList();
            var opponentPieces = _pieceHolder.GetPieces().Values.Where(p => p.Owner == PieceOwner.Opponent).ToList();

            // Все шашки съедены
            if (playerPieces.Count == 0)
            {
                EndGame(Winner.Opponent);
                return;
            }
            if (opponentPieces.Count == 0)
            {
                EndGame(Winner.Player);
                return;
            }

            // Проверяем на отсутствие ходов (уже обрабатывается в OnPlayerChanged, но здесь подтвердим)
            bool playerHasMoves = false;

            for (int i = 0; i < playerPieces.Count; i++)
            {
                if (playerPieces[i].GetPossibleMoves(_board, true).Count > 0)
                    playerHasMoves = true;
            }
            
            bool opponentHasMoves = false;
                
            for (int i = 0; i < opponentPieces.Count; i++)
            {
                if (opponentPieces[i].GetPossibleMoves(_board, true).Count > 0)
                    opponentHasMoves = true;
            }
            
            if (!playerHasMoves && playerPieces.Count > 0)
            {
                // Уничтожаем шашки, если заблокированы (как в исходном коде), и проверяем заново
                Debug.Log("[GameController] Игрок заблокирован — уничтожаем шашки");
                foreach (var p in playerPieces)
                {
                    _pieceHolder.PieceDestory(p);
                }
                CheckGameOver(); // Рекурсивно проверим после уничтожения
                return;
            }
            if (!opponentHasMoves && opponentPieces.Count > 0)
            {
                Debug.Log("[GameController] Оппонент заблокирован — уничтожаем шашки");
                foreach (var p in opponentPieces)
                {
                    _pieceHolder.PieceDestory(p);
                }
                CheckGameOver();
                return;
            }
        }

        private void EndGame(Winner winner)
        {
            _isGameOver = true;
            _canTick = false; // Останавливаем таймер
            Debug.Log($"[GameController] Игра окончена! Победитель: {winner}");
            OnGameOver?.Invoke(winner);
            _endGamePanel.Activate(winner);
            _gamePanel.SetActive(false);
            if (_transitionPanel != null)
                _transitionPanel.Hide(); // Скрываем панель при окончании игры
        }

        // Метод для сдачи (вызвать из UI кнопки)
        public void Surrender(PieceOwner owner)
        {
            if (_isGameOver) return;
            EndGame(owner == PieceOwner.Player ? Winner.Opponent : Winner.Player);
        }
        
        private IEnumerator SwitchTurn()
        {
            if (_transitionPanel != null)
                _transitionPanel.Show();
            yield return new WaitForSeconds(2);
            if (_transitionPanel != null)
                _transitionPanel.Hide();
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
                HighlightOpponentPieces(false);
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

                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero, 100f);
                if (hits.Length == 0)
                {
                    Debug.Log("[GameController] Raycast не попал");
                    return;
                }

                foreach (var h in hits)
                {
                    Debug.Log($"[GameController] Попал по: {h.collider.gameObject.name}, слой: {LayerMask.LayerToName(h.collider.gameObject.layer)}");
                }

                GameObject hitObject = hits.FirstOrDefault(h => h.collider != null).collider.gameObject;

                PieceView clickedPiece = hitObject.GetComponent<PieceView>();
                BoardCell targetCell = hitObject.GetComponent<BoardCell>();

                switch (_currentState)
                {
                    case GameState.SelectingAbilityPiece:
                        if (clickedPiece != null )
                        {
                            var ability = _powerUpManager.GetCurrentAbility();
                            if ((ability == AbilityType.Freeze &&
                                 clickedPiece.Owner != _currentPlayer) 
                                || (ability != AbilityType.Freeze &&
                                    clickedPiece.Owner == _currentPlayer))
                            {
                                _powerUpManager.ApplyToPiece(clickedPiece);
                                SetAbilitySelectionMode(false);
                                Debug.Log($"[GameController] Способность {ability} применена к шашке ({clickedPiece.Row}, {clickedPiece.Col})");
                            }
                        }
                        break;

                    case GameState.SelectingSwapFirstPiece:
                        if (clickedPiece != null && clickedPiece.Owner == _currentPlayer)
                        {
                            _firstSwapPiece = clickedPiece;
                            _powerUpManager.ApplyToPiece(clickedPiece);
                            _currentState = GameState.SelectingSwapSecondPiece;
                            HighlightOpponentPieces(true);
                            Debug.Log($"[GameController] Выбрана своя шашка для обмена ({clickedPiece.Row}, {clickedPiece.Col}). Теперь выберите соседнюю чужую.");
                        }
                        break;

                    case GameState.SelectingSwapSecondPiece:
                        if (clickedPiece != null && clickedPiece.Owner != _currentPlayer && IsAdjacent(_firstSwapPiece, clickedPiece))
                        {
                            var swapAbility = _powerUpManager.GetAbilityInstance(AbilityType.SwapSides) as SwapSidesAbility;
                            if (swapAbility != null)
                            {
                                swapAbility.PerformSwap(_firstSwapPiece, clickedPiece, _board, _pieceHolder);
                                _powerUpManager.ConsumeAbility(_currentPlayer, AbilityType.SwapSides);
                                CheckForKingPromotion(_firstSwapPiece);
                                CheckForKingPromotion(clickedPiece);
                                SelectPiece(_firstSwapPiece);
                            }
                            SetAbilitySelectionMode(false);
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
                            if (clickedPiece != null && clickedPiece.Owner == _currentPlayer && _selectedPiece != clickedPiece)
                            {
                                SelectPiece(clickedPiece);
                                break;
                            }
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
            if (!piece.IsTotalKing && 
                ((piece.Owner == PieceOwner.Player && piece.Row == 0) ||
                 (piece.Owner == PieceOwner.Opponent && piece.Row == _board.Rows - 1)))
            {
                piece.PromoteToKing();
                Debug.Log($"[GameController] Шашка ({piece.Row}, {piece.Col}) стала дамкой после обмена!");
            }
        }

        private void OnPlayerChanged()
        {
            var allPieces = _pieceHolder.GetPieces().Values.ToList();
            var pieces = allPieces.Where(p => p.Owner == _currentPlayer).ToList();
            
            bool hasMove = false;

            for (var index = 0; index < allPieces.Count; index++)
            {
                var p = allPieces[index];
                
                p.SetTempKing(false);

                if (p.Owner != _currentPlayer)
                {
                    p.SetFrozen(false);
                }
                else 
                {
                    p.SetShield(false);
                }
                  
                var moves = p.GetPossibleMoves(_board, true);
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
            
            CheckGameOver(); // Проверяем после возможного уничтожения шашек
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
            if (_transitionPanel == null)
                _transitionPanel = FindFirstObjectByType<TurnTransitionPanel>();
        }
#endif
    }
}