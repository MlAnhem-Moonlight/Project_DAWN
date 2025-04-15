using UnityEngine;

public class FlyTowardsTarget
{
    private Transform _transform;
    private Transform _target;
    private float _speed;

    public FlyTowardsTarget(Transform transform, Transform target, float speed)
    {
        _transform = transform;
        _target = target;
        _speed = speed;
    }

    public bool Move()
    {
        if (_target == null) return false;

        float step = _speed * Time.deltaTime;
        Vector3 targetPosition = new Vector3(_target.position.x, _transform.position.y, _transform.position.z);
        _transform.position = Vector3.MoveTowards(_transform.position, targetPosition, step);

        return Vector3.Distance(_transform.position, targetPosition) < 0.1f;
    }
}
