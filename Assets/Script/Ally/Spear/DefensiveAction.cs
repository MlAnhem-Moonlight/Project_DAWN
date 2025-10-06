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
    private readonly float _offset;

    private DefensiveMovement _patrolMovement;
    private SpearBehavior _spear;

    // Cooldown skill
    private float _skillCooldown = 2f;   // 2 giây hồi chiêu
    private float _lastSkillTime = -Mathf.Infinity;

    public DefensiveAction(Transform self, Transform defTarget, float defRadius, float attackRange, float speed, Animator anim, float offset)
    {
        _self = self;
        _defTarget = defTarget;
        _defRadius = defRadius;
        _attackRange = attackRange;
        _speed = speed;
        _anim = anim;
        _offset = offset;

        _patrolMovement = new DefensiveMovement(_self, _defTarget, offset, speed, _defRadius);
        _spear = _self.GetComponent<SpearBehavior>();
    }

    public override NodeState Evaluate()
    {
        //Debug.Log($"[{_self.name}] Thực hiện hành động tấn công bảo vệ.");
        // Nếu không phải trạng thái phòng thủ thì bỏ qua
        if (_spear == null || _spear.spearState != AllyState.Defensive)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // Lấy mục tiêu enemy (được CheckEnemyInRangeAlly đặt vào blackboard)
        object targetObj = parent.GetData("target");
        Transform target = targetObj as Transform;

        // Không có enemy → tuần tra quanh defensive target
        if (target == null)
        {
            _patrolMovement.Evaluate(); // gọi logic phòng thủ tuần tra
            state = NodeState.FAILURE;
            return state;
        }
        Vector3 pos = _defTarget.position + Vector3.left * _offset;
        float distToDef = Vector2.Distance(_self.position, pos);
        float distToEnemy = Mathf.Sqrt(_self.position.x -  target.position.x);

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
                    _anim.SetInteger("State", 1); // normal attack
                    _anim.SetFloat("Direct", target.position.x - _self.position.x > 0 ? 1f : -1f);
                }
                state = NodeState.RUNNING; // vẫn đang chờ hồi chiêu
            }
            return state;
        }

        // Nếu ra ngoài phạm vi phòng thủ → quay lại
        if (distToDef > _defRadius)
        {
            //Debug.Log($"[{_self.name}] Ra ngoài phạm vi bảo vệ ({distToDef:F1} > {_defRadius}), quay lại vị trí phòng thủ.");
            _patrolMovement.Evaluate();
            state = NodeState.FAILURE;
            return state;
        }
        else
        {

            // Nếu enemy trong phạm vi bảo vệ nhưng ngoài tầm đánh → chase
            ChaseTarget(target, distToEnemy, _attackRange, Vector2.Distance(target.position, pos), _defRadius);
        }


        state = NodeState.RUNNING;
        return state;
    }

    /// <summary>
    /// Di chuyển đuổi mục tiêu (chase).
    /// </summary>
    private void ChaseTarget(Transform target, float disToEnemy, float atkRange, float distToDef, float defRadius)
    {
        if (target == null)
        {
            //Debug.LogWarning($"[{_self.name}] Không có mục tiêu để đuổi.");
            return;
        }
        if (distToDef > _defRadius) return;
        if (disToEnemy <= atkRange) 
        {
            //Debug.Log($"[{_self.name}] Đã trong tầm tấn công, không di chuyển.");
            return;
        } 
        //Debug.Log($"[{_self.name}] Đuổi mục tiêu {target.name} (cách {Vector2.Distance(_self.position, target.position):F1})");
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
            _anim.SetInteger("State", 2); // 1 = animation skill
            _anim.SetFloat("Direct", target.position.x - _self.position.x > 0 ? 1f : -1f);
        }

        // Thêm logic gây damage hoặc spawn skill object
        Debug.Log($"[{_self.name}] Dùng skill vào {target.name} lúc {Time.time:F2}");
    }
}
