using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TonMovement : Nodes
{
    private Transform _transform;
    private Transform _target;
    private Transform _defaultTarget;
    private float _speed;
    private float _range;
    private Animator _animator;

    public TonMovement(Transform transform, float speed, float range, Animator animator, Transform defaultTarget)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _defaultTarget = defaultTarget;
        _target = defaultTarget;
        _animator = animator;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        Debug.Log($"{_transform.name} target {target}");
    }

    public Transform getTarget()
    {
        return _target;
    }
    public override NodeState Evaluate()
    {
        
        if (_target != _defaultTarget && Vector3.Distance(_transform.position, _target.position) > _range+6f)
        {
            _target = _defaultTarget;
        }

        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _animator.SetInteger("State", 0);
        _animator.SetFloat("Movement", _transform.position.x - targetPosition.x > 0 ? -1f : 1f);
        if (Vector3.Distance(_transform.position, targetPosition) <= _range)
        {
            Debug.Log("Reached Target");
            return state = NodeState.SUCCESS;
        }
        else
        {
            state = NodeState.RUNNING;
            _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
        }
        //Debug.Log(Vector3.Distance(_transform.position, targetPosition) < _range - 0.1f);
        return state;
    }
}
