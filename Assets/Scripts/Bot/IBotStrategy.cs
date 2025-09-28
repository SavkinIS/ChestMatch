namespace Shashki
{
    public interface IBotStrategy
    {
        (Move move, PieceView piece) ChooseMove(BoardRoot board, PieceHolder pieceHolder, PowerUpManager powerUpManager);
    }
    
        public class HardBotStrategySettings
        {
            public bool UseAbilities { get; set; } = true;
            // Шанс (0.0 до 1.0), с которым бот попытается использовать способность в свой ход
            public float AbilityUseChance { get; set; } = 0.5f; 
            // Вес дамки при оценке (дамка в 3 раза ценнее обычной шашки)
            public int EvalKingWeight { get; set; } = 3; 
            // Вес за контроль центра доски
            public int EvalCenterControlWeight { get; set; } = 2;
            // Вес за продвижение шашек к вражеской базе
            public int EvalAdvancementWeight { get; set; } = 1;
            // Вес за безопасное расположение шашек (например, на заднем ряду)
            public int EvalSafetyWeight { get; set; } = 1; 
            public AbilityType AbilityType { get; set; }
        }
}