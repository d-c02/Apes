using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DeckInterface;
using System.Collections.Generic;
using System.Collections.Specialized;

public partial class ApeManager : Node
{

	// Called when the node enters the scene tree for the first time.
	
	[Export] private TimeManager m_TimeManager;

	[Export] private map m_Map;

	private List<ape> m_Apes;

	private System.Collections.Generic.Dictionary<ProjectEnum, Project> m_Projects;

	private List<ProjectEnum> m_DeadProjectIDs;

	private int m_IdolStatueProjectIndex;

	private int m_InfluenceProjectIndex;

    private int m_InsightProjectIndex;

    private int m_FervorProjectIndex;

	private System.Collections.Generic.Dictionary<Tuple<AspectEnum, ActionEnum>, ActionEnum> m_ActionTransformations;

	private enum ActionTargetsEnum {None,  Project, Ape};

	private enum ApeTargetAspectEnum {Self, Enemy, Weak}

	private System.Collections.Generic.Dictionary<ActionEnum, ActionTargetsEnum> m_ActionTargets;

	private System.Collections.Generic.Dictionary<ActionEnum, List<ApeTargetAspectEnum>> m_TargetApes;

	[Export]
    private GpuParticles3D m_FervorApeDeathParticles;

    [Export]
    private GpuParticles3D m_InsightApeDeathParticles;

    [Export]
    private GpuParticles3D m_InfluenceApeDeathParticles;

	private const float m_ApeDeathParticlesVerticalOffset = 3.0f;

    public override void _Ready()
	{
		m_Apes = new List<ape>();
		m_Projects = new System.Collections.Generic.Dictionary<ProjectEnum, Project>();
		m_DeadProjectIDs = new List<ProjectEnum>();
		m_ActionTransformations = new System.Collections.Generic.Dictionary<Tuple<AspectEnum, ActionEnum>, ActionEnum>();

		PrepareActionTargets();
		PrepareTargetApes();

        m_Map.Generate();

		SpawnInitialProjects();

		for (int i = 0; i < 15; i++)
		{
			SpawnApe();
		}

		ConfigureDecks();

		//DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
    }

    private void PrepareActionTargets()
    {
		m_ActionTargets = new System.Collections.Generic.Dictionary<ActionEnum, ActionTargetsEnum>();

		//List of actions and their target below

		//None
		m_ActionTargets[ActionEnum.Idle] = ActionTargetsEnum.None;

		//Project
		m_ActionTargets[ActionEnum.Work_One] = ActionTargetsEnum.Project;
        m_ActionTargets[ActionEnum.Work_Two] = ActionTargetsEnum.Project;
        m_ActionTargets[ActionEnum.Work_Three] = ActionTargetsEnum.Project;

		//Ape
		m_ActionTargets[ActionEnum.Stun] = ActionTargetsEnum.Ape;
    }

	private void PrepareTargetApes()
	{
		m_TargetApes = new System.Collections.Generic.Dictionary<ActionEnum, List<ApeTargetAspectEnum>>();

		m_TargetApes[ActionEnum.Stun] = new List<ApeTargetAspectEnum>();
		m_TargetApes[ActionEnum.Stun].Add(ApeTargetAspectEnum.Enemy);
        m_TargetApes[ActionEnum.Stun].Add(ApeTargetAspectEnum.Weak);
    }

	public bool HasTargetProject(ActionEnum action)
	{
		return m_ActionTargets[action] == ActionTargetsEnum.Project;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		
		if (Input.IsActionJustPressed("DEBUG_SPAWN_APE"))
		{
			SpawnApe();
		}


		if (Input.IsActionJustPressed("DEBUG_START_NEW_TIME_PHASE"))
		{
			bool canStart = true;
			for (int i = 0; i < m_Apes.Count; i++)
			{
				if (!m_Apes[i].IsReadyForNextPhase())
				{
					canStart = false;
				}
			}
			if (canStart)
			{
                StartNewPhase();
            }
		}

		if (Input.IsActionJustPressed("DEBUG_CLEAR_APE_WORK"))
		{
			foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
			{
				m_Projects[entry.Key].ClearApeWork();
			}
		}
    }

