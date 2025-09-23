using System;
using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "SwapSidesAbility", menuName = "Shashki/Abilities/SwapSides", order = 1)]
    public class SwapSidesAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            // Устанавливаем способность на шашку (первую выбранную - свою)
            piece.SetAbility(this);
            Debug.Log($"[SwapSidesAbility] Способность применена к своей шашке ({piece.Row}, {piece.Col}). Теперь выберите соседнюю чужую шашку для обмена.");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // Этот метод не используется для SwapSides, так как обмен происходит сразу при выборе второй шашки
        }

        public void PerformSwap(PieceView myPiece, PieceView opponentPiece, BoardRoot board, PieceHolder pieceHolder)
        {
            if (myPiece == null || opponentPiece == null) return;

            // Сохраняем позиции
            int myRow = myPiece.Row;
            int myCol = myPiece.Col;
            int oppRow = opponentPiece.Row;
            int oppCol = opponentPiece.Col;

            // Обновляем карту в BoardRoot
            board.MovePieceInMap(myPiece, myRow, myCol, oppRow, oppCol);
            board.MovePieceInMap(opponentPiece, oppRow, oppCol, myRow, myCol);

            // Обновляем данные шашек
            myPiece.SetDataAfterMove(oppRow, oppCol);
            opponentPiece.SetDataAfterMove(myRow, myCol);

            // Перемещаем визуально
            myPiece.transform.position = board.GetCell(oppRow, oppCol).transform.position + Vector3.back * 0.01f;
            opponentPiece.transform.position = board.GetCell(myRow, myCol).transform.position + Vector3.back * 0.01f;

            // Очищаем способность
            myPiece.SetAbility(null);

            Debug.Log($"[SwapSidesAbility] Обмен выполнен: своя шашка с ({myRow}, {myCol}) на ({oppRow}, {oppCol}) с чужой.");
        }
        public void OnValidate()
        {
            _id = AbilityType.SwapSides;
        }
    }
}