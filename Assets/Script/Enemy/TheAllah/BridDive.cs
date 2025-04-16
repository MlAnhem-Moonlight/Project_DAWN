using BehaviorTree;
using UnityEngine;

public class BridDive : Nodes
{
    private Transform _transform;
    private Rigidbody2D _rb;
    private Transform _target;
    private Animator _animator;
    private float _diveSpeed;
    private BridCollisionHandler _collisionHandler;

    public BridDive(Transform transform, Rigidbody2D rb, BridCollisionHandler collisionHandler, Transform target, float diveSpeed, Animator animator)
    {
        _transform = transform;
        _rb = rb;
        _collisionHandler = collisionHandler;
        _target = target;
        _diveSpeed = diveSpeed;
        _animator = animator;
        if (GetData("LastSeenTargetPosition") is Vector3 lastSeenPosition)
        {
            _target.position = lastSeenPosition;
        }
        _collisionHandler.OnBridCollision += HandleCollision; // Đăng ký sự kiện va chạm
    }

    public override NodeState Evaluate()
    {
        _rb.gravityScale = 0.5f; // Tạo cảm giác quái bị hút xuống nhưng vẫn có thể chỉnh hướng
        if (_target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        _animator.SetTrigger("Dive"); // Kích hoạt animation lao xuống

        // Di chuyển quái theo hướng của mục tiêu tại vị trí được phát hiện
        Vector2 directionToTarget = (_target.position - _transform.position).normalized;
        // Chỉ cần lao thẳng vào hướng của mục tiêu tại thời điểm phát hiện
        _rb.linearVelocity = directionToTarget * _diveSpeed;

        return NodeState.RUNNING;
    }

    private void HandleCollision()
    {
        GameObject.Destroy(_transform.gameObject); // Hủy quái vật khi va chạm
        state = NodeState.SUCCESS;
    }
}
