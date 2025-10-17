using BehaviorTree;
using UnityEngine;
using Spear.Movement;

public class DefensiveAction : Nodes
{
    private readonly Transform _transform;
    private readonly Transform _defTarget;
    private readonly float _defRadius;      // mainBaseRadius (vùng quét)
    private readonly float _attackRange;    // attack range
    private readonly float _patrolRadius;   // patrol radius (vùng cho phép di chuyển)
    private readonly float _speed;
    private readonly Animator _anim;
    private readonly float _offset;

    private AnimationController _controller;
    private DefensiveMovement _patrolMovement;
    private SpearBehavior _spear;

    private float _skillCooldown = 2f;
    private float _lastSkillTime = -Mathf.Infinity;

    // Ngưỡng xem là "rìa" patrol (tolerance)
    private const float EDGE_EPS = 0.15f;

    public DefensiveAction(Transform self, Transform defTarget, float patrolRadius, float attackRange, float mainBaseRadius, float speed, float skillCD, Animator anim, float offset)
    {
        _transform = self;
        _defTarget = defTarget;
        _patrolRadius = patrolRadius;
        _attackRange = attackRange;
        _defRadius = mainBaseRadius;
        _speed = speed;
        _anim = anim;
        _offset = offset;
        _skillCooldown = skillCD;

        _patrolMovement = new DefensiveMovement(_transform, _defTarget, offset, speed, patrolRadius, attackRange);
        _spear = _transform.GetComponent<SpearBehavior>();
        _controller = _transform.GetComponent<AnimationController>();
    }

    public override NodeState Evaluate()
    {
        // Kiểm tra trạng thái hợp lệ để thực hiện hành động
        if (_spear == null || _spear.spearState != AllyState.Defensive ||
            _spear.currentState == AnimatorState.Attack ||
            _spear.currentState == AnimatorState.UsingSkill)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // Lấy target từ blackboard
        object targetObj = parent.GetData("target");
        Transform target = targetObj as Transform;

        // Nếu không có mục tiêu → patrol
        if (target == null)
        {
            _patrolMovement.Evaluate();
            state = NodeState.FAILURE;
            return state;
        }

        // Vị trí tham chiếu
        Vector3 defPos = _defTarget.position + Vector3.left * _offset;

        // Khoảng cách:
        float distSpearToDef = Mathf.Abs(_transform.position.x - defPos.x);
        float distEnemyToDef = Vector2.Distance(new Vector2(target.position.x, target.position.y), new Vector2(defPos.x, defPos.y));
        float distSpearToEnemy = Mathf.Abs(_transform.position.x - target.position.x);

        bool spearInsidePatrol = distSpearToDef <= _patrolRadius + 0.001f;
        //bool spearAtEdge = Mathf.Abs(distSpearToDef - _patrolRadius) <= EDGE_EPS || distSpearToDef >= _patrolRadius - EDGE_EPS;
        bool enemyInPatrolArea = distEnemyToDef <= _patrolRadius;
        bool enemyInMainBase = distEnemyToDef <= _defRadius;
        bool enemyInAttack = distSpearToEnemy <= _attackRange;
        // Nếu spear đang ở ngoài vùng cho phép (vô tình đi quá xa) -> quay về patrol
        if (!spearInsidePatrol)
        {
            _patrolMovement.Evaluate();
            state = NodeState.FAILURE;
            return state;
        }

        // Ưu tiên: nếu spear có thể tấn công mục tiêu ngay (tức distSpearToEnemy <= attackRange), thì tấn công bất kể vị trí enemy
        // (theo yêu cầu: spear ở rìa có thể đánh dù enemy ở đâu)
        if (enemyInAttack)
        {
            // Attack ngay
            if (Time.time >= _lastSkillTime + _skillCooldown)
            {
                UseSkill(target);
                _lastSkillTime = Time.time;
                state = NodeState.SUCCESS;
            }
            else
            {
                SwitchAnim(target.position, "Attack 1", "Attack 0", 0f);
                state = NodeState.RUNNING;
            }
            return state;
        }
        Debug.Log($"{enemyInPatrolArea && enemyInMainBase}");
        // Nếu enemy nằm trong cùng patrolRadius và trong vùng quét (mainBaseRadius)
        if (enemyInPatrolArea && enemyInMainBase)
        {
            // Nếu chưa vào tầm đánh thì chase, nhưng đảm bảo không rời khỏi patrolRadius khi chase
            if (!enemyInAttack)
            {
                // Tính vị trí dự kiến khi chase: di chuyển một bước và kiểm tra khoảng cách đến defPos
                Vector3 desiredPos = Vector3.MoveTowards(_transform.position, new Vector3(target.position.x, _transform.position.y, _transform.position.z), _speed * Time.deltaTime);
                float distDesiredToDef = Vector2.Distance(new Vector2(desiredPos.x, desiredPos.y), new Vector2(defPos.x, defPos.y));

                if (distDesiredToDef <= _patrolRadius + 0.001f)
                {
                    ChaseTarget(target);
                    state = NodeState.RUNNING;
                    return state;
                }
                else
                {
                    // Nếu chase sẽ làm vượt patrolRadius -> không chase, quay lại patrol
                    _patrolMovement.Evaluate();
                    state = NodeState.FAILURE;
                    return state;
                }
            }
            // else branch attack already handled above
        }

        // Các trường hợp khác: không đủ điều kiện tấn công/chase -> patrol
        _patrolMovement.Evaluate();
        state = NodeState.FAILURE;
        return state;
    }

    private void ChaseTarget(Transform target)
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPos, _speed * Time.deltaTime);
        SwitchAnim(target.position, "Run2 1", "Run2");
    }

    private void UseSkill(Transform target)
    {
        SwitchAnim(target.position, "Skill 1", "Skill 0", 0f);
        Debug.Log($"[{_transform.name}] Dùng skill vào {target.name} lúc {Time.time:F2}");
        // TODO: thêm logic gây damage / spawn skill object tại đây
    }

    private void SwitchAnim(Vector3 targetPos, string leftState, string rightState, float crossFade = 0.1f)
    {
        if (_transform.position.x - targetPos.x > 0)
            _controller.ChangeAnimation(_anim, leftState, crossFade);
        else
            _controller.ChangeAnimation(_anim, rightState, crossFade);
    }
}
