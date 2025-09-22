
using System;
using System.Collections.Generic;
using Shashki;

public interface IGameFlowModel
{
    string SceneToLoad { get; set; }

    Type NextState { get; set; }
    List<AbilityType> AvailableAbilities { get; set; } 
    // Здесь в будущем могут быть и другие данные:
    // int PlayerScore { get; set; }
    // PlayerConfig PlayerConfig { get; set; }
}


public class GameFlowModel : IGameFlowModel
{
    public string SceneToLoad { get; set; }
    public Type NextState { get; set; }
    public List<AbilityType> AvailableAbilities { get; set; }
}