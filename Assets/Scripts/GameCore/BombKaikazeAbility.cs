using System.Collections.Generic;
using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "BombKaikazeAbility", menuName = "Shashki/Abilities/BombKaikaze", order = 1)]
    public class BombKaikazeAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            if (piece == null)
            {
                Debug.LogWarning("[BombKaikazeAbility] Нельзя применить: шашка не выбрана");
                return;
            }

            piece.SetAbility(this);
            piece.SetBomb();
            manager.SetBombPiece(piece); // Назначаем бомбу в менеджер для взрыва в конце хода
            Debug.Log($"[BombKaikazeAbility] Шашка ({piece.Row}, {piece.Col}) помечена как бомба-каикадзе");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            if (piece == null || board == null || pieceHolder == null)
            {
                Debug.LogWarning("[BombKaikazeAbility] Нельзя выполнить взрыв: шашка, доска или держатель отсутствуют");
                return;
            }

            int centerRow = piece.Row;
            int centerCol = piece.Col;
            Debug.Log($"[BombKaikazeAbility] Взрыв шашки на ({centerRow}, {centerCol})");

            // Зона 3x3: от (Row-1, Col-1) до (Row+1, Col+1)
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int targetRow = centerRow + dr;
                    int targetCol = centerCol + dc;

                    if (!board.IsInside(targetRow, targetCol))
                        continue; // Пропускаем клетки за пределами доски

                    var targetPiece = board.GetPieceAt(targetRow, targetCol);
                    if (targetPiece != null && !targetPiece.IsShielded)
                    {
                        pieceHolder.PieceDestory(targetPiece);
                        Debug.Log($"[BombKaikazeAbility] Уничтожена шашка на ({targetRow}, {targetCol})");
                    }
                }
            }

            // Уничтожаем саму шашку-бомбу
            pieceHolder.GetPieces().Remove((centerRow, centerCol));
            board.UnregisterPiece(centerRow, centerCol);
            Debug.Log($"[BombKaikazeAbility] Шашка-бомба на ({centerRow}, {centerCol}) уничтожена");
        }
        
        public void OnValidate()
        {
            _id = AbilityType.BombKamikaze;
        }
    }
    
}