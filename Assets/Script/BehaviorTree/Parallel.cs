using System.Collections.Generic;

namespace BehaviorTree
{
    /// <summary>
    /// Parallel node: chạy tất cả node con cùng lúc
    /// - Nếu có child RUNNING -> Parallel trả về RUNNING
    /// - Nếu tất cả SUCCESS -> Parallel SUCCESS
    /// - Nếu có 1 child FAILURE -> Parallel FAILURE (có thể tùy chỉnh strategy)
    /// </summary>
    public class Parallel : Nodes
    {
        public Parallel() : base() { }
        public Parallel(List<Nodes> children) : base(children) { }

        public override NodeState Evaluate()
        {
            bool anyRunning = false;

            foreach (Nodes node in children)
            {
                NodeState childState = node.Evaluate();
                switch (childState)
                {
                    case NodeState.FAILURE:
                        // Nếu muốn "thất bại 1 thằng thì cả Parallel fail"
                        state = NodeState.FAILURE;
                        return state;

                    case NodeState.SUCCESS:
                        // ok, check tiếp thằng khác
                        continue;

                    case NodeState.RUNNING:
                        anyRunning = true;
                        continue;
                }
            }

            // Nếu có child đang RUNNING → RUNNING
            if (anyRunning)
            {
                state = NodeState.RUNNING;
            }
            else
            {
                // Nếu tất cả SUCCESS → SUCCESS
                state = NodeState.SUCCESS;
            }

            return state;
        }
    }
}
