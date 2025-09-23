using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "FreezeAbility", menuName = "Shashki/Abilities/Freeze", order = 1)]
    public class FreezeAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            if (piece == null || piece.Owner == manager.GameCore.Owner)  // Только соперник
            {
                Debug.LogWarning("[FreezeAbility] Нельзя заморозить свою шашку");
                return;
            }

            piece.SetFrozen(true);
            piece.SetAbility(this);
            Debug.Log($"[FreezeAbility] Шашка соперника ({piece.Row}, {piece.Col}) заморожена на 1 ход");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // Пассивный эффект
        }
        
        public void OnValidate()
        {
            _id = AbilityType.Freeze;
        }
    }
}