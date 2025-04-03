using BehaviorTree;
using UnityEngine;

public class MageAttackNode : Nodes
{
    private TheMageMovement _theMageMovement;

    public MageAttackNode(TheMageMovement theMageMovement)
    {
        _theMageMovement = theMageMovement;
    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        Debug.Log("Attacking");
        state = NodeState.SUCCESS;
        return state;
    }
}
