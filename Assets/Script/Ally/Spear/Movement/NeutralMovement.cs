using BehaviorTree;
using Spear.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace Spear.Movement
{
    /// <summary>
    /// Handles platformer-style patrol movement, moving left and right from a base position
    /// </summary>
    public class NeutralMovement : IMovementStrategy
    {
        private Transform _transform;
        private List<Transform> _waypoints;
        private float _speed;

        private int _currentWaypointIndex = 0;
        private int _movementCount = 0; // Tracks the number of movements

        private float _waitTime = 1f, _waitCounter = 0f;
        private bool _waiting = false;

        public NeutralMovement(Transform transform, Transform[] waypoints, float speed)
        {
            _transform = transform;
            _waypoints = new List<Transform>(waypoints);
            _speed = speed;
        }

        public void Tick()
        {
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
                Transform wp = _waypoints[_currentWaypointIndex];
                if (Mathf.Abs(_transform.position.x - wp.position.x) < 0.01f)
                {
                    _transform.position = new Vector3(wp.position.x, _transform.position.y, _transform.position.z);
                    _waitCounter = 0f;
                    _waiting = true;

                    _movementCount++;
                    if (_movementCount % 3 == 0)
                    {
                        AddRandomWaypoint();
                    }

                    _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
                }
                else
                {
                    _transform.position = Vector3.MoveTowards(
                        _transform.position,
                        new Vector3(wp.position.x, _transform.position.y, _transform.position.z),
                        _speed * Time.deltaTime
                    );
                }
            }
        }

        private void AddRandomWaypoint()
        {
            if (_waypoints.Count < 2) return;

            // Find the two farthest waypoints
            float maxDistance = 0f;
            Transform farthestA = null, farthestB = null;

            for (int i = 0; i < _waypoints.Count; i++)
            {
                for (int j = i + 1; j < _waypoints.Count; j++)
                {
                    float distance = Vector3.Distance(_waypoints[i].position, _waypoints[j].position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestA = _waypoints[i];
                        farthestB = _waypoints[j];
                    }
                }
            }

            if (farthestA != null && farthestB != null)
            {
                // Generate a random waypoint between the two farthest waypoints
                Vector3 randomPosition = Vector3.Lerp(farthestA.position, farthestB.position, Random.Range(0.3f, 0.7f));
                GameObject newWaypoint = new GameObject("RandomWaypoint");
                newWaypoint.transform.position = randomPosition;

                _waypoints.Add(newWaypoint.transform);
            }
        }
    }
}