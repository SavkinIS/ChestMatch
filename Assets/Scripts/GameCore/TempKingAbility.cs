using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "TempKingAbility", menuName = "Shashki/Abilities/TempKing", order = 1)]
    public class TempKingAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            if (piece == null)
            {
                Debug.LogWarning("[TempKingAbility] Нельзя применить: шашка не выбрана");
                return;
            }

            piece.SetTempKing(true);  // Устанавливаем временный флаг
            Debug.Log($"[TempKingAbility] Шашка ({piece.Row}, {piece.Col}) стала дамкой на 1 ход");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // Не нужно — эффект уже в GetPossibleMoves
        }
        
        public void OnValidate()
        {
            _id = AbilityType.TempKing;
        }
    }
}