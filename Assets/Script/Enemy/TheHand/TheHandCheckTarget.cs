using BehaviorTree;
using UnityEngine;

public class TheHandCheckTarget : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerName;
    private Animator _animator;

    public TheHandCheckTarget(Transform transform, float range, Transform defaultTarget, string layerName, Animator animator)
    {
        _transform = transform;
        _range = range;
        _defaultTarget = defaultTarget;
        _layerName = layerName;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_transform.position, _range);
        Transform _closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer(_layerName) || hitCollider.transform == _defaultTarget)
            {
                float distance = Vector3.Distance(_transform.position, hitCollider.transform.position);
                if (hitCollider.transform == _defaultTarget)
                {
                    _closestTarget = _defaultTarget;
                    break;
                }
                else if (distance < closestDistance)
                {
                    _closestTarget = hitCollider.transform;
                    closestDistance = distance;
                }
            }
        }
        if (_closestTarget != null)
            _animator.SetFloat("Movement", _transform.position.x - _closestTarget.position.x > 0 ? -1 : 1);
        else
        {
            _animator.SetFloat("Movement", _transform.position.x - _defaultTarget.position.x > 0 ? -1 : 1);
        }
        parent.SetData("target", _closestTarget);

        state = NodeState.SUCCESS;

        return state;
    }
}
