using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheHandMovement : Nodes
{
    private Transform _transform;
    private Transform _target;
    private Animator _animator;
    private float _speed;
    private float _range;
    public int direction { get; private set; }
    public TheHandMovement(Transform transform, float speed, float range, Animator animator, Transform target = null)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _target = target;
        _animator = animator;
        direction = 0;


    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
    public override NodeState Evaluate()
    {
        _animator.SetBool("IsAttack", false);
        if (_target == null)
        {
            state = NodeState.FAILURE;
            direction = 0;
            return state;
        }

        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);

        if (Vector3.Distance(_transform.position, targetPosition) < 0.1f)
        {
            state = NodeState.SUCCESS;
            direction = 0;
        }
        else
        {
            state = NodeState.RUNNING;

        }

        return state;
    }
}
