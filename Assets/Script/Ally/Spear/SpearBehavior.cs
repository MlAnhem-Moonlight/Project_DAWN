using BehaviorTree;
using Spear.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimatorState
{
    Idle,
    Walk,
    Attack,
    Die,
    Running,
    UsingSkill
}

[RequireComponent(typeof(Transform))]
public class SpearBehavior : BhTree
{
    [Header("Settings")]
    public AllyState spearState = AllyState.Neutral;
    public AnimatorState currentState = AnimatorState.Idle;
    public float defensiveOffset = 1.5f;
    public float patrolRadius = 5f;
    public float mainBaseRadius = 3f;

    [Header("References")]
    public bool isSpear;
    public Transform defensiveTarget;
    public Transform waypoints;
    public Transform startPos, endPos;
    public Animator animator;
    public float attackSpeed = 1f;
    public float skillCD = 2.9f;
    public float atkRange = 3f;
    public float speed = 10f;

    private AggressiveMovement _aggressiveMovement;
    private DefensiveMovement _defensiveMovement;
    private NeutralMovement _neutralMovement;
    private DefensiveAction _defensiveAction;

    public void ChangeState(AnimatorState state)
    {
        currentState = state;
    }

    public void ChangeDefensiveTarget(string name)
    {
        defensiveTarget = GameObject.Find(name).transform;
    }    

    protected override Nodes SetupTree()
    {
        defensiveTarget = GameObject.Find("Player").transform;
        startPos = GameObject.Find("PatrolStartPos").transform;
        endPos = GameObject.Find("PatrolEndPos").transform;
        waypoints = GameObject.Find("Waypoint").transform;

        speed = GetComponent<Stats>() ? GetComponent<Stats>().currentSPD : 10f;
        skillCD = GetComponent<Stats>() ? GetComponent<Stats>().currentSkillCD : 5f;
        attackSpeed = GetComponent<Stats>() ? GetComponent<Stats>().currentAtkSpd : 1f;

        animator = GetComponent<Animator>();
        //Only work for Spear and Knight
        if (isSpear)
        {
            speed = GetComponent<SpearStats>() ? GetComponent<SpearStats>().currentSPD : 10f;
            skillCD = GetComponent<SpearStats>() ? GetComponent<SpearStats>().currentSkillCD : 2.9f;
            attackSpeed = GetComponent<SpearStats>() ? GetComponent<SpearStats>().currentAtkSpd : 1f;
        }
        else
        {
            speed = GetComponent<KnightStats>() ? GetComponent<KnightStats>().currentSPD : 10f;
            skillCD = GetComponent<KnightStats>() ? GetComponent<KnightStats>().currentSkillCD : 2.9f;
            attackSpeed = GetComponent<KnightStats>() ? GetComponent<KnightStats>().currentAtkSpd : 1f;
        }

        SetupAnimatorSpeedDirect(animator, attackSpeed,"Attack 0", "AttackSpd");
        _aggressiveMovement = new AggressiveMovement(transform, speed, atkRange, skillCD);
        _defensiveMovement = new DefensiveMovement(transform, 
                                                    defensiveTarget, 
                                                    defensiveOffset, 
                                                    speed, 
                                                    patrolRadius, 
                                                    atkRange);
        _neutralMovement = new NeutralMovement(transform, waypoints, speed / 2, startPos, endPos, animator);
        _defensiveAction =  new DefensiveAction(transform,
                                                defensiveTarget,
                                                patrolRadius,
                                                atkRange,
                                                mainBaseRadius,
                                                speed,
                                                skillCD,
                                                animator,
                                                defensiveOffset);
        Nodes root = new Selector(new List<Nodes>
            {
                 new Sequence(new List<Nodes> 
                 {
                    _aggressiveMovement, //Movement
                 }),// Aggressive behavior branch
                 new Selector(new List<Nodes> 
                 {

                    _defensiveMovement, //Movement
                    
                    new Sequence(new List<Nodes>
                    {
                        //Check Range from transform to Defensive Target, cant go too far from it
                        new CheckEnemyInRangeAlly(transform, mainBaseRadius, LayerMask.GetMask("Demon")), // Indicate that an enemy was found and "attacked".)
                        //&& check if any monster in range ? attack(chase if out of attack range) : patrol around
                        _defensiveAction// If no enemy in range, patrol around defensive target
                    }),
                 }),// Defensive behavior branch
                 _neutralMovement //Movement
            });

        return root;
    }

    public void SetState(AllyState newState)
    {
        spearState = newState;
    }

    /// <summary>
    /// Vẽ Gizmo hiển thị phạm vi bảo vệ và tầm tấn công.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (transform == null || defensiveTarget == null) return;
        Vector3 targetDef = defensiveTarget.position + Vector3.left * defensiveOffset;
        // Màu xanh lam cho phạm vi bảo vệ
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(defensiveTarget != null ? targetDef : transform.position, patrolRadius);
        // Màu đỏ cho tầm tấn công
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, atkRange);
        // Màu vàng cho phạm vi quét kẻ địch
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, mainBaseRadius);

    }
}
