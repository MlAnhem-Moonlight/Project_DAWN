using BehaviorTree;
using UnityEngine;

public class CastSpellNode : Nodes
{
    private TheMageMovement _theMageMovement;
    private float _cooldown;
    private float _lastCastTime;
    private float _castDuration = 1.5f;
    private float _castStartTime;
    private bool _isCasting;
    private Animator _animator;
    private string animationClipName = "CastingSpell";

    public CastSpellNode(TheMageMovement theMageMovement, float cooldown, Animator animator)
    {
        _theMageMovement = theMageMovement;
        _cooldown = cooldown;
        _lastCastTime = -cooldown;
        _isCasting = false;
        _animator = animator;
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        AnimationClip specificClip = null;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationClipName) // Tìm đúng Animation Clip theo tên
            {
                _castDuration = clip.length; // Lấy thời gian của Animation Clip
            }
        }
    }

    public override NodeState Evaluate()
    {
        if (Time.time - _lastCastTime >= _cooldown)
        {
            if (!_isCasting)
            {
                Debug.Log("Casting Spell");
                _castStartTime = Time.time;
                _isCasting = true;
                _animator.SetBool("IsCastingSpell", true);
                state = NodeState.RUNNING;
            }

            if (Time.time - _castStartTime >= _castDuration)
            {
                Debug.Log("Spell Casted");
                _lastCastTime = Time.time;
                _isCasting = false;
                _animator.SetBool("IsCastingSpell", false);
                state = NodeState.SUCCESS;
            }
        }
        else
        {
            state = NodeState.FAILURE;
        }

        return state;
    }
}
