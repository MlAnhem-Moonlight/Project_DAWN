using UnityEngine;

public class DebuffSkill : MonoBehaviour
{
    [Header("GameObject Stats")]
    public GameObject owner;

    [Header("Hitbox Settings")]
    [Tooltip("Bán kính quét collider")]
    public float size = 1f;

    [Tooltip("Layer mục tiêu (có thể chọn nhiều)")]
    public LayerMask targetLayers;
    
    public float value; 
    public float castDuration; // Thời gian thi triển kỹ năng 
    public int type = 0; // 0: Giảm tốc độ, 1: Phá giáp, 2: Giảm sát thương, 3 : Giảm tốc độ đánh

    private bool isCasting = true;

    private void OnEnable()
    {
        value = owner.GetComponent<Stats>().currentSkillDmg;
        castDuration = owner.GetComponent<Stats>().currentSkillDuration;
    }

    void Update()
    {
        // Quét tất cả Collider2D nằm trong bán kính 'size' và thuộc Layer trong targetLayers
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            size,
            targetLayers
        );
        // Duyệt qua toàn bộ kết quả
        foreach (Collider2D hit in hits)
        {
            // Thực hiện debuff hoặc xử lý tại đây, ví dụ:
            if(!isCasting)
            {
                isCasting = true;
                hit.GetComponent<Stats>()?.ApplyDebuff(value/100, castDuration, type);
            }
        }
    }

    void SetCastingSkill()
    {
        isCasting = false;
    }

#if UNITY_EDITOR
    // Vẽ vùng tròn quét trong Scene view để dễ chỉnh sửa
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, size);
    }
#endif
}

