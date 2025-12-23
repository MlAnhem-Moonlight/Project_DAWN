using BehaviorTree;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TheHandBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 2f;
    public float detectRange = 2f;
    public float skillCD = 5f;
    public float attackSpeed = 1f;
    public UnityEngine.Animator animator;
    public UnityEngine.Transform target;
    public bool isAttacking = false;

    private TheHandMovement theHandMovement;
    private TheHandAttack theHandAttack;

    public void resetAttacking()
    {
        if (theHandAttack != null) theHandAttack.resetAttacking();
    }

    protected override Nodes SetupTree()
    {
        speed = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentSPD : 10f;
        skillCD = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentSkillCD : 5f;
        attackSpeed = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentAtkSpd : 1f;
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        }
        SetupAttackSpeed(animator, attackSpeed);

        theHandMovement = new TheHandMovement(transform, speed, attackRange, animator, target, GameObject.FindGameObjectWithTag("DefaultTarget").transform);
        theHandAttack = new TheHandAttack(transform, animator, isAttacking, skillCD);

        Nodes root = new Sequence(new List<Nodes>
        {
            new TheHandCheckTarget(transform, detectRange + 2f, target, "Human", "Construction", animator),
            
            new Selector(new List<Nodes>
            {
                new Sequence(new List<Nodes>
                {
                    new TheHandSetTarget(theHandMovement, transform, attackRange),
                    theHandAttack,
                }),
                
                theHandMovement,
            }),
        });
        return root;
    }

    private void OnDrawGizmos()
    {
        if (transform == null) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}