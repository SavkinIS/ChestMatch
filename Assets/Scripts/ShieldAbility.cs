using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "ShieldAbility", menuName = "Shashki/Abilities/Shield", order = 1)]
    public class ShieldAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            if (piece == null)
            {
                Debug.LogWarning("[ShieldAbility] Нельзя применить: шашка не выбрана");
                return;
            }

            piece.SetAbility(this);
            piece.SetShield(true);  // Устанавливаем флаг защиты
            Debug.Log($"[ShieldAbility] Шашка ({piece.Row}, {piece.Col}) защищена щитом");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // Ничего не делаем — щит пассивный, защита от поедания обрабатывается в GetPossibleMoves
        }
        
        public void OnValidate()
        {
            _id = AbilityType.Shield;
        }
    }
}