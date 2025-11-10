using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SpawnProjectile : MonoBehaviour
{
    [Header("Tower Settings")]
    public float attackRange = 5f;
    public float fireRate = 1.5f;   // Giây giữa mỗi lần bắn
    public Transform firePoint;
    public LayerMask demonLayer;

    [Header("Pool & Prefabs")]
    public List<GameObject> projectiles = new List<GameObject>(); // Pool mũi tên
    public int projectileIndex = 0;

    [Header("Runtime")]
    public GameObject currentTarget;

    private float fireCooldown = 0f;
    private readonly List<GameObject> enemiesInRange = new();
    private CircleCollider2D rangeCollider;

    private void Awake()
    {
        // Tự thêm hoặc lấy collider sẵn
        rangeCollider = GetComponent<CircleCollider2D>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = attackRange;
    }

    private void OnValidate()
    {
        // Cập nhật collider mỗi khi thay đổi attackRange trong Inspector
        if (rangeCollider == null)
            rangeCollider = GetComponent<CircleCollider2D>();

        if (rangeCollider != null)
        {
            rangeCollider.isTrigger = true;
            rangeCollider.radius = attackRange;
        }
    }

    public GameObject GetTarget()
    {
        return currentTarget;
    }

    private void Update()
    {
        // Dọn enemy null
        enemiesInRange.RemoveAll(e => e == null);

        // Nếu chưa có target hoặc target ra khỏi tầm → tìm lại
        if (currentTarget == null || !IsInRange(currentTarget))
            currentTarget = GetNearestEnemy();

        // Đếm cooldown bắn
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;

        // Nếu có target hợp lệ → bắn
        if (currentTarget != null && fireCooldown <= 0)
        {
            // Kiểm tra lại lần cuối (phòng trường hợp target ra khỏi range ngay frame này)
            if (IsInRange(currentTarget))
            {
                FireProjectile(currentTarget);
                fireCooldown = fireRate;
            }
            else
            {
                currentTarget = null; // Reset target ra khỏi tầm
            }
        }
    }

    private bool IsInRange(GameObject target)
    {
        return Vector2.Distance(transform.position, target.transform.position) <= attackRange;
    }

    private GameObject GetNearestEnemy()
    {
        float minDist = Mathf.Infinity;
        GameObject nearest = null;
        foreach (var e in enemiesInRange)
        {
            if (e == null || e.GetComponent<Stats>() == null)
                continue;

            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    private void FireProjectile(GameObject target)
    {
        if (projectiles.Count == 0) return;

        // Lấy mũi tên trong pool (xoay vòng)
        GameObject arrow = projectiles[projectileIndex];
        projectileIndex = (projectileIndex + 1) % projectiles.Count;

        if (arrow == null) return;

        // Đặt lại vị trí, hướng, target rồi bật
        arrow.transform.position = firePoint.position;
        //arrow.transform.rotation = Quaternion.identity;
        
        // Gán target cho mũi tên
        ProjectileArrow arrowScript = arrow.GetComponent<ProjectileArrow>();
        if (arrowScript != null)
        {
            arrowScript.startPoint = firePoint;
            arrowScript.target = target;
            arrow.SetActive(true);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & demonLayer) != 0)
        {
            if (!enemiesInRange.Contains(other.gameObject))
                enemiesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (enemiesInRange.Contains(other.gameObject))
        {
            enemiesInRange.Remove(other.gameObject);
            if (currentTarget == other.gameObject)
                currentTarget = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
