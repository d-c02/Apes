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
        m_Finished = false;
        m_Work = 0;
        for (int i = 0; i < m_MaxWork; i++)
        {
            m_WorkSprites[i].Frame = (int)WorkAspectEnum.Empty;
        }
    }
}
