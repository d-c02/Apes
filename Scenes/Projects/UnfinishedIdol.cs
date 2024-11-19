using Godot;
using System;
//Can I refactor this into ApeManager?
using static DeckInterface;

public partial class UnfinishedIdol : Project
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//m_MaxWork = 10;
		//m_NumWorkPerRow = 5;

        ConfigureWork();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BillboardWork();
	}

    public override void OnFinish()
    {
        int fervorCount = 0;
        int insightCount = 0;
        int influenceCount = 0;
        for (int i = 0; i < m_MaxWork; i++)
        {
            if (m_WorkSprites[i].Frame == (int) WorkAspects.Fervor)
            {
                fervorCount++;
            }
            else if (m_WorkSprites[i].Frame == (int)WorkAspects.Insight)
            {
                insightCount++;
            }
            else if (m_WorkSprites[i].Frame == (int)WorkAspects.Influence)
            {
                influenceCount++;
            }
        }

        if (fervorCount == insightCount && insightCount == influenceCount)
        {

        }
        else if (fervorCount == insightCount)
        {

        }
        else if (insightCount == influenceCount)
        {

        }
        else if (influenceCount == fervorCount)
        {

        }   
        else
        {
            int maxCount = Math.Max(Math.Max(fervorCount, influenceCount), insightCount);
            
            if (maxCount == fervorCount)
            {

            }
            else if (maxCount == insightCount)
            {

            }
            else
            {

            }
        }

        this.QueueFree();
        m_ApeManager.RemoveProject(m_ID);
        m_ApeManager.SpawnProject((int) Projects.Fervor_Idol, m_Coords);
    }
}
