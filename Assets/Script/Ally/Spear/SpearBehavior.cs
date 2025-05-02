using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using Spear.Movement;

[UnityEngine.RequireComponent(typeof(UnityEngine.Transform))]
public class SpearBehavior : Tree
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
    public UnityEngine.Transform[] waypoints;

    private IMovementStrategy _currentStrategy;
    private AggressiveMovement _aggressiveMovement;
    private DefensiveMovement _defensiveMovement;
    private NeutralMovement _neutralMovement;

    protected override Nodes SetupTree()
    {
        _aggressiveMovement = new AggressiveMovement(transform, speed);
        _defensiveMovement = new DefensiveMovement(transform, defensiveTarget, defensiveOffset, speed);

        _neutralMovement = new NeutralMovement(transform, waypoints, speed);


        Nodes root = new Selector(new List<Nodes>
            {
                // Aggressive behavior branch
                new Sequence(new List<Nodes>
                {
                    new Condition(() => spearState == AllyState.Aggressive),
                    new Action(() => {
                        _currentStrategy = _aggressiveMovement;
                        _currentStrategy.Tick();
                        return NodeState.SUCCESS;
                    })
                }),
                // Defensive behavior branch
                new Sequence(new List<Nodes>
                {
                    new Condition(() => spearState == AllyState.Defensive),
                    new Action(() => {
                        _currentStrategy = _defensiveMovement;
                        _currentStrategy.Tick();
                        return NodeState.SUCCESS;
                    })
                }),
                // Neutral behavior branch (fallback)
                new Sequence(new List<Nodes>
                {
                    new Action(() => {
                        _currentStrategy = _neutralMovement;
                        _currentStrategy.Tick();
                        return NodeState.SUCCESS;
                    })
                })

            });

        return root;
    }

    public void SetState(AllyState newState)
    {
        spearState = newState;
    }

}
