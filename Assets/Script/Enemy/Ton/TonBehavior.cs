using BehaviorTree;
using System;
using System.Collections.Generic;
using UnityEngine;


public class TonBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 2.9f;
    public float skillCD = 5f;
    public float attackSpeed = 1f; // đòn/giây  
    public UnityEngine.Transform defaultTarget;
    public UnityEngine.Animator animator;
    public bool isAttacking = false;


    private TonMovement tonMovement;
    private TonAttackNode tonAttackNode;

    public void resetAttacking()
    {
        tonAttackNode.resetAttacking();
    }

    protected override Nodes SetupTree()
    {
        speed = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSPD : 10f;
        skillCD = GetComponent<TankStats>() ? GetComponent<TankStats>().currentSkillCD : 5f;
        attackSpeed = GetComponent<TankStats>() ? GetComponent<TankStats>().currentAtkSpd : 1f;
        //animator.SetFloat("AttackSpd", attackSpeed);
        SetupAttackSpeed(animator, attackSpeed);

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

    void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn kiểm tra trong Scene View (sử dụng bán kính hiện tại nếu đang chạy)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
