using BehaviorTree;
using UnityEngine;
using Spear.Movement;

public class DefensiveAction : Nodes
{
    private readonly Transform _self;
    private readonly Transform _defTarget;
    private readonly float _defRadius;
    private readonly float _attackRange;
    private readonly float _speed;
    private readonly Animator _anim;

    private IMovementStrategy _patrolMovement;

    // Cooldown skill
    private float _skillCooldown = 2f;   // 2 giây hồi chiêu
    private float _lastSkillTime = -Mathf.Infinity;

    public DefensiveAction(Transform self, Transform defTarget, float defRadius, float attackRange, float speed, Animator anim)
    {
        _self = self;
        _defTarget = defTarget;
        _defRadius = defRadius;
        _attackRange = attackRange;
        _speed = speed;
        _anim = anim;

        _patrolMovement = new DefensiveMovement(_self, _defTarget, 1.5f, _speed / 2);
    }

    public override NodeState Evaluate()
    {
        object targetObj = GetData("target");
        if (targetObj == null)
        {
            // Không có enemy thì patrol quanh defensive target
            _patrolMovement.Tick();
            state = NodeState.RUNNING;
            return state;
        }

        Transform target = (Transform)targetObj;

        float distToDef = Vector2.Distance(_self.position, _defTarget.position);
        float distToEnemy = Vector2.Distance(_self.position, target.position);

        // Nếu ra ngoài phạm vi bảo vệ -> quay lại
        if (distToDef > _defRadius)
        {
            _patrolMovement.Tick();
            state = NodeState.RUNNING;
            return state;
        }

        // Nếu enemy trong tầm đánh
        if (distToEnemy <= _attackRange)
        {
            if (Time.time >= _lastSkillTime + _skillCooldown)
            {
                UseSkill(target);
                _lastSkillTime = Time.time;
                state = NodeState.SUCCESS; // thành công vì dùng skill
                return state;
            }
            else
            {
                // đang hồi chiêu → idle chờ hoặc di chuyển nhẹ
                if (_anim != null) _anim.SetInteger("State", 0); // idle
                state = NodeState.RUNNING;
                return state;
            }
        }
        else
        {
            // Nếu enemy trong phạm vi bảo vệ nhưng ngoài attack range → chase
            ChaseTarget(target);
            state = NodeState.RUNNING;
            return state;
        }
    }

    /// <summary>
    /// Hàm di chuyển đến target (chase).
    /// </summary>
    private void ChaseTarget(Transform target)
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(target.position.x, _self.position.y, _self.position.z);
        _self.position = Vector3.MoveTowards(
            _self.position,
            targetPos,
            _speed * Time.deltaTime
        );

        if (_anim != null)
        {
            _anim.SetInteger("State", 0); // ví dụ 0 = chạy bộ
            float dir = target.position.x - _self.position.x;
            _anim.SetFloat("Direct", dir > 0 ? 1 : -1);
        }
    }

    /// <summary>
    /// Hàm sử dụng skill (attack).
    /// </summary>
    private void UseSkill(Transform target)
    {
        if (_anim != null)
        {
            _anim.SetInteger("State", 1); // 1 = animation skill
        }

        // Thêm logic gây damage hoặc spawn skill object
        Debug.Log($"{_self.name} dùng skill vào {target.name} lúc {Time.time}");
    }
}
