using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

public class ArcherBehavior : BhTree
{


    protected override Nodes SetupTree()
    {
        Nodes root = new Selector(new List<Nodes>
        {
            //* Arg: tấn công enemy xa nhất trong tầm bắn
            new ArcherArg(),
            //*Def: tấn công enemy gần nhất trong tầm bắn
            new ArcherDef(),
            //*Neu: di chuyển xung quanh checkpoint
            new ArcherNeu()
        });
        return root;
    }
}
