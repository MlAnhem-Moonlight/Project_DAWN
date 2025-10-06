using BehaviorTree;
using Spear.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SpearBehavior : BhTree
{
    [Header("Settings")]
    public AllyState spearState = AllyState.Neutral;
    public float speed = 10f;
    public float defensiveOffset = 1.5f;
    public float patrolRadius = 5f;
    public float mainBaseRadius = 3f;

    [Header("References")]
    public Transform defensiveTarget;
    public Transform mainBase;
    public Transform camp;
    public Transform waypoints;
    public Transform startPos, endPos;
    public Animator animator;
    public float attackSpeed = 1f;
    public float skillCD = 2.9f;
    public float atkRange = 3f;

    private IMovementStrategy _currentStrategy;
    private AggressiveMovement _aggressiveMovement;
    private DefensiveMovement _defensiveMovement;
    private NeutralMovement _neutralMovement;

    protected override Nodes SetupTree()
    {
        speed = GetComponent<SpearStats>() ? GetComponent<SpearStats>().currentSPD : 10f;

        _aggressiveMovement = new AggressiveMovement(transform, speed);
        _defensiveMovement = new DefensiveMovement(transform, defensiveTarget, defensiveOffset, speed, patrolRadius);
        _neutralMovement = new NeutralMovement(transform, waypoints, speed / 2, startPos, endPos, animator);

        Nodes root = new Selector(new List<Nodes>
            {
                 new Selector(new List<Nodes> // Aggressive behavior branch
                 {
                    new Sequence(new List<Nodes>
                    {
                        new Condition(() => spearState == AllyState.Aggressive),
                        new Action(() => {
                            _currentStrategy = _aggressiveMovement;
                            _currentStrategy.Tick();
                            return NodeState.SUCCESS;
                        })
                    }), //Movement
                    //Find nearest enemy in range ? attack : move to nearest enemy
                 }),
                 new Selector(new List<Nodes> // Defensive behavior branch
                 {

                    _defensiveMovement, //Movement
                    //Check Range from transform to Defensive Target, cant go too far from it
                    new CheckEnemyInRangeAlly(mainBase, mainBaseRadius, LayerMask.GetMask("Demon")), // Indicate that an enemy was found and "attacked".)
                    //&& check if any monster in range ? attack(chase if out of attack range) : patrol around
                    new DefensiveAction(transform, defensiveTarget, patrolRadius, atkRange, speed, animator) // If no enemy in range, patrol around defensive target
                 }),
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
        if (transform == null) return;
        // Màu xanh lam cho phạm vi bảo vệ
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireSphere(defensiveTarget != null ? defensiveTarget.position : transform.position, patrolRadius);
        // Màu đỏ cho tầm tấn công
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, atkRange);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, mainBaseRadius);
        // Màu đỏ cho tầm tấn công
        //Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        //Gizmos.DrawWireSphere(_self.position, _attackRange);
    }
}
