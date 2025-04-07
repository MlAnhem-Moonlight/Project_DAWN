using BehaviorTree;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System;

public class TheHandBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public UnityEngine.Animator animator;
    //hành vi : di chuyển và tấn công tất cả các vật thể sống trên đường đi, chỉ target 1 mục tiêu cho tới khi mục tiêu đó bị diệt

    protected override Nodes SetupTree()
    {


        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {

            }),
            new Sequence(new List<Nodes>
            {

            }),

        });
        return root;
    }
}