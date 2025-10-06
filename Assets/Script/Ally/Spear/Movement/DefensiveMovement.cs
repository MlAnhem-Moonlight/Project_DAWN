using BehaviorTree;
using UnityEngine;

namespace Spear.Movement
{
    /// <summary>
    /// Giữ vị trí phòng thủ tương đối so với một mục tiêu (defensive target).
    /// Chỉ di chuyển nếu ra ngoài bán kính tuần tra (_patrolRadius).
    /// </summary>
    public class DefensiveMovement : Nodes
    {
        private readonly Transform _transform;
        private readonly Transform _defensiveTarget;
        private readonly float _offset;
        private readonly float _speed;
        private readonly float _patrolRadius;

        public DefensiveMovement(Transform transform, Transform defensiveTarget, float offset, float speed, float patrolRadius)
        {
            _transform = transform;
            _defensiveTarget = defensiveTarget;
            _offset = offset;
            _speed = speed;
            _patrolRadius = patrolRadius;
        }

        public override NodeState Evaluate()
        {
            if (_transform.GetComponent<SpearBehavior>().spearState != AllyState.Defensive)
            {
                state = NodeState.FAILURE;
                return state;
            }
            if (_defensiveTarget == null) return state = NodeState.FAILURE;
            //Debug.Log($"[{_transform.name}] Thực hiện hành động phòng thủ.");
            // Tính vị trí mục tiêu phòng thủ lý tưởng
            Vector3 targetPos = _defensiveTarget.position + Vector3.left * _offset;

            // Tính khoảng cách hiện tại
            float distance = Vector2.Distance(_transform.position, targetPos);

            // Nếu ở trong phạm vi tuần tra thì không di chuyển
            if (distance <= _patrolRadius)
            {
                _transform.GetComponent<Animator>()?.SetInteger("State", 5);
                _transform.GetComponent<Animator>()?.SetFloat("Direct", _transform.position.x - targetPos.x > 0 ? -1f : 1f);
                return state = NodeState.FAILURE;
            }
            _transform.GetComponent<Animator>()?.SetFloat("Direct", _transform.position.x - targetPos.x > 0 ? -1f : 1f);
            _transform.GetComponent<Animator>()?.SetInteger("State",0);
            // Nếu ra ngoài phạm vi tuần tra -> di chuyển về
            _transform.position = Vector3.MoveTowards(
                _transform.position,
                new Vector3(targetPos.x, _transform.position.y, _transform.position.z),
                _speed * Time.deltaTime
            );
            //Debug.Log($"{_transform.name} di chuyển về vị trí phòng thủ {targetPos} (cách {distance:F2})");
            return state = NodeState.RUNNING;
            
        }

    }
}
