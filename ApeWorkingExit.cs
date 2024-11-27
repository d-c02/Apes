using Godot;
using System;


public partial class ApeWorkingExit : State
{

    [Export] private ape m_Ape;
    private map m_Map;

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

    public override void Enter()
    {

        //adjust pathfinding for other apes
        m_Map.SetPointSolid(m_Ape.GetPrevNavCoords(), false);
        m_Map.SetPointSolid(m_Ape.GetNavCoords(), false);
        m_IDPath = m_Map.getIdPath(m_Ape.GetNavCoords(), m_Ape.GetPrevNavCoords(), true);
        m_Map.SetPointSolid(m_Ape.GetNavCoords(), true);
        m_Map.SetPointSolid(m_Ape.GetPrevNavCoords(), true);

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

    public override void Exit()
    {
        m_Ape.Velocity = Vector3.Zero;
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
        }
    }

    public void SetMap(ref map Map)
    {
        m_Map = Map;
    }
}
