using BehaviorTree;
using System.Collections.Generic;
using System;

public class TheHandBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 2f;
    public UnityEngine.Animator animator;
    public UnityEngine.Transform target;
    private TheHandMovement theHandMovement;
    //hành vi : di chuyển và tấn công tất cả các vật thể sống trên đường đi, chỉ target 1 mục tiêu cho tới khi mục tiêu đó bị diệt

    protected override Nodes SetupTree()
    {
        //skill tăng x3 tốc độ tấn công trong x giây tăng dần theo thời gian
        speed = GetComponent<MeleeDPSStats>() ? GetComponent<MeleeDPSStats>().currentSPD : 10f;
        target = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        theHandMovement = new TheHandMovement(transform, speed, attackRange, animator, target);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new TheHandCheckTarget(transform, attackRange, target, "Human","Construction",animator),
                new TheHandSetTarget(theHandMovement),
                new TheHandAttack(theHandMovement,animator),
            }),

            theHandMovement,
        });
        return root;
    }
}