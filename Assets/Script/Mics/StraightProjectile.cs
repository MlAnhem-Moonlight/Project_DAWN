using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;              // Tốc độ bay
    public float lifeTime = 5f;            // Tự hủy nếu không va
    public float dmg = 10f;
    public GameObject target;

    [Header("Vị trí và hướng ban đầu")]
    public Transform _startPosition; // Có thể set trong Inspector hoặc code
    public Quaternion _startRotation;

    [Header("Animator")]
    public Animator animator, fireAnimator;

    private bool hasHit = false;
    private Vector2 moveDir;  // hướng bay thực tế (đã xác định 1 lần)

    private void OnEnable()
    {
        dmg = GetComponentInParent<Stats>()?.currentDMG ?? dmg;
        target = GetComponentInParent<ArcherBehavior>()?.GetTarget();

        hasHit = false;

        // Đặt lại vị trí ban đầu
        if (_startPosition != null)
            transform.position = _startPosition.position;

        // --- Xác định hướng bay ---
        if (target != null)
        {
            // Nếu có mục tiêu → hướng bay về target
            moveDir = (target.transform.position - transform.position).normalized;
        }
        else
        {
            // Nếu không có target → bay thẳng theo hướng nhân vật (phải hoặc trái)
            float dirX = Mathf.Sign(GetComponentInParent<Transform>().localScale.x);
            moveDir = new Vector2(dirX, 0f);
        }

        // Cập nhật hướng quay để sprite hướng theo chiều bay
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Tự hủy sau thời gian lifeTime
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
        if (target == null)
        {
            // Kiểm tra layer bằng tên
            if (other.gameObject.layer == LayerMask.NameToLayer("Human") ||
            other.gameObject.layer == LayerMask.NameToLayer("Construction"))
            {
                if (fireAnimator != null) fireAnimator.SetTrigger("Hit");
                hasHit = true;
                other.gameObject.GetComponent<Stats>().TakeDamage(dmg); //Dealing damage
            }
        }
        else
        {
            if (other.gameObject == target)
            {
                hasHit = true;
                other.gameObject.GetComponent<Stats>().TakeDamage(dmg); //Dealing damage
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
