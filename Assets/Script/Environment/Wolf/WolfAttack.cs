using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WolfAttack : Nodes
{
    public WolfAttack()
    {
    }

    public override NodeState Evaluate()
    {
        Debug.Log("Wolf is attacking!");
        state = NodeState.FAILURE;
        return state;
    }
}
