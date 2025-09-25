using System.Collections.Generic;
using UnityEngine;

namespace Shashki
{
    public class EasyBotStrategy : IBotStrategy
    {
        public (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder)
        {
            var allPossibleMoves = new List<(Move move, PieceView piece) >();

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
    }
}