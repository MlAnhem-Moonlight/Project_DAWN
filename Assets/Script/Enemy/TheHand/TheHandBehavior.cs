using BehaviorTree;
using System.Collections.Generic;
using System;

public class TheHandBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 2f;
    public float skillCD = 5f;
    public float attackSpeed = 1f; // đòn/giây  
    public UnityEngine.Animator animator;
    public UnityEngine.Transform target;
    public bool isAttacking = false;

    private TheHandMovement theHandMovement;
    private TheHandAttack theHandAttack; // tái sử dụng node tấn công của Ton vì hành vi giống nhau
    //hành vi : di chuyển và tấn công tất cả các vật thể sống trên đường đi, chỉ target 1 mục tiêu cho tới khi mục tiêu đó bị diệt

    public void resetAttacking()
    {
        theHandAttack.resetAttacking();
    }

    protected override Nodes SetupTree()
    {
        speed = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentSPD : 10f;
        skillCD = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentSkillCD : 5f;
        attackSpeed = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentAtkSpd : 1f;

        SetupAttackSpeed(animator, attackSpeed);
        target = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        theHandMovement = new TheHandMovement(transform, speed, attackRange, animator, target);
        theHandAttack = new TheHandAttack(transform, animator, isAttacking, skillCD);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new TheHandCheckTarget(transform, attackRange, target, "Human","Construction",animator),
                new TheHandSetTarget(theHandMovement),
                theHandAttack,
            }),

            theHandMovement,
        });
        return root;
    }
}