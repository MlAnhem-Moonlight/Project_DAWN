using System;

namespace BehaviorTree
{
    public class Condition : Nodes
    {
        private readonly Func<bool> _condition;

        public Condition(Func<bool> condition)
        {
            _condition = condition;
        }

        public override NodeState Evaluate()
        {
            return _condition() ? NodeState.SUCCESS : NodeState.FAILURE;
        }
    }
}
