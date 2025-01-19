using Godot;
using System;
using System.Collections.Generic;

public interface DeckInterface
{
    public enum AspectEnum { Insight, Influence, Fervor, Any };

    public enum WorkAspectEnum { Empty, Insight, Influence, Fervor, Any };

    public enum ActionEnum
    {
        Idle,
        Work_One,
        Work_Two,
        Work_Three
    };

    public enum PlayerActionEnum
    { 
        Fervor_Work_One,
        Influence_Work_One,
        Insight_Work_One,
        Any_Work_One
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
        Lab,
        Workshop
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
