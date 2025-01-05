using Godot;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using static DeckInterface;
using System.Collections.Generic;
using System.ComponentModel.Design;

public abstract partial class Project : Node3D
{
    [Export] protected bool m_Persists = false;
	[Export] protected ProjectEnum m_NextProject = ProjectEnum.None;
    [Export] protected bool m_Destructible = false;
	protected bool m_Finished = false;
    protected int m_CurStage = 0;
    protected int m_MaxStage = 1;
    protected int m_QueuedWork = 0;
	protected int m_Work = 0;

	[Export] protected int m_MaxWork = 1;
	[Export] protected int m_NumWorkPerRow = 5;
	protected float m_DisappearProximity = 3;
	[Export] protected float m_WorkRadius = 1.05f;
	[Export] protected float m_VerticalOffset = 1.2f;
	protected Vector3I m_Coords;
	protected ProjectEnum m_ID;

	[Export] public Node3D m_WorkAnchor;
	protected AnimatedSprite3D[] m_WorkSprites;
	[Export] protected Vector2I m_Dimensions;

	[Export]
	protected WorkAspectEnum m_WorkAspect;

    protected Dictionary<Vector2I, bool> m_ApeSlots;

	//In range of 1-10
	[Export(PropertyHint.Range, "1,10,")] 
	protected int m_Spite = 5;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        ConfigureWork();
        m_ApeSlots = new Dictionary<Vector2I, bool>();
		ConfigureSlots();
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

	public bool IsSlot(Vector2I Coords)
	{
		return m_ApeSlots.ContainsKey(Coords);
	}

	public Vector2I GetOpenSlot()
	{
		Random rnd = new Random();
		foreach (KeyValuePair<Vector2I, bool> entry in m_ApeSlots.OrderBy(x => rnd.Next()))
		{
			if (entry.Value)
			{
				return entry.Key;
			}
		}
		return new Vector2I(-1, -1);
	}

	public void SetOpenSlot(Vector2I slot, bool open)
	{
		m_ApeSlots[slot] = open;
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

	public virtual void ConfigureSlots()
	{

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BillboardWork();
	}

	public void QueueWork(AspectEnum aspect, int amount)
	{
		if (m_WorkAspect == WorkAspectEnum.Any)
		{
			QueueAddWork(aspect, amount);
		}
		else if (aspect == AspectEnum.Any)
		{
			QueueAddWork(aspect, amount);
		}
		else if (m_WorkAspect == WorkAspectEnum.Fervor)
		{
			if (aspect == AspectEnum.Fervor)
			{
				QueueAddWork(aspect, amount);
			}
			else if (aspect == AspectEnum.Influence)
			{
				QueueRemoveWork(aspect, amount);
			}
			else
			{
				throw new Exception("Invalid work being assigned!!!");
			}
		}
		else if (m_WorkAspect == WorkAspectEnum.Insight)
		{
			if (aspect == AspectEnum.Insight)
			{
				QueueAddWork(aspect, amount);
			}
			else if (aspect == AspectEnum.Fervor)
			{
				QueueRemoveWork(aspect, amount);
			}
			else
			{
				throw new Exception("Invalid work being assigned!!!");
			}
		}
		else if (m_WorkAspect == WorkAspectEnum.Influence)
		{
			if (aspect == AspectEnum.Influence)
			{
				QueueAddWork(aspect, amount);
			}
			else if (aspect == AspectEnum.Insight)
			{
				QueueRemoveWork(aspect, amount);
			}
			else
			{
				throw new Exception("Invalid work being assigned!!!");
			}
		}
    }

	private void QueueAddWork(AspectEnum aspect, int amount)
	{
        int prevWork = m_QueuedWork;
        m_QueuedWork += amount;

		if (m_QueuedWork > 0 && m_QueuedWork <= m_MaxWork)
		{
			for (int i = prevWork; i < m_QueuedWork; i++)
			{
				if (i < m_Work)
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
				else
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
					else if (aspect == AspectEnum.Any)
					{
						animation = "any_queued";
					}
					m_WorkSprites[i].Animation = animation;
				}
			}
		}
    }

	private void QueueRemoveWork(AspectEnum aspect, int amount)
	{
		int prevWork = m_QueuedWork;
        m_QueuedWork -= amount;

		if (m_QueuedWork >= 0 && m_QueuedWork < m_MaxWork)
		{
            for (int i = prevWork - 1; i >= m_QueuedWork; i--)
            {
                if (m_Work <= i)
                {
                    m_WorkSprites[i].Animation = "empty";
                }
                else
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
            }
        }
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
        if (m_QueuedWork < 0)
        {
            m_QueuedWork = 0;
        }

        if (m_QueuedWork >= m_MaxWork)
		{
			m_Finished = true;
			OnFinish();
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

	public abstract void OnFinish();


	public bool IsFinished()
	{
		return m_Finished;
	}

	public bool Persists()
	{
		return m_Persists;
	}

	public bool HasNextProject()
	{
		return m_NextProject != ProjectEnum.None;
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

	public WorkAspectEnum GetWorkAspect()
	{
		return m_WorkAspect;
	}

	public int GetSpite()
	{
		return m_Spite;
	}

	public virtual void ResetPhase()
	{
		m_Finished = false;
		m_Work = 0;
		m_QueuedWork = 0;
        for (int i = 0; i < m_MaxWork; i++)
		{
            m_WorkSprites[i].Animation = "empty";
        }

    }
}
