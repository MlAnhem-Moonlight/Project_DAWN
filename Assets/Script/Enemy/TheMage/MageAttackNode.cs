using BehaviorTree;
using UnityEngine;

public class MageAttackNode : Nodes
{
    private TheMageMovement _theMageMovement;
    private Animator _animator;

    public MageAttackNode(TheMageMovement theMageMovement, Animator animator)
    {
        _theMageMovement = theMageMovement;
        _animator = animator;

    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        Debug.Log("Attacking");
        _animator.SetBool("IsAttack", true);
        state = NodeState.SUCCESS;
        return state;
    }
}
