using Godot;
using System;

public partial class ApeWorkingTransit : State
{
    [Export]
    ape m_Ape;

    map m_Map;

    ApeManager m_ApeManager;

    [Export]
    public int m_IdleSpeed { get; set; } = 14;

    [Export]
    public int m_IdleAcceleration { get; set; } = 75;

    [Export]
    private int m_Gravity { get; set; } = 50;

    private Vector3 m_TargetVelocity = Vector3.Zero;

    Godot.Collections.Array<Vector2I> m_IDPath;

    Vector2 m_NextPos;

    const float m_MaxNavPointDist = 0.5f;

    const float m_TransitVelocity = 1000.0f;

    int m_NextPosCtr;

    public override void Enter()
    {

        Vector2I SlotOffset = m_ApeManager.GetOpenSlot(m_Ape.GetTargetProject());
        if (SlotOffset.X == -1 || SlotOffset.Y == -1)
        {
            m_Ape.SetAction(DeckInterface.ActionEnum.Idle);
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeIdle");
        }

        m_Ape.SetSlot(SlotOffset);
        m_ApeManager.SetOpenSlot(m_Ape.GetTargetProject(), SlotOffset, false);

        Vector2I FinalPos = m_Map.PosCoordsToNavCoords(m_ApeManager.GetProjectLocation(m_Ape.GetTargetProject()) + SlotOffset);

        //adjust pathfinding for other apes
        m_Map.SetPointSolid(m_Ape.GetNavCoords(), false);
        m_Map.SetPointSolid(FinalPos, false);
        m_IDPath = m_Map.getIdPath(m_Ape.GetNavCoords(), FinalPos, true);
        m_Map.SetPointSolid(FinalPos, true);
        m_Map.SetPointSolid(m_Ape.GetNavCoords(), true);


        if (m_IDPath.Count > 2)
        {
            //Makes ape exit closer to target project
            m_Map.SetPointSolid(m_Ape.GetNavCoords(), false);
            m_Ape.SetPrevNavCoords(m_IDPath[m_IDPath.Count - 2]);
            m_Map.SetPointSolid(m_Ape.GetPrevNavCoords());
        }
        else
        {
            m_Ape.SetPrevNavCoords(m_Ape.GetNavCoords());
        }
        m_Ape.SetNavCoords(m_IDPath[m_IDPath.Count - 1]);

        if (m_IDPath.Count <= 1)
        {
            EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorking");
        }
        else
        {
            m_NextPosCtr = 1;
            m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
            //Vector2 v2Pos = m_Map.GetPointPosition(FinalPos);
            //m_Ape.GlobalPosition = new Vector3(v2Pos.X, m_ApeManager.GetGlobalPosition(m_Ape.GetTargetProject()).Y, v2Pos.Y);
        }
    }

    public override void Exit()
    {
        m_Ape.Velocity = new Vector3(0, 0, 0);
        Vector2I SlotOffset = m_Ape.GetSlot();
        Vector2I FinalPos = m_Map.PosCoordsToNavCoords(m_ApeManager.GetProjectLocation(m_Ape.GetTargetProject()) + SlotOffset);

        Vector2 v2Pos = m_Map.GetPointPosition(FinalPos);
        m_Ape.GlobalPosition = new Vector3(v2Pos.X, m_ApeManager.GetGlobalPosition(m_Ape.GetTargetProject()).Y, v2Pos.Y);

        Vector2I dims = m_ApeManager.GetProjectDimensions(m_Ape.GetTargetProject());
        Vector2I ProjectPos = FinalPos - SlotOffset + dims;
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Math.Abs(m_NextPos.X - m_Ape.GlobalPosition.X) < m_MaxNavPointDist && Math.Abs(m_NextPos.Y - m_Ape.GlobalPosition.Z) < m_MaxNavPointDist)
        {
            m_NextPosCtr += 1;
            if (m_NextPosCtr >= m_IDPath.Count)
            {
                m_IDPath.Clear();
                EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorking");
            }
            else
            {
                m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
            }
        }
        else
        {
            Vector3 direction = new Vector3(m_NextPos.X - m_Ape.GlobalPosition.X, 0, m_NextPos.Y - m_Ape.GlobalPosition.Z).Normalized();

            m_TargetVelocity.X = direction.X * m_TransitVelocity * (float)delta;
            m_TargetVelocity.Z = direction.Z * m_TransitVelocity * (float)delta;
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

    public override void Update(double delta)
    {
        
    }

    public void SetMap(ref map Map)
    {
        m_Map = Map;
    }

    public void SetApeManager(ref ApeManager apeManager)
    {
        m_ApeManager = apeManager;
    }
}
