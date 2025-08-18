using BehaviorTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEditor.PlayerSettings;

public class DeerMovement : Nodes
{
    private Transform _transform;
    private Transform _startArea, _endArea;
    private float _speed, _detectionRange, _runSpeed;
    private int walkState = Random.Range(0, 3);
    private Vector3? _currentDestination = null;
    private float _idleTimer = 4f;
    private Animator _animator;

    // Threat handling
    private Transform _currentThreat = null;
    private float _threatTimer = 0f;
    private const float THREAT_SAFE_TIME = 2f;
    private const float THREAT_SAFE_DISTANCE_FACTOR = 1.5f;
    private Vector3 runTarget;

    public DeerMovement(Transform transform, float speed, float runSpeed, float range, Transform startArea, Transform endArea, Animator animator)
    {
        _transform = transform;
        _speed = speed;
        _runSpeed = runSpeed;
        _detectionRange = range;
        _startArea = startArea;
        _endArea = endArea;
        _animator = animator;
    }

    /* Sửa hành vi, do hiện tại hành vi random ra chạy quá nhiều, sau khi chạy khỏi threat thì nên là state nghỉ ngơi*/

    public override NodeState Evaluate()
    {

        // Always check for threats
        Transform threat = TargetSelector.GetClosestTarget(_transform, _detectionRange, "Human", "Wolf", null);
        bool isThreatened = threat != null;

        if (isThreatened)
        {
            _currentThreat = threat;
            _threatTimer = 0f;
        }
        else if (_currentThreat != null)
        {
            // If threat exists, check if it's far enough
            float threatDistance = Vector3.Distance(_transform.position, _currentThreat.position);
            if (threatDistance > THREAT_SAFE_DISTANCE_FACTOR * _detectionRange)
            {
                _threatTimer += Time.deltaTime;
                if (_threatTimer >= THREAT_SAFE_TIME)
                {
                    _currentThreat = null;
                    _threatTimer = 0f;
                }
            }
            else
            {
                _threatTimer = 0f;
            }
        }
        Debug.Log($"Current Threat: {_currentThreat?.name ?? "None"}");
        // If there is a threat, always run away
        if (_currentThreat != null)
        {
            Vector3 runDirection;

            // Nếu ở mép phải -> luôn chạy sang trái, vượt qua threat
            if (_transform.position.x >= _endArea.position.x)
            {
                runTarget = new Vector3(
                    _currentThreat.position.x - 1f, // 1f = khoảng cách an toàn
                    _transform.position.y,
                    _transform.position.z
                );
            }
            // Nếu ở mép trái -> luôn chạy sang phải, vượt qua threat
            else if (_transform.position.x <= _startArea.position.x)
            {
                runTarget = new Vector3(
                    _currentThreat.position.x + 1f, // 1f = khoảng cách an toàn
                    _transform.position.y,
                    _transform.position.z
                );
            }
            // Bình thường thì chạy xa threat
            else if (runTarget == _transform.position)
            {
                runDirection = new Vector3(
                    Mathf.Sign(_transform.position.x - _currentThreat.position.x),
                    0f,
                    0f
                );
                runTarget = new Vector3(
                    _transform.position.x + runDirection.x * _runSpeed * Time.deltaTime,
                    _transform.position.y,
                    _transform.position.z
                );
            }

            // Flip theo hướng chạy
            RotationObject.Flip(runTarget, _transform);

            // Di chuyển chỉ trên trục X
            _transform.position = new Vector3(
                Vector3.MoveTowards(_transform.position, runTarget, _runSpeed * Time.deltaTime).x,
                _transform.position.y,
                _transform.position.z
            );

            // Animation
            _animator.SetFloat("State", 4); // Running animation

            // Reset hành vi bình thường
            _currentDestination = null;
            _idleTimer = 4f;

            return NodeState.RUNNING;
        }



        // Normal behavior
        if (walkState == 0)
        {
            if (_currentDestination == null)
            {
                _currentDestination = new Vector3(
                    Random.Range(_startArea.position.x, _endArea.position.x),
                    _transform.position.y,
                    _transform.position.z
                );
            }
            RotationObject.Flip(_currentDestination, _transform);
            float step = _speed * Time.deltaTime;
            _transform.position = Vector3.MoveTowards(_transform.position, _currentDestination.Value, step);
            _animator.SetFloat("State", 3);

            if (Vector3.Distance(_transform.position, _currentDestination.Value) < 0.01f)
            {
                _currentDestination = null;
                walkState = Random.Range(0, 3);
            }
        }
        else
        {
            if(_idleTimer == 4f) _animator.SetFloat("State", Random.Range(-1, 1));
            if (_idleTimer <= 0)
            {
                walkState = Random.Range(0, 3);
                _idleTimer = 4f;
            }
            else
            {
                _idleTimer -= Time.deltaTime;
            }
        }

        return NodeState.RUNNING;
    }

}
