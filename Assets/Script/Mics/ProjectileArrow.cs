using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileArrow : MonoBehaviour
{
    [Header("Physics Settings")]
    public float launchForce = 30f;
    public float arcAngle = 10f;                 // Góc bắn thấp → bay gần như thẳng
    public float gravityScaleStart = 0.05f;      // Trọng lực ban đầu rất nhỏ
    public float gravityScaleMax = 1.5f;         // Trọng lực tối đa
    public float gravityRampSpeed = 0.5f;        // Tốc độ tăng trọng lực
    public float linearDampingIncreaseRate = 0.1f; // Giảm tốc ngang
    public float lifeTime = 5f;
    public float dmg = 10f;                      // Damage cơ bản

    [Header("References")]
    public Transform startPoint;
    public GameObject target;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 startPos;
    private bool launched = false;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true; // để dùng OnTriggerEnter2D
        dmg = GetComponentInParent<Stats>()?.currentDMG ?? 10f;
    }

    private void OnEnable()
    {
        // Reset trạng thái vật lý
        rb.gravityScale = gravityScaleStart;
        rb.linearDamping = 0f;
        rb.angularVelocity = 0;
        rb.linearVelocity = Vector2.zero;
        hasHit = false;

        target = GetComponentInParent<ArcherBehavior>()?.GetTarget();

        if (startPoint != null)
            transform.position = startPoint.position;

        // --- Xác định hướng ---
        Vector2 dir;
        if (target != null)
        {
            dir = (target.transform.position - transform.position).normalized;
        }
        else
        {
            float dirX = Mathf.Sign(GetComponentInParent<Transform>().localScale.x);
            dir = Quaternion.Euler(0, 0, arcAngle * dirX) * Vector2.right * dirX;
        }

        rb.AddForce(dir * launchForce, ForceMode2D.Impulse);
        startPos = transform.position;
        launched = true;

        Invoke(nameof(DisableProjectile), lifeTime);
    }

    private void Update()
    {
        if (!launched || hasHit) return;

        // 🔹 tăng dần trọng lực
        rb.gravityScale = Mathf.MoveTowards(rb.gravityScale, gravityScaleMax, gravityRampSpeed * Time.deltaTime);

        // 🔹 tăng dần lực cản
        rb.linearDamping = Mathf.MoveTowards(rb.linearDamping, 0.4f, linearDampingIncreaseRate * Time.deltaTime);

        // 🔹 xoay đầu mũi tên theo hướng bay
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.transform == transform.parent) return; // bỏ qua người bắn

        bool validHit = false;

        if (target != null)
        {
            // 🔸 Có target → chỉ va chạm đúng target
            if (other.gameObject == target)
                validHit = true;
        }
        else
        {
            // 🔸 Không có target → va bất kỳ collider nào (trừ người bắn)
            validHit = true;
        }

        if (validHit)
        {
            hasHit = true;

            // Gây damage nếu có Stats component
            other.GetComponent<Stats>()?.TakeDamage(dmg);

            // Dừng vật lý để không lăn tiếp
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.linearDamping = 10f;

            //// (Tuỳ chọn) ghim mũi tên vào vật thể trúng
            //transform.SetParent(other.transform);

            //// Ẩn sau 0.3 giây cho tự nhiên
            Invoke(nameof(DisableProjectile), 0.3f);
        }
    }

    private void DisableProjectile()
    {
        CancelInvoke();
        hasHit = false;
        launched = false;
        gameObject.SetActive(false);
    }
}
