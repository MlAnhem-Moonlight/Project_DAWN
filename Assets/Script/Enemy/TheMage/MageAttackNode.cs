using BehaviorTree;
using UnityEngine;

public class MageAttackNode : Nodes
{
    private Animator _animator;

    private float _attackDuration = 1f;
    private float _attackStartTime;
    private bool _isAttacking;
    private Transform _transform;

    private string attackClipName = "Attack";

    public MageAttackNode(Transform transform, Animator animator)
    {
        _transform = transform;
        _animator = animator;

        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == attackClipName)
                {
                    _attackDuration = clip.length;
                    break;
                }
            }
        }

        _isAttacking = false;
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)parent.GetData("target");

        // Kiểm tra target còn hợp lệ hay không
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            state = NodeState.FAILURE;
            _isAttacking = false;
            return state;
        }

        Stats stats = target.GetComponent<Stats>();
        if (stats == null || stats.currentHP <= 0)
        {
            state = NodeState.FAILURE;
            _isAttacking = false;
            return state;
        }

        if (_isAttacking)
        {
            // Nếu đang trong quá trình attack
            if (Time.time - _attackStartTime >= _attackDuration)
            {
                _isAttacking = false;
                state = NodeState.SUCCESS;
            }
            else
            {
                state = NodeState.RUNNING;
            }
        }
        else
        {
            // Bắt đầu attack
            _animator.SetInteger("Anim", -1);
            _animator.SetFloat("Attack", _transform.position.x - target.position.x > 0 ? -1f : 1f);

            _attackStartTime = Time.time;
            _isAttacking = true;
            state = NodeState.RUNNING;
        }

        return state;
    }
}
