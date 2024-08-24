using Godot;
using System;

public partial class FPSCounter : ColorRect
{
	// Called when the node enters the scene tree for the first time.

	[Export]
	Label text;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		text.Text = "FPS: " + Engine.GetFramesPerSecond().ToString();
	}
}
