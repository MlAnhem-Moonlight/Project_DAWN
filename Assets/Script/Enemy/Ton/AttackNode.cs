using BehaviorTree;
using UnityEngine;

public class TonAttackNode : Nodes
{
    private TonMovement _tonMovement;

    public TonAttackNode(TonMovement tonMovement)
    {
        _tonMovement = tonMovement;
    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        //Debug.Log("Attacking");
        state = NodeState.SUCCESS;
        return state;
    }
}
