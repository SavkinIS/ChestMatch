using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shashki
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D _collider;
        [SerializeField] private int _row;
        [SerializeField] private int _col;
        [SerializeField] private PieceOwner _owner;
        [SerializeField] private Color _color;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private bool _isKing;
        [SerializeField] private AbilityBase _ability;
        [SerializeField] private bool _isShielded; // Новое поле: флаг щита
        [SerializeField] private ParticleSystem _highlightEffectAuraBlue;
        [SerializeField] private ParticleSystem _highlightEffectAuraRed;
       
        [SerializeField] private ParticleSystem _highlightEffect;

        //[SerializeField] private Material _highlightMat;
        [SerializeField] private Color _blockedColor;

        [Space] [Header("Destroy")] 
        [SerializeField] private ParticleSystem _destroyEffectBlue;
        [SerializeField] private ParticleSystem _destroyEffectRed;
        
        [Space]
        [Header("Ability")]
        [SerializeField] private List<SpriteEffectHolder> _abilityIcons;
        [SerializeField] private SpriteEffectHolder _abilityKing;
        
        
        private ParticleSystem _destroyEffect;
        private bool _isFrozen;
        private bool _isTempKing;
        private bool AbilityIsShow
            => _isBomb || _isFrozen || _isTempKing || _isShielded;
        private Dictionary<AbilityType, SpriteEffectHolder> _abilitySpriteDic;
        private bool _isBomb;

        public int Row => _row;
        public int Col => _col;
        public PieceOwner Owner => _owner;
        public bool IsKing => _isKing || _isTempKing;
        public bool IsFrozen => _isFrozen;
        public AbilityBase Ability => _ability;
        public bool IsShielded => _isShielded; // Геттер для флага щита
        public bool IsTotalKing => _isKing;

        private void Awake()
        {
            _highlightEffectAuraBlue.gameObject.SetActive(false);
            _highlightEffectAuraRed.gameObject.SetActive(false);

            _highlightEffect.Stop();
            _highlightEffect.gameObject.SetActive(false);

            _destroyEffect = _owner == PieceOwner.Player ? _destroyEffectBlue : _destroyEffectRed;
            _destroyEffect.gameObject.SetActive(false);
            _destroyEffectRed.gameObject.SetActive(false);
            _destroyEffectBlue.gameObject.SetActive(false);
            
            _abilitySpriteDic = _abilityIcons.ToDictionary(icon => icon.AbilityType);
            _abilityKing.gameObject.SetActive(false);
        }

        public void SetData(int row, int col, PieceOwner owner, Color color, Color blocked, Material highlightMat)
        {
            _row = row;
            _col = col;
            _owner = owner;
            _color = color;
            _isShielded = false; // Инициализируем щит как выключенный
            _isFrozen = false;
            _isTempKing = false;

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            if (_renderer != null)
            {
                _renderer.color = _color;
            }

            _highlightEffect = _owner == PieceOwner.Player ? _highlightEffectAuraBlue : _highlightEffectAuraRed;
            _destroyEffect = _owner == PieceOwner.Player ? _destroyEffectBlue : _destroyEffectRed;
            _blockedColor = blocked;
            //_highlightMat = highlightMat;

            // ParticleSystemRenderer psRenderer = _highlightEffectAura.GetComponent<ParticleSystemRenderer>();
            //
            // if (psRenderer != null && _highlightMat != null)
            // {
            //     psRenderer.material = _highlightMat; // Установка материала
            // }
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
            CheckIsKing();
            // TODO: визуалка (например, смена спрайта или добавление короны)
        }

        public void SetAbility(AbilityBase ability)
        {
            _ability = ability;
            Debug.Log($"[PieceView] Шашке ({_row}, {_col}) присвоена способность {ability?.DisplayName}");
           
            // TODO: визуалка способности (например, иконка или цвет)
        }

        private void ActivateSprite(AbilityType abilityType)
        {
            if (_abilitySpriteDic.TryGetValue(abilityType, out var spriteEffectHolder))
                spriteEffectHolder.gameObject.SetActive(true);

            CheckIsKing();
        }

        public void ExecuteAbility(BoardRoot board, PieceHolder pieceHolder)
        {
            if (_ability != null)
            {
                _ability.Execute(this, board, pieceHolder);
                _ability = null; // Очищаем способность после выполнения
                
            }
        }

        private void CheckIsKing()
        {
            if (_isKing && !AbilityIsShow)
            {
                _abilityKing.gameObject.SetActive(true);
            }
        }

        public List<Move> GetPossibleMoves(BoardRoot board, bool ignoreFrozen = false)
        {
            if (!ignoreFrozen)
            {
                if  (_isFrozen)
                    return new List<Move>();
            }
                
            List<Move> moves = new List<Move>();
            List<Move> captureMoves = new List<Move>();

            // Собираем ходы с поеданием (включая цепочки)
            AddCaptureChainMoves(board, Row, Col, new List<PieceView>(), captureMoves);

            if (captureMoves.Count > 0)
            {
                Debug.Log(
                    $"[PieceView] Найдено {captureMoves.Count} ходов с поеданием для шашки ({Row}, {Col}): {string.Join(", ", captureMoves.Select(m => $"({m.To.Row}, {m.To.Col}) с поеданием {string.Join(", ", m.CapturedPieces.Select(p => $"({p.Row}, {p.Col})"))}"))}");

                if (captureMoves.Count > 0)
                    SetBaseColor();
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

            Debug.Log(
                $"[PieceView] Найдено {moves.Count} обычных ходов для шашки ({Row}, {Col}): {string.Join(", ", moves.Select(m => $"({m.To.Row}, {m.To.Col})"))}");

            if (moves.Count > 0)
                SetBaseColor();
            else
                SetBlocked();

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

        private void AddCaptureChainMoves(BoardRoot board, int currentRow, int currentCol, List<PieceView> captured,
            List<Move> moves)
        {
            int[] dirs = { -1, 1 };
            foreach (int dr in dirs)
            {
                foreach (int dc in dirs)
                {
                    int midRow = currentRow + dr;
                    int midCol = currentCol + dc;
                    int landRow = currentRow + dr * 2;
                    int landCol = currentCol + dc * 2;

                    if (board.IsInside(midRow, midCol) && board.IsInside(landRow, landCol))
                    {
                        var midCell = board.GetCell(midRow, midCol);
                        var landCell = board.GetCell(landRow, landCol);
                        var midPiece = board.GetPieceAt(midRow, midCol);

                        bool canCapture = midCell != null && landCell != null && midPiece != null &&
                                          midPiece.Owner != Owner &&
                                          board.GetPieceAt(landRow, landCol) == null &&
                                          !captured.Contains(midPiece) &&
                                          !midPiece.IsShielded; // НОВОЕ: игнорируем защищённые шашки

                        if (canCapture)
                        {
                            var newCaptured = new List<PieceView>(captured) { midPiece };
                            var move = new Move
                            {
                                From = board.GetCell(currentRow, currentCol),
                                To = landCell,
                                IsCapture = true,
                                CapturedPieces = new List<PieceView>(newCaptured)
                            };
                            moves.Add(move);
                            Debug.Log(
                                $"[PieceView] Добавлен ход с поеданием: ({currentRow}, {currentCol}) -> ({landRow}, {landCol}), съедена шашка на ({midRow}, {midCol})");

                            board.UnregisterPiece(midRow, midCol);
                            AddCaptureChainMoves(board, landRow, landCol, newCaptured, moves);
                            board.RegisterPiece(midPiece, midRow, midCol);
                        }
                    }
                    else
                    {
                        Debug.Log(
                            $"[PieceView] Поедание с ({currentRow}, {currentCol}) в направлении ({dr}, {dc}) невозможно: вне доски");
                    }

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

                            bool canCapture = midCell != null && landCell != null && midPiece != null &&
                                              midPiece.Owner != Owner &&
                                              board.GetPieceAt(landRow, landCol) == null &&
                                              !captured.Contains(midPiece) &&
                                              !midPiece.IsShielded; // НОВОЕ: игнорируем защищённые шашки

                            if (canCapture && (midRow == 0 || midRow == board.Rows - 1 || midCol == 0 ||
                                               midCol == board.Cols - 1))
                            {
                                Debug.Log(
                                    $"[PieceView] Поедание шашки на ({midRow}, {midCol}) запрещено (на границе доски)");
                                canCapture = false;
                            }

                            if (canCapture)
                            {
                                var newCaptured = new List<PieceView>(captured) { midPiece };
                                var move = new Move
                                {
                                    From = board.GetCell(currentRow, currentCol),
                                    To = landCell,
                                    IsCapture = true,
                                    CapturedPieces = new List<PieceView>(newCaptured)
                                };
                                moves.Add(move);
                                Debug.Log(
                                    $"[PieceView] Добавлен ход дамки с поеданием: ({currentRow}, {currentCol}) -> ({landRow}, {landCol}), съедена шашка на ({midRow}, {midCol})");

                                board.UnregisterPiece(midRow, midCol);
                                AddCaptureChainMoves(board, landRow, landCol, newCaptured, moves);
                                board.RegisterPiece(midPiece, midRow, midCol);
                                break;
                            }

                            if (midPiece != null) break;
                        }
                    }
                }
            }
        }

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

        public void SetHighlight()
        {
            _highlightEffect.gameObject.SetActive(true);
            _highlightEffect.Play(true);
        }

        public void SetBlocked()
        {
            if (_renderer != null)
            {
                _renderer.color = _blockedColor;
            }
        }

        public void SetBaseColor()
        {
            _highlightEffect.Stop();
            _highlightEffect.gameObject.SetActive(false);
            if (_renderer != null)
            {
                _renderer.color = _color;
            }
        }

        public void DestroyPiece()
        {
            _destroyEffect.gameObject.SetActive(true);
            _destroyEffect.Play();
            _renderer.enabled = false;
            _collider.enabled = false;
            ResetAbility(AbilityType.BombKamikaze);
            StartCoroutine(DestroyAfterEffect());
        }

        private IEnumerator DestroyAfterEffect()
        {
            yield return new WaitForSeconds(1f);
            _destroyEffect.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void SetFrozen(bool state)
        {
            _isFrozen = state;
            Debug.Log($"[PieceView] Шашка {_row}, {_col} заморожена: {_isFrozen}");
            if (_isFrozen)
            {
                SetBlocked();
                ActivateSprite(AbilityType.Freeze);
            }
            else
            {
                SetBaseColor();
                _ability = null;
                ResetAbility(AbilityType.Freeze);
            }
        }

        public void SetTempKing(bool isTempKing)
        {
            _isTempKing = isTempKing;
            if (isTempKing)
            {
                ActivateSprite(AbilityType.TempKing);
            }
            else
            {
                _ability = null;
                ResetAbility(AbilityType.TempKing);
            }
        }
        
        private void ResetAbility(AbilityType type)
        {
            if (_abilitySpriteDic.TryGetValue(type, out var spriteEffectHolder))
                spriteEffectHolder.gameObject.SetActive(false);
            CheckIsKing();
        }

        public void SetBomb()
        {
           _isBomb = true;
           ActivateSprite(AbilityType.BombKamikaze);
        }
        
        
        public void SetShield(bool isShielded)
        {
            _isShielded = isShielded;
            if (_isShielded)
            {
                Debug.Log($"[PieceView] Щит для шашки ({_row}, {_col}): {_isShielded}");
                ActivateSprite(AbilityType.Shield);
            }
            else
            {
                _ability = null;
                ResetAbility(AbilityType.Shield);
            }
        }

    }
}