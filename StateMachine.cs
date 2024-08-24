using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{
    private State _CurState;
    private Dictionary<string, State> _States = new Dictionary<string, State>();
    [Export] private State _InitialState;
    public override void _Ready()
    {
        Godot.Collections.Array<Godot.Node> Children = GetChildren();
        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is State)
            {
                State TmpState = (State)Children[i];
                //TmpState.Transitioned += OnChildTransition;
                TmpState.Connect(State.SignalName.Transitioned, new Callable(this, StateMachine.MethodName.OnChildTransition));
                _States[TmpState.Name] = TmpState;
            }
        }

        if (_InitialState != null)
        {
            _InitialState.Enter();
            _CurState = _InitialState;
        }
    }

    public override void _Process(double delta)
    {
        if (_CurState != null)
        {
            _CurState.Update(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_CurState != null)
        {
            _CurState.PhysicsUpdate(delta);
        }
    }

    private void OnChildTransition(string prevState, string nextState)
    {
        if (_States[prevState] != _CurState)
        {
            return;
        }
        State newState = _States[nextState];
        if (newState == null)
        {
            return;
        }

        if (_CurState != null)
        {
            _CurState.Exit();
        }
        _CurState = _States[nextState];
        _CurState.Enter();
    }
}
