using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using Spear.Movement;

[UnityEngine.RequireComponent(typeof(UnityEngine.Transform))]
public class SpearBehavior : BhTree
{
    [UnityEngine.Header("Settings")]
    public AllyState spearState = AllyState.Neutral;
    public float speed = 10f;
    public float defensiveOffset = 1.5f;
    public float patrolRadius = 5f;
    public float mainBaseRadius = 3f;

    [UnityEngine.Header("References")]
    public UnityEngine.Transform defensiveTarget;
    public UnityEngine.Transform mainBase;  
    public UnityEngine.Transform camp;
    public UnityEngine.Transform waypoints;
    public UnityEngine.Transform startPos, endPos;
    public UnityEngine.Animator animator;
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
        _defensiveMovement = new DefensiveMovement(transform, defensiveTarget, defensiveOffset, speed);
        _neutralMovement = new NeutralMovement(transform, waypoints, speed/2, startPos, endPos, animator);

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
                    new Sequence(new List<Nodes>
                    {
                        new Condition(() => spearState == AllyState.Defensive),
                        new Action(() => {
                            _currentStrategy = _defensiveMovement;
                            _currentStrategy.Tick();
                            return NodeState.SUCCESS;
                        })
                    }), //Movement
                    //Check Range from transform to Defensive Target, cant go too far from it
                    new CheckEnemyInRangeAlly(mainBase, mainBaseRadius, UnityEngine.LayerMask.NameToLayer("Demon")), // Indicate that an enemy was found and "attacked".)
                    //&& check if any monster in range ? attack(chase if out of attack range) : patrol around
                    new DefensiveAction(transform, defensiveTarget, patrolRadius, atkRange, speed, animator) // If no enemy in range, patrol around defensive target
                 }),
                 new Selector(new List<Nodes> // Neutral behavior branch (fallback)
                 {  
                    
                    new Sequence(new List<Nodes>
                    {
                        new Action(() => {
                            _currentStrategy = _neutralMovement;
                            _currentStrategy.Tick();
                            return NodeState.SUCCESS;
                        })
                    }) //Movement
                 }),

            });

        return root;
    }

    public void SetState(AllyState newState)
    {
        spearState = newState;
    }

}
