using Godot;
using System;

public partial class ApeWandering : State
{
    [Export]
    ape Ape;

    map m_Map;

    [Export]
    public int m_IdleSpeed { get; set; } = 14;

    [Export]
    public int m_IdleAcceleration { get; set; } = 75;

    [Export]
    private int m_Gravity { get; set; } = 50;

    private Vector3 m_TargetVelocity = Vector3.Zero;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void Enter()
    {
        
    }

    public override void Exit()
    {
        
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        //Ape.Velocity = new Vector3(0, 100, 0);
    }

    public void SetMap(ref map Map)
    {
        m_Map = Map;
    }
}
