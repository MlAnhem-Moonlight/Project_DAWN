using UnityEngine;
using Unity.Cinemachine;

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
    public float shootCooldown = 0.5f;
    
    public bool isShooting = false;
    private Camera mainCam;
    private CinemachineCamera cinemachineCamera;  // ✅ Thêm Cinemachine reference

    private float lastShootTime = -Mathf.Infinity;
    private Vector3 lastMouseWorldPos = Vector3.zero;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        _controller = GetComponent<AnimationController>();
        mainCam = Camera.main;
        
        // ✅ Tìm Cinemachine Virtual Camera (nếu có)
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        
        //Debug.Log(mainCam.name);
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
                // ✅ Kiểm tra cooldown trước khi bắn
                if (Time.time >= lastShootTime + shootCooldown)
                {
                    SpawnArrow();
                    lastShootTime = Time.time;
                }
                else
                {
                    //Debug.LogWarning($"[Arrow] Cooldown chưa hết! Còn {shootCooldown - (Time.time - lastShootTime):F2}s");
                }
            }
        }
    }

    void SpawnArrow()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector2 dir = (mouseWorldPos - firePoint.position).normalized;

        GameObject arrow = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        arrow.transform.right = dir; // 🔥 xoay mũi tên theo hướng bắn

        ArrowDamage arrowScript = arrow.GetComponent<ArrowDamage>();
        if (arrowScript != null)
        {
            arrowScript.SetDirection(dir);
        }
    }


    private void CheckMovement(float dir, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (dir > 0)
            _controller.ChangeAnimation(animator, leftAnim, crossFade);
        else
            _controller.ChangeAnimation(animator, rightAnim, crossFade);
    }

    private Vector3 GetMouseWorldPos()
    {
        if (mainCam == null || firePoint == null)
            return firePoint.position + Vector3.right * lastNonZeroDirection;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // ✅ Plane XY (chuẩn cho 2D side-scroll)
        Plane plane = new Plane(
            Vector3.forward,        // pháp tuyến Z
            firePoint.position      // đi qua firePoint
        );

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            lastMouseWorldPos = hit;
            return hit;
        }

        return firePoint.position + Vector3.right * lastNonZeroDirection;
    }



    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!isShooting || firePoint == null) return;

        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 dir = (mouseWorldPos - firePoint.position).normalized;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            firePoint.position,
            firePoint.position + dir * 5f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(firePoint.position, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(mouseWorldPos, 0.1f);
    }


    // ✅ Helper method: Vẽ hình tròn bằng Gizmos
    private void DrawCircleGizmo(Vector3 center, float radius, int segments = 16)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 360f / segments * Mathf.Deg2Rad;
            points[i] = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }

        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}