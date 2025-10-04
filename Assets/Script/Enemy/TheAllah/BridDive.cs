using BehaviorTree;
using UnityEngine;

public class BridDive : Nodes
{
    private Transform _transform;
    private Rigidbody2D _rb;
    private Animator _animator;


    public BridDive(Transform transform, Rigidbody2D rb, Animator animator)
    {
        _transform = transform;
        _rb = rb;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        // Kích hoạt animation rơi xuống
        //_animator.SetTrigger("Dive");

        // Set mass and gravity scale để rơi thẳng xuống
        _rb.mass = 100f;
        _rb.gravityScale = 1f;

        // Xoay hướng chim theo vector rơi
        RotateDownward();



        return NodeState.RUNNING;
    }

    private void RotateDownward()
    {
        Vector2 velocity = _rb.linearVelocity;

        if (velocity.sqrMagnitude > 0.01f) // tránh lỗi khi velocity = 0
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            _transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Nếu chưa có vận tốc, ép xoay thẳng xuống
            _transform.rotation = Quaternion.Euler(0, 0, -90f);
        }
    }


}
