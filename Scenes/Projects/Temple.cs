using Godot;
using System;
using static DeckInterface;

public partial class Temple : Project
{

    public override void OnFinish()
    {
        m_Persists = false;
        m_ApeManager.AssignJob(AspectEnum.Influence, DeckInterface.JobEnum.Influence_Priest);
    }
}
