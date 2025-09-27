using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Shashki
{
    public class BotPlayer
    {
        private readonly IBotStrategy _strategy;
        private readonly GameCore _gameCore;
        private readonly BoardRoot _board;
        private readonly PieceHolder _pieceHolder;
        private readonly float _maxTimer;
        private Action _moveCompleteAction;


        public BotPlayer(IBotStrategy strategy, GameCore gameCore, BoardRoot board,
            PieceHolder pieceHolder, float maxTimer, Action moveCompleteAction)
        {
            _strategy = strategy;
            _gameCore = gameCore;
            _board = board;
            _pieceHolder = pieceHolder;
            _maxTimer = maxTimer;
            _moveCompleteAction = moveCompleteAction;
        }

        public IEnumerator MakeMoveCoroutine()
        {
            float seconds = Random.Range(0.5f, 2f);
            yield return new WaitForSeconds(seconds);
            
            (Move move, PieceView piece) chooseMove = _strategy.ChooseMove(_board, _pieceHolder);

            if (chooseMove.move != null)
            {
                var to = chooseMove.move.To;
                var cell = _board.GetCell(to.X, to.Y);
                _pieceHolder.TryMove(chooseMove.piece, cell, out bool continueCapturing);
                _moveCompleteAction?.Invoke();
            }
        }
    }
}