using Godot;
using System;
using static DeckInterface;

public partial class ApeIdle : State
{

    [Export]
    ape Ape;

    [Export]
    private int Gravity { get; set; } = 50;

    private Vector3 TargetVelocity = Vector3.Zero;

    private double WanderCtr = 0;

    private const double WanderBaseline = 1.0;

    private double NextWanderTime;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

    public override void Enter()
    {
        if (!Ape.IsWorking())
        {
            Ape.SetReadyForNextPhase(true);
        }
        GenerateNextWanderTime();
    }

    public override void Exit()
    {
        WanderCtr = 0;
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!Ape.IsOnFloor())
        {
            TargetVelocity.Y -= Gravity * (float)delta;
        }
        else
        {
            TargetVelocity.Y = 0;
        }

        WanderCtr += delta;

        if (Ape.CanWorkTransition())
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorkingTransit");
        }

        if (WanderCtr > NextWanderTime)
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "Wandering");
        }

        Ape.Velocity = TargetVelocity;
    }

    private void GenerateNextWanderTime()
    {
        Random rnd = new Random();
        NextWanderTime = WanderBaseline + rnd.NextDouble();
    }
}
