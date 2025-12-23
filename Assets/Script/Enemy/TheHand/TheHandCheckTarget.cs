using BehaviorTree;
using UnityEngine;

public class TheHandCheckTarget : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerHuman;
    private Animator _animator;

    public TheHandCheckTarget(Transform transform, float range, Transform defaultTarget, string layerHuman, string layerConstruction, Animator animator)
    {
        _transform = transform;
        _range = range;
        _defaultTarget = defaultTarget;
        _layerHuman = layerHuman;  // ✅ Chỉ cần Human layer
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        Transform currentTarget = parent.GetData("target") as Transform;

        // ✅ Nếu target hiện tại là Human (không phải default) và còn sống
        if (currentTarget != null && currentTarget != _defaultTarget && currentTarget.gameObject.activeInHierarchy)
        {
            Stats stats = currentTarget.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                _animator.SetFloat("Direct", _transform.position.x - currentTarget.position.x > 0 ? -1 : 1);

                state = NodeState.SUCCESS;
                return state;
            }
        }


        Transform closestHumanTarget = TargetSelector.GetClosestTarget(_transform, _range, _layerHuman, "Construction");
        
        if (closestHumanTarget != null && closestHumanTarget.gameObject.activeInHierarchy)
        {
            Stats stats = closestHumanTarget.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                _animator.SetFloat("Direct", _transform.position.x - closestHumanTarget.position.x > 0 ? -1 : 1);
                parent.SetData("target", closestHumanTarget);
                state = NodeState.SUCCESS;
                return state;
            }
        }

        _animator.SetFloat("Direct", _transform.position.x - _defaultTarget.position.x > 0 ? -1 : 1);
        parent.SetData("target", _defaultTarget);
        state = NodeState.SUCCESS;
        return state;
    }
}
