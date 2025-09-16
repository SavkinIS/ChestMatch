using System;
using UnityEngine.SceneManagement;

public interface ISceneLoader
{
    void LoadScene(string sceneName, Action callback = null);
}


public class SceneLoader : ISceneLoader
{
    public void LoadScene(string sceneName, Action callback = null)
    {
       var asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
       asyncOp.completed += (_) => callback?.Invoke();
    }
}