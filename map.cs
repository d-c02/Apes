using Godot;
using System;

public partial class map : GridMap
{
	enum Blocks { Center, Corner, Ramp, Sand, Water};

	const int mapSize = 100;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector3I Coords = new Vector3I(0, 0, 0);

		for (int i = 0; i < mapSize; i++)
		{
			for (int j = 0; j < mapSize; j++)
			{
				int k = 0;
				Coords = new Vector3I(i, k, j);
				if (i < 25 || i > 75 || j < 25 || j > 75)
				{
                    SetCellItem(Coords, (int)Blocks.Water, 0);
                }
                else
				{
                    SetCellItem(Coords, (int)Blocks.Center, 0);
                }
            }
		}
        
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
