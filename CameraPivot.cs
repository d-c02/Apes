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
        Vector3 CameraDir = GlobalPosition - _Camera.GlobalPosition;
        CameraDir.Y = 0;

        CameraDir = CameraDir.Normalized();
        Vector3 OrthogCameraDir = new Vector3(-1 * CameraDir.Z, 0, CameraDir.X);
        _PanVelocity = new Vector3(0, 0, 0);
        _RotationSpeed = 0;
		if (Input.IsActionPressed("PanLeft"))
		{
            _PanVelocity -= _PanSpeed * OrthogCameraDir;
		}
		if (Input.IsActionPressed("PanRight"))
		{
            _PanVelocity += _PanSpeed * OrthogCameraDir;
		}
        if (Input.IsActionPressed("PanUp"))
        {
            _PanVelocity += _PanSpeed * CameraDir;
        }
        if (Input.IsActionPressed("PanDown"))
        {
            _PanVelocity -= _PanSpeed * CameraDir;
        }
        if (Input.IsActionPressed("RotateLeft"))
        {
            _RotationSpeed -= _TiltSpeed;
        }
        if (Input.IsActionPressed("RotateRight"))
        {
            _RotationSpeed += _TiltSpeed;
        }
        if (Input.IsActionPressed("DEBUG_PAN_DOWN"))
        {
            _Camera.RotateX(-1.0f * (float) delta);
        }
        if (Input.IsActionPressed("DEBUG_PAN_UP"))
        {
            _Camera.RotateX(1.0f * (float)delta);
        }

        _PanVelocity = _PanVelocity.Normalized() * _PanSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        Rotation = new Vector3(Rotation.X, Rotation.Y + _RotationSpeed * (float)delta, Rotation.Z);
        Position = new Vector3(Position.X + _PanVelocity.X * (float) delta, Position.Y + _PanVelocity.Y * (float) delta, Position.Z + _PanVelocity.Z * (float) delta);
    }

    [Export] private Camera3D _Camera;
    private const float _PanSpeed = 40.0f;
    private const float _TiltSpeed = 2.0f;
    private Vector3 _PanVelocity;
    private float _RotationSpeed;
}
