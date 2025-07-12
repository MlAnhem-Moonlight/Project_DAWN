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
    private static bool _isWandering = false;

    private Animator _animator;

    private float _sleepTimer = 0f;
    private const float SLEEP_DURATION = 3f; // Duration of sleep state

    public EnvMovement(Transform transform, float speed, float range,Transform target, Transform startArea, Transform endArea, bool iswandering, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = startArea;
        _endArea = endArea;
        _isWandering = iswandering;
        _animator = animator;
    }

    public EnvMovement(Transform transform, float speed, float range, Transform target, bool iswandering, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = null;
        _endArea = null;
        _isWandering = iswandering;
        _animator = animator;
    }

    public EnvMovement(Transform transform, float speed, float range, Transform startArea, Transform endArea, bool iswandering, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _target = null;
        _startArea = startArea;
        _endArea = endArea;
        _isWandering = iswandering;
        _animator = animator;
    }

    public static void SetTarget(Transform target)
    {
        _target = target;
    }

    public static void SetWandering(bool isWandering)
    {
        _isWandering = isWandering;
    }

    public override NodeState Evaluate()
    {
        
        if (_isWandering == false) 
        {
            Debug.Log("ryydy"); 
            Hunting();
        } 
        else 
        {
            Debug.Log("ryydyyyyyyyyyyyy");
            Wander();
        }
        return state;
    }

    private void Wander()
    {
        float step = _speed * Time.deltaTime;
        if (walkState == 0)
        {
                // Load and play sleep animation
                Debug.Log("Sleeping");
                _animator.Play("Idle");
                if (_sleepTimer == 0f)
                {
                    
                    _animator.SetFloat("State", Random.Range(0, 2));
                }

                // Wait for animation duration
                _sleepTimer += Time.deltaTime;
                if (_sleepTimer >= SLEEP_DURATION)
                {
                    _sleepTimer = 0f;
                    walkState = Random.Range(0, 7);
                    _animator.SetFloat("State", -1); // Reset animation state
                }
            state = NodeState.RUNNING;
        }
        else
        {
            _animator.SetFloat("State", -1);
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

    private void Hunting()
    {
        _animator.SetFloat("State", -1);
        Debug.Log("Hunting for target: " + _target);
        if (_target == null)
        {
            _isWandering = true;
            state = NodeState.FAILURE;

        }
        else
        {
            float step = _speed * Time.deltaTime;
            Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
            _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
            state = NodeState.RUNNING;
            if (Vector3.Distance(_transform.position, targetPosition) < 0.05f)
            {
                Debug.Log("Target reached: " + _target.name);
                state = NodeState.SUCCESS;
            }
            
        }

    }
}
