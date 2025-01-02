using Godot;
using SmallApesv2;
using System;
using System.Diagnostics;
using static SmallApesv2.PlayerDeckInterface;

public partial class Card : Area2D
{
	// Called when the node enters the scene tree for the first time.
	private Vector2 m_BasePosition;
	private Vector2 m_TargetPosition;
	private float m_VerticalSelectionOffset = -28.0f;
	private float m_TransitionSpeed = 5.0f;
	private bool m_Active = false;
	private Vector2 m_ClickOffset;
	private bool m_Held = false;
	private int m_CardActivationVerticalOffset = -150;
	private bool m_Playable = false;
	private ImmediateMesh m_DebugRaycastMesh;
	[Export] MeshInstance3D m_MeshInstance3D;
	private PlayerDeckInterface m_DeckInterface;

	public override void _Ready()
	{
		m_BasePosition = Position;
		m_TargetPosition = m_BasePosition;
		m_ClickOffset = new Vector2(0, 0);
	}

	public void SetDeckInterface(PlayerDeckInterface deckInterface)
	{
		m_DeckInterface = deckInterface;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
        Position = Position.Lerp(m_TargetPosition, m_TransitionSpeed * (float) delta);
		if (Input.IsActionJustPressed("Select") && m_Active)
		{
			m_Held = true;
			ZIndex = (int) RenderingServer.CanvasItemZMax;

			//Unused. Implement in the future to have the card not snap to mouse pos.
			m_ClickOffset = GetLocalMousePosition() - Position;
		}
		if (m_Held)
		{
			if (!Input.IsActionPressed("Select"))
			{
				if (m_Playable)
				{
                    if (CheckCollision())
                    {

                    }
                }
				m_Held = false;
                m_Active = false;
                ZIndex = (int)RenderingServer.CanvasItemZMin;
                m_TargetPosition = m_BasePosition;
            }
			else if (Input.IsActionJustPressed("CancelSelect"))
			{
                m_Held = false;
                m_Active = false;
                ZIndex = (int)RenderingServer.CanvasItemZMin;
                m_TargetPosition = m_BasePosition;
            }
			else
			{
				//m_TargetPosition = ToLocal(GetViewport().GetMousePosition());

				//Weird. Lags behind without the * 10 but seems like an inelegant solution. Will do for now.
				m_TargetPosition = (GetLocalMousePosition()) * 10;
            }
		}

        if (Position.Y < m_CardActivationVerticalOffset)
        {
            Visible = false;
			m_Playable = true;
        }
        else
        {
            Visible = true;
			m_Playable = false;
        }
    }

	public bool CheckCollision()
	{
		Camera3D camera = GetViewport().GetCamera3D();
		Vector3 start = camera.ProjectRayOrigin(GetViewport().GetMousePosition());
		Vector3 end = camera.ProjectPosition(GetViewport().GetMousePosition(), 1000);

        PhysicsDirectSpaceState3D spaceState = camera.GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(start, end, 4);
        var result = spaceState.IntersectRay(query);
		
		m_DebugRaycastMesh = new ImmediateMesh();
        m_DebugRaycastMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        m_DebugRaycastMesh.SurfaceSetNormal(new Vector3(0, 0, 1));
        m_DebugRaycastMesh.SurfaceSetUV(new Vector2(0, 0));
        m_DebugRaycastMesh.SurfaceAddVertex(start);

        m_DebugRaycastMesh.SurfaceSetNormal(new Vector3(0, 0, 1));
        m_DebugRaycastMesh.SurfaceSetUV(new Vector2(0, 1));
        m_DebugRaycastMesh.SurfaceAddVertex(end);

        m_DebugRaycastMesh.SurfaceEnd();


		m_MeshInstance3D.Mesh = m_DebugRaycastMesh;

		if (result.ContainsKey("collider"))
		{
            Node collider = (Node) result["collider"];
			Node target;

            if (result["collider"].Obj is StaticBody3D)
			{
				target = collider.GetParent();
			}
			else if (result["collider"].Obj is CharacterBody3D)
			{
				target = collider;
			}
			else
			{
				throw new Exception("Invalid card target, provide implementation");
			}

			return m_DeckInterface.DoCardAction(target);
        }

        return false;
	}

    public override void _MouseEnter()
    {
		if (!m_Active)
		{
            m_Active = true;
            m_TargetPosition = m_BasePosition + new Vector2(0, m_VerticalSelectionOffset);
        }
    }

    public void SetBasePosition(Vector2 position)
	{
		m_BasePosition = position;
		m_TargetPosition = position;
	}

    public override void _MouseExit()
    {
		if (!m_Held)
		{
            m_Active = false;
            m_TargetPosition = m_BasePosition;
        }
    }

	
}
