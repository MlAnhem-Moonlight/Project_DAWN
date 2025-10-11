using BehaviorTree;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TheHandBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 2f;
    public float scanRange = 100f;
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
        target = GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        theHandMovement = new TheHandMovement(transform, speed, attackRange, animator, target, GameObject.FindGameObjectWithTag("DefaultTarget").transform);
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

    /// <summary>
    /// Vẽ Gizmo hiển thị phạm vi bảo vệ và tầm tấn công.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (transform == null) return;

        // Màu vàng cho phạm vi quét kẻ địch
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, scanRange);

    }
}