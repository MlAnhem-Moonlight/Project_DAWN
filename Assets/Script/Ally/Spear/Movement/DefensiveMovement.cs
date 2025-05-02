using Spear.Movement;
using UnityEngine;

namespace Spear.Movement
{
    ///&#x20;
    /// Holds a defensive position relative to a target transform.
    ///&#x20;
    public class DefensiveMovement : IMovementStrategy
    {
        private readonly Transform _transform;
        private readonly Transform _defensiveTarget;
        private readonly float _offset;
        private readonly float _speed;

        public DefensiveMovement(Transform transform, Transform defensiveTarget, float offset, float speed)
        {
            _transform = transform;
            _defensiveTarget = defensiveTarget;
            _offset = offset;
            _speed = speed;
        }

        public void Tick()
        {
            if (_defensiveTarget == null) return;
            Vector3 targetPos = _defensiveTarget.position + Vector3.left * _offset;
            _transform.position = Vector3.MoveTowards(
                _transform.position,
                new Vector3(targetPos.x, _transform.position.y, _transform.position.z),
                _speed * Time.deltaTime
            );
        }
    }
}