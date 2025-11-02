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
        if (target == null)
            return state = NodeState.FAILURE;

        // Xác định hướng để flip/rotate
        float dir = _transform.position.x - target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Attack", dir);
        //Debug.Log($"Attacking:  {_isAttacking}");
        if (_isAttacking) // nếu đang đánh thì không làm gì
        {
            //Debug.Log("In state attacking");
            return state = NodeState.RUNNING;
        }

        //Debug.Log($"CD : {Time.time} : {nextSkillTime}");

        // === Quyết định dùng skill hay đánh thường ===
        if (Time.time >= nextSkillTime && target.gameObject.layer == LayerMask.NameToLayer("Human"))
        {
            // Phát skill
            _animator.SetInteger("State", 2); // đảm bảo state này chuyển sang các clip có Tag = "Skill"
            nextSkillTime += _skillCD;
            //Debug.Log("Using skill");
        }
        else
        {
            // Phát normal attack
            _animator.SetInteger("State", 1); // đảm bảo state này chuyển sang các clip có Tag = "Attack"
            //Debug.Log("Normal attack");
        }
        _isAttacking = true;
        return state = NodeState.RUNNING;
    }
}
