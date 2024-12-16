using Godot;
using System;

public interface DeckInterface
{
    public enum AspectEnum { Insight, Influence, Fervor };

    public enum WorkAspectEnum { Empty, Insight, Influence, Fervor, Any };

    public enum ActionEnum
    {
        Idle,
        Work_One,
        Work_Two,
        Work_Three
    };

    public enum DeckEnum
    {
        Fervor_Default,
        Insight_Default,
        Influence_Default
    };

    public enum ProjectEnum
    {
        None,
        Unfinished_Idol,
        Fervor_Idol,
        Insight_Idol,
        Influence_Idol,
        Jail,
        Temple,
        Lab
    };

    public struct Deck
    {
        public Deck(ActionEnum[] actions)
        {
            Actions = actions;
            size = Actions.Length;
        }

        public ActionEnum GetAction(int action)
        {
            return Actions[action];
        }

        ActionEnum[] Actions;

        public int size;
    }
}
