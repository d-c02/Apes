using Godot;
using System;

public partial class ape : CharacterBody3D
{
    [Export]
    private AnimationTree _AnimationTree;

    private AnimationNodeStateMachinePlayback _AnimationNodeStateMachinePlayback;

    private AnimationNodeTimeScale _RunTimeScale;

    public bool InSpecialJumpTransition = false;

    public override void _Ready()
    {
        _AnimationTree = GetChild<AnimationTree>(0);
        _AnimationNodeStateMachinePlayback = _AnimationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
        //_RunTimeScale = _AnimationTree.Get("parameters/Run/TimeScale/scale").As<AnimationNodeTimeScale>();
    }
    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
    }

    //TODO: Anim state stuff needs to be changed to stringnames instead of strings to optimize performance.
    public void SetAnimState(string state)
    {
        _AnimationNodeStateMachinePlayback.Travel(state);
    }

    public StringName GetAnimState()
    {
        return _AnimationNodeStateMachinePlayback.GetCurrentNode();
    }
}
