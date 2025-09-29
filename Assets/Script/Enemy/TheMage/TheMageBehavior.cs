using BehaviorTree;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TheMageBehavior : BhTree
{
    public float speed = 10f;
    public float attackRange = 5f;
    public float attackSpeed = 1f; // đòn/giây
    public float spellRange = 10f;
    public float spellCooldown = 5f;

    public UnityEngine.Transform defaultTarget;
    public UnityEngine.Animator animator;

    private TheMageMovement _theMageMovement;

    protected override Nodes SetupTree()
    {
        speed = GetComponent<MageStats>() ? GetComponent<MageStats>().currentSPD : 10f;
        attackSpeed = GetComponent<MageStats>() ? GetComponent<MageStats>().currentAtkSpd : 1f;
        SetupAttackSpeed(animator, attackSpeed);


        defaultTarget = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        _theMageMovement = new TheMageMovement(transform, speed, attackRange, animator, defaultTarget);

        Nodes root = new Selector(new List<Nodes>
        {
            // ƯU TIÊN 1: Cast Spell
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, spellRange, defaultTarget, "Human","Human",animator),
                new CastSpellNode(_theMageMovement,transform, spellCooldown, animator, spellRange),
            }),
            
            // ƯU TIÊN 2: Attack nếu trong range
            new Sequence(new List<Nodes>
            {
                new MageCheckEnemyInRange(transform, attackRange, defaultTarget, "Human","Construction",animator),
                new MageSetTargetNode(_theMageMovement),
                new MageAttackNode(transform, animator),
            }),

            // ƯU TIÊN 3: Di chuyển nếu không làm gì khác
            _theMageMovement,
        });

        return root;
    }
//#if UNITY_EDITOR
//    // Vẽ vùng tròn quét trong Scene view để dễ chỉnh sửa
//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, spellRange);
//    }
//#endif
}
