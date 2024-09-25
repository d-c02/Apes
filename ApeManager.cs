using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ApeManager : Node
{
	// Called when the node enters the scene tree for the first time.

	[Export] private map m_Map;

	private List<ape> m_Apes;
	public override void _Ready()
	{
		for (int i = 0; i < 100; i++)
		{
			SpawnApe();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("DEBUG_SPAWN_APE"))
		{
			SpawnApe();
		}
	}

	public void SpawnApe()
	{
        //Vector2I Coords = m_Map.getRandomOpenNavCoords(true);
        Vector2I Coords = m_Map.getRandomOpenNavCoords(true);

        var apeScene = new PackedScene();
        apeScene = ResourceLoader.Load<PackedScene>("res://ape.tscn");
		ape Ape = apeScene.Instantiate<ape>();
		Ape.SetMap(ref m_Map);
        Ape.SetNavCoords(Coords);
		Vector2 PosCoords = m_Map.GetPointPosition(Coords);
		Ape.SetMap(ref m_Map);
		AddChild(Ape);
        Ape.GlobalPosition = new Vector3(PosCoords.X, 10, PosCoords.Y);
        //Apes.Append(Ape);
    }
}
