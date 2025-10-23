using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;
using Spear.Movement;

public class ArcherBehavior : BhTree
{
    [Header("Settings")]
    public AllyState spearState = AllyState.Neutral;
    public AnimatorState currentState = AnimatorState.Idle;
    public Transform checkpoint,waypoints;
    public Transform startPos, endPos;

    [Header("References")]
    public float speed;
    public float stopDistance;
    //public float atkRangeMax,atkRangeMin;
    public float attackSpeed = 1f;
    public float skillCD = 2.9f;
    public float atkRange = 3f;
    public Animator animator;
    public GameObject target;

    protected override Nodes SetupTree()
    {
        startPos = GameObject.Find("PatrolStartPos").transform;
        endPos = GameObject.Find("PatrolEndPos").transform;
        waypoints = GameObject.Find("Waypoint").transform;
        checkpoint = GameObject.Find("ArcherCheckpoint").transform;

        speed = GetComponent<Stats>() ? GetComponent<Stats>().currentSPD : 10f;
        skillCD = GetComponent<Stats>() ? GetComponent<Stats>().currentSkillCD : 5f;
        attackSpeed = GetComponent<Stats>() ? GetComponent<Stats>().currentAtkSpd : 1f;

        animator = GetComponent<Animator>();
        Nodes root = new Selector(new List<Nodes>
        {
            //Khối di chuyển trong combat
            new Sequence(new List<Nodes>
            {
                //Di chuyển bằng checkpoint
                new ArcherMovement(transform,checkpoint,speed,stopDistance,atkRange),

                //* Arg: tấn công enemy xa nhất trong tầm bắn || Def : tấn công enemy gần nhất trong tầm bắn
                new ArcherArg(transform,atkRange,skillCD),

            }),
            //*Neu: di chuyển xung quanh checkpoint
            new ArcherNeu(transform, waypoints, speed / 2, startPos, endPos, animator),
        });
        return root;
    }

    public GameObject GetTarget()
    {
        return target;
    }

    public void SetTarget(GameObject _target)
    {
        target = _target;
    }


    public void ChangeState(AnimatorState state)
    {
        currentState = state;
    }
    public void SetState(AllyState newState)
    {
        spearState = newState;
    }
    public void SetCheckpoint(Vector3 pos)
    {
        if (checkpoint == null)
        {
            GameObject obj = new GameObject($"{name}_Checkpoint");
            checkpoint = obj.transform;
        }
        checkpoint.position = pos;
    }

    private void OnDrawGizmos()
    {
        // Màu đỏ cho tầm tấn công
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }
}
