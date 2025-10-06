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

    private DefensiveMovement _patrolMovement;
    private SpearBehavior _spear;

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

        _patrolMovement = new DefensiveMovement(_self, _defTarget, 1.5f, _speed / 2, _defRadius);
        _spear = _self.GetComponent<SpearBehavior>();
    }

    public override NodeState Evaluate()
    {
        // Nếu không phải trạng thái phòng thủ thì bỏ qua
        if (_spear == null || _spear.spearState != AllyState.Defensive)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // Lấy mục tiêu enemy (được CheckEnemyInRangeAlly đặt vào blackboard)
        object targetObj = GetData("target");
        Transform target = targetObj as Transform;

        // Không có enemy → tuần tra quanh defensive target
        if (target == null)
        {
            _patrolMovement.Evaluate(); // gọi logic phòng thủ tuần tra
            state = NodeState.RUNNING;
            return state;
        }

        float distToDef = Vector2.Distance(_self.position, _defTarget.position);
        float distToEnemy = Vector2.Distance(_self.position, target.position);

        // Nếu ra ngoài phạm vi phòng thủ → quay lại
        if (distToDef > _defRadius)
        {
            Debug.Log($"[{_self.name}] Ra ngoài phạm vi bảo vệ ({distToDef:F1} > {_defRadius}), quay lại vị trí phòng thủ.");
            _patrolMovement.Evaluate();
            state = NodeState.RUNNING;
            return state;
        }

        // Nếu enemy trong tầm tấn công
        if (distToEnemy <= _attackRange)
        {
            if (Time.time >= _lastSkillTime + _skillCooldown)
            {
                UseSkill(target);
                _lastSkillTime = Time.time;
                state = NodeState.SUCCESS; // đã tấn công
            }
            else
            {
                if (_anim != null)
                {
                    _anim.SetInteger("State", 5); // idle chờ
                    _anim.SetFloat("Direct", target.position.x - _self.position.x > 0 ? 1f : -1f);
                }
                state = NodeState.RUNNING; // vẫn đang chờ hồi chiêu
            }
            return state;
        }

        // Nếu enemy trong phạm vi bảo vệ nhưng ngoài tầm đánh → chase
        Debug.Log($"[{_self.name}] Đuổi mục tiêu {target.name} (cách {distToEnemy:F1})");
        ChaseTarget(target);
        state = NodeState.RUNNING;
        return state;
    }

    /// <summary>
    /// Di chuyển đuổi mục tiêu (chase).
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
            _anim.SetInteger("State", 0); // 0 = chạy
            _anim.SetFloat("Direct", target.position.x - _self.position.x > 0 ? 1f : -1f);
        }
    }

    /// <summary>
    /// Dùng skill tấn công mục tiêu.
    /// </summary>
    private void UseSkill(Transform target)
    {
        if (_anim != null)
        {
            _anim.SetInteger("State", 1); // 1 = animation skill
            _anim.SetFloat("Direct", target.position.x - _self.position.x > 0 ? 1f : -1f);
        }

        // Thêm logic gây damage hoặc spawn skill object
        Debug.Log($"[{_self.name}] Dùng skill vào {target.name} lúc {Time.time:F2}");
    }
}
