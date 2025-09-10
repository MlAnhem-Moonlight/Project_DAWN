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

    public CastSpellNode(TheMageMovement theMageMovement,Transform transform, float cooldown, Animator animator)
    {
        _theMageMovement = theMageMovement;
        _transform = transform;
        _cooldown = cooldown;
        _lastCastTime = -cooldown; // cho cast ngay từ đầu
        _isCasting = false;
        _animator = animator;

        // Lấy duration từ clip
        _castDuration = 1.5f;
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationClipName)
            {
                _castDuration = clip.length;
                break;
            }
        }
    }

    public override NodeState Evaluate()
    {
        // Nếu đang casting -> check thời gian
        if (_isCasting)
        {
            if (Time.time - _castStartTime >= _castDuration)
            {
                // Kết thúc cast
                Debug.Log("Spell Casted!");
                _lastCastTime = Time.time; // bắt đầu tính cooldown
                _isCasting = false;
                _theMageMovement.isAttack = false;
                state = NodeState.SUCCESS;
            }
            else
            {
                // Đang cast
                state = NodeState.RUNNING;
            }
        }
        // Nếu không casting và cooldown đã xong -> bắt đầu cast mới
        else if (Time.time - _lastCastTime >= _cooldown)
        {
            Debug.Log("Start Casting Spell");
            _castStartTime = Time.time;
            _theMageMovement.isAttack = true;   // block di chuyển + attack
            _isCasting = true;
            _animator.SetInteger("Anim", 1);

            Transform target = (Transform)parent.GetData("target");
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
