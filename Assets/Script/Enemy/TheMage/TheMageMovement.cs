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
    public bool isAttack { get; set; }
    public Vector3 _targetPosition;

    public TheMageMovement(Transform transform, float speed, float range, Animator animator, Transform defaultTarget = null, Transform target = null)
    {
        _transform = transform;
        _speed = speed;
        _range = range;
        _defaultTarget = defaultTarget;
        _target = target ?? defaultTarget;
        _animator = animator;

        isAttack = false;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public override NodeState Evaluate()
    {
        _animator.SetInteger("Anim", 0);

        // Kiểm tra xem target hiện tại còn hợp lệ hay không
        if (_target != _defaultTarget && (_target == null || !_target.gameObject.activeInHierarchy || _target.GetComponent<Stats>().currentHP <= 0))
        {
            // Target hiện tại không hợp lệ, quay lại default target
            _target = _defaultTarget;
        }

        if (_target == null)
        {
            _target = _defaultTarget;
        }

        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _targetPosition = targetPosition;
        float step = _speed * Time.deltaTime;

        _animator.SetFloat("Movement", _transform.position.x - targetPosition.x > 0 ? -1f : 1f);

        if (!isAttack)
        {
            if (Vector3.Distance(_transform.position, targetPosition) < _range - 0.5f)
            {
                state = NodeState.SUCCESS;
            }
            else
            {
                _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
                state = NodeState.RUNNING;
            }
        }
        else
        {
            state = NodeState.SUCCESS;
        }

        return state;
    }
}
