using BehaviorTree;
using UnityEngine;

public class BridDive : Nodes
{
    private Transform _transform;
    private Rigidbody2D _rb;
    private Animator _animator;
    private BridCollisionHandler _collisionHandler;

    public BridDive(Transform transform, Rigidbody2D rb, BridCollisionHandler collisionHandler, Animator animator)
    {
        _transform = transform;
        _rb = rb;
        _collisionHandler = collisionHandler;
        _animator = animator;
    }

    public override NodeState Evaluate()
    {
        // Kích hoạt animation rơi xuống
        _animator.SetTrigger("Dive");
        // Set mass and gravity scale for falling straight down
        _rb.mass = 100f; // Tăng khối lượng
        _rb.gravityScale = 1f; // Tăng gravity scale để rơi thẳng xuống

        _collisionHandler.OnBridCollision += HandleCollision; // Đăng ký sự kiện va chạm
        // Không cần thiết lập vận tốc vì gravity sẽ tự động kéo đối tượng xuống
        return NodeState.RUNNING;
    }

    private void HandleCollision()
    {
        //nếu va chạm gọi animation nổ và tính dmg sau đó gọi destroy
        GameObject.Destroy(_transform.gameObject); // Hủy đối tượng khi va chạm
        state = NodeState.SUCCESS;
    }
}
