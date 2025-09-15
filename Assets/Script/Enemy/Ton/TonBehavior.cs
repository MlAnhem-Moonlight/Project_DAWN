using BehaviorTree;
using System.Collections.Generic;
using System;

public class TonBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public float skillCD = 5f;
    public UnityEngine.Transform defaultTarget;
    public UnityEngine.Animator animator;
    private TonMovement _tonMovement;

    protected override Nodes SetupTree()
    {
        speed = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSPD : 10f;
        skillCD = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSkillCD : 5f;
        defaultTarget = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        _tonMovement = new TonMovement(transform, speed, attackRange, animator, defaultTarget);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new CheckEnemyInRange(transform, attackRange,"Human", "Construction"),
                new TonSetTargetNode(_tonMovement),
                new TonAttackNode(transform,animator,skillCD),
            }),
            _tonMovement,
        });
        return root;
    }
}
