using Godot;
using System;

public partial class ape : CharacterBody3D
{
    [Export]
    private AnimationTree _AnimationTree;

    private AnimationNodeStateMachinePlayback _AnimationNodeStateMachinePlayback;

    
}
