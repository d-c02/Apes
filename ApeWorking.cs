using Godot;
using System;
using static DeckInterface;
public partial class ApeWorking : State
{

    [Export] ape m_Ape;
    private ApeManager m_ApeManager;

    ProjectEnum m_TargetProject = ProjectEnum.None;

    public override void Enter()
    {
        m_Ape.SetReadyForNextPhase(true);
        m_Ape.LookAt(m_ApeManager.GetGlobalPosition(m_Ape.GetTargetProject()));
        SetWorkAnim();
        m_TargetProject = m_Ape.GetTargetProject();
    }

    public override void Exit()
    {
        //Check if project is null or not!!!
        if (m_ApeManager.IsProjectActive(m_Ape.GetTargetProject()))
        {
            m_ApeManager.SetOpenSlot(m_Ape.GetTargetProject(), m_Ape.GetSlot(), true);
        }
        //else
        //{
        //    m_Ape.SetTargetProject(ProjectEnum.None);
        //}

        CleanUpWorkAnim();
        m_Ape.SetWorkTransition(false);
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!m_Ape.IsWorking() || !m_Ape.IsReadyForNextPhase() || m_TargetProject != m_Ape.GetTargetProject())
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorkingExit");
        }
    }

    public void SetApeManager(ref ApeManager apeManager)
    {
        m_ApeManager = apeManager;
    }

    private void SetWorkAnim()
    {
        if (m_Ape.GetAspect() == AspectEnum.Insight)
        {
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Insight_Working");
            GetParent().GetParent().GetNode<Node3D>("Pivot/Ape/Armature/Skeleton3D/RightHand/Pencil").Visible = true;
            GetParent().GetParent().GetNode<Node3D>("Pivot/Ape/Armature/Skeleton3D/LeftHand/Clipboard").Visible = true;
        }
        else if (m_Ape.GetAspect() == AspectEnum.Influence)
        {
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Influence_Working");
        }
        else if (m_Ape.GetAspect() == AspectEnum.Fervor)
        {
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Fervor_Working");
            GetParent().GetParent().GetNode<MeshInstance3D>("Pivot/Ape/Armature/Skeleton3D/RightHand/Hammer").Visible = true;
        }
    }

    private void CleanUpWorkAnim()
    {
        if (m_Ape.GetAspect() == AspectEnum.Insight)
        {
            GetParent().GetParent().GetNode<Node3D>("Pivot/Ape/Armature/Skeleton3D/RightHand/Pencil").Visible = false;
            GetParent().GetParent().GetNode<Node3D>("Pivot/Ape/Armature/Skeleton3D/LeftHand/Clipboard").Visible = false;
        }
        else if (m_Ape.GetAspect() == AspectEnum.Fervor)
        {
            GetParent().GetParent().GetNode<MeshInstance3D>("Pivot/Ape/Armature/Skeleton3D/RightHand/Hammer").Visible = false;
        }
    }
}
