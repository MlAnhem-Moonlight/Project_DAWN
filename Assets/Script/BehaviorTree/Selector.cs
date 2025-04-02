using System.Collections;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class Selector : Nodes
    {
        public Selector() : base() { }
        public Selector(List<Nodes> children) : base(children) { }

        public override NodeState Evaluate()
        {

            foreach (Nodes node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        state = NodeState.SUCCESS;
                        return state;
                    case NodeState.RUNNING:
                        state = NodeState.RUNNING;
                        return state;
                    default:
                        continue;
                }
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}

