using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using static DeckInterface;

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

    [Export]
    Sprite3D m_ActionSprite;

    [Export] Material InsightBody;
    [Export] Material InsightMouth;
    [Export] Material InfluenceBody;
    [Export] Material InfluenceMouth;
    [Export] Material FervorBody;
    [Export] Material FervorMouth;

    private List<int> m_Decks;

    private ApeManager m_ApeManager;

    private int m_Action = (int) Actions.Idle;

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

        m_Decks = new List<int>();
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

    public int GetAspect()
    {
        return m_Aspect;
    }

    public void SetApeManager(ApeManager apeManager)
    {
        m_ApeManager = apeManager;
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

    public void AddDeck(int deck)
    {
        m_Decks.Add(deck);
    }

    public bool RemoveDeck(int deck)
    {
        return m_Decks.Remove(deck);
    }


    private int DrawAction()
    {
        int deckSum = 0;
        int result = (int) Actions.Idle;
        for (int i = 0; i < m_Decks.Count; i++)
        {
            deckSum += m_ApeManager.GetDeckSize(m_Decks[i]);
        }

        Random rnd = new Random();
        int action = rnd.Next(0, deckSum);
        int curSum = 0;

        for (int i = 0; i < m_Decks.Count; i++)
        {
            curSum += m_ApeManager.GetDeckSize(m_Decks[i]);
            if (action < curSum)
            {
                result = m_ApeManager.GetAction(m_Decks[i], action - (curSum - m_ApeManager.GetDeckSize(m_Decks[i])));
                break;
            }
        }

        SetActionSprite(result);
        return result;
    }

    private void SetActionSprite(int action)
    {
        if (action == (int)Actions.Idle)
        {
            m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/Idle.png");
        }
        else if (action == (int)Actions.Work_One)
        {
            if (m_Aspect == (int) Aspects.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorOne.png");
            }
            if (m_Aspect == (int)Aspects.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceOne.png");
            }
            if (m_Aspect == (int)Aspects.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightOne.png");
            }
        }
        else if (action == (int)Actions.Work_Two)
        {
            if (m_Aspect == (int)Aspects.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorTwo.png");
            }
            if (m_Aspect == (int)Aspects.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceTwo.png");
            }
            if (m_Aspect == (int)Aspects.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightTwo.png");
            }
        }
        else if (action == (int)Actions.Work_Three)
        {
            if (m_Aspect == (int)Aspects.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorThree.png");
            }
            if (m_Aspect == (int)Aspects.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceThree.png");
            }
            if (m_Aspect == (int)Aspects.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightThree.png");
            }
        }
    }

    private void ProcessAction(int action)
    {
        if (action == (int) Actions.Idle)
        {
            
        }
        else if (action == (int) Actions.Work_One)
        {

        }
        else if (action == (int)Actions.Work_Two)
        {

        }
        else if (action == (int)Actions.Work_Three)
        {

        }
    }

    public void StartNewPhase()
    {
        m_Action = DrawAction();
    }
}
