using System.Collections.Generic;
using UnityEngine;
using Zenject.SpaceFighter;

namespace Shashki
{
    public class MediumBotStrategy : IBotStrategy
    {
        public (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder)
        {
            var allPossibleMoves = new List<(Move move, PieceView piece) >();
            var captureMoves = new List<(Move move, PieceView piece)>();

            foreach (var piece in pieceHolder.GetPieces().Values)
            {
                if (piece.Owner == PieceOwner.Opponent)
                {
                    var moves = piece.GetPossibleMoves(board);
                    foreach (var move in moves)
                    {
                        allPossibleMoves.Add((move, piece));
                    }

                    for (int i = 0; i < moves.Count; i++)
                    {
                        if (moves[i].IsCapture)
                            captureMoves.Add((moves[i], piece));
                    }
                }
            }

            if (captureMoves.Count > 0)
            {
                int randomMove = Random.Range(0, captureMoves.Count);
                return captureMoves[randomMove];
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