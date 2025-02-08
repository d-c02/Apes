using Godot;
using System;
using static DeckInterface;

public partial class Lab : Project
{


    public override void OnFinish()
    {
        m_Persists = false;
        m_ApeManager.AssignJob(AspectEnum.Insight, DeckInterface.JobEnum.Insight_Scientist);
    }
}
