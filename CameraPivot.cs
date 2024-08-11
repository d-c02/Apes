using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class CameraPivot : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        _PanVelocity = new Vector2(0, 0);
		if (Input.IsActionPressed("PanLeft"))
		{
            _PanVelocity.X -= _PanSpeed;
		}
		if (Input.IsActionPressed("PanRight"))
		{
            _PanVelocity.X += _PanSpeed;
		}
        if (Input.IsActionPressed("PanUp"))
        {
            _PanVelocity.Y += _PanSpeed;
        }
        if (Input.IsActionPressed("PanDown"))
        {
            _PanVelocity.Y -= _PanSpeed;
        }
        if (Input.IsActionPressed("TiltLeft"))
        {
            _RotationSpeed -= _TiltSpeed;
        }
        if (Input.IsActionPressed("TiltRight"))
        {
            _RotationSpeed += _TiltSpeed;
        }
        
    }

    public override void _PhysicsProcess(double delta)
    {
        Rotation = new Vector3(Rotation.X + _RotationSpeed * (float) delta, Rotation.Y, Rotation.Z);
        Position = new Vector3(Position.X + _PanVelocity.X, Position.Y, Position.Z + _PanVelocity.Y);
    }

    [Export] private Camera3D _Camera;
    private const float _PanSpeed = 10.0f;
    private const float _TiltSpeed = 10.0f;
    private Vector2 _PanVelocity;
    private float _RotationSpeed;
}
