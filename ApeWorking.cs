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
        m_ApeManager.SetOpenSlot(m_Ape.GetTargetProject(), m_Ape.GetSlot(), true);
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (m_Ape.GetAction() != ActionEnum.Work_One && m_Ape.GetAction() != ActionEnum.Work_Two && m_Ape.GetAction() != ActionEnum.Work_Three)
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorkingExit");
        }
    }
}
