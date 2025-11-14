using BehaviorTree;
using UnityEngine;

public class TonAttackNode : Nodes
{
    private Transform _transform;
    private Animator _animator;
    private float _skillCD;
    private bool _isAttacking;

    private float nextSkillTime = 0f;

    public TonAttackNode(Transform transform, Animator animator, bool isAttacking, float skillCD)
    {
        _transform = transform;
        _animator = animator;
        _isAttacking = isAttacking;
        _skillCD = skillCD;
        nextSkillTime += _skillCD;

    }

    public void resetAttacking()
    {
        _isAttacking = false;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");

        // Kiểm tra target còn hợp lệ hay không
        if (target == null || !target.gameObject.activeInHierarchy)
            return state = NodeState.FAILURE;

        Stats stats = target.GetComponent<Stats>();
        if (stats == null || stats.currentHP <= 0)
            return state = NodeState.FAILURE;

        // Xác định hướng để flip/rotate
        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Attack", dir);

        if (_isAttacking) // nếu đang đánh thì không làm gì
        {
            return state = NodeState.RUNNING;
        }

        // === Quyết định dùng skill hay đánh thường ===
        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            // Phát skill
            _animator.SetInteger("State", 2);
            nextSkillTime += _skillCD;
        }
        else
        {
            // Phát normal attack
            _animator.SetInteger("State", 1);
        }
        _isAttacking = true;
        return state = NodeState.RUNNING;
    }
}
