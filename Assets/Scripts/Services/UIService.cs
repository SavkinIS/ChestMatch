using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IUIService
{
    void ShowWindow(WindowId windowId);
    void HideWindow(WindowId windowId);
    T GetWindow<T>(WindowId gameplay) where T : BaseWindow;
}

[Serializable]
public class UIWindowConfig
{
    public WindowId WindowId;
    public BaseWindow Prefab;
}


public class UIService : MonoBehaviour,IUIService
{
    
    [SerializeField] private UIWindowConfig[] _windowConfigs;
    [SerializeField] private Transform _uiRoot;

    private readonly Dictionary<WindowId, BaseWindow> _windowsCache = new ();
    private DiContainer _container;


    [Inject]
    public void Construct(DiContainer container)
    {
        _container = container;
        Initialize();
    }
    
    public void Initialize()
    {
        DontDestroyOnLoad(this.gameObject);
        foreach (var config in _windowConfigs)
        {
            BaseWindow window = Instantiate(config.Prefab, _uiRoot);
            _windowsCache[config.WindowId] =  window;
            _container.InjectGameObject(window.gameObject);
            window.Hide();
        }
    }
    
    public void ShowWindow(WindowId windowId)
    {
        if (_windowsCache.TryGetValue(windowId, out var window))
        {
            foreach(var w in _windowsCache.Values)
                w.Hide();
            
            window.Show();
        }
        else
        {
            Debug.LogError($"Окно с ID: {windowId} не найдено в конфигурации!");
        }
        
       
    }

    public void HideWindow(WindowId windowId)
    {
        if (_windowsCache.TryGetValue(windowId, out var window))
        {
            window.Hide();
        }
        else
        {
            Debug.LogError($"Окно с ID: {windowId} не найдено в конфигурации!");
        }
    }

    public T GetWindow<T>(WindowId windowId) where T : BaseWindow
    {
        if (_windowsCache.TryGetValue(windowId, out var window))
        {
            return window as T; 
        }
        return null;
    }
}