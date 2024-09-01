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
		Vector2I Coords = Map.getRandomOpenCoords(true);
		//ape Ape = new ape();
        var apeScene = GD.Load<PackedScene>("res://ape.tscn");
		ape Ape = apeScene.Instantiate<ape>();
		Ape.SetMap(ref Map);
        Ape.Position = new Vector3(Coords.X, 10, Coords.Y);
		AddChild(Ape);
		//Apes.Append(Ape);
	}
}
