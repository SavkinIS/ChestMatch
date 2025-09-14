using UnityEngine;

namespace Shashki
{
    public enum AbilityType
    {
        None,
        BombKamikaze, 
        SwapSides,
        Shield, 
        TempKing,      
        Freeze,         
        HorizontalJump 
    }

    public abstract class AbilityBase : ScriptableObject
    {
        protected AbilityType _id; // Уникальный ID способности
        [SerializeField] private string _displayName; // Название для UI
        [SerializeField] private Sprite _icon; // Иконка для UI кнопки
        [SerializeField] [TextArea] private string _description; // Описание способности

        public AbilityType Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public string Description => _description;

        // Вызывается, когда игрок применяет способность к шашке
        public abstract void Apply(PieceView piece, PowerUpManager manager);

        // Вызывается после хода, если способность активна (например, взрыв)
        public virtual void Execute(PieceView piece, BoardRoot board, PieceHolder pieceHolder)
        {
            // По умолчанию ничего не делает, переопределяется в дочерних классах
        }
    }
}