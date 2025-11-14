using BehaviorTree;
using UnityEngine;

public class CastSpellNode : Nodes
{
    private TheMageMovement _theMageMovement;
    private float _cooldown;
    private float _lastCastTime;
    private float _castDuration;
    private float _castStartTime;
    private bool _isCasting;
    private Transform _transform;
    private Animator _animator;
    private string animationClipName = "CastingSpell";

    public CastSpellNode(TheMageMovement theMageMovement, Transform transform, float cooldown, Animator animator, float spellRange)
    {
        _theMageMovement = theMageMovement;
        _transform = transform;
        _cooldown = cooldown;
        _lastCastTime = -cooldown;
        _isCasting = false;
        _animator = animator;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationClipName)
            {
                _castDuration = clip.length / _animator.GetFloat("CastingSpellSpd");
                break;
            }
        }
    }

    public override NodeState Evaluate()
    {
        Transform target = (Transform)parent.GetData("target");

        // Kiểm tra target còn hợp lệ hay không
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            _isCasting = false;
            _theMageMovement.isAttack = false;
            state = NodeState.FAILURE;
            return state;
        }

        Stats stats = target.GetComponent<Stats>();
        if (stats == null || stats.currentHP <= 0)
        {
            _isCasting = false;
            _theMageMovement.isAttack = false;
            state = NodeState.FAILURE;
            return state;
        }

        // Nếu đang casting -> check thời gian
        if (_isCasting)
        {
            if (Time.time - _castStartTime >= _castDuration)
            {
                _lastCastTime = Time.time;
                _isCasting = false;
                _theMageMovement.isAttack = false;
                state = NodeState.SUCCESS;
            }
            else
            {
                state = NodeState.RUNNING;
            }
        }
        // Nếu không casting và cooldown đã xong -> bắt đầu cast mới
        else if (Time.time - _lastCastTime >= _cooldown)
        {
            _castStartTime = Time.time;
            _theMageMovement.isAttack = true;
            _isCasting = true;
            _animator.SetInteger("Anim", 1);

            _animator.SetFloat("Spell", _transform.position.x - target.position.x > 0 ? -1f : 1f);

            state = NodeState.RUNNING;
        }
        else
        {
            // Cooldown chưa xong
            state = NodeState.FAILURE;
        }

        return state;
    }
}
