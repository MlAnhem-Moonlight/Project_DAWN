using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheHandMovement : Nodes
{
    private Transform _transform;
    private Transform _target, _defaultTarget;
    private Animator _animator;
    private float _speed;
    private float _range;


    public TheHandMovement(Transform transform, float speed, float range, Animator animator, Transform target, Transform defaultTarget)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _target = target;
        _animator = animator;
        _defaultTarget = defaultTarget;

    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override NodeState Evaluate()
    {
        _animator.SetInteger("State", 0);
        //Debug.Log(_target);
        if (_target == null || _target.gameObject.activeInHierarchy == false)
        {
            _target = _defaultTarget;
        }
        float dir = _transform.position.x - _target.position.x > 0 ? -1f : 1f;
        _animator.SetFloat("Direct", dir);
        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        

        if (Vector3.Distance(_transform.position, targetPosition) < _range)
        {
            state = NodeState.FAILURE;
        }
        else
        {
            _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
            state = NodeState.RUNNING;
        }

        return state;
    }
}
