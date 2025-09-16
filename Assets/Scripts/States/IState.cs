public interface IState : IExitableState
{
    void Enter();
}

public interface IExitableState
{
    void Exit();
   
}

public interface IStateMachineBehavior 
{ void SetStateMachine(StateMachine stateMachine);
}

public interface IPayloadedState<TPayload> : IExitableState
{
    void Enter(TPayload payload);
}


public interface ITickable
{
    void Tick();
}
