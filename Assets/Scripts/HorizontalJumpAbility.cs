using UnityEngine;

namespace Shashki
{
    [CreateAssetMenu(fileName = "HorizontalJumpAbility", menuName = "Shashki/Abilities/HorizontalJump", order = 1)]
    public class HorizontalJumpAbility : AbilityBase
    {
        public override void Apply(PieceView piece, PowerUpManager manager)
        {
            piece.SetAbility(this);
            // Переходим в состояние выбора цели (добавь enum GameState.SelectingJumpTarget)
            Debug.Log($"[HorizontalJumpAbility] Шашка ({piece.Row}, {piece.Col}) готова к скачку. Выберите горизонтальную цель.");
        }

        public override void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // Выполняется при выборе цели
        }

        public void PerformJump(PieceView myPiece, BoardCell target, BoardRoot board, PieceHolder pieceHolder)
        {
            if (!IsHorizontalAdjacent(myPiece, target, board)) return;

            board.MovePieceInMap(myPiece, myPiece.Row, myPiece.Col, target.Row, target.Col);
            myPiece.SetDataAfterMove(target.Row, target.Col);
            myPiece.transform.position = target.transform.position + Vector3.back * 0.01f;
            myPiece.SetAbility(null);

            Debug.Log($"[HorizontalJumpAbility] Скачок выполнен на ({target.Row}, {target.Col})");
        }

        public bool IsHorizontalAdjacent(PieceView piece, BoardCell target, BoardRoot board)
        {
            return target.Row == piece.Row && Mathf.Abs(target.Col - piece.Col) == 1 && board.GetPieceAt(target.Row, target.Col) == null;
        }
        
        public void OnValidate()
        {
            _id = AbilityType.HorizontalJump;
        }
    }
}