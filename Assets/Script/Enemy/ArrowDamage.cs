using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ArrowDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public LayerMask enemyLayer;

    [Header("Projectile Settings")]
    public float arrowSpeed = 25f;

    private Rigidbody2D rb;
    private Vector2 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // Nếu bay quá xa thì hủy
        if (Vector2.Distance(player.transform.position, transform.position) > 10f)
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Được gọi từ Movement
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        // Xoay mũi tên theo hướng bay
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Reset velocity cũ
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Bắn
        rb.linearVelocity = direction * arrowSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0)
            return;

        Stats damageable = collision.gameObject.GetComponent<Stats>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
