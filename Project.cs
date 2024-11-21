using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static DeckInterface;

public abstract partial class Project : Node3D
{
    [Export] protected bool m_Persists = false;
	[Export] protected ProjectEnum m_NextProject;
    [Export] protected bool m_Destructible = false;
	protected bool m_Finished = false;
    protected int m_CurStage = 0;
    protected int m_MaxStage = 1;
    protected int m_QueuedWork = 0;
	protected int m_Work = 0;

	[Export] protected int m_MaxWork = 1;
	[Export] protected int m_NumWorkPerRow = 5;
	protected float m_DisappearProximity = 3;
	[Export] protected float m_WorkRadius = 0.8f;
	[Export] protected float m_VerticalOffset = 1f;
	protected Vector3I m_Coords;
	protected ProjectEnum m_ID;

	[Export] public Node3D m_WorkAnchor;
	protected AnimatedSprite3D[] m_WorkSprites;
	[Export] protected Vector2I m_Dimensions;

	[Export]
	protected WorkAspectEnum m_WorkAspect;
	protected enum WorkAspectEnum {Empty, Insight, Influence, Fervor, Any};

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

	public Vector2I GetDimensions()
	{
		return m_Dimensions;
	}

	protected void ConfigureWork()
	{
		//Change?
		m_WorkAnchor.SetDisableScale(true);
        
		m_WorkSprites = new AnimatedSprite3D[m_MaxWork];
		for (int i = 0; i < m_MaxWork; i++)
		{
			//Instantiate
            var workIcon = new PackedScene();
            workIcon = ResourceLoader.Load<PackedScene>("res://Scenes/Projects/Supporting/WorkIcon.tscn");
			m_WorkSprites[i] = workIcon.Instantiate<AnimatedSprite3D>();
            m_WorkAnchor.AddChild(m_WorkSprites[i]);
            m_WorkSprites[i].Animation = "empty";
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

	public int QueueWork(AspectEnum aspect, int amount)
	{
		int remainder = 0;
		int prevWork = m_QueuedWork;
		if (amount + m_QueuedWork < m_MaxWork)
		{
            m_QueuedWork += amount;
		}
		else
		{
			remainder = amount + m_QueuedWork - m_MaxWork;
            m_QueuedWork = m_MaxWork;
		}

		for (int i = prevWork; i < m_QueuedWork; i++)
		{
			string animation = "";
			if (aspect == AspectEnum.Fervor)
			{
				animation = "fervor_queued";
			}
			else if (aspect == AspectEnum.Influence)
			{
                animation = "influence_queued";
            }
            else if (aspect == AspectEnum.Insight)
            {
                animation = "insight_queued";
            }

            m_WorkSprites[i].Animation = animation;
        }

		return remainder;
	}

	public int RemoveWork(int aspect, int amount)
	{
		int remainder = 0;
		int prevWork = m_QueuedWork;
		if (m_QueuedWork - amount >= 0)
		{
            m_QueuedWork -= amount;
		}
		else
		{
			remainder = amount - m_QueuedWork;
            m_QueuedWork = 0;
		}

		for (int i = prevWork - 1; i >= m_QueuedWork; i--)
		{
			m_WorkSprites[i].Frame = (int)WorkAspectEnum.Empty;
		}
		return remainder;
	}

	public void ClearQueuedWork()
	{
        if (m_QueuedWork > m_Work)
        {
            for (int i = m_Work; i < m_QueuedWork; i++)
            {
				m_WorkSprites[i].Animation = "empty";
            }
        }
        else if (m_QueuedWork < m_Work)
        {
            for (int i = m_QueuedWork; i < m_Work; i++)
            {
                if (m_WorkAspect == WorkAspectEnum.Any)
                {
                    m_WorkSprites[i].Animation = "any";
                }
                else if (m_WorkAspect == WorkAspectEnum.Insight)
                {
                    m_WorkSprites[i].Animation = "insight";
                }
                else if (m_WorkAspect == WorkAspectEnum.Influence)
                {
                    m_WorkSprites[i].Animation = "influence";
                }
                else if (m_WorkAspect == WorkAspectEnum.Fervor)
                {
                    m_WorkSprites[i].Animation = "fervor";
                }
            }
        }
		m_QueuedWork = m_Work;
    }

	public void ProcessQueuedWork()
	{
		if (m_QueuedWork >= m_MaxWork)
		{
			m_Finished = true;
		}
		else
		{
			if (m_QueuedWork > m_Work)
			{
                for (int i = m_Work; i < m_QueuedWork; i++)
                {
					//Create generic work texture for generic work instance, and have global work texture int corresponding to insight, influence, fervor, any
                    if (m_WorkAspect == WorkAspectEnum.Any)
					{
						m_WorkSprites[i].Animation = "any";
                    }
					else if (m_WorkAspect == WorkAspectEnum.Insight)
					{
                        m_WorkSprites[i].Animation = "insight";
                    }
                    else if (m_WorkAspect == WorkAspectEnum.Influence)
                    {
                        m_WorkSprites[i].Animation = "influence";
                    }
                    else if (m_WorkAspect == WorkAspectEnum.Fervor)
                    {
                        m_WorkSprites[i].Animation = "fervor";
                    }
                }
            }
			else if (m_QueuedWork < m_Work)
			{
				for (int i = m_QueuedWork; i < m_Work; i++)
				{
					m_WorkSprites[i].Animation = "empty";
				}
			}

			m_Work = m_QueuedWork;
		}
	}

	public bool IsWorkEmpty()
	{
		return m_QueuedWork == 0;
	}

	public bool IsWorkFull()
	{
		return m_QueuedWork >= m_MaxWork;
	}

	public void SetCoords(Vector3I Coords)
	{
		m_Coords = Coords;
	}

	public Vector3I GetCoords()
	{
		return m_Coords;
	}

	public void SetID(ProjectEnum ID)
	{
		m_ID = ID;
	}

	public ProjectEnum GetID()
	{
		return m_ID;
	}

	public virtual void OnFinish()
	{

	}


	public bool IsFinished()
	{
		return m_Finished;
	}

	public bool Persists()
	{
		return m_Persists;
	}

	public ProjectEnum GetNextProject()
	{
		return m_NextProject;
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
