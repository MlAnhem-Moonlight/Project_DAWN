using BehaviorTree;
using UnityEngine;

/// <summary>
/// Di chuyển Archer đến checkpoint (mỗi Archer có checkpoint riêng).
/// Khi tới nơi (trong phạm vi _stopDistance), ẩn checkpoint và trả về SUCCESS.
/// </summary>
public class ArcherMovement : Nodes
{
    private readonly Transform _transform;
    private Transform _checkpoint;
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

    public void SetCheckPoint(Transform checkpoint)
    {
        _checkpoint = checkpoint;
    }

    public override NodeState Evaluate()
    {
        var archer = _transform.GetComponent<ArcherBehavior>();
        // Nếu Archer đang ở trạng thái trung lập thì không di chuyển
        if (archer.spearState == AllyState.Neutral)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // Nếu chưa có checkpoint thì không di chuyển
        if (_checkpoint == null)
        {
            GameObject cpObj = GameObject.Find($"{_transform.name}_CheckPoint");
            if (cpObj != null)
                _checkpoint = cpObj.transform;
            else
            {
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // Nếu đang tấn công hoặc dùng kỹ năng thì dừng di chuyển
        if (archer.currentState == AnimatorState.Attack || archer.currentState == AnimatorState.UsingSkill)
        {
            return NodeState.SUCCESS;
        }

        Vector3 targetPos = new Vector3(_checkpoint.position.x, _transform.position.y, _transform.position.z);
        float distance = Vector2.Distance(_transform.position, targetPos);

        // Nếu đã đến vị trí checkpoint hoặc có mục tiêu
        if (distance <= _stopDistance || archer.target != null)
        {
            if (_checkpoint.gameObject.activeSelf)
                _checkpoint.gameObject.SetActive(false); // Ẩn checkpoint khi đã tới nơi

            return state = NodeState.SUCCESS;
        }

        // Nếu chưa đến checkpoint thì di chuyển
        CheckMovement(targetPos, "Run2 1", "Run2",0f);
        _transform.position = Vector3.MoveTowards(
            _transform.position,
            targetPos,
            _speed * Time.deltaTime
        );

        if (!_checkpoint.gameObject.activeSelf)
            _checkpoint.gameObject.SetActive(true); // Hiện checkpoint nếu bị ẩn

        return state = NodeState.FAILURE;
    }

    private void CheckMovement(Vector3 targetPos, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (_transform.position.x - targetPos.x > 0)
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), leftAnim, crossFade);
        else
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), rightAnim, crossFade);
    }
}
