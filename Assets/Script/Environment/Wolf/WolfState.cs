using UnityEngine;
using BehaviorTree;

public class WolfState : Nodes
{
    private Transform _trasform;

    public WolfState(Transform transform)
    {
        _trasform = transform;
    }

    public override NodeState Evaluate()
    {


        return state;
    }

}
