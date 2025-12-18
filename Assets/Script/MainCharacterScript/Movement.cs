using UnityEngine;

public class Movement : MonoBehaviour
{
    public float maxWalkingSpeed = 5f;
    public float maxRunningSpeed = 10f;
    public float acceleration = 2f;
    public float sprintAcceleration = 4f;
    public Animator animator;
    public AnimatorState playerState = AnimatorState.Idle;

    private float currentSpeed = 0f;
    private Vector3 movement;
    private float previousDirection = 0f;
    private float lastNonZeroDirection = 1f;
    private AnimationController _controller;

    [Header("Shoot Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    public bool isShooting = false;
    Camera mainCam;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<AnimationController>();
        mainCam = Camera.main;
    }

    void Update()
    {
        PlayerMovement();
        AttackCommand();
        if(Input.GetKeyDown(KeyCode.Q)) isShooting = !isShooting;
    }

    public void setState(AnimatorState State)
    {
        playerState = State;
    }

    public void PlayerMovement()
    {
        movement.x = Input.GetAxis("Horizontal");

        if (movement.x != 0)
        {
            lastNonZeroDirection = Mathf.Sign(movement.x);
        }

        if (movement.x < 0 && previousDirection >= 0 || movement.x > 0 && previousDirection <= 0)
        {
            currentSpeed = currentSpeed / 2f;
        }

        if (playerState == AnimatorState.Idle && movement.x == 0)
        {
            CheckMovement(lastNonZeroDirection, "Idle 1", "Idle", 0f);
        }
        else if (playerState != AnimatorState.Attack && playerState != AnimatorState.UsingSkill)
        {
            if (movement.x != 0)
                CheckMovement(lastNonZeroDirection, "Walk 1", "Walk", 0f);
            else
                playerState = AnimatorState.Idle;
        }

        if (playerState != AnimatorState.Walk) return;

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

    public void AttackCommand()
    {
        if (Input.GetKeyDown(KeyCode.F) && (playerState != AnimatorState.Attack && playerState != AnimatorState.UsingSkill))
        {
            if (isShooting == false)
            {
                CheckMovement(lastNonZeroDirection, "Attack 1", "Attack", 0f);
            }
            else
            {
                SpawnArrow();
            }
        }
    }

    void SpawnArrow()
    {
        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        ArrowDamage arrowScript = arrow.GetComponent<ArrowDamage>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(firePoint.position, mainCam);
        }
    }

    private void CheckMovement(float dir, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (dir > 0)
            _controller.ChangeAnimation(animator, leftAnim, crossFade);
        else
            _controller.ChangeAnimation(animator, rightAnim, crossFade);
    }
}