using System.Collections;
using System.Collections.Generic;

namespace BehaviorTree
{
    public class Sequence : Nodes
    {
        public Sequence() : base() { }
        public Sequence(List<Nodes> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;

            foreach (Nodes node in children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        state = NodeState.FAILURE; 
                        return state;
                    case NodeState.SUCCESS:
                        continue;
                        //break;
                    case NodeState.RUNNING:
                        anyChildIsRunning = true;
                        //break;
                        continue;
                    default:
                        state = NodeState.FAILURE;
                        return state;
                        //break;
                }
            }

            state = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
            return state;
        }
    }
}

