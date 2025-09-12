using BehaviorTree;
using UnityEngine;

public class TonAttackNode : Nodes
{
    private Transform _transform;
    private Animator _animator;

    public TonAttackNode(Transform transform, Animator animator)
    {
        _transform = transform;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        // Implement attack logic here
        //Debug.Log("Attacking");
        _animator.SetInteger("State", 1);
        Transform target = (Transform)GetData("target");
        _animator.SetFloat("Attack", _transform.position.x - target.position.x > 0 ? -1f : 1f);
        state = NodeState.RUNNING;
        return state;
    }
}
