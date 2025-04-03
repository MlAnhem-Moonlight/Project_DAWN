using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheMageMovement : Nodes
{
    private Transform _transform;
    private Transform _target;
    private Transform _defaultTarget;
    private float _speed;
    private float _range;

    public TheMageMovement(Transform transform, float speed, float range, Transform defaultTarget = null, Transform target = null)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _defaultTarget = defaultTarget;
        _target = target ?? defaultTarget;
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
            return state;
        }

        if (_target != _defaultTarget && Vector3.Distance(_transform.position, _target.position) > _range)
        {
            _target = _defaultTarget;
        }

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
