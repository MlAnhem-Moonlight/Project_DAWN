using BehaviorTree;
using UnityEngine;

public class TheHandAttack : Nodes
{
    private TheHandMovement _theHandMovement;
    private Animator _animator;

    public TheHandAttack(TheHandMovement theHandMovement, Animator animator)
    {
        _theHandMovement = theHandMovement;
        _animator = animator;

    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        Debug.Log("Attacking");
        //_animator.SetBool("IsAttack", true);
        state = NodeState.SUCCESS;
        return state;
    }
}
