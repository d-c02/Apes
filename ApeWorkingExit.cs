using Godot;
using System;
using System.Diagnostics;

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

    private ApeManager m_ApeManager;

    const float m_MaxStuckTime = 1.0f;
    private float m_StuckTime = 0.0f;

    public override void Enter()
    {
        m_Ape.SetAnimState("parameters/BodyAnimGate/transition_request", "Walking");

        Vector2I SlotOffset = new Vector2I(-1, -1);
        if (m_Ape.IsWorking())
        {
            SlotOffset = m_ApeManager.GetOpenSlot(m_Ape.GetTargetProject());
            if (SlotOffset.X == -1 || SlotOffset.Y == -1)
            {
                m_Ape.SetAction(DeckInterface.ActionEnum.Idle);
            }
        }

        //We run this check again because we potentially set the ape's action in the previous call. Messy I know.
        if (m_Ape.IsWorking())
        {
            m_Ape.SetSlot(SlotOffset);
            m_ApeManager.SetOpenSlot(m_Ape.GetTargetProject(), SlotOffset, false);

            Vector2I FinalPos = m_Map.PosCoordsToNavCoords(m_ApeManager.GetProjectLocation(m_Ape.GetTargetProject()) + SlotOffset);

            //adjust pathfinding for other apes
            m_Map.SetPointSolid(m_Ape.GetNavCoords(), false);
            m_Map.SetPointSolid(FinalPos, false);
            m_IDPath = m_Map.getIdPath(m_Ape.GetNavCoords(), FinalPos, true);
            m_Map.SetPointSolid(FinalPos, true);
            m_Map.SetPointSolid(m_Ape.GetNavCoords(), true);

            m_Ape.SetNavCoords(m_IDPath[m_IDPath.Count - 1]);

            if (m_IDPath.Count <= 1)
            {
                EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorking");
            }
            else
            {
                if (m_IDPath.Count > 2)
                {
                    //Makes ape exit closer to target project
                    m_Map.SetPointSolid(m_Ape.GetPrevNavCoords(), false);
                    m_Ape.SetPrevNavCoords(m_IDPath[m_IDPath.Count - 2]);
                    m_Map.SetPointSolid(m_Ape.GetPrevNavCoords());
                }

                m_NextPosCtr = 1;
                m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
                //Vector2 v2Pos = m_Map.GetPointPosition(FinalPos);
                //m_Ape.GlobalPosition = new Vector3(v2Pos.X, m_ApeManager.GetGlobalPosition(m_Ape.GetTargetProject()).Y, v2Pos.Y);
            }
        }
        else
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
    }

    public override void Exit()
    {
        m_Ape.Velocity = Vector3.Zero;
        m_StuckTime = 0;
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
                if (m_Ape.IsWorking())
                {
                    EmitSignal(SignalName.Transitioned, this.Name + "", "ApeWorking");
                }
                else
                {
                    EmitSignal(SignalName.Transitioned, this.Name + "", "ApeIdle");
                }
            }
            else
            {
                m_NextPos = m_Map.GetPointPosition(m_IDPath[m_NextPosCtr]);
            }
        }
        else
        {
            if (m_StuckTime < m_MaxStuckTime)
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

                m_StuckTime = 0;
                m_Ape.Velocity = m_TargetVelocity;
                m_Ape.LookAt(m_Ape.GlobalPosition + m_TargetVelocity);
            }
            else
            {
                Debug.WriteLine("Teleported ape!");

                //Teleport ape to next position
                m_Ape.Velocity = Vector3.Zero;
                m_StuckTime = 0;

                Vector3 pos = new Vector3(m_NextPos.X, m_Ape.GlobalPosition.Y + 2, m_NextPos.Y);
                PhysicsDirectSpaceState3D spaceState = m_Ape.GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(pos, new Vector3(pos.X, -1, pos.Z), 1);
                var result = spaceState.IntersectRay(query);
                if (result.Count > 0)
                {
                    m_Ape.GlobalPosition = (Vector3)result["position"];
                }
            }
        }
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
