using Godot;
using System;

public interface DeckInterface
{
    public enum Actions
    {
        Idle,
        Work_One,
        Work_Two,
        Work_Three
    };

    public enum Decks
    {
        Fervor_Default,
        Insight_Default,
        Influence_Default
    };

    public struct Deck
    {
        public Deck(int[] actions)
        {
            Actions = actions;
            size = Actions.Length;
        }

        public int GetAction(int action)
        {
            return Actions[action];
        }

        int[] Actions;

        public int size;
    }
}
