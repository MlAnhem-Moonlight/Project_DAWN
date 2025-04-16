using BehaviorTree;
using UnityEngine;

public class BridCheckDistance : Nodes
{
    private Transform _transform;
    private Transform _target;
    private float _diveThreshold;
    private bool _isDiving;

    public BridCheckDistance(Transform transform, Transform target, float diveThreshold)
    {
        _transform = transform;
        _target = target;
        _diveThreshold = diveThreshold;
    }

    public override NodeState Evaluate()
    {
        if (_target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }
        //việc kiểm tra khoảng cách làm cho quái quay lại trạng thái di chuyển dù đang lao xuống do nhân vật đi ra ngoài tầm rơi
        //hành vi mong muốn là khi tới khoảng cách nhất định thì quái sẽ lao xuống vị trí cuối cùng nhìn thấy nhân vật
        float distanceX = Mathf.Abs(_transform.position.x - _target.position.x);
        if (distanceX <= _diveThreshold)      _isDiving = true;
        if(_isDiving)
        {
            state = NodeState.SUCCESS;
            if(GetData("LastSeenTargetPosition") == null)
            {
                SetData("LastSeenTargetPosition", _target.position);
            }
        }
        else
            state = NodeState.FAILURE;
        return state;
    }
}
