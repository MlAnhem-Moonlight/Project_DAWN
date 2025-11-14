using BehaviorTree;
using UnityEngine;

public class BridMovement : Nodes
{
    private Transform _transform;
    private Rigidbody2D _rb;
    private Transform _target;
    private Animator _animator;
    private float _speed;
    private float _minHeight; // Độ cao tối thiểu để vỗ cánh
    private float _flapForce = 1f; // Lực đẩy khi vỗ cánh

    public BridMovement(Transform transform, Rigidbody2D rb, float speed, Animator animator, Transform target, float minHeight)
    {
        _transform = transform;
        _rb = rb;
        _speed = speed;
        _target = target;
        _animator = animator;
        _minHeight = minHeight;
    }

    public override NodeState Evaluate()
    {
        //Debug.Log("BridMovement: Đang di chuyển về phía mục tiêu: " + _target.name);
        if (_target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }

        FlyTowardsTarget();
        return NodeState.RUNNING;
    }

    private void FlyTowardsTarget()
    {
        Vector2 direction = (_target.position - _transform.position).normalized;

        // Chỉ gán giá trị tốc độ theo trục x
        _rb.linearVelocity = new Vector2(direction.x * _speed, _rb.linearVelocity.y);

        // Khi độ cao giảm xuống dưới mức tối thiểu, vỗ cánh đẩy lên
        if (_transform.position.y < _minHeight)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _flapForce);
            //_animator.SetTrigger("Flap"); // Animation vỗ cánh
        }
    }
}
