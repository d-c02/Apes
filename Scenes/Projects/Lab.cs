using Godot;
using System;

public partial class Lab : Project
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
        throw new NotImplementedException();
    }
}
