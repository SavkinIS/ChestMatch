using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

public class StateMachine
{
     private Dictionary<Type, IExitableState> _states;
     private IExitableState _activeState;


     public StateMachine(IEnumerable<IExitableState> states)
     {
          _states = new Dictionary<Type, IExitableState>(states.Count());
          foreach (var state in states)
          {
               _states[state.GetType()] = state;
               if (state is IStateMachineBehavior behavior)
                    behavior.SetStateMachine(this);
          }
          
     }

     public IExitableState ActiveState => _activeState;

     public void Enter<TState>() where TState : class,IState
     {
          _activeState?.Exit();
          var state = GetState<TState>();
          state.Enter();
          _activeState = state;
     }
     

     public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
     {
          _activeState?.Exit();
          var state = GetState<TState>();
          state.Enter(payload);
          _activeState = state;
     }

     private TState GetState<TState>() where TState : class, IExitableState
     {
          return _states[typeof(TState)] as TState;
     }
     
     public void Tick()
     {
          
          if (_activeState is ITickable tickable)
               tickable.Tick();
     }
}