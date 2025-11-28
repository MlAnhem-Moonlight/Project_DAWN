using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;
using Spear.Movement;

public class ArcherBehavior : BhTree
{
    [Header("Settings")]
    public AllyState spearState = AllyState.Neutral;
    public AnimatorState currentState = AnimatorState.Idle;
    public Transform checkpoint, waypoints;
    public Transform startPos, endPos;

    [Header("References")]
    public float speed;
    public float stopDistance;
    public float attackSpeed = 1f;
    public float skillCD = 2.9f;
    public float atkRange = 3f;
    public Animator animator;
    public GameObject target;


    private ArcherMovement archerMove;

    protected override Nodes SetupTree()
    {
        startPos = GameObject.Find("PatrolStartPos")?.transform;
        endPos = GameObject.Find("PatrolEndPos")?.transform;
        waypoints = GameObject.Find("Waypoint")?.transform;
        checkpoint = GameObject.Find($"{name}_CheckPoint")?.transform;

        var stats = GetComponent<Stats>();
        if (stats != null)
        {
            speed = stats.currentSPD;
            skillCD = stats.currentSkillCD;
            attackSpeed = stats.currentAtkSpd;
        }

        animator = GetComponent<Animator>();
        archerMove = new ArcherMovement(transform, checkpoint, startPos, endPos, waypoints, speed, stopDistance, atkRange);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                archerMove,
                new ArcherArg(transform, atkRange, skillCD),
            }),
            new ArcherNeu(transform, waypoints, speed / 2, startPos, endPos, animator),
        });

        return root;
    }

    public GameObject GetTarget() => target;
    public void SetTarget(GameObject _target) => target = _target;

    public void ChangeState(AnimatorState state) => currentState = state;
    public void SetState(AllyState newState) => spearState = newState;

    // ✅ Đặt checkpoint tại vị trí mới (dùng cho AI combat)
    public void SetCheckpoint(Vector2 point)
    {
        string checkpointName = $"{name}_CheckPoint";

        // Nếu chưa có transform checkpoint -> tìm hoặc tạo mới
        if (checkpoint == null)
        {
            GameObject found = GameObject.Find(checkpointName);
            if (found != null)
                checkpoint = found.transform;
            else
                checkpoint = new GameObject(checkpointName).transform;
        }

        // Đặt vị trí (giữ nguyên Z của unit)
        checkpoint.position = new Vector3(point.x, point.y, transform.position.z);
    }

    // ✅ Lấy checkpoint (auto tìm nếu bị null)
    public Transform GetCheckpoint()
    {
        if (checkpoint == null)
        {
            GameObject found = GameObject.Find($"{name}_CheckPoint");
            if (found != null)
                checkpoint = found.transform;
        }
        return checkpoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, atkRange);
    }
}
