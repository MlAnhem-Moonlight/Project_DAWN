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

    public CastSpellNode(TheMageMovement theMageMovement, float cooldown)
    {
        _theMageMovement = theMageMovement;
        _cooldown = cooldown;
        _lastCastTime = -cooldown;
        _isCasting = false;
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
                state = NodeState.RUNNING;
            }

            if (Time.time - _castStartTime >= _castDuration)
            {
                Debug.Log("Spell Casted");
                _lastCastTime = Time.time;
                _isCasting = false;
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
