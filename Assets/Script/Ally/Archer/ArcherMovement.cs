using BehaviorTree;
using UnityEngine;

/// <summary>
/// Di chuyển Archer đến checkpoint (vị trí cố định trên map).
/// Khi tới nơi (trong phạm vi _stopDistance), trả về SUCCESS.
/// </summary>
public class ArcherMovement : Nodes
{
    private readonly Transform _transform;
    private readonly Transform _checkpoint;
    private readonly float _speed;
    private readonly float _stopDistance;
    private readonly float _attackRange;
    private AnimationController _controller;

    public ArcherMovement(Transform transform, Transform checkpoint, float speed, float stopDistance, float attackRange)
    {
        _transform = transform;
        _checkpoint = checkpoint;
        _speed = speed;
        _stopDistance = stopDistance;
        _attackRange = attackRange;
        _controller = _transform.GetComponent<AnimationController>();
    }

    public override NodeState Evaluate()
    {
        if (_transform.GetComponent<ArcherBehavior>().spearState == AllyState.Neutral)
        {
            Debug.Log($"[{_transform.name}] ArcherMovement FAILURE due to state or has target");
            state = NodeState.FAILURE;
            return state;
        }
        if (_transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.Attack
            || _transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.UsingSkill)
        {
           
            return NodeState.SUCCESS;
        }

        // Tính vị trí di chuyển (chỉ theo trục X để không thay đổi độ cao)
        Vector3 targetPos = new Vector3(_checkpoint.position.x, _transform.position.y, _transform.position.z);
        float distance = Vector2.Distance(_transform.position, targetPos);
        // Nếu đã đến vị trí checkpoint
        if (distance <= _stopDistance)
        {
            
            return state = NodeState.SUCCESS;
        }

        // Nếu chưa đến thì di chuyển
        CheckMovement(targetPos, "Run2 1", "Run2");
        _transform.position = Vector3.MoveTowards(
            _transform.position,
            targetPos,
            _speed * Time.deltaTime
        );
        return state = NodeState.RUNNING;
    }

    private void CheckMovement(Vector3 targetPos, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (_transform.position.x - targetPos.x > 0)
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), leftAnim, crossFade);
        else
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), rightAnim, crossFade);
    }
}
