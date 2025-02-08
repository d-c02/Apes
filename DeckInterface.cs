using Godot;
using System;
using System.Collections.Generic;

public interface DeckInterface
{
    public enum AspectEnum { Insight, Influence, Fervor, Any };

    public enum WorkAspectEnum { Empty, Insight, Influence, Fervor, Any };

    public enum ActionEnum
    {
        None,
        Idle,
        Work_One,
        Work_Two,
        Work_Three,
        Stun,
        Idle_To_One_Transformation,
        Fervor_To_Influence_Work_Transformation
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
        Influence_Default,
        Insight_Scientist,
        Influence_Priest
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

    public enum JobEnum
    {
        Unemployed,
        Insight_Scientist,
        Influence_Priest,
        Fervor_Craftsman
    }

    public struct Deck
    {
        public Deck(ActionEnum[] actions, int[] time)
        {
            Actions = actions;
            size = Actions.Length;
            timeIncrements = time;
        }

        public ActionEnum GetAction(int action)
        {
            return Actions[action];
        }

        ActionEnum[] Actions;

        public int size;

        public int[] timeIncrements;
    }
}
