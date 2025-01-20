using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static DeckInterface;
using System.Collections.Generic;
using System.Collections.Specialized;
using SmallApesv2;

public partial class CardDock : Control
{
    private float m_CardHorizontalDistance = 120;

    private List<Card> m_Deck;

    private Stack<Card> m_Draw;

    private List<Card> m_Hand;

    private Stack<Card> m_Discard;

    [Export]
    ApeManager m_ApeManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        m_Deck = new List<Card>();
        m_Draw = new Stack<Card>();
        m_Hand = new List<Card>();
        m_Discard = new Stack<Card>();

        for (int i = 0; i < 5; i++)
        {
            //Instantiate
            var cardScene = new PackedScene();
            cardScene = ResourceLoader.Load<PackedScene>("res://Card.tscn");
            Card card = cardScene.Instantiate<Card>();

            if (i == 0)
            {
                card.SetDeckInterface(new pc_InsightWorkOne());
                card.Modulate = new Color(0, 0, 1);
            }
            else if (i == 1)
            {
                card.SetDeckInterface(new pc_InfluenceWorkOne());
                card.Modulate = new Color(0, 1, 0);
            }
            else if (i == 2)
            {
                card.SetDeckInterface(new pc_FervorWorkOne());
                card.Modulate = new Color(1, 0, 0);
            }
            else if (i == 3)
            {
                card.SetDeckInterface(new pc_AnyWorkOne());
            }
            else
            {
                card.SetDeckInterface(new pc_KillApe());
                card.Modulate = new Color(0, 0, 0);
            }

            card.SetApeManager(ref m_ApeManager);
            AddCard(card);
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("DEBUG_DRAW_CARD"))
        {
            DrawCard();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        for (int i = 0; i < m_Hand.Count; i++)
        {
            if (!m_Hand[i].GetInHand())
            {
                m_Discard.Push(m_Hand[i]);
                m_Hand.RemoveAt(i);
                OrderCards();
            }
        }
    }

    private static Random rnd = new Random(23);

    public void AddCard(Card card)
    {
        AddChild(card);

        card.Position = new Vector2(1000, 0);
        card.SetBasePosition(new Vector2(1000, 0));
        card.Visible = false;

        //Messy, could be sped up
        m_Deck.Add(card);
        m_Draw.Push(card);
        
        //Use an actual shuffle algorithm later
        m_Draw = new Stack<Card>(m_Draw.OrderBy((item) => rnd.Next()).ToList<Card>());
    }

    private void DrawCard()
    {
        if (m_Draw.Count > 0)
        {
            m_Hand.Add(m_Draw.Pop());
            m_Hand[m_Hand.Count - 1].Visible = true;
            m_Hand[m_Hand.Count - 1].SetInHand(true);

            OrderCards();
        }
        else if (m_Discard.Count > 0)
        {
            //Use an actual shuffle algorithm later
            m_Draw = new Stack<Card>(m_Discard.OrderBy((item) => rnd.Next()).ToList<Card>());
            m_Discard.Clear();

            m_Hand.Add(m_Draw.Pop());
            m_Hand[m_Hand.Count - 1].Visible = true;
            m_Hand[m_Hand.Count - 1].SetInHand(true);

            OrderCards();
        }

        //m_Hand[m_Hand.Count - 1].Position += new Vector2(1000, 0);
    }

    private void OrderCards()
    {
        int center = (int)Math.Floor((double)m_Hand.Count / 2);

        for (int i = 0; i < m_Hand.Count; i++)
        {
            if (m_Hand.Count % 2 == 0)
            {
                m_Hand[i].SetBasePosition(new Vector2(m_CardHorizontalDistance * (i - center) + m_CardHorizontalDistance / 2, 0));
            }
            else
            {
                m_Hand[i].SetBasePosition(new Vector2(m_CardHorizontalDistance * (i - center), 0));
            }
        }
    }
}
