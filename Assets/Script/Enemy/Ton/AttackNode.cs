using BehaviorTree;
using UnityEngine;

public class AttackNode : Nodes
{
    private TonMovement _tonMovement;

    public AttackNode(TonMovement tonMovement)
    {
        _tonMovement = tonMovement;
    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        Debug.Log("Attacking");
        state = NodeState.SUCCESS;
        return state;
    }
}
