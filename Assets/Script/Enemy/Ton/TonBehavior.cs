using BehaviorTree;
using System.Collections.Generic;
using System;

public class TonBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 2.9f;
    public float skillCD = 5f;
    public UnityEngine.Transform defaultTarget;
    public UnityEngine.Animator animator;
    public bool isAttacking = false;

    private TonMovement tonMovement;
    private TonAttackNode tonAttackNode;

    public void resetAttacking()
    {
        tonAttackNode.resetAttacking();
    }

    private void SetupAnimator()
    {

    }

    protected override Nodes SetupTree()
    {
        speed = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSPD : 10f;
        skillCD = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSkillCD : 5f;

        defaultTarget = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        tonMovement = new TonMovement(transform, speed, attackRange, animator, defaultTarget);
        tonAttackNode = new TonAttackNode(transform, animator, isAttacking, skillCD);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new CheckEnemyInRange(transform, attackRange,"Human", "Construction"),
                new TonSetTargetNode(tonMovement),
                tonAttackNode,
            }),
            tonMovement,
        });
        return root;
    }
}
