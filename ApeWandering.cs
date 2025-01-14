using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ApeWandering : State
{
    [Export]
    ape m_Ape;

    map m_Map;

    [Export]
    public int m_IdleSpeed { get; set; } = 14;

    [Export]
    public int m_IdleAcceleration { get; set; } = 75;

    [Export]
    private int m_Gravity { get; set; } = 50;

    private Vector3 m_TargetVelocity = Vector3.Zero;

    private const int m_MinWanderRange = 5;

    private const int m_MaxWanderRange = 10;

    Godot.Collections.Array<Vector2I> m_IDPath;

    Vector2 m_NextPos;

    int m_NextPosCtr;

    const float m_MaxNavPointDist = 0.5f;

    const float m_WanderingVelocity = 1000.0f;


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
        Random rnd = new Random();
        int wanderRange = rnd.Next(m_MinWanderRange, m_MaxWanderRange);
        Vector2I NextPosDist = new Vector2I();
        NextPosDist.X = rnd.Next(0, wanderRange + 1);
        NextPosDist.Y = wanderRange - NextPosDist.X;
        if (rnd.Next(0, 2) == 0)
        {
            NextPosDist.X *= -1;
        }
        if (rnd.Next(0, 2) == 0)
        {
            NextPosDist.Y *= -1;
        }

        Vector2I FinalPos = new Vector2I(m_Ape.GetNavCoords().X + NextPosDist.X, m_Ape.GetNavCoords().Y + NextPosDist.Y);

        if (m_Map.IsInBounds(FinalPos))
        {
            //adjust pathfinding for other apes
            m_Map.SetPointSolid(m_Ape.GetNavCoords(), false);
            m_IDPath = m_Map.getIdPath(m_Ape.GetNavCoords(), FinalPos, true);


            if (m_IDPath.Count <= 1)
            {
                m_Map.SetPointSolid(m_Ape.GetNavCoords(), true);
                EmitSignal(SignalName.Transitioned, this.Name + "", "ApeIdle");
            }
            else
            {
                m_Map.SetPointSolid(m_IDPath[m_IDPath.Count - 1], true);
                m_Ape.SetNavCoords(m_IDPath[m_IDPath.Count - 1]);

                m_NextPosCtr = 1;
                m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
            }
        }
        else
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeIdle");
        }
    }

    public override void Exit()
    {
        m_TargetVelocity = Vector3.Zero;
    }

    public override void Update(double delta)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        //Ape.Velocity = new Vector3(0, 100, 0);
        if (Math.Abs(m_NextPos.X - m_Ape.GlobalPosition.X) < m_MaxNavPointDist && Math.Abs(m_NextPos.Y - m_Ape.GlobalPosition.Z) < m_MaxNavPointDist)
        {
            m_NextPosCtr += 1;
            if (m_NextPosCtr >= m_IDPath.Count)
            {
                m_IDPath.Clear();
                EmitSignal(SignalName.Transitioned, this.Name + "", "ApeIdle");
            }
            else
            {
                m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
            }
        }
        else
        {
            Vector3 direction = new Vector3(m_NextPos.X - m_Ape.GlobalPosition.X, 0, m_NextPos.Y - m_Ape.GlobalPosition.Z).Normalized();

            m_TargetVelocity.X = direction.X * m_WanderingVelocity * (float)delta;
            m_TargetVelocity.Z = direction.Z * m_WanderingVelocity * (float)delta;
            if (!m_Ape.IsOnFloor())
            {
                m_TargetVelocity.Y -= m_Gravity * (float)delta;
            }
            else
            {
                m_TargetVelocity.Y = 0;
            }


            m_Ape.Velocity = m_TargetVelocity;
            m_Ape.LookAt(m_Ape.GlobalPosition + m_TargetVelocity);
        }
    }

    public void SetMap(ref map Map)
    {
        m_Map = Map;
    }
}
