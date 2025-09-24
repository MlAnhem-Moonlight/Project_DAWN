using BehaviorTree;
using UnityEngine;

public class TheHandAttack : Nodes
{
    private Transform _transform;
    private Animator _animator;
    private float _skillCD;
    private bool _isAttacking;

    private float nextSkillTime = 0f;

    public TheHandAttack(Transform transform, Animator animator, bool isAttacking, float skillCD)
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
        if (target == null)
            return state = NodeState.FAILURE;

        // Xác định hướng để flip/rotate
        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Direct", dir);
        //Debug.Log($"Attacking:  {_isAttacking}");
        if (_isAttacking) // nếu đang đánh thì không làm gì
        {
            //Debug.Log("In state attacking");
            return state = NodeState.RUNNING;
        }

        //Debug.Log($"CD : {Time.time} : {nextSkillTime}");
        //Debug.Log($"Target layer: {LayerMask.LayerToName(target.gameObject.layer)}");
        // === Quyết định dùng skill hay đánh thường ===
        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            // Phát skill
            _transform.gameObject.GetComponentInChildren<DealingDmg>()?.SetUsingSkill(2); // dùng skill Rage
            nextSkillTime += _skillCD;
            Debug.Log("Using skill");
        }
        _animator.SetInteger("State", 1);
        _isAttacking = true;
        return state = NodeState.RUNNING;
    }
}
