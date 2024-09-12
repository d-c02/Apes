using Godot;
using System;
using System.ComponentModel.Design;

public partial class ape : CharacterBody3D
{
    [Export]
    private AnimationTree _AnimationTree;

    private AnimationNodeStateMachinePlayback _AnimationNodeStateMachinePlayback;

    private AnimationNodeTimeScale _RunTimeScale;

    public bool InSpecialJumpTransition = false;

    private Vector2I m_NavCoords;

    private map m_Map;

    int m_Aspect = (int) Aspects.Insight;

    enum Aspects {Insight, Influence, Fervor };

    [Export] Material InsightBody;
    [Export] Material InsightMouth;
    [Export] Material InfluenceBody;
    [Export] Material InfluenceMouth;
    [Export] Material FervorBody;
    [Export] Material FervorMouth;

    public override void _Ready()
    {
        //_AnimationTree = GetChild<AnimationTree>(0);
        _AnimationNodeStateMachinePlayback = _AnimationTree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();
        Random rnd = new Random();

        MeshInstance3D body = GetNode<MeshInstance3D>("Pivot/Character/Ape/Body");
        MeshInstance3D mouth = GetNode<MeshInstance3D>("Pivot/Character/Ape/Mouth");


        m_Aspect = rnd.Next(0, 3);
        if (m_Aspect == (int) Aspects.Insight)
        {
            body.SetSurfaceOverrideMaterial(0, InsightBody);
            mouth.SetSurfaceOverrideMaterial(0, InsightMouth);
        }
        else if (m_Aspect == (int) Aspects.Influence)
        {
            body.SetSurfaceOverrideMaterial(0, InfluenceBody);
            mouth.SetSurfaceOverrideMaterial(0, InfluenceMouth);
        }
        else if (m_Aspect == (int) Aspects.Fervor)
        {
            body.SetSurfaceOverrideMaterial(0, FervorBody);
            mouth.SetSurfaceOverrideMaterial(0, FervorMouth);
        }
        
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

    public void SetMap(ref map Map)
    {
        m_Map = Map;
        GetNode<ApeWandering>("StateMachine/Wandering").SetMap(ref m_Map);
    }

    public void SetNavCoords(int x, int y)
    {
        m_NavCoords = new Vector2I(x, y);
    }

    public void SetNavCoords(Vector2I Coords)
    {
        m_NavCoords = Coords;
    }

    public Vector2I GetNavCoords()
    {
        return m_NavCoords;
    }

    public int GetAspect()
    {
        return m_Aspect;
    }
}
