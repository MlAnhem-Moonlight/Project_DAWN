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
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            parent.ClearData("target");
            return state = NodeState.FAILURE;
        }

        Stats stats = target.GetComponent<Stats>();
        if (stats == null || stats.currentHP <= 0)
        {
            parent.ClearData("target");
            return state = NodeState.FAILURE;
        }

        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Direct", dir);

        if (_isAttacking)
            return state = NodeState.RUNNING;

        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            _transform.gameObject.GetComponentInChildren<DealingDmg>()?.SetUsingSkill(2);
            nextSkillTime += _skillCD;
        }

        _animator.SetInteger("State", 1);
        _isAttacking = true;
        return state = NodeState.RUNNING;
    }
}