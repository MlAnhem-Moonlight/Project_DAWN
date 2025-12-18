using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;       // Tốc độ bay
    public float lifeTime = 5f;     // Tự hủy nếu không va
    public float dmg = 10f;
    public GameObject target;

    [Header("Vị trí và hướng ban đầu")]
    public Transform _startPosition;
    public Quaternion _startRotation;

    [Header("Animator")]
    public Animator animator, fireAnimator;

    private bool hasHit = false;
    private Vector2 moveDir;

    public string layerEnemy;

    private void OnEnable()
    {
        dmg = GetComponentInParent<Stats>()?.currentDMG ?? dmg;
        target = GetComponentInParent<TheMageBehavior>()?.GetTarget();

        hasHit = false;

        // Đặt lại vị trí ban đầu nếu có
        if (_startPosition != null)
            transform.position = _startPosition.position;

        // --- Xác định hướng bay ---
        float dirX;

        if (target != null)
        {
            // Nếu có mục tiêu → xác định hướng trái/phải
            dirX = Mathf.Sign(target.transform.position.x - transform.position.x);
        }
        else
        {
            // Nếu không có target → theo hướng nhân vật
            dirX = Mathf.Sign(GetComponentInParent<Transform>().localScale.x);
        }

        moveDir = new Vector2(dirX, 0f); // Bay ngang

        // --- Xoay sprite theo hướng bay ---
        // Giữ nguyên góc nghiêng Z ban đầu (90 độ), chỉ đảo nếu bay sang trái
        float baseZ = 90f;
        float finalZ = (dirX > 0) ? baseZ : -baseZ;
        transform.rotation = Quaternion.Euler(0f, 0f, finalZ);

        // Tự hủy sau thời gian lifeTime
        CancelInvoke();
        Invoke(nameof(DisableProjectile), lifeTime);
    }

    private void Update()
    {
        if (!hasHit)
        {
            transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (target == null)
        {
            // Kiểm tra layer bằng tên
            if (other.gameObject.layer == LayerMask.NameToLayer(layerEnemy) ||
                other.gameObject.layer == LayerMask.NameToLayer("Construction"))
            {
                if (fireAnimator != null) fireAnimator.SetTrigger("Hit");
                hasHit = true;
                other.gameObject.GetComponent<Stats>()?.TakeDamage(dmg);
                DisableProjectile();
            }
        }
        else
        {
            if (other.gameObject == target)
            {
                Debug.Log("Hit target " + other.name);
                hasHit = true;
                other.gameObject.GetComponent<Stats>()?.TakeDamage(dmg);
                DisableProjectile();
            }
        }
    }

    public void DisableProjectile()
    {
        CancelInvoke(nameof(DisableProjectile));
        gameObject.SetActive(false);
    }
}
