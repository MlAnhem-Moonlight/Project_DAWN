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


    public TheHandMovement(Transform transform, float speed, float range, Animator animator, Transform target)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _target = target;
        _animator = animator;

    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override NodeState Evaluate()
    {
        _animator.SetInteger("State", 0);

        //Debug.Log("TheHandMovement");
        if (_target == null)
        {
            state = NodeState.FAILURE;

            return state;
        }
        float dir = _transform.position.x - _target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Direct", dir);
        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);

        if (Vector3.Distance(_transform.position, targetPosition) < 0.1f)
        {
            state = NodeState.SUCCESS;

        }
        else
        {
            state = NodeState.RUNNING;

        }

        return state;
    }
}
