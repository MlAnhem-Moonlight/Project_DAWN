using BehaviorTree;
using UnityEngine;

public class TheHandSetTarget : Nodes
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private TheHandMovement _theHandMovement;
    private Transform _transform;
    private float _range;

    public TheHandSetTarget(TheHandMovement theHandMovement, Transform transform, float range)
    {
        _theHandMovement = theHandMovement;
        _transform = transform;
        _range = range;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        _theHandMovement.SetTarget(target);
        float distance = Mathf.Abs(_transform.position.x - target.position.x);

        // Nếu trong tầm đánh -> SUCCESS
        if (distance <= _range)
        {
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}
