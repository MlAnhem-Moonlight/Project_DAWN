using UnityEngine;

public class ArrowDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public LayerMask enemyLayer;

    [Header("Projectile Settings")]
    public float arrowSpeed = 25f;

    private Rigidbody2D rb;
    private Camera mainCam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"[ArrowDamage] {gameObject.name} không có Rigidbody2D component!");
        }
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra layer enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            // Nếu enemy có script nhận damage
            Stats damageable = collision.gameObject.GetComponent<Stats>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Hủy mũi tên khi trúng enemy
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector3 firePointPosition, Camera camera)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        mainCam = camera;

        // ✅ Lấy hướng từ raycast chuột (2D)
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        
        Vector2 shootDirection = (mouseWorldPos - (Vector2)firePointPosition).normalized;

        // ✅ Xoay arrow theo hướng bắn
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // ✅ Thiết lập Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            
            // Xóa velocity cũ trước
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // ✅ Áp dụng lực
            rb.linearVelocity = shootDirection * arrowSpeed;
            
            Debug.Log($"[Arrow] Bắn từ {firePointPosition} theo hướng {shootDirection} với tốc độ {arrowSpeed}");
        }
        else
        {
            Debug.LogError($"[ArrowDamage] Không tìm thấy Rigidbody2D trên {gameObject.name}!");
        }
    }
}
