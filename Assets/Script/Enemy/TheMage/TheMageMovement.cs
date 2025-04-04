using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheMageMovement : Nodes
{
    private Transform _transform;
    private Transform _target;
    private Animator _animator;
    private Transform _defaultTarget;
    private float _speed;
    private float _range;
    public int direction { get; private set; }

    public TheMageMovement(Transform transform, float speed, float range, Animator animator, Transform defaultTarget = null, Transform target = null)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _defaultTarget = defaultTarget;
        _target = target ?? defaultTarget;
        _animator = animator;
        direction = 0;
        _animator.SetFloat("Movement", direction);

    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
    public override NodeState Evaluate()
    {
        if (_target == null)
        {
            state = NodeState.FAILURE;
            direction = 0;
            _animator.SetFloat("Movement", direction);
            return state;
        }

        if (_target == null)
        {
            _target = _defaultTarget;
        }
        
        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);

        if (Vector3.Distance(_transform.position, targetPosition) < 0.1f)
        {
            state = NodeState.SUCCESS;
            direction = 0;
            _animator.SetFloat("Movement", direction);
        }
        else
        {
            state = NodeState.RUNNING;
            direction = _target.position.x > _transform.position.x ? 1 : -1;
            _animator.SetFloat("Movement", direction);
        }

        return state;
    }
}
