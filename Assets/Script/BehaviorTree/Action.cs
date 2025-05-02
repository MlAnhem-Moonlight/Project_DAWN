using System;

namespace BehaviorTree
{
    public class Action : Nodes
    {
        private readonly Func<NodeState> _action;

        public Action(Func<NodeState> action)
        {
            _action = action;
        }

        public override NodeState Evaluate()
        {
            return _action();
        }
    }
}
