using BehaviorTree;
using UnityEngine;
using System.Linq;

namespace Spear.Movement
{
    /// <summary>
    /// Di chuyển chủ động tấn công – tìm kẻ địch gần nhất (tag hoặc layer chỉ định),
    /// tiến đến tầm tấn công, sau đó SUCCESS để chuyển sang node hành động (attack).
    /// </summary>
    public class AggressiveMovement : Nodes
    {
        private readonly Transform _transform;
        private readonly float _speed;
        private readonly float _attackRange;
        private readonly LayerMask _enemyLayer;
        private AnimationController _controller;

        // Cooldown skill
        private float _skillCooldown = 2f;   // 2 giây hồi chiêu
        private float _lastSkillTime = -Mathf.Infinity;

        public AggressiveMovement(Transform transform, float speed, float attackRange, float skillCD, string layerName = "Demon")
        {
            _transform = transform;
            _speed = speed;
            _attackRange = attackRange;
            _skillCooldown = skillCD;
            _enemyLayer = LayerMask.GetMask(layerName);
            _controller = _transform.GetComponent<AnimationController>();
        }

        public override NodeState Evaluate()
        {
            var spear = _transform.GetComponent<SpearBehavior>();
            if (spear == null || spear.spearState != AllyState.Aggressive
                || spear.currentState == AnimatorState.Attack
                || spear.currentState == AnimatorState.UsingSkill)
            {
                state = NodeState.FAILURE;
                return state;
            }

            // --- Tìm enemy gần nhất ---
            Collider2D[] hits = Physics2D.OverlapCircleAll(_transform.position, 300f, _enemyLayer); // phạm vi quét xa
            Transform nearestEnemy = hits
                .Where(h => h.GetComponent<Stats>() != null)
                .OrderBy(h => Vector2.Distance(_transform.position, h.transform.position))
                .Select(h => h.transform)
                .FirstOrDefault();

            // --- Không có enemy ---
            if (nearestEnemy == null)
            {
                // Không có mục tiêu → Idle
                CheckMovement(_transform.position + Vector3.right * 0.01f, "Idle 1", "Idle 0");
                state = NodeState.FAILURE;
                return state;
            }

            // --- Có enemy ---
            //parent.SetData("target", nearestEnemy);
        
            float distance = Mathf.Sqrt(_transform.position.x - nearestEnemy.position.x);
            //Debug.Log(nearestEnemy.name + " Distance: " + distance);
            // --- Nếu trong tầm tấn công ---
            if (distance <= _attackRange)
            {

                if (Time.time >= _lastSkillTime + _skillCooldown)
                {
                    UseSkill(nearestEnemy);
                    _lastSkillTime = Time.time;
                    //state = NodeState.RUNNING; // đã tấn công
                }
                else
                {
                    if (_transform.GetComponent<Animator>())
                    {
                        //_anim.SetInteger("State", 1); // normal attack
                        //_anim.SetFloat("Direct", target.position.x - _transform.position.x > 0 ? 1f : -1f);
                        CheckMovement(nearestEnemy.position, "Attack 1", "Attack 0", 0f);
                    }
                    //state = NodeState.RUNNING; // vẫn đang chờ hồi chiêu
                }
                state = NodeState.RUNNING;
                return state;
            }

            // --- Nếu chưa trong tầm tấn công → di chuyển tới ---
            Vector3 targetPos = new Vector3(nearestEnemy.position.x, _transform.position.y, _transform.position.z);
            _transform.position = Vector3.MoveTowards(_transform.position, targetPos, _speed * Time.deltaTime);

            CheckMovement(nearestEnemy.position, "Run2 1", "Run2");
            state = NodeState.RUNNING;
            return state;
        }

        private void UseSkill(Transform target)
        {
            if (_transform.GetComponent<Animator>())
            {
                //_anim.SetInteger("State", 2); // 1 = animation skill
                //_anim.SetFloat("Direct", target.position.x - _transform.position.x > 0 ? 1f : -1f);
                CheckMovement(target.position, "Skill 1", "Skill 0", 0f);
            }

            // Thêm logic gây damage hoặc spawn skill object
            //Debug.Log($"[{_transform.name}] Dùng skill vào {target.name} lúc {Time.time:F2}");
        }


        private void CheckMovement(Vector3 targetPos, string state1, string state2, float crossFade = 0.1f)
        {
            if (_transform.position.x - targetPos.x > 0)
                _controller.ChangeAnimation(_transform.GetComponent<Animator>(), state1);
            else
                _controller.ChangeAnimation(_transform.GetComponent<Animator>(), state2);
        }

    }
}
