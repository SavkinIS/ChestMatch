using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shashki
{
    public class EasyBotStrategy : IBotStrategy
    {
        private readonly PieceOwner _botOwner = PieceOwner.Opponent;
        
        public (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder, PowerUpManager powerUpManager)
        {
            var allPossibleMoves = new List<(Move move, PieceView piece) >();

            if (Random.value < 0.4f)
            {
                TryUseBestAbility(powerUpManager, pieceHolder, board);
            }
            
            foreach (var piece in pieceHolder.GetPieces().Values)
            {
                if (piece.Owner == PieceOwner.Opponent)
                {
                    var moves = piece.GetPossibleMoves(board);
                    foreach (var move in moves)
                    {
                        allPossibleMoves.Add((move, piece));
                    }

                }
            }

            if (allPossibleMoves.Count > 0)
            {
                int randomMove = Random.Range(0, allPossibleMoves.Count);
                return allPossibleMoves[randomMove];
            }
            
            return (null, null);
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
    }
}