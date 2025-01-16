using Godot;
using System;
using static DeckInterface;

public partial class ApeIdle : State
{

    [Export]
    ape m_Ape;

    [Export]
    private int Gravity { get; set; } = 50;

    private Vector3 TargetVelocity = Vector3.Zero;

    private double WanderCtr = 0;

    private const double WanderBaseline = 1.0;

    private double NextWanderTime;

    private bool m_PrevSleeping = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

    public override void Enter()
    {
        if (!m_Ape.GetSleeping())
        {
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Idle");
        }
        else
        {
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Sleeping");
            m_Ape.SetAnimState("parameters/EyeAnimGate/transition_request", "EyesClosed");
        }

        if (!m_Ape.IsWorking() || m_Ape.GetSleeping())
        {
            m_Ape.SetReadyForNextPhase(true);
        }
        GenerateNextWanderTime();
    }

    public override void Exit()
    {
        WanderCtr = 0;
        m_Ape.SetAnimState("parameters/EyeAnimGate/transition_request", "Blinking");
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        if (!m_PrevSleeping && m_Ape.GetSleeping())
        {
            m_PrevSleeping = true;
            m_Ape.SetReadyForNextPhase(true);
            m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Sleeping");
            m_Ape.SetAnimState("parameters/EyeAnimGate/transition_request", "EyesClosed");
        }
        else if (!m_Ape.GetSleeping())
        {
            m_PrevSleeping = false;
        }

        if (!m_Ape.IsOnFloor())
        {
            TargetVelocity.Y -= Gravity * (float)delta;
        }
        else
        {
            TargetVelocity.Y = 0;
        }

        if (!m_Ape.GetSleeping())
        {
            WanderCtr += delta;
        }

        if (m_Ape.CanWorkTransition())
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorkingTransit");
        }

        if (WanderCtr > NextWanderTime)
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "Wandering");
        }

        m_Ape.Velocity = TargetVelocity;
    }

    private void GenerateNextWanderTime()
    {
        Random rnd = new Random();
        NextWanderTime = WanderBaseline + rnd.NextDouble();
    }
}
