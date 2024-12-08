using Godot;
using System;
using static DeckInterface;
public partial class ApeWorking : State
{

    [Export] ape m_Ape;
    private ApeManager m_ApeManager;

    public override void Enter()
    {
        
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

        m_Ape.SetWorkTransition(false);
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!m_Ape.IsWorking())
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorkingExit");
        }
    }

    public void SetApeManager(ref ApeManager apeManager)
    {
        m_ApeManager = apeManager;
    }
}
