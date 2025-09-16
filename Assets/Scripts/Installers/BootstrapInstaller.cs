using Shashki;
using Zenject;

public class BootstrapInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Используем BindInterfacesAndSelfTo для каждого состояния.
        // Это зарегистрирует их по всем интерфейсам + по их собственному типу.
        Container.BindInterfacesAndSelfTo<BootstrapState>().AsSingle();
        Container.BindInterfacesAndSelfTo<LoadingState>().AsSingle();
        Container.BindInterfacesAndSelfTo<LobbyState>().AsSingle();
        Container.BindInterfacesAndSelfTo<GameplayState>().AsSingle(); 
        Container.BindInterfacesAndSelfTo<PauseState>().AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsState>().AsSingle();
        
        Container.BindInterfacesAndSelfTo<GameFlowModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<SceneLoader>().AsSingle();
        
        

        // Биндинг для StateMachine остается прежним.
        Container.Bind<StateMachine>().AsSingle();
        
    }
    
    
}