using BehaviorTree;
using UnityEngine;

public class TheHandAttack : Nodes
{
    private Transform _transform;
    private Animator _animator;
    private float _skillCD;
    private bool _isAttacking;

    private float nextSkillTime = 0f;
    private float attackDuration = 0f;  // ✅ Theo dõi thời gian attack

    public TheHandAttack(Transform transform, Animator animator, bool isAttacking, float skillCD)
    {
        _transform = transform;
        _animator = animator;
        _isAttacking = isAttacking;
        _skillCD = skillCD;
        nextSkillTime = Time.time + _skillCD;
    }

    public void resetAttacking()
    {
        _isAttacking = false;
        attackDuration = 0f;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)GetData("target");
        Debug.Log($"[TheHandAttack] Target: {(target != null ? target.name : "null")}, IsAttacking: {_isAttacking}, AttackDuration: {attackDuration:F2}s");
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            parent.ClearData("target");
            resetAttacking();
            return state = NodeState.FAILURE;
        }

        Stats stats = target.GetComponent<Stats>();
        if (stats == null || stats.currentHP <= 0)
        {
            parent.ClearData("target");
            resetAttacking();
            return state = NodeState.FAILURE;
        }

        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Direct", dir);

        // ✅ Nếu đang attack, kiểm tra animation kết thúc hay chưa
        if (_isAttacking)
        {
            attackDuration += Time.deltaTime;
            
            // ✅ Giả sử animation kéo dài ~0.5s (tùy animation của bạn)
            // Điều chỉnh thời gian này dựa trên animation thực tế
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            
            // ✅ Nếu animation không còn playing → xong attack
            if (stateInfo.IsName("Attack") && stateInfo.normalizedTime >= 1f)
            {
                resetAttacking();
            }
            
            return state = NodeState.RUNNING;
        }

        // ✅ Khởi động attack mới
        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            _transform.gameObject.GetComponentInChildren<DealingDmg>()?.SetUsingSkill(2);
            nextSkillTime = Time.time + _skillCD;
        }

        _animator.SetInteger("State", 1);
        _isAttacking = true;
        attackDuration = 0f;
        return state = NodeState.RUNNING;
    }
}