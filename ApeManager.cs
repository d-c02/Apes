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

	private System.Collections.Generic.Dictionary<int, Project> m_Projects;

	private int m_IdolStatueProjectIndex;

	private int m_InfluenceProjectIndex;

    private int m_InsightProjectIndex;

    private int m_FervorProjectIndex;

    public override void _Ready()
	{
		m_Apes = new List<ape>();
		m_Projects = new System.Collections.Generic.Dictionary<int, Project>();
		for (int i = 0; i < 1; i++)
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
			SpawnProject((int) Projects.Unfinished_Idol);
		}
		
		
		if (Input.IsActionJustPressed("DEBUG_START_NEW_TIME_PHASE"))
		{
			for (int i = 0; i < m_Apes.Count; i++)
			{
				m_Apes[i].StartNewPhase();
			}
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
		if (Ape.GetAspect() == (int) Aspects.Fervor)
		{
			Ape.AddDeck((int) Decks.Fervor_Default);
		}
		else if (Ape.GetAspect() == (int)Aspects.Influence)
		{
            Ape.AddDeck((int)Decks.Influence_Default);
        }
        else if (Ape.GetAspect() == (int)Aspects.Insight)
        {
            Ape.AddDeck((int)Decks.Insight_Default);
        }
        //

        m_Apes.Add(Ape);
    }

	public void SpawnProject(int project, Vector3I? ProjCoords = null)
	{
		string ProjectPath = "";

		
		//Big AWFUL if statement chain you need to fix later or you will DIE in REAL LIFE
		if (project == (int) Projects.Unfinished_Idol)
		{
			ProjectPath = "res://Scenes/Projects/UnfinishedIdol.tscn";
        }

		else if (project == (int) Projects.Fervor_Idol)
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
				m_Map.SetPointSolid(NavCoords + new Vector2I(xOffset, zOffset), true);
			}
		}

        m_Map.SetPointSolid(NavCoords);
        Vector2 PosCoords = m_Map.GetPointPosition(NavCoords);

		projectInstance.SetID(project);

		projectInstance.SetCoords(Coords);

        projectInstance.GlobalPosition = new Vector3(PosCoords.X, 8, PosCoords.Y);

        projectInstance.GlobalPosition = new Vector3(projectInstance.GlobalPosition.X + projectInstance.GetDimensions().X - 1, projectInstance.GlobalPosition.Y, projectInstance.GlobalPosition.Z + projectInstance.GetDimensions().Y - 1);

        projectInstance.UpdateVerticalPosition();

		projectInstance.SetApeManager(this);
        m_Projects[project] = projectInstance;
    }

    //Ape action stuff starts here

    Deck[] m_Decks;

    private void ConfigureDecks()
	{
		m_Decks = new Deck[Enum.GetNames(typeof(Decks)).Length];

        for (int i = 0; i < Enum.GetNames(typeof(Decks)).Length; i++)
		{
			if (i == (int) Decks.Fervor_Default)
			{
				int[] actions = {
					(int) Actions.Idle,
					(int) Actions.Work_One,
					(int) Actions.Work_Two,
					(int) Actions.Work_Three
				};
				m_Decks[i] = new Deck(actions);
			}
			else if (i == (int) Decks.Insight_Default)
			{
                int[] actions = {
                    (int) Actions.Idle,
                    (int) Actions.Work_One,
                    (int) Actions.Work_Two,
                    (int) Actions.Work_Three
                };
                m_Decks[i] = new Deck(actions);
            }
			else if (i == (int) Decks.Influence_Default)
			{
                int[] actions = {
                    (int) Actions.Idle,
                    (int) Actions.Work_One,
                    (int) Actions.Work_Two,
                    (int) Actions.Work_Three
                };
                m_Decks[i] = new Deck(actions);
            }
		}
	}

	public void ProcessWork(int aspect, int amount)
	{
		foreach (KeyValuePair<int, Project> entry in m_Projects)
		{
			m_Projects[entry.Key].AddWork(aspect, amount);
		}
	}

	public void QueueActions()
	{

	}

	public int GetDeckSize(int deck)
	{
		return m_Decks[deck].size;
	}

	public int GetAction(int deck, int action)
	{
		return m_Decks[deck].GetAction(action);
	}

	public void RemoveProject(int ID)
	{
		m_Projects.Remove(ID);
	}
}
