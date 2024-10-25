using Godot;
using System;
using System.Linq;

public abstract partial class Project : Node3D
{
    protected bool m_Complete = false;
    protected bool m_Persists = false;
    protected bool m_Destructible = false;
    protected int m_CurStage = 0;
    protected int m_MaxStage = 1;
    protected int m_CurWork = 0;
	protected int m_MaxWork = 1;
	protected int m_NumWorkPerRow = 5;
	protected float m_DisappearProximity = 3;
	[Export] protected float m_WorkRadius = 0.8f;
	[Export] protected float m_VerticalOffset = 1f;

	[Export] public Node3D m_WorkAnchor;
	protected Sprite3D[] m_WorkSprites;


	protected int m_WorkAspect;
	protected enum WorkAspects {Empty, Insight, Influence, Fervor, Any};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

        ConfigureWork();
	}

	public void UpdateVerticalPosition()
	{
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition, new Vector3(GlobalPosition.X, -1, GlobalPosition.Z), 1);
        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            GlobalPosition = (Vector3)result["position"];
        }
    }

	protected void ConfigureWork()
	{
		m_WorkSprites = new Sprite3D[m_MaxWork];
		for (int i = 0; i < m_MaxWork; i++)
		{
			//Instantiate
            var workIcon = new PackedScene();
            workIcon = ResourceLoader.Load<PackedScene>("res://Scenes/Projects/Supporting/WorkIcon.tscn");
			m_WorkSprites[i] = workIcon.Instantiate<Sprite3D>();
            m_WorkAnchor.AddChild(m_WorkSprites[i]);
            m_WorkSprites[i].Frame = (int)WorkAspects.Empty;
        }

		int CurRow = 1;
        bool OddRowSize = m_NumWorkPerRow % 2 == 1;
		int RowSize = m_NumWorkPerRow;
        for (int i = 0; i < m_MaxWork; i++)
		{
			if (i >= CurRow * m_NumWorkPerRow)
			{
				if (i > m_MaxWork - m_NumWorkPerRow)
				{
					RowSize = (m_MaxWork - (m_NumWorkPerRow * CurRow));
                    OddRowSize = (m_MaxWork - (m_NumWorkPerRow * CurRow)) % 2 == 1;
				}
                CurRow++;
            }

			int centerOffset = i % m_NumWorkPerRow;
            int center = (int) Math.Floor((double)RowSize / 2);
            if (OddRowSize)
			{
				m_WorkSprites[i].Position = new Vector3((m_WorkRadius * centerOffset) - center, (CurRow - 1) * m_VerticalOffset, 0);

			}
			else
			{
                m_WorkSprites[i].Position = new Vector3((m_WorkRadius * centerOffset) - (center + (m_WorkRadius / 2)), (CurRow - 1) * m_VerticalOffset, 0);
            }
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BillboardWork();
	}

    public abstract void OnFinish();

	public int AddWork(int aspect, int amount)
	{
		int remainder = 0;
		int prevWork = m_CurWork;
		if (amount + m_CurWork < m_MaxWork)
		{
			m_CurWork += amount;
		}
		else
		{
			remainder = amount + m_CurWork - m_MaxWork;
            m_CurWork = m_MaxWork;
			m_Complete = true;
		}

		for (int i = prevWork; i < m_CurWork; i++)
		{
			m_WorkSprites[i].Frame = aspect;
		}
		return remainder;
	}

	public int RemoveWork(int amount)
	{
		int remainder = 0;
		int prevWork = m_CurWork;
		if (m_CurWork - amount >= 0)
		{
			m_CurWork -= amount;
		}
		else
		{
			remainder = amount - m_CurWork;
			m_CurWork = 0;
		}
		if (m_CurWork < m_MaxWork)
		{
			m_Complete = false;
		}

		for (int i = prevWork - 1; i >= m_CurWork; i--)
		{
			m_WorkSprites[i].Frame = (int)WorkAspects.Empty;
		}
		return remainder;
	}

	protected void BillboardWork()
	{


        Vector3 CameraPos = GetViewport().GetCamera3D().GlobalPosition;

		//Faces camera no matter what 
		//m_WorkAnchor.LookAt(CameraPos);

        //Faces camera rotation but not position
        Vector3 cameraBasisZ = GetViewport().GetCamera3D().GlobalTransform.Basis.Z;
        float cameraPosY = GetViewport().GetCamera3D().GlobalPosition.Y;
        m_WorkAnchor.LookAt(new Vector3(m_WorkAnchor.GlobalPosition.X - cameraBasisZ.X, m_WorkAnchor.GlobalPosition.Y, m_WorkAnchor.GlobalPosition.Z - cameraBasisZ.Z));


		//Makes work disappear when camera comes close
        Vector2 CameraDist = new Vector2(GlobalPosition.X - CameraPos.X, GlobalPosition.Z - CameraPos.Z);
        if (CameraDist.Length() < m_DisappearProximity)
        {
            m_WorkAnchor.Visible = false;
        }
        else
        {
            m_WorkAnchor.Visible = true;
        }
    }
}
