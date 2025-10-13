using BehaviorTree;
using UnityEngine;

public class TheHandCheckTarget : Nodes
{
    private Transform _transform;
    private float _range;
    private Transform _defaultTarget;
    private string _layerHuman;
    private string _layerConstruction;
    private Animator _animator;

    public TheHandCheckTarget(Transform transform, float range, Transform defaultTarget, string layerHuman, string layerConstruction, Animator animator)
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
        Transform currentTarget = parent.GetData("target") as Transform;
        Debug.Log("Checking target: " + (currentTarget != null ? currentTarget.name : "null"));
        // Nếu đã có target và nó vẫn còn tồn tại
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            Stats stats = currentTarget.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                float distance = Vector2.Distance(_transform.position, currentTarget.position);

                // Kiểm tra hướng
                _animator.SetFloat("Direct", _transform.position.x - currentTarget.position.x > 0 ? -1 : 1);

                // Ngoài tầm đánh -> FAILURE để di chuyển lại gần
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // Nếu target không hợp lệ => tìm mới
        Transform closestTarget = TargetSelector.GetClosestTarget(_transform, _range, _layerHuman, _layerConstruction);
        if (closestTarget != null && closestTarget.gameObject.activeInHierarchy)
        {
            Stats stats = closestTarget.GetComponent<Stats>();
            if (stats != null && stats.currentHP > 0)
            {
                _animator.SetFloat("Direct", _transform.position.x - closestTarget.position.x > 0 ? -1 : 1);
                parent.SetData("target", closestTarget);
                state = NodeState.FAILURE; // mới phát hiện target mới => cần di chuyển tới
                return state;
            }
        }

        // Không có mục tiêu khả dụng, fallback về default
        _animator.SetFloat("Direct", _transform.position.x - _defaultTarget.position.x > 0 ? -1 : 1);
        parent.SetData("target", _defaultTarget);
        state = NodeState.FAILURE;
        return state;
    }
}