	private void StartNewPhase()
	{

		m_TimeManager.IncrementTime();

        foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
        {
            m_Projects[entry.Key].ProcessQueuedWork();
			if (m_Projects[entry.Key].IsFinished())
			{
				if (!m_Projects[entry.Key].Persists())
				{
                    m_DeadProjectIDs.Add(entry.Key);
                }
				else
				{
					m_Projects[entry.Key].ResetPhase();
				}
			}
        }

		PruneProjects();
		m_DeadProjectIDs.Clear();


        for (int i = 0; i < m_Apes.Count; i++)
        {
            m_Apes[i].StartNewPhase();
        }
		DrawTargets();
        QueueActions();
    }

	//This function REALLY REALLY sucks. Fix it later

	private void DrawTargets()
	{
        System.Collections.Generic.Dictionary<AspectEnum, List<ProjectEnum>> ProjectDict = new System.Collections.Generic.Dictionary<AspectEnum, List<ProjectEnum>>();

        foreach (AspectEnum value in Enum.GetValues(typeof(AspectEnum)))
        {
            ProjectDict[value] = new List<ProjectEnum>();
        }

        foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
        {
            WorkAspectEnum workAspect = entry.Value.GetWorkAspect();
            if (workAspect == WorkAspectEnum.Any)
            {
                ProjectDict[AspectEnum.Fervor].Add(entry.Key);
                ProjectDict[AspectEnum.Insight].Add(entry.Key);
                ProjectDict[AspectEnum.Influence].Add(entry.Key);
            }
            else if (workAspect == WorkAspectEnum.Fervor)
            {
                ProjectDict[AspectEnum.Fervor].Add(entry.Key);
            }
            else if (workAspect == WorkAspectEnum.Insight)
            {
                ProjectDict[AspectEnum.Insight].Add(entry.Key);
            }
            else if (workAspect == WorkAspectEnum.Influence)
            {
                ProjectDict[AspectEnum.Influence].Add(entry.Key);
            }
        }

        System.Collections.Generic.Dictionary<AspectEnum, List<int>> ApeDict = new System.Collections.Generic.Dictionary<AspectEnum, List<int>>();

		ApeDict[AspectEnum.Insight] = new List<int>();
        ApeDict[AspectEnum.Influence] = new List<int>();
        ApeDict[AspectEnum.Fervor] = new List<int>();

        for (int i = 0; i < m_Apes.Count; i++)
		{
			if (m_Apes[i].GetAspect() == AspectEnum.Insight)
			{
				ApeDict[AspectEnum.Insight].Add(i);
			}
			else if (m_Apes[i].GetAspect() == AspectEnum.Influence)
			{
                ApeDict[AspectEnum.Influence].Add(i);
            }
			else if (m_Apes[i].GetAspect() == AspectEnum.Fervor)
			{
                ApeDict[AspectEnum.Fervor].Add(i);
            }
		}

        Random rnd = new Random();
        for (int i = 0; i < m_Apes.Count; i++)
        {

			if (!m_ActionTargets.ContainsKey(m_Apes[i].GetAction()))
			{
				throw new Exception("No target for action " + m_Apes[i].GetAction().ToString());
			}

            if (m_ActionTargets[m_Apes[i].GetAction()] == ActionTargetsEnum.Project)
            {
                List<int> SpiteProjects = new List<int>();
                List<ProjectEnum> Projects = new List<ProjectEnum>();
                int projectCount = 0;

                ProjectEnum formerProject = m_Apes[i].GetTargetProject();
                int numAvailableProjects = ProjectDict[m_Apes[i].GetAspect()].Count;
                int numAvailableEnemyAspectProjects = ProjectDict[m_Apes[i].GetEnemyAspect()].Count;

                if (numAvailableProjects > 0)
                {
                    for (int j = 0; j < numAvailableProjects; j++)
                    {
                        int projectSpite = m_Projects[ProjectDict[m_Apes[i].GetAspect()][j]].GetSpite(); //Sickening line of code

                        projectCount += (10 - Math.Abs((m_Apes[i].GetSpite() - projectSpite)));
                        SpiteProjects.Add(projectCount);
                        Projects.Add(ProjectDict[m_Apes[i].GetAspect()][j]);
                    }

                    for (int j = 0; j < numAvailableEnemyAspectProjects; j++)
                    {
                        int projectSpite = m_Projects[ProjectDict[m_Apes[i].GetEnemyAspect()][j]].GetSpite(); //Sickening line of code

                        //With this, an ape is more likely to counteract a non-spiteful project if they are spiteful and vice versa
                        projectCount += (Math.Abs((m_Apes[i].GetSpite() - projectSpite) + 1));
                        SpiteProjects.Add(projectCount);
                        Projects.Add(ProjectDict[m_Apes[i].GetEnemyAspect()][j]);
                    }

                    int projectSelection = rnd.Next(1, projectCount + 1);

                    for (int j = 0; j < SpiteProjects.Count; j++)
                    {
                        if (projectSelection <= SpiteProjects[j])
                        {
                            m_Apes[i].SetTargetProject(Projects[j]);

                            if (m_Apes[i].GetTargetProject() == formerProject)
                            {
                                m_Apes[i].SetReadyForNextPhase(true);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    m_Apes[i].SetAction(ActionEnum.Idle);
                }
            }
			if (m_ActionTargets[m_Apes[i].GetAction()] == ActionTargetsEnum.Ape)
			{
				int targetApesTotal = 0;
				foreach (ApeTargetAspectEnum value in m_TargetApes[m_Apes[i].GetAction()])
				{
					if (value == ApeTargetAspectEnum.Self)
					{
						targetApesTotal += ApeDict[m_Apes[i].GetAspect()].Count;
					}
					else if (value == ApeTargetAspectEnum.Enemy)
					{
                        targetApesTotal += ApeDict[m_Apes[i].GetEnemyAspect()].Count;
                    }
					else if (value == ApeTargetAspectEnum.Weak)
					{
                        targetApesTotal += ApeDict[m_Apes[i].GetWeakAspect()].Count;
                    }
				}

				int index = rnd.Next(0, targetApesTotal);
                int targetApe = -1;

                foreach (ApeTargetAspectEnum value in m_TargetApes[m_Apes[i].GetAction()])
                {
                    if (value == ApeTargetAspectEnum.Self)
                    {
                        if (index > ApeDict[m_Apes[i].GetAspect()].Count)
						{
							index -= ApeDict[m_Apes[i].GetAspect()].Count;
                        }
						else
						{
							targetApe = ApeDict[m_Apes[i].GetAspect()][index];
							break;
						}
                    }
                    else if (value == ApeTargetAspectEnum.Enemy)
                    {
                        if (index > ApeDict[m_Apes[i].GetEnemyAspect()].Count)
                        {
                            index -= ApeDict[m_Apes[i].GetEnemyAspect()].Count;
                        }
                        else
                        {
                            targetApe = ApeDict[m_Apes[i].GetEnemyAspect()][index];
							break;
                        }
                    }
                    else if (value == ApeTargetAspectEnum.Weak)
                    {
                        if (index > ApeDict[m_Apes[i].GetWeakAspect()].Count)
                        {
                            index -= ApeDict[m_Apes[i].GetWeakAspect()].Count;
                        }
                        else
                        {
                            targetApe = ApeDict[m_Apes[i].GetWeakAspect()][index];
							break;
                        }
                    }
                }

				m_Apes[i].SetTargetApe(m_Apes[targetApe]);
            }
        }
    }

	private void PruneProjects()
	{

		for (int i = 0; i < m_DeadProjectIDs.Count; i++)
		{
			RemoveProject(m_DeadProjectIDs[i]);
        }
    }

	public void SpawnApe()
	{
        //Vector2I Coords = m_Map.getRandomOpenNavCoords(true);
        Vector2I Coords = m_Map.getRandomOpenNavCoords(true);

        var apeScene = new PackedScene();
        apeScene = ResourceLoader.Load<PackedScene>("res://ape.tscn");
		ape Ape = apeScene.Instantiate<ape>();
		Ape.SetMap(ref m_Map);
        Ape.SetNavCoords(Coords);
		Vector2 PosCoords = m_Map.GetPointPosition(Coords);
		Ape.SetMap(ref m_Map);
		AddChild(Ape);
        Ape.GlobalPosition = new Vector3(PosCoords.X, 10, PosCoords.Y);
		Ape.SetApeManager(this);

		//Remove later?
		if (Ape.GetAspect() == AspectEnum.Fervor)
		{
			Ape.AddDeck(DeckEnum.Fervor_Default);
		}
		else if (Ape.GetAspect() == AspectEnum.Influence)
		{
            Ape.AddDeck(DeckEnum.Influence_Default);
        }
        else if (Ape.GetAspect() == AspectEnum.Insight)
        {
            Ape.AddDeck(DeckEnum.Insight_Default);
        }
        //

        m_Apes.Add(Ape);
    }

	public void SpawnProject(ProjectEnum project, Vector3I? ProjCoords = null)
	{
		string ProjectPath = "";

		
		//Big AWFUL if statement chain you need to fix later or you will DIE in REAL LIFE
		if (project == ProjectEnum.Unfinished_Idol)
		{
			ProjectPath = "res://Scenes/Projects/UnfinishedIdol.tscn";
        }

		else if (project == ProjectEnum.Fervor_Idol)
		{
            ProjectPath = "res://Scenes/Projects/FervorIdol.tscn";
        }

        else if (project == ProjectEnum.Temple)
        {
            ProjectPath = "res://Scenes/Projects/Temple.tscn";
        }

        else if (project == ProjectEnum.Jail)
        {
            ProjectPath = "res://Scenes/Projects/Jail.tscn";
        }

        else if (project == ProjectEnum.Lab)
        {
            ProjectPath = "res://Scenes/Projects/Lab.tscn";
        }

		else if (project == ProjectEnum.Workshop)
		{
			ProjectPath = "res://Scenes/Projects/workshop.tscn";
		}

        Debug.Assert(ProjectPath != "", "Invalid project! ID: " + project.ToString());

        var projectScene = new PackedScene();
		projectScene = ResourceLoader.Load<PackedScene>(ProjectPath);
		Project projectInstance = projectScene.Instantiate<Project>();
		AddChild(projectInstance);
		Vector3I Coords;
		if (ProjCoords == null)
		{
            Coords = m_Map.GetProjectLocation(projectInstance.GetDimensions());
        }
		else
		{
			Coords = (Vector3I) ProjCoords;
		}
		Vector2I NavCoords = m_Map.PosCoordsToNavCoords(new Vector2I(Coords.X, Coords.Z));
		for (int xOffset = 0; xOffset < projectInstance.GetDimensions().X; xOffset++)
		{
			for (int zOffset = 0; zOffset < projectInstance.GetDimensions().Y; zOffset++)
			{
				Vector2I offset = new Vector2I(xOffset, zOffset);
                //if (!projectInstance.IsSlot(offset))
				//{
                    m_Map.SetPointSolid(NavCoords + offset, true);
                //}
            }
		}

        //m_Map.SetPointSolid(NavCoords);

        Vector2 PosCoords = m_Map.GetPointPosition(NavCoords);

		projectInstance.SetID(project);

		projectInstance.SetCoords(Coords);

        projectInstance.GlobalPosition = new Vector3(PosCoords.X, Coords.Y * 2, PosCoords.Y);

        projectInstance.GlobalPosition = new Vector3(projectInstance.GlobalPosition.X + projectInstance.GetDimensions().X - 1, projectInstance.GlobalPosition.Y, projectInstance.GlobalPosition.Z + projectInstance.GetDimensions().Y - 1);

        //projectInstance.UpdateVerticalPosition();

        m_Projects[project] = projectInstance;
    }

    //Ape action stuff starts here

    //Deck[] m_Decks;
	System.Collections.Generic.Dictionary<DeckEnum, Deck> m_Decks;


    private void ConfigureDecks()
	{
		m_Decks = new System.Collections.Generic.Dictionary<DeckEnum, Deck>();

        //for (int i = 0; i < Enum.GetNames(typeof(DeckEnum)).Length; i++)
		foreach (DeckEnum deck in Enum.GetValues(typeof(DeckEnum)))
		{
			if (deck == DeckEnum.Fervor_Default)
			{
				ActionEnum[] actions = {
					ActionEnum.Idle,
					ActionEnum.Work_One,
					ActionEnum.Stun
				};
				m_Decks[deck] = new Deck(actions);
			}
			else if (deck == DeckEnum.Insight_Default)
			{
                ActionEnum[] actions = {
                    ActionEnum.Idle,
                    ActionEnum.Work_One
                };
                m_Decks[deck] = new Deck(actions);
            }
			else if (deck == DeckEnum.Influence_Default)
			{
                ActionEnum[] actions = {
                    ActionEnum.Idle,
                    ActionEnum.Work_One
                };
                m_Decks[deck] = new Deck(actions);
            }
		}
	}

	public void QueueWork(AspectEnum aspect, int amount, ProjectEnum targetProject)
	{
			m_Projects[targetProject].QueueWork(aspect, amount);
	}

	public int GetDeckSize(DeckEnum deck)
	{
		return m_Decks[deck].size;
	}

	public ActionEnum GetAction(DeckEnum deck, int action)
	{
		return m_Decks[deck].GetAction(action);
	}

	public void RemoveProject(ProjectEnum ID)
	{
		bool hasNextProject = m_Projects[ID].HasNextProject();
		ProjectEnum nextProject = m_Projects[ID].GetNextProject();
		Vector3I Coords = m_Projects[ID].GetCoords();

        for (int i = 0; i < m_Apes.Count; i++)
        {
            if (m_Apes[i].GetTargetProject() == ID)
            {
				if (hasNextProject)
				{
                    m_Apes[i].SetTargetProject(nextProject);
                }
				else
				{
					m_Apes[i].SetTargetProject(ProjectEnum.None);
				}
            }
        }

        m_Projects[ID].QueueFree();
        bool remove = m_Projects.Remove(ID);
		if (hasNextProject)
		{
			SpawnProject(nextProject, Coords);
		}
	}

	public Vector2I GetProjectLocation(ProjectEnum project)
	{
		return new Vector2I(m_Projects[project].GetCoords().X, m_Projects[project].GetCoords().Z);
	}

	public Vector2I GetOpenSlot(ProjectEnum project)
	{
		return m_Projects[project].GetOpenSlot();
	}

	public void SetOpenSlot(ProjectEnum project, Vector2I slot, bool open)
	{
            m_Projects[project].SetOpenSlot(slot, open);
	}

	public Vector3 GetGlobalPosition(ProjectEnum project)
	{
		return m_Projects[project].GlobalPosition;
	}

	public Vector2I GetProjectDimensions(ProjectEnum project)
	{
		return m_Projects[project].GetDimensions();
	}

	public bool IsProjectActive(ProjectEnum project)
	{
		return m_Projects.ContainsKey(project);
	}

	//Fix inevitably big if statement chain
    public void QueueActions()
    {
        for (int i = 0; i < m_Apes.Count; i++)
        {
            ActionEnum action = m_Apes[i].GetAction();
            AspectEnum aspect = m_Apes[i].GetAspect();

			if (m_Apes[i].IsWorking())
			{
				m_Apes[i].SetWorkTransition(true);
			}

            if (action == ActionEnum.Idle)
            {

            }
            else if (action == ActionEnum.Work_One)
            {
                QueueWork(aspect, 1, m_Apes[i].GetTargetProject());
            }
            else if (action == ActionEnum.Work_Two)
            {
                QueueWork(aspect, 2, m_Apes[i].GetTargetProject());
            }
            else if (action == ActionEnum.Work_Three)
            {
                QueueWork(aspect, 3, m_Apes[i].GetTargetProject());
            }
			else if (action == ActionEnum.Stun)
			{
				m_Apes[i].GetTargetApe().SetActionOverride(ActionEnum.Idle);
				m_Apes[i].GetTargetApe().SetSleeping(true);
			}
		}
	}

	private void SpawnInitialProjects()
	{
		SpawnProject(ProjectEnum.Unfinished_Idol);
		SpawnProject(ProjectEnum.Lab);
		SpawnProject(ProjectEnum.Temple);
		SpawnProject(ProjectEnum.Workshop);
	}

	public int GetTime()
	{
		return m_TimeManager.GetTime();
	}

	public void CreateDayActionTransformation(AspectEnum aspect, ActionEnum action, ActionEnum resultAction)
	{
		m_ActionTransformations.Add(new Tuple<AspectEnum, ActionEnum>(aspect, action), resultAction);
	}

	public void RemoveDayActionTransformation(AspectEnum aspect, ActionEnum action)
	{
        Tuple<AspectEnum, ActionEnum> actionTuple = new Tuple<AspectEnum, ActionEnum>(aspect, action);
        if (m_ActionTransformations.ContainsKey(actionTuple))
        {
            m_ActionTransformations.Remove(actionTuple);
        }
    }

	public ActionEnum ApplyActionTransformations(AspectEnum aspect, ActionEnum action)
	{
		Tuple<AspectEnum, ActionEnum> actionTuple = new Tuple<AspectEnum, ActionEnum>(aspect, action);
		if (m_ActionTransformations.ContainsKey(actionTuple))
		{
			return m_ActionTransformations[actionTuple];
		}
		else
		{
			return action;
		}
	}

	public void RecalculateActions()
	{
        foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
        {
            m_Projects[entry.Key].ClearQueuedWork();
        }

		for (int i = 0; i < m_Apes.Count; i++)
		{
			m_Apes[i].ResetActionOverride();
		}

		QueueActions();

        foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
        {
            m_Projects[entry.Key].ReAddPlayerWork();
        }
    }

	public void ProcessKilledApes()
	{
		Stack<int> deadApes = new Stack<int>();
		for (int i = 0; i < m_Apes.Count; i++)
		{
			if (m_Apes[i].GetDead())
			{
				deadApes.Push(i);
			}
		}

		while (deadApes.Count > 0)
		{
			int ape = deadApes.Pop();

            m_Apes[ape].QueueFree();
			m_Apes.RemoveAt(ape);
		}
	}

	public void PlayApeDeathParticles(Transform3D transform, AspectEnum aspect)
	{
		Color col = new Godot.Color(0x000000);

        Random rng = new Random();
        transform = transform.Translated(new Vector3(0, m_ApeDeathParticlesVerticalOffset, 0));
        if (aspect == AspectEnum.Fervor)
		{
			for (int i = 0; i < 20; i++)
			{
				m_FervorApeDeathParticles.Restart();
				m_FervorApeDeathParticles.Preprocess = 0.0f;
				Vector3 Velocity = new Vector3((float)((rng.Next() / int.MaxValue) - 0.5) * 5, (float)(rng.Next() / int.MaxValue) * 5, (float)((rng.Next() / int.MaxValue) - 0.5) * 5);
				m_FervorApeDeathParticles.EmitParticle(transform, Velocity, col, col, 1);
			}
		}
		else if (aspect == AspectEnum.Insight)
		{
			for (int i = 0; i < 20; i++)
			{
				m_InsightApeDeathParticles.Restart();
                m_InsightApeDeathParticles.Preprocess = 0.0f;
                Vector3 Velocity = new Vector3((float)((rng.Next() / int.MaxValue) - 0.5) * 5, (float)(rng.Next() / int.MaxValue) * 5, (float)((rng.Next() / int.MaxValue) - 0.5) * 5);
                m_InsightApeDeathParticles.EmitParticle(transform, Velocity, col, col, 1);
			}
		}
		else if (aspect == AspectEnum.Influence)
		{
			for (int i = 0; i < 20; i++)
			{
                m_InsightApeDeathParticles.Restart();
                m_InfluenceApeDeathParticles.Preprocess = 0.0f;
				Vector3 Velocity = new Vector3((float)((rng.Next() / int.MaxValue) - 0.5) * 5, (float)(rng.Next() / int.MaxValue) * 5, (float)((rng.Next() / int.MaxValue) - 0.5) * 5);
                m_InfluenceApeDeathParticles.EmitParticle(transform, Velocity, col, col, 1);
			}
		}
		//m_ApeDeathParticles.GlobalPosition = position + new Vector3(0, m_ApeDeathParticlesVerticalOffset, 0);
		//m_ApeDeathParticles.Emitting = true;


	}
}
