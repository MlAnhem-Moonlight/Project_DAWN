using BehaviorTree;
using UnityEngine;

public class MageCheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerHuman;
    private string _layerConstruction;
    private Animator _animator;

    public MageCheckEnemyInRange(Transform transform, float range, Transform defaultTarget, string layerHuman, string layerConstruction, Animator animator)
    {
        _transform = transform;
        _range = range;
        _defaultTarget = defaultTarget;
        _layerHuman = layerHuman;
        _layerConstruction = layerConstruction;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        Transform closestTarget = TargetSelector.GetClosestTarget(_transform, _range, _layerHuman, _layerConstruction, _defaultTarget);
        
        // Check if the closest target is within range
        if (closestTarget != null && Vector3.Distance(_transform.position, closestTarget.position) <= _range)
        {

            parent.SetData("target", closestTarget);
            //Debug.Log($"Target found: {parent.GetData("target")}");
            state = NodeState.SUCCESS;
        }
        else
        {
            parent.SetData("target", _defaultTarget);
            state = NodeState.FAILURE;
        }

        return state;
    }
}
