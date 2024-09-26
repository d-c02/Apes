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
	[Export] protected float m_WorkRadius = 0.5f;
	[Export] protected float m_VerticalOffset = 1.5f;

	[Export] public Node3D m_WorkAnchor;
	protected Sprite3D[] m_WorkSprites;


	protected int m_WorkAspect;
	enum WorkAspects {Empty, Insight, Influence, Fervor, Any};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConfigureWork();
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
        for (int i = 0; i < m_MaxWork; i++)
		{
			if (i >= CurRow * m_NumWorkPerRow)
			{
				if (i > m_MaxWork - m_NumWorkPerRow)
				{
					OddRowSize = (m_MaxWork - (m_NumWorkPerRow * CurRow)) % 2 == 1;
				}
                CurRow++;
            }

			int centerOffset = i % m_NumWorkPerRow;
			if (OddRowSize)
			{
				if (centerOffset % 2 == 0)
				{
					m_WorkSprites[i].Position = new Vector3(m_WorkRadius * centerOffset, (CurRow - 1) * m_VerticalOffset, 0);
				}
				else
				{
                    m_WorkSprites[i].Position = new Vector3(m_WorkRadius * -1 * (centerOffset + 1), (CurRow - 1) * m_VerticalOffset, 0);
                }
			}
			else
			{
                if (centerOffset % 2 == 0)
                {
                    m_WorkSprites[i].Position = new Vector3((m_WorkRadius * centerOffset) + (m_WorkRadius), (CurRow - 1) * m_VerticalOffset, 0);
                }
                else
                {
                    m_WorkSprites[i].Position = new Vector3((m_WorkRadius * -1 * (centerOffset - 1)) - (m_WorkRadius), (CurRow - 1) * m_VerticalOffset, 0);
                }
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

		return remainder;
	}

	public int RemoveWork(int amount)
	{
		int remainder = 0;
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
