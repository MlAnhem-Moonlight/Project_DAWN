using BehaviorTree;
using UnityEngine;

public class CheckInRange : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerHuman;
    private string _layerDeer;
    private Animator _animator;
    private Transform Target = null;

    public CheckInRange(Transform transform, float range, Animator animator, string layerHuman, string layerDeer)
    {
        _transform = transform;
        _range = range;
        _animator = animator;
        _layerHuman = layerHuman;
        _layerDeer = layerDeer;
    }

    public override NodeState Evaluate()
    {
        //Debug.Log("Checking in range for target");
        Target = TargetSelector.GetClosestTarget(_transform, _range, _layerHuman, _layerDeer, _defaultTarget);
        if (Target != null)
        { 
            parent.SetData("target", Target);
            EnvMovement.SetTarget(Target);
            EnvMovement.SetWandering(false);
            state = NodeState.SUCCESS;
        }
        state = NodeState.FAILURE;
        return state;

    }
}
