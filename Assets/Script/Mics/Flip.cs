using UnityEngine;

public class RotationObject
{
    public static void Flip(Vector3? pos, Transform _transform)
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

    public static void Flip(Vector3 pos, Transform _transform)
    {
        Vector3 scale = _transform.localScale;

        // Flip logic based only on x axis
        if (pos.x > _transform.position.x)
        {
            scale.x = Mathf.Abs(scale.x); // Face right
        }
        else if (pos.x < _transform.position.x)
        {
            scale.x = -Mathf.Abs(scale.x); // Face left
        }
        // If pos.x == _transform.position.x, keep current facing

        _transform.localScale = scale;
    }
}
