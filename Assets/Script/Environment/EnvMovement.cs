using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class EnvMovement : Nodes
{
    private Transform _transform;
    private Transform _startArea,_endArea;
    private float _speed;
    private int walkState = Random.Range(0, 7);
    private static Transform _target;
    private Vector3? _wanderTarget = null;
    private bool _isWandering = false;


    public EnvMovement(Transform transform, float speed, float range,Transform target, Transform startArea, Transform endArea, bool iswandering)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = startArea;
        _endArea = endArea;
        _isWandering = iswandering;
    }

    public EnvMovement(Transform transform, float speed, float range, Transform target, bool iswandering)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = null;
        _endArea = null;
        _isWandering = iswandering;
    }

    public EnvMovement(Transform transform, float speed, float range, Transform startArea, Transform endArea, bool iswandering)
    {
        _transform = transform;
        _speed = speed;
        _target = null;
        _startArea = startArea;
        _endArea = endArea;
        _isWandering = iswandering;
    }

    public static void SetTarget(Transform target)
    {
        _target = target;
    }

    public override NodeState Evaluate()
    {
        if (!_isWandering) 
        {
            Hunting();
        } 
        else 
        {
            Wander();
        }
        return state;
    }

    private void Wander()
    {
        float step = _speed * Time.deltaTime;
        if (_target == null)
        {

            if (walkState == 0)
            {
                Debug.Log("Sleep");
                walkState = Random.Range(0, 7);
                state = NodeState.RUNNING;
            }
            else
            {
                Debug.Log("Wandering");
                // Store the wander target as a persistent field to avoid picking a new one every frame
                if (_wanderTarget == null)// || Vector3.Distance(_transform.position, _wanderTarget.Value) < 0.05f
                {
                    _wanderTarget = new Vector3(Random.Range(_startArea.position.x, _endArea.position.x), _transform.position.y, _transform.position.z);
                }
                _transform.position = Vector3.MoveTowards(_transform.position, _wanderTarget.Value, step);
                if (Vector3.Distance(_transform.position, _wanderTarget.Value) < 0.05f)
                {
                    walkState = Random.Range(0, 7);
                    Debug.Log("Walk State: " + walkState);
                    _wanderTarget = null;
                }
                state = NodeState.RUNNING;
            }
        }
        else
        {
            Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
            _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
            state = NodeState.RUNNING;
        }
        
    }

    private void Hunting()
    {
        float step = _speed * Time.deltaTime;
        if (_target == null)
        {

            if (walkState == 0)
            {
                Debug.Log("Sleep");
                walkState = Random.Range(0, 7);
                state = NodeState.RUNNING;
            }
            else
            {
                Debug.Log("Wandering");
                // Store the wander target as a persistent field to avoid picking a new one every frame
                if (_wanderTarget == null)// || Vector3.Distance(_transform.position, _wanderTarget.Value) < 0.05f
                {
                    _wanderTarget = new Vector3(Random.Range(_startArea.position.x, _endArea.position.x), _transform.position.y, _transform.position.z);
                }
                _transform.position = Vector3.MoveTowards(_transform.position, _wanderTarget.Value, step);
                if (Vector3.Distance(_transform.position, _wanderTarget.Value) < 0.05f)
                {
                    walkState = Random.Range(0, 7);
                    Debug.Log("Walk State: " + walkState);
                    _wanderTarget = null;
                }
                state = NodeState.RUNNING;
            }
        }
        else
        {
            Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
            _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
            state = NodeState.RUNNING;
        }

    }
}
