using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ApeManager : Node
{
	// Called when the node enters the scene tree for the first time.

	[Export] private map Map;

	private List<ape> Apes;
	public override void _Ready()
	{
		for (int i = 0; i < 21; i++)
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
		Vector2I Coords = Map.getRandomOpenNavCoords(true);

		var apeScene = new PackedScene();
        apeScene = ResourceLoader.Load<PackedScene>("res://ape.tscn");
		ape Ape = apeScene.Instantiate<ape>();
		Ape.SetMap(ref Map);
        Ape.SetNavCoords(Coords);
		Vector2 PosCoords = Map.GetPointPosition(Coords);
        Ape.Position = new Vector3(PosCoords.X, 10, PosCoords.Y);
		Ape.SetMap(ref Map);
		AddChild(Ape);
		//Apes.Append(Ape);
	}
}
