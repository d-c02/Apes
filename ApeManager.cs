using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DeckInterface;

public partial class ApeManager : Node
{

	// Called when the node enters the scene tree for the first time.

	[Export] private map m_Map;

	private List<ape> m_Apes;

	private System.Collections.Generic.Dictionary<ProjectEnum, Project> m_Projects;

	private List<ProjectEnum> m_DeadProjectIDs;

	private int m_IdolStatueProjectIndex;

	private int m_InfluenceProjectIndex;

    private int m_InsightProjectIndex;

    private int m_FervorProjectIndex;

    public override void _Ready()
	{
		m_Apes = new List<ape>();
		m_Projects = new System.Collections.Generic.Dictionary<ProjectEnum, Project>();
		m_DeadProjectIDs = new List<ProjectEnum>();
		for (int i = 0; i < 3; i++)
		{
			SpawnApe();
		}

		ConfigureDecks();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
		if (Input.IsActionJustPressed("DEBUG_SPAWN_APE"))
		{
			SpawnProject((int) ProjectEnum.Unfinished_Idol);
		}
		
		
		if (Input.IsActionJustPressed("DEBUG_START_NEW_TIME_PHASE"))
		{
			StartNewPhase();
		}
	}

	private void StartNewPhase()
	{
        foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
        {
            m_Projects[entry.Key].ProcessQueuedWork();
			if (m_Projects[entry.Key].IsFinished())
			{
				m_DeadProjectIDs.Add(entry.Key);
			}
        }

		PruneProjects();
		m_DeadProjectIDs.Clear();

        for (int i = 0; i < m_Apes.Count; i++)
        {
            m_Apes[i].StartNewPhase();
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

        projectInstance.GlobalPosition = new Vector3(PosCoords.X, 8, PosCoords.Y);

        projectInstance.GlobalPosition = new Vector3(projectInstance.GlobalPosition.X + projectInstance.GetDimensions().X - 1, projectInstance.GlobalPosition.Y, projectInstance.GlobalPosition.Z + projectInstance.GetDimensions().Y - 1);

        projectInstance.UpdateVerticalPosition();

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
					ActionEnum.Work_Two,
					ActionEnum.Work_Three
				};
				m_Decks[deck] = new Deck(actions);
			}
			else if (deck == DeckEnum.Insight_Default)
			{
                ActionEnum[] actions = {
                    ActionEnum.Idle,
                    ActionEnum.Work_One,
                    ActionEnum.Work_Two,
                    ActionEnum.Work_Three
                };
                m_Decks[deck] = new Deck(actions);
            }
			else if (deck == DeckEnum.Influence_Default)
			{
                ActionEnum[] actions = {
                    ActionEnum.Idle,
                    ActionEnum.Work_One,
                    ActionEnum.Work_Two,
                    ActionEnum.Work_Three
                };
                m_Decks[deck] = new Deck(actions);
            }
		}
	}

	public void QueueWork(AspectEnum aspect, int amount)
	{

		//Temporary. Bias logic needed
		foreach (KeyValuePair<ProjectEnum, Project> entry in m_Projects)
		{
			m_Projects[entry.Key].QueueWork(aspect, amount);
		}
	}

	public void QueueActions()
	{

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
		bool persists = m_Projects[ID].Persists();
		ProjectEnum nextProject = m_Projects[ID].GetNextProject();
		Vector3I Coords = m_Projects[ID].GetCoords();

		m_Projects[ID].QueueFree();
        bool remove = m_Projects.Remove(ID);
		if (persists)
		{
			SpawnProject(nextProject, Coords);
		}
	}
}
