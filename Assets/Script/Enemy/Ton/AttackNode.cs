using BehaviorTree;
using UnityEngine;

public class TonAttackNode : Nodes
{
    private Transform _transform;
    private Animator _animator;

    // Thời gian hồi chiêu của skill (giây)
    private float skillCooldown;
    // Thời điểm tiếp theo có thể dùng skill
    private float nextSkillTime = 0f;

    public TonAttackNode(Transform transform, Animator animator, float skillCd = 5f)
    {
        _transform = transform;
        _animator = animator;
        skillCooldown = skillCd;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        if (target == null)
        {
            state = NodeState.FAILURE;
            return state;
        }
        // Tính hướng để xoay/flip
        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Attack", dir);
        //Debug.Log($"Attacking {Time.time >= nextSkillTime}  {target.gameObject.layer == LayerMask.NameToLayer("Human")}");
        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            // === Skill attack ===
            _animator.SetInteger("State", 2); // 2: skill animation (tùy animator)
            // Thực hiện logic skill: gây sát thương, hiệu ứng…
            // Skill xong thì đặt thời gian cooldown
            //Debug.Log("Skill Attack");
            nextSkillTime = Time.time + skillCooldown;
        }
        else
        {
            // === Normal attack ===
            _animator.SetInteger("State", 1); // 1: normal attack animation
            // Thực hiện logic đánh thường
        }

        state = NodeState.RUNNING;
        return state;
    }
}

