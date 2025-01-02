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
    private List<Card> m_Cards;
    private int m_HandSize = 5;
    private int m_CardsInHand = 0;
    private float m_CardHorizontalDistance = 120;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        m_Cards = new List<Card>();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("DEBUG_DRAW_CARD"))
        {
            DrawCard();
        }
    }

    private void DrawCard()
    {

        //Instantiate
        var cardScene = new PackedScene();
        cardScene = ResourceLoader.Load<PackedScene>("res://Card.tscn");
        Card card = cardScene.Instantiate<Card>();
        AddChild(card);
        m_Cards.Add(card);

        m_HandSize++;

        int center = (int)Math.Floor((double)m_Cards.Count / 2);

        for (int i = 0; i < m_Cards.Count; i++)
        {
            if (m_Cards.Count % 2 == 0)
            {
                m_Cards[i].SetBasePosition(new Vector2(m_CardHorizontalDistance * (i - center) + m_CardHorizontalDistance / 2, 0));
            }
            else
            {
                m_Cards[i].SetBasePosition(new Vector2(m_CardHorizontalDistance * (i - center), 0));
            }
        }
        m_Cards[m_Cards.Count - 1].Position += new Vector2(1000, 0);
        m_Cards[m_Cards.Count - 1].SetDeckInterface(new pc_AnyWorkOne());
    }
}
