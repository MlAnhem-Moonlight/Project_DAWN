using BehaviorTree;
using UnityEngine;

public class MageCheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerName;

    public MageCheckEnemyInRange(Transform transform, float range, Transform defaultTarget, string layerName)
    {
        _transform = transform;
        _range = range;
        _defaultTarget = defaultTarget;
        _layerName = layerName;
    }

    public override NodeState Evaluate()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_transform.position, _range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer(_layerName) || hitCollider.transform == _defaultTarget)
            {
                float distance = Vector3.Distance(_transform.position, hitCollider.transform.position);
                if (hitCollider.transform == _defaultTarget)
                {
                    closestTarget = _defaultTarget;
                    break;
                }
                else if (distance < closestDistance)
                {
                    closestTarget = hitCollider.transform;
                    closestDistance = distance;
                }
            }
        }
        parent.SetData("target", closestTarget);
        state = NodeState.SUCCESS;

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
