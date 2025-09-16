
public interface IGameFlowModel
{
    string SceneToLoad { get; set; }
    // Здесь в будущем могут быть и другие данные:
    // int PlayerScore { get; set; }
    // PlayerConfig PlayerConfig { get; set; }
}


public class GameFlowModel : IGameFlowModel
{
    public string SceneToLoad { get; set; }
}