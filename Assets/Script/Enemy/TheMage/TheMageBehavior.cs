using BehaviorTree;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System;

public class TheMageBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public float spellRange = 10f;
    public float spellCooldown = 5f;
    public UnityEngine.Transform defaultTarget;
    private TheMageMovement _theMageMovement;

    protected override Nodes SetupTree()
    {
        _theMageMovement = new TheMageMovement(transform, speed, attackRange, defaultTarget);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, spellRange, defaultTarget,"Human"),
                new CastSpellNode(_theMageMovement, spellCooldown),
            }),
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, attackRange, defaultTarget,"Human"),
                new MageSetTargetNode(_theMageMovement),
                new MageAttackNode(_theMageMovement),
            }),
            _theMageMovement,
        });
        return root;
    }
}
