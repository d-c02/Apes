using Godot;
using System;

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

	public override void _Ready()
	{
		m_BasePosition = Position;
		m_TargetPosition = m_BasePosition;
		m_ClickOffset = new Vector2(0, 0);
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

			//Unused. Implement in the future to have the card not snap to mouse pos.
			m_ClickOffset = GetLocalMousePosition() - Position;
		}
		if (m_Held)
		{
			if (!Input.IsActionPressed("Select"))
			{
				m_Held = false;
                m_Active = false;
                m_TargetPosition = m_BasePosition;
            }
			else
			{
				//m_TargetPosition = ToLocal(GetViewport().GetMousePosition());
				m_TargetPosition = (GetLocalMousePosition()) * 10;
            }
		}
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
