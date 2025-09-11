using BehaviorTree;
using UnityEngine;

public class MageAttackNode : Nodes
{
    private Animator _animator;

    private float _attackDuration = 1f; // mặc định, sẽ lấy từ clip
    private float _attackStartTime;
    private bool _isAttacking;
    private Transform _transform;


    private string attackClipName = "Attack"; // tên clip animation attack

    public MageAttackNode(Transform transform, Animator animator)
    {
        _transform = transform;
        _animator = animator;

        // Tìm clip Attack để lấy độ dài
        if (_animator != null && _animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == attackClipName)
                {
                    _attackDuration = clip.length;
                    Debug.Log($"Attack animation duration set to {_attackDuration} seconds.");
                    break;
                }
            }
        }

        _isAttacking = false;
    }

    public override NodeState Evaluate()
    {
        if (_isAttacking)
        {
            // Nếu đang trong quá trình attack
            if (Time.time - _attackStartTime >= _attackDuration)
            {
                _isAttacking = false;
                state = NodeState.SUCCESS; // kết thúc attack
            }
            else
            {
                state = NodeState.RUNNING; // anim chưa xong
            }
        }
        else
        {
            // Bắt đầu attack
            //Debug.Log("Start Attack");
            _animator.SetInteger("Anim", -1); // trigger attack anim
            Transform target = (Transform)parent.GetData("target");
            _animator.SetFloat("Attack", _transform.position.x - target.position.x > 0 ? -1f : 1f);

            _attackStartTime = Time.time;
            _isAttacking = true;
            state = NodeState.RUNNING;
        }

        return state;
    }
}
