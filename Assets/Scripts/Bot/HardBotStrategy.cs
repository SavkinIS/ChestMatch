using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class HardBotStrategy : IBotStrategy
    {
        private readonly HardBotStrategySettings _settings;
        private readonly PieceOwner _botOwner = PieceOwner.Opponent;

        public HardBotStrategy(HardBotStrategySettings settings)
        {
            _settings = settings;
        }

        public (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder, PowerUpManager powerUpManager)
        {
            // 1. Рассмотреть возможность использования способности
            if (_settings.UseAbilities && Random.value < _settings.AbilityUseChance)
            {
                if (TryUseBestAbility(powerUpManager, pieceHolder, board))
                {
                   
                }
            }

            // 2. Если способность не использована, найти лучший ход
            return FindBestMove(board, pieceHolder);
        }

        private bool TryUseBestAbility(PowerUpManager powerUpManager, PieceHolder pieceHolder, BoardRoot board)
        {
            // Приоритет способностей: Заморозка > Щит
            // Попытка использовать "Заморозку"
            if (powerUpManager.GetAbilityCount(_botOwner, AbilityType.Freeze) > 0)
            {
                var playerPieces = pieceHolder.GetPieces().Values
                    .Where(p => p.Owner == PieceOwner.Player && !p.IsFrozen && !p.IsShielded).ToList();

                if (playerPieces.Any())
                {
                    // Найти лучшую цель: дамка или шашка с наибольшим количеством ходов
                    PieceView target = playerPieces.OrderByDescending(p => p.IsKing)
                        .ThenByDescending(p => p.GetPossibleMoves(board).Count)
                        .FirstOrDefault();
                    if (target != null && powerUpManager.BotApplyAbility(AbilityType.Freeze, target))
                    {
                        Debug.Log($"[HardBot] Использовал Заморозку на шашку игрока ({target.Row}, {target.Col})");
                        return true;
                    }
                }
            }

            // Попытка использовать "Щит"
            if (powerUpManager.GetAbilityCount(_botOwner, AbilityType.Shield) > 0)
            {
                var myThreatenedPieces = GetThreatenedPieces(pieceHolder, _botOwner, board);
                if (myThreatenedPieces.Any())
                {
                    // Защитить самую ценную шашку под угрозой
                    PieceView target = myThreatenedPieces.OrderByDescending(p => p.IsKing).FirstOrDefault();
                    if (target != null && powerUpManager.BotApplyAbility(AbilityType.Shield, target))
                    {
                        Debug.Log($"[HardBot] Использовал Щит на свою шашку ({target.Row}, {target.Col})");
                        return true;
                    }
                }
            }

            return false;
        }

        private List<PieceView> GetThreatenedPieces(PieceHolder pieceHolder, PieceOwner owner, BoardRoot board)
        {
            var threatenedPieces = new List<PieceView>();
            var opponentPieces = pieceHolder.GetPieces().Values.Where(p => p.Owner != owner).ToList();

            foreach (var opponentPiece in opponentPieces)
            {
                var captureMoves = opponentPiece.GetPossibleMoves(board).Where(m => m.IsCapture);
                foreach (var move in captureMoves)
                {
                    threatenedPieces.AddRange(move.CapturedPieces);
                }
            }
            return threatenedPieces.Where(p => p.Owner == owner && !p.IsShielded).Distinct().ToList();
        }

        private (Move move, PieceView piece) FindBestMove(BoardRoot board, PieceHolder pieceHolder)
        {
            var allPossibleMoves = new List<(Move move, PieceView piece)>();
            var captureMoves = new List<(Move move, PieceView piece)>();

            foreach (var piece in pieceHolder.GetPieces().Values.Where(p => p.Owner == _botOwner))
            {
                var moves = piece.GetPossibleMoves(board);
                foreach (var move in moves)
                {
                    if (move.IsCapture) captureMoves.Add((move, piece));
                    allPossibleMoves.Add((move, piece));
                }
            }

            // Бот обязан бить, если есть такая возможность
            var movesToEvaluate = captureMoves.Any() ? captureMoves : allPossibleMoves;

            if (!movesToEvaluate.Any()) return (null, null);

            (Move move, PieceView piece) bestMove = default;
            float bestScore = float.MinValue;

            foreach (var moveTuple in movesToEvaluate)
            {
                float score = ScoreMove(moveTuple.move, moveTuple.piece, pieceHolder);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = moveTuple;
                }
            }
            return bestMove;
        }

        private float ScoreMove(Move move, PieceView piece, PieceHolder pieceHolder)
        {
            var currentPieces = pieceHolder.GetPieces().Values.ToList();
            if (move.IsCapture)
            {
                currentPieces = currentPieces.Except(move.CapturedPieces).ToList();
            }
            return EvaluateBoardState(currentPieces, piece, ((int)move.To.X, (int)move.To.Y));
        }

        private float EvaluateBoardState(List<PieceView> pieces, PieceView movedPiece, (int newRow, int newCol) newPos)
        {
            float score = 0;
            foreach (var p in pieces)
            {
                int pieceValue = p.IsKing ? _settings.EvalKingWeight : 1;
                score += (p.Owner == _botOwner) ? pieceValue : -pieceValue;

                if (p.Owner == _botOwner)
                {
                    int row = p == movedPiece ? newPos.newRow : p.Row;
                    int col = p == movedPiece ? newPos.newCol : p.Col;

                    // Контроль центра (для доски 8x8 центральные колонки 2,3,4,5)
                    if (col > 1 && col < 6) score += _settings.EvalCenterControlWeight * 0.1f;
                    // Продвижение (для бота чем больше индекс строки, тем лучше)
                    score += row * _settings.EvalAdvancementWeight * 0.1f;
                    // Безопасность (бонус за шашки на заднем ряду)
                    if (row == 0) score += _settings.EvalSafetyWeight * 0.1f;
                }
            }
            return score;
        }
    }
}