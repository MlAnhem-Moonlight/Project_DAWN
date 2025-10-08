using BehaviorTree;
using UnityEngine;
using Spear.Movement;

public class DefensiveAction : Nodes
{
    private readonly Transform _transform;
    private readonly Transform _defTarget;
    private readonly float _defRadius;
    private readonly float _attackRange;
    private readonly float _speed;
    private readonly Animator _anim;
    private readonly float _offset;

    private AnimationController _controller;
    private DefensiveMovement _patrolMovement;
    private SpearBehavior _spear;

    // Cooldown skill
    private float _skillCooldown = 2f;   // 2 giây hồi chiêu
    private float _lastSkillTime = -Mathf.Infinity;

    public DefensiveAction(Transform self, Transform defTarget, float defRadius, float attackRange, float speed, float skillCD, Animator anim, float offset)
    {
        _transform = self;
        _defTarget = defTarget;
        _defRadius = defRadius;
        _attackRange = attackRange;
        _speed = speed;
        _anim = anim;
        _offset = offset;
        _skillCooldown = skillCD;
        _patrolMovement = new DefensiveMovement(_transform, _defTarget, offset, speed, _defRadius, attackRange);
        _spear = _transform.GetComponent<SpearBehavior>();
        _controller = _transform.GetComponent<AnimationController>();
    }



    public override NodeState Evaluate()
    {
        //Debug.Log($"[{_transform.name}] Thực hiện hành động tấn công bảo vệ.");
        // Nếu không phải trạng thái phòng thủ thì bỏ qua
        if (_spear == null || _spear.spearState != AllyState.Defensive 
            || _spear.currentState == AnimatorState.Attack
            || _spear.currentState == AnimatorState.UsingSkill)
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
        float distToDef = Vector2.Distance(_transform.position, pos);
        float distToEnemy = Mathf.Sqrt(_transform.position.x -  target.position.x);

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
                    //_anim.SetInteger("State", 1); // normal attack
                    //_anim.SetFloat("Direct", target.position.x - _transform.position.x > 0 ? 1f : -1f);
                    CheckMovement(target.position, "Attack 1", "Attack 0");
                }
                state = NodeState.RUNNING; // vẫn đang chờ hồi chiêu
            }
            return state;
        }
        else
        {
            // Nếu ra ngoài phạm vi phòng thủ → quay lại
            if (distToDef > _defRadius)
            {
                //Debug.Log($"[{_transform.name}] Ra ngoài phạm vi bảo vệ ({distToDef:F1} > {_defRadius}), quay lại vị trí phòng thủ.");
                _patrolMovement.Evaluate();
                state = NodeState.FAILURE;
                return state;
            }
            else
            {

                // Nếu enemy trong phạm vi bảo vệ nhưng ngoài tầm đánh → chase
                ChaseTarget(target, distToEnemy, _attackRange, Vector2.Distance(target.position, pos), _defRadius);
            }

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
            //Debug.LogWarning($"[{_transform.name}] Không có mục tiêu để đuổi.");
            return;
        }
        if (distToDef > _defRadius) return;
        if (disToEnemy <= atkRange) 
        {
            //Debug.Log($"[{_transform.name}] Đã trong tầm tấn công, không di chuyển.");
            return;
        } 
        //Debug.Log($"[{_transform.name}] Đuổi mục tiêu {target.name} (cách {Vector2.Distance(_transform.position, target.position):F1})");
        Vector3 targetPos = new Vector3(target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(
            _transform.position,
            targetPos,
            _speed * Time.deltaTime
        );

        if (_anim != null)
        {
            //_anim.SetInteger("State", 0); // 0 = chạy
            //_anim.SetFloat("Direct", target.position.x - _transform.position.x > 0 ? 1f : -1f);
            CheckMovement(target.position, "Run2 1", "Run2");
        }
    }

    /// <summary>
    /// Dùng skill tấn công mục tiêu.
    /// </summary>
    private void UseSkill(Transform target)
    {
        if (_anim != null)
        {
            //_anim.SetInteger("State", 2); // 1 = animation skill
            //_anim.SetFloat("Direct", target.position.x - _transform.position.x > 0 ? 1f : -1f);
            CheckMovement(target.position, "Skill 1", "Skill 0");
        }

        // Thêm logic gây damage hoặc spawn skill object
        Debug.Log($"[{_transform.name}] Dùng skill vào {target.name} lúc {Time.time:F2}");
    }

    private void CheckMovement(Vector3 targetPos, string state1, string state2)
    {
        if (_transform.position.x - targetPos.x > 0) _controller.ChangeAnimation(_anim, state1);
        else _controller.ChangeAnimation(_anim, state2);
    }
}
