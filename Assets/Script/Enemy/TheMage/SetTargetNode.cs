using BehaviorTree;
using UnityEngine;

public class MageSetTargetNode : Nodes
{
    private TheMageMovement _theMageMovement;

    public MageSetTargetNode(TheMageMovement theMageMovement)
    {
        _theMageMovement = theMageMovement;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)parent.GetData("target");

        // Kiểm tra target còn hợp lệ hay không
        if (target != null
            && target.gameObject.activeInHierarchy
            && !_theMageMovement.isAttack)
        {
            Stats stats = target.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                _theMageMovement.SetTarget(target);
                state = NodeState.SUCCESS;
            }
            else
            {
                // Target đã chết
                state = NodeState.FAILURE;
            }
        }
        else
        {
            // Target không tồn tại hoặc bị ẩn
            state = NodeState.FAILURE;
        }

        return state;
    }
}
