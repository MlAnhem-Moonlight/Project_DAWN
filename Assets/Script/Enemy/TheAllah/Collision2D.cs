using UnityEngine;

public class BridCollisionHandler : MonoBehaviour
{
    public string animatorState = "Explosion";
    public Animator animator;

    [Header("Explosion Damage Settings")]
    public float explosionRadius = 3f;   // bán kính vụ nổ
    public float damage = 50;              // lượng dmg gây ra
    public LayerMask humanLayer;

    private void Start()
    {
        damage = GetComponent<Stats>() ? GetComponent<Stats>().currentDMG : 10;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        animator.Play(animatorState);
    }

    // Hàm này được gọi từ animation event (Animation Event gọi "Explosion")
    void Explosion()
    {

        gameObject.SetActive(false);
    }

    private void DealExplosionDamage()
    {
        // Quét trong vòng tròn, chỉ lấy collider thuộc humanLayer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, humanLayer);

        foreach (Collider2D hit in hits)
        {
            Stats health = hit.GetComponent<Stats>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    // để vẽ vùng nổ trong Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
