using UnityEngine;

public class Movement : MonoBehaviour
{
    public float maxWalkingSpeed = 5f; 
    public float maxRunningSpeed = 10f; 
    public float acceleration = 2f; 
    public float sprintAcceleration = 4f; 
    private float currentSpeed = 0f; 
    private Vector3 movement;
    private float previousDirection = 0f;

    void Update()
    {
        
        movement.x = Input.GetAxis("Horizontal");

        
        if (movement.x < 0 && previousDirection >= 0 || movement.x > 0 && previousDirection <= 0)
        {
            currentSpeed = currentSpeed / 2f; 
        }

        
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = isSprinting ? maxRunningSpeed : maxWalkingSpeed;
        float currentAcceleration = isSprinting ? sprintAcceleration : acceleration;

        
        if (movement.x != 0)
        {
            currentSpeed += currentAcceleration * Time.deltaTime; 
            currentSpeed = Mathf.Min(currentSpeed, targetSpeed); 
        }
        else
        {
            currentSpeed -= acceleration * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f); 
        }

        previousDirection = movement.x;

        transform.position += movement.normalized * currentSpeed * Time.deltaTime;
    }
}
