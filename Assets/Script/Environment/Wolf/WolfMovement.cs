using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class WolfMovement : Nodes 
{
    private Transform _transform;
    private Transform _startArea,_endArea;
    private float _speed;
    private int walkState = Random.Range(0, 5);
    private static Transform _target;
    private Vector3? _wanderTarget = null;
    private static bool _isWandering = false;
    private static int _atk = 2;

    private Animator _animator;

    private float _sleepTimer = 0f;
    private bool sleepingBefore = false;
    private const float SLEEP_DURATION = 3f; // Duration of sleep state

    public WolfMovement(Transform transform, float speed, float range,Transform target, Transform startArea, Transform endArea, bool iswandering, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = startArea;
        _endArea = endArea;
        _isWandering = iswandering;
        _animator = animator;
    }

    public WolfMovement(Transform transform, float speed, float range, Transform target, bool iswandering, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _target = target;
        _startArea = null;
        _endArea = null;
        _isWandering = iswandering;
        _animator = animator;
    }

    public WolfMovement(Transform transform, float speed, float range, Transform startArea, Transform endArea, bool iswandering, Animator animator)
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

    public static void SetAtk(int atk)
    {
        _atk = atk;
    }

    public override NodeState Evaluate()
    {
        
        if (_isWandering == false) 
        {
            Hunting();
        } 
        else 
        {
            Wander();
        }
        return state;
    }

    private void Flip(Vector3? pos)
    {
        if (pos.HasValue)
        {
            Vector3 targetPosition = pos.Value;
            Vector3 scale = _transform.localScale;

            // Flip logic based on target position
            if (targetPosition.x > _transform.position.x)
            {
                scale.x = Mathf.Abs(scale.x); // Face right
            }
            else if (targetPosition.x < _transform.position.x)
            {
                scale.x = -Mathf.Abs(scale.x); // Face left
            }

            _transform.localScale = scale;
        }
    }

    private void Wander()
    {
        
        float step = _speed * Time.deltaTime;
        if (walkState == 0)
        {
            // Load and play sleep animation
            if (!sleepingBefore && _sleepTimer == 0f)
            {

                _animator.SetFloat("State", 0);
                sleepingBefore = true;
            }
            else
            {
                _animator.SetFloat("State", 1);
                sleepingBefore = false;
            }

            // Wait for animation duration
            _sleepTimer += Time.deltaTime;
            if (_sleepTimer >= SLEEP_DURATION)
            {
                _sleepTimer = 0f;
                walkState = Random.Range(0, 5);
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
            Flip(_wanderTarget);
            _transform.position = Vector3.MoveTowards(_transform.position, _wanderTarget.Value, step);
            if (Vector3.Distance(_transform.position, _wanderTarget.Value) < 1f)
            {
                walkState = Random.Range(0, 5);
                _wanderTarget = null;
            }
            state = NodeState.RUNNING;
        }  
    }

    private void Hunting()
    {
        if (_target == null)
        {
            _animator.SetFloat("State", -1);
            _isWandering = true;
            state = NodeState.FAILURE;
        }
        else
        {
            Flip(_target.position); // Pass the position of the target
            float step = _speed * Time.deltaTime;
            Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);

            if (Vector3.Distance(_transform.position, targetPosition) < 1f && _animator.GetFloat("State") != _atk)
            {
                //Debug.Log("Attacking target: " + _target);
                _animator.SetFloat("State", _atk); // Set attack animation state
            }
            else if (Vector3.Distance(_transform.position, targetPosition) > 8f)
            {
                _animator.SetFloat("State", -1);
                _target = null;
            }
            else if (Vector3.Distance(_transform.position, targetPosition) >= 1f)
            {
                _animator.SetFloat("State", -1);
                _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);
            }
            state = NodeState.RUNNING;
        }
    }
}
