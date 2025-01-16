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
    private AnimationNodeStateMachinePlayback _AnimationNodeStateMachinePlayback;

    private AnimationNodeTimeScale _RunTimeScale;

    public bool InSpecialJumpTransition = false;

    private Vector2I m_NavCoords;

    private Vector2I m_PrevNavCoords;

    private Vector2I m_Slot;

    private map m_Map;

    AspectEnum m_Aspect = AspectEnum.Insight;
    AspectEnum m_EnemyAspect = AspectEnum.Influence;

    [Export]
    Sprite3D m_ActionSprite;

    [Export] Material InsightBody;
    [Export] Material InsightMouth;
    [Export] Material InfluenceBody;
    [Export] Material InfluenceMouth;
    [Export] Material FervorBody;
    [Export] Material FervorMouth;

    private List<DeckEnum> m_Decks;

    private ApeManager m_ApeManager;

    private ActionEnum m_Action = ActionEnum.Idle;

    private ProjectEnum m_TargetProject;

    private int m_Spite = 5;

    private bool m_WorkTransition = false;
    private bool m_TargetProjectChanged = false;


    private bool m_ReadyForNextPhase = false;

    [Export]
    private AnimationTree m_AnimationTree;

    private bool m_Sleeping = false;

    public override void _Ready()
    {

        //_AnimationTree = GetChild<AnimationTree>(0);
        Random rnd = new Random();

        MeshInstance3D body = GetNode<MeshInstance3D>("Pivot/Ape/Armature/Skeleton3D/Body");
        MeshInstance3D mouth = GetNode<MeshInstance3D>("Pivot/Ape/Armature/Skeleton3D/Mouth");

        body.MaterialOverride = null;
        mouth.MaterialOverride = null;
        int aspectInt = rnd.Next(0, 3);
        if (aspectInt == 0)
        {
            m_Aspect = AspectEnum.Insight;
            m_EnemyAspect = AspectEnum.Influence;
        }
        else if (aspectInt == 1)
        {
            m_Aspect = AspectEnum.Influence;
            m_EnemyAspect = AspectEnum.Fervor;
        }
        else if (aspectInt == 2)
        {
            m_Aspect = AspectEnum.Fervor;
            m_EnemyAspect = AspectEnum.Insight;
        }

        if (m_Aspect == AspectEnum.Insight)
        {
            //body.MaterialOverride = InsightBody;
            //mouth.MaterialOverride = InsightMouth;
            body.SetSurfaceOverrideMaterial(0, InsightBody);
            mouth.SetSurfaceOverrideMaterial(0, InsightMouth);
        }
        else if (m_Aspect == AspectEnum.Influence)
        {
            //body.MaterialOverride = InfluenceBody;
            //mouth.MaterialOverride = InfluenceMouth;
            //body.SetSurfaceOverrideMaterial(0, InfluenceBody);
            body.SetSurfaceOverrideMaterial(0, InfluenceBody);
            mouth.SetSurfaceOverrideMaterial(0, InfluenceMouth);
        }
        else if (m_Aspect == AspectEnum.Fervor)
        {
            //body.MaterialOverride = FervorBody;
            //mouth.MaterialOverride = FervorMouth;

            body.SetSurfaceOverrideMaterial(0, FervorBody);
            mouth.SetSurfaceOverrideMaterial(0, FervorMouth);
        }

        m_Decks = new List<DeckEnum>();
        //_RunTimeScale = _AnimationTree.Get("parameters/Run/TimeScale/scale").As<AnimationNodeTimeScale>();
        m_TargetProject = ProjectEnum.Unfinished_Idol;

        SetAction(ActionEnum.Idle);
        m_ReadyForNextPhase = true;

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
        GetNode<ApeWorkingTransit>("StateMachine/ApeWorkingTransit").SetMap(ref m_Map);
        GetNode<ApeWorkingExit>("StateMachine/ApeWorkingExit").SetMap(ref m_Map);
    }

    public AspectEnum GetAspect()
    {
        return m_Aspect;
    }

    public void SetApeManager(ApeManager apeManager)
    {
        m_ApeManager = apeManager;
        GetNode<ApeWorkingTransit>("StateMachine/ApeWorkingTransit").SetApeManager(ref m_ApeManager);
        GetNode<ApeWorkingExit>("StateMachine/ApeWorkingExit").SetApeManager(ref m_ApeManager);
        GetNode<ApeWorking>("StateMachine/ApeWorking").SetApeManager(ref m_ApeManager);
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

    public void AddDeck(DeckEnum deck)
    {
        m_Decks.Add(deck);
    }

    public bool RemoveDeck(DeckEnum deck)
    {
        return m_Decks.Remove(deck);
    }


    private ActionEnum DrawAction()
    {
        int deckSum = 0;
        ActionEnum result = ActionEnum.Idle;
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

    private void SetActionSprite(ActionEnum action)
    {
        if (action == ActionEnum.Idle)
        {
            m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/Idle.png");
        }
        else if (action == ActionEnum.Work_One)
        {
            if (m_Aspect == AspectEnum.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorOne.png");
            }
            if (m_Aspect == AspectEnum.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceOne.png");
            }
            if (m_Aspect == AspectEnum.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightOne.png");
            }
        }
        else if (action == ActionEnum.Work_Two)
        {
            if (m_Aspect == AspectEnum.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorTwo.png");
            }
            if (m_Aspect == AspectEnum.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceTwo.png");
            }
            if (m_Aspect == AspectEnum.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightTwo.png");
            }
        }
        else if (action == ActionEnum.Work_Three)
        {
            if (m_Aspect == AspectEnum.Fervor)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/FervorThree.png");
            }
            if (m_Aspect == AspectEnum.Influence)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InfluenceThree.png");
            }
            if (m_Aspect == AspectEnum.Insight)
            {
                m_ActionSprite.Texture = (Texture2D)GD.Load("res://Assets/Apes/UI_Icons/WorkIcons/InsightThree.png");
            }
        }
    }


    public ActionEnum GetAction()
    {
        return m_Action;
    }
    
    public void SetAction(ActionEnum action)
    {
        m_Action = action;
        SetActionSprite(m_Action);
    }

    public ProjectEnum GetTargetProject()
    {
        return m_TargetProject;
    }

    public void SetTargetProject(ProjectEnum project)
    {
        if (!(m_TargetProject == project))
        {
            m_TargetProjectChanged = true;
        }
        m_TargetProject = project;
    }

    public void StartNewPhase()
    {
        m_ReadyForNextPhase = false;
        
        if (m_ApeManager.GetTime() == 4)
        {
            m_Action = ActionEnum.Idle;
            SetActionSprite(ActionEnum.Idle);
            m_WorkTransition = false;
            m_Sleeping = true;
        }
        else
        {
            m_Action = DrawAction();
            m_Sleeping = false;
        }

        ////Eventually refactor to have a list of actionenums in a dict that are ready to work
        //if (m_Action == ActionEnum.Idle)
        //{
        //    m_ReadyForNextPhase = true;
        //}
    }

    public Vector2I GetPrevNavCoords()
    {
        return m_PrevNavCoords;
    }

    public void SetPrevNavCoords(Vector2I PrevNavCoords)
    {
        m_PrevNavCoords = PrevNavCoords;
    }

    public void SetSlot(Vector2I Slot)
    {
        m_Slot = Slot;
    }

    public Vector2I GetSlot()
    {
        return m_Slot;
    }

    public int GetSpite()
    {
        return m_Spite;
    }

    public void SetSpite(int Spite)
    {
        m_Spite = Spite;
    }

    public bool IsWorking()
    {
        return (m_Action == ActionEnum.Work_One || m_Action == ActionEnum.Work_Two || m_Action == ActionEnum.Work_Three);
    }

    public bool CanWorkTransition()
    {
        if (m_WorkTransition)
        {
            m_WorkTransition = false;
            return true;
        }
        return false;
    }

    public void SetWorkTransition(bool workTransition)
    {
        m_WorkTransition = workTransition;
    }

    public void SetReadyForNextPhase(bool ready)
    {
        m_ReadyForNextPhase = ready;
    }

    public bool IsReadyForNextPhase()
    {
        return m_ReadyForNextPhase;
    }

    public AspectEnum GetEnemyAspect()
    {
        return m_EnemyAspect;
    }

    public void SetAnimState(string path, string set)
    {
        m_AnimationTree.Set(path, set);
    }

    public bool GetSleeping()
    {
        return m_Sleeping;
    }
}
