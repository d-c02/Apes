using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeckInterface;

namespace SmallApesv2
{
    public interface PlayerDeckInterface
    {
        public bool DoCardAction(Node node)
        {
            return false;
        }

    }

    public class pc_FervorWorkOne : PlayerDeckInterface
    {
        bool PlayerDeckInterface.DoCardAction(Node node)
        {
            if (node is Project)
            {
                Project project = (Project)node;
                if (project.GetWorkAspect() == WorkAspectEnum.Influence)
                {
                    return false;
                }
                else
                {
                    project.QueueWork(AspectEnum.Fervor, 1, true);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class pc_InsightWorkOne : PlayerDeckInterface
    {
        bool PlayerDeckInterface.DoCardAction(Node node)
        {
            if (node is Project)
            {
                Project project = (Project)node;
                if (project.GetWorkAspect() == WorkAspectEnum.Fervor)
                {
                    return false;
                }
                else
                {
                    project.QueueWork(AspectEnum.Insight, 1, true);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class pc_InfluenceWorkOne : PlayerDeckInterface
    {
        bool PlayerDeckInterface.DoCardAction(Node node)
        {
            if (node is Project)
            {
                Project project = (Project)node;
                if (project.GetWorkAspect() == WorkAspectEnum.Insight)
                {
                    return false;
                }
                else
                {
                    project.QueueWork(AspectEnum.Influence, 1, true);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class pc_AnyWorkOne : PlayerDeckInterface
    {
        bool PlayerDeckInterface.DoCardAction(Node node)
        {
            if (node is Project)
            {
                Project project = (Project)node;
                project.QueueWork(AspectEnum.Any, 1, true);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
