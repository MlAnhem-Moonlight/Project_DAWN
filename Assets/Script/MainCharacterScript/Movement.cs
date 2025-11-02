using UnityEngine;

public class Movement : MonoBehaviour
{
    public float maxWalkingSpeed = 5f; 
    public float maxRunningSpeed = 10f; 
    public float acceleration = 2f; 
    public float sprintAcceleration = 4f; 
    public Animator animator;

    private float currentSpeed = 0f; 
    private Vector3 movement;
    private float previousDirection = 0f;
    private AnimationController _controller;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<AnimationController>();
    }

    void Update()
    {
        
        movement.x = Input.GetAxis("Horizontal");

        
        if (movement.x < 0 && previousDirection >= 0 || movement.x > 0 && previousDirection <= 0)
        {
            currentSpeed = currentSpeed / 2f; 
        }
        if(Input.GetAxis("Horizontal") == 0)
        {
            float dir = Random.Range(-1, 1);
            CheckMovement(dir, "Idle 1", "Idle", 0f);
        }
        else CheckMovement(Input.GetAxis("Horizontal"), "Walk 1", "Walk", 0f);
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

    private void CheckMovement(float dir, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (dir > 0)
            _controller.ChangeAnimation(animator, leftAnim, crossFade);
        else
            _controller.ChangeAnimation(animator, rightAnim, crossFade);
    }
}
