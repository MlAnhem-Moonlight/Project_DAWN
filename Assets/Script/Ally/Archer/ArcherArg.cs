using BehaviorTree;
using UnityEngine;
using System.Linq;

public class ArcherArg : Nodes
{
    private readonly Transform _transform;
    private readonly float _attackRange;
    private readonly LayerMask _enemyLayer;
    private readonly float _skillCooldown;
    private float _lastSkillTime = -Mathf.Infinity;

    private AnimationController _controller;

    public ArcherArg(Transform transform, float attackRange, float skillCD, string layerName = "Demon")
    {
        _transform = transform;
        _attackRange = attackRange;
        _skillCooldown = 1;
        _enemyLayer = LayerMask.GetMask(layerName);
        _controller = _transform.GetComponent<AnimationController>();
    }

    public override NodeState Evaluate()
    {

        var archer = _transform.GetComponent<ArcherBehavior>();
        if (archer == null 
            || _transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.Attack
            || _transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.UsingSkill)
        {
            state = NodeState.FAILURE;
            return state;
        }

        // --- Quét kẻ địch trong tầm ---
        Collider2D[] hits = Physics2D.OverlapCircleAll(_transform.position, _attackRange, _enemyLayer)
            .Where(h => h.GetComponent<Stats>() != null)
            .ToArray();

        if (hits.Length == 0)
        {
            // No enemies -> go to idle animation/state
            archer.SetTarget(null);
            archer.ChangeState(AnimatorState.Idle);
            var anim = _transform.GetComponent<Animator>();
            if (_controller != null && anim != null)
            {
                // Use a default idle animation (choose one consistent with other scripts)
                _controller.ChangeAnimation(anim, "Idle 0", 0.1f);
            }

            state = NodeState.FAILURE;
            return state;
        }

        // --- Chọn mục tiêu dựa theo state ---
        Transform target = null;
        if (archer.spearState == AllyState.Aggressive)
        {
            // Aggressive → tấn công xa nhất
            target = hits
                .OrderByDescending(h => Vector2.Distance(_transform.position, h.transform.position))
                .FirstOrDefault()
                .transform;
        }
        else //(archer.spearState == AllyState.Defensive)
        {
            // Defensive → tấn công gần nhất
            target = hits
                .OrderBy(h => Vector2.Distance(_transform.position, h.transform.position))
                .FirstOrDefault()
                .transform;
        }
        //Debug.Log($"[{_transform.name}] Chọn mục tiêu: {target.name}"); 
        if (target == null)
        {
            _transform.GetComponent<ArcherBehavior>().SetTarget(null);
            state = NodeState.FAILURE;
            return state;
        }

        float distance = Vector2.Distance(_transform.position, target.position);
        _transform.GetComponent<ArcherBehavior>().SetTarget(target.gameObject);
        // --- Tấn công ---
        if (distance <= _attackRange)
        {
            if (Time.time >= _lastSkillTime + _skillCooldown)
            {
                Debug.Log($"[{_transform.name}] ArcherArg Use Skill on {target.name} at {Time.time:F2}");
                CheckAnimation(target.position, "Skill 1", "Skill 0", 0f);
                _lastSkillTime = Time.time;
                //FireArrow(target);

            }
            else
            {
                CheckAnimation(target.position, "Attack 1", "Attack 0", 0f);
            }
            state = NodeState.RUNNING;
            return state;
        }
        else
        {
            _transform.GetComponent<ArcherBehavior>().SetTarget(null);
            state = NodeState.FAILURE;
            return state;
        }

    }

    private void FireArrow(Transform target)
    {
        // Placeholder logic cho bắn tên
        // var archer = _transform.GetComponent<ArcherBehavior>();
        // if (archer != null && archer.arrowPrefab != null)
        // {
        //     Vector3 shootDir = (target.position - _transform.position).normalized;
        //     GameObject arrow = Object.Instantiate(archer.arrowPrefab, _transform.position, Quaternion.identity);
        //     arrow.GetComponent<Projectile>().Initialize(shootDir, target);
        // }
        // Debug.Log($"[{_transform.name}] Bắn tên vào {target.name} lúc {Time.time:F2}");
    }

    private void CheckAnimation(Vector3 targetPos, string animRight, string animLeft, float crossFade = 0.1f)
    {
        if (_transform.position.x - targetPos.x > 0)
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), animRight, crossFade);
        else
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), animLeft, crossFade);
    }
}
