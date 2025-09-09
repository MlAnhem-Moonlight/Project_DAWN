using BehaviorTree;
using System.Collections.Generic;
using System;

public class TheMageBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public float spellRange = 10f;
    public float spellCooldown = 5f;
    public UnityEngine.Transform defaultTarget;
    public UnityEngine.Animator animator;
    private TheMageMovement _theMageMovement;

    protected override Nodes SetupTree()
    {
        defaultTarget = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        _theMageMovement = new TheMageMovement(transform, speed, attackRange, animator, defaultTarget);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, spellRange, defaultTarget, "Human","Human",animator),
                new CastSpellNode(_theMageMovement, spellCooldown,animator),
            }),
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, attackRange, defaultTarget, "Human","Construction",animator),
                new MageSetTargetNode(_theMageMovement),
                new MageAttackNode(_theMageMovement,animator),
            }),
            _theMageMovement,
        });
        return root;
    }
}
