using BehaviorTree;
using UnityEngine;

public class TheHandSetTarget : Nodes
{
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

        if (target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // ✅ Set target cho movement node
        _theHandMovement.SetTarget(target);
        float distance = Mathf.Abs(_transform.position.x - target.position.x);

        // ✅ Nếu trong tầm đánh → SUCCESS (cho attack node xử lý)
        // ✅ Nếu ngoài tầm → FAILURE (cho movement node di chuyển lại gần)
        if (distance <= _range)
        {
            state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.FAILURE;  // ✅ Ngoài tầm → di chuyển lại
        }

        return state;
    }
}
