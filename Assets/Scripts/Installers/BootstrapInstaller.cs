using Shashki;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class BootstrapInstaller : MonoInstaller
{
    [SerializeField] private UIService _uiServiceInstance;
    [SerializeField] private LoadingCurtain _loadingCurtain;
    [SerializeField] private GameCore _gameCorePrefab;
    [SerializeField] private AbilityConteiner _abilityContainer;
    
    
    public override void InstallBindings()
    {
       
        Container.BindInterfacesAndSelfTo<BootstrapState>().AsSingle();
        Container.BindInterfacesAndSelfTo<LoadingState>().AsSingle();
        Container.BindInterfacesAndSelfTo<LobbyState>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameplayState>().AsSingle(); 
        Container.BindInterfacesAndSelfTo<PauseState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsState>().AsSingle();
        
        Container.BindInterfacesAndSelfTo<GameFlowModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<SceneLoader>().AsSingle();
        
        Container.Bind<IUIService>().FromInstance(_uiServiceInstance).AsSingle();
        Container.Bind<LoadingCurtain>().FromInstance(_loadingCurtain).AsSingle();
        Container.Bind<AbilityConteiner>().FromInstance(_abilityContainer).AsSingle();
        
        
        Container.Bind<GameCore>()
            .WithId("GameSessionPrefab")
            .FromInstance(_gameCorePrefab)
            .AsCached();
        
        Container.Bind<StateMachine>().AsSingle();
    }
    
    
}