using BehaviorTree;
using System.Collections.Generic;
using UnityEngine;

namespace Spear.Movement
{
    /// <summary>
    /// Neutral wandering movement with idle-walk switching.
    /// NPC luôn di chuyển tới 1 waypoint duy nhất, 
    /// sau khi tới nơi thì waypoint đó sẽ đổi sang vị trí mới.
    /// </summary>
    public class NeutralMovement : Nodes
    {
        private Transform _transform;
        private Transform _waypoint;   // chỉ dùng 1 waypoint
        private float _speed;

        private float _waitTime = 1f, _waitCounter = 0f;
        private bool _waiting = false;

        private Transform _startPos;
        private Transform _endPos;

        private Animator _animator;
        private int _currentState = 4; // 4 = walk, 5 = idle

        public NeutralMovement(Transform transform, Transform waypoint, float speed,
                               Transform startPos, Transform endPos, Animator animator)
        {
            _transform = transform;
            _waypoint = waypoint;
            _speed = speed;
            _startPos = startPos;
            _endPos = endPos;
            _animator = animator;
        }

        public override NodeState Evaluate()
        {
            if (_transform.GetComponent<SpearBehavior>().spearState != AllyState.Neutral)
            {
                state = NodeState.FAILURE;
                return state;
            }
            if (_waiting)
            {
                _waitCounter += Time.deltaTime;
                if (_waitCounter >= _waitTime)
                {
                    _waiting = false;

                }
            }
            else
            {
                if (Mathf.Abs(_transform.position.x - _waypoint.position.x) < 0.5f)
                {
                    // Đặt NPC tại waypoint (chặn rung)
                    //_transform.position = new Vector3(_waypoint.position.x, _transform.position.y, _transform.position.z);

                    
                    _waitCounter = 0f;
                    _waiting = true;

                    // Random Idle hoặc Walk
                    ChooseNextState();
                    float targetPosition = _waypoint.position.x,
                    dir = _transform.position.x - targetPosition > 0 ? -1f : 1f;
                    _animator?.SetFloat("Direct", dir);
                }
                else
                {
                    if (_currentState == 4) // Walk
                    {
                        _transform.position = Vector3.MoveTowards(
                            _transform.position,
                            new Vector3(_waypoint.position.x, _transform.position.y, _transform.position.z),
                            _speed * Time.deltaTime
                        );
                    }
                    // Nếu Idle thì đứng yên
                }
            }
            return state = NodeState.RUNNING;
        }

        private void MoveWaypointToNewPosition()
        {
            if (_startPos == null || _endPos == null) return;

            // Random vị trí mới trong khoảng start – end
            Vector3 newPosition = new Vector3(
                Random.Range(_startPos.position.x, _endPos.position.x),
                _transform.position.y,
                _transform.position.z
            );

            _waypoint.position = newPosition;
            Debug.Log($"New waypoint position: {_waypoint.position}");
        }

        private void ChooseNextState()
        {
            int choice = Random.Range(0, 2); // 0 = Idle, 1 = Walk

            if (choice == 0) // Idle
            {
                _currentState = 5;
                _animator?.SetInteger("State", 5);
                

                _waitTime = Random.Range(5f, 6f); // Idle lâu
            }
            else // Walk
            {
                _currentState = 4;
                _animator?.SetInteger("State", 4);


                _waitTime = 1f;

                // Ngay khi chọn Walk thì random vị trí waypoint mới
                MoveWaypointToNewPosition();
            }
        }
    }
}
