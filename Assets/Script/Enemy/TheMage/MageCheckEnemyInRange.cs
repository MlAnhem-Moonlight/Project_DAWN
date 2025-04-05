using BehaviorTree;
using UnityEngine;

public class MageCheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerName;
    private Animator _animator;

    public MageCheckEnemyInRange(Transform transform, float range, Transform defaultTarget, string layerName, Animator animator)
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
        parent.SetData("target", _closestTarget);
        if(_closestTarget != null)
            _animator.SetFloat("Movement", _transform.position.x - _closestTarget.position.x > 0 ? -1 : 1);
        else _animator.SetFloat("Movement", _transform.position.x - _defaultTarget.position.x > 0 ? -1 : 1);
        state = NodeState.SUCCESS;
        //cải tiến nếu người chơi quá gần thì sẽ tạo khoảng cách với người chơi

        /*if (closestTarget != null)
        {
            parent.SetData("target", closestTarget);
            state = NodeState.SUCCESS;
        }
        else
        {
            parent.SetData("target", _defaultTarget);
            state = NodeState.FAILURE;
        }
        */
        return state;
    }
}
