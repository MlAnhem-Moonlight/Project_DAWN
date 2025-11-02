using BehaviorTree;
using UnityEngine;

public class CheckEnemyInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private string _layerHuman;
    private string _layerConstruction;

    public CheckEnemyInRange(Transform transform, float range, string layerHuman, string layerConstruction)
    {
        _transform = transform;
        _range = range;        
        _layerHuman = layerHuman;
        _layerConstruction = layerConstruction;
    }

    public override NodeState Evaluate()
    {
        
        Transform closestTarget = TargetSelector.GetClosestTarget(_transform, _range, _layerHuman, _layerConstruction);
        // Check if the closest target is within range
        if (closestTarget != null 
            && Vector3.Distance(_transform.position, closestTarget.position) <= _range 
            && closestTarget.gameObject.tag != "AttackBox")
        {

            parent.SetData("target", closestTarget);
            state = NodeState.SUCCESS;

        }
        else
        {
            state = NodeState.FAILURE;
        }
        
        return state;
    }
}
