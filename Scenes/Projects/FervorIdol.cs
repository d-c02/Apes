using Godot;
using System;
using static DeckInterface;

public partial class FervorIdol : Project
{

	public override void OnFinish()
	{
		m_NextProject = DeckInterface.ProjectEnum.None;
	}

    public override void ResetPhase()
    {
        base.ResetPhase();
    }
}
