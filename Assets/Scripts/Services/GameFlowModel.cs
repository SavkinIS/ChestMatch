
using System;
using System.Collections.Generic;
using Shashki;

public enum BotDifficulty
{
    Easy,
    Medium
}

public interface IGameFlowModel
{
    string SceneToLoad { get; set; }

    Type NextState { get; set; }
    List<AbilityType> AvailableAbilities { get; set; } 
}


public class GameFlowModel : IGameFlowModel
{
    public string SceneToLoad { get; set; }
    public Type NextState { get; set; }
    public List<AbilityType> AvailableAbilities { get; set; }
    public BotDifficulty BotDifficulty { get; set; }
}