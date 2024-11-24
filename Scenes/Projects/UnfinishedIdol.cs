using Godot;
using System;
//Can I refactor this into ApeManager?
using static DeckInterface;

public partial class UnfinishedIdol : Project
{

    public override void ConfigureSlots()
    {
        for (int x = 0; x < m_Dimensions.X; x++)
        {
            for (int z = 0; z < m_Dimensions.Y; z++)
            {
                if (x == 0 || x == m_Dimensions.X - 1 || z == 0 || z == m_Dimensions.Y - 1)
                {
                    m_ApeSlots[new Vector2I(x, z)] = true;
                }
            }
        }
    }

    public override void OnFinish()
    {
        int fervorCount = 0;
        int insightCount = 0;
        int influenceCount = 0;
        for (int i = 0; i < m_MaxWork; i++)
        {
            if (m_WorkSprites[i].Frame == (int) WorkAspectEnum.Fervor)
            {
                fervorCount++;
            }
            else if (m_WorkSprites[i].Frame == (int)WorkAspectEnum.Insight)
            {
                insightCount++;
            }
            else if (m_WorkSprites[i].Frame == (int)WorkAspectEnum.Influence)
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

        m_NextProject = ProjectEnum.Fervor_Idol;
    }
}
