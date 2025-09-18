using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealingDmg : MonoBehaviour
{
    [Header("Damage Settings")]
    public float knockbackForce = 5f;          // Lực đẩy
    public float damageAmount = 5f;            // Damage thường
    public float skillDamageAmount = 10f;      // Damage skill

    [Header("References")]
    public Animator attackerAnimator;          // Animator của nhân vật
    public LayerMask targetLayers;             // Layer mục tiêu (VD: Human)

    private bool usingSkill = false;
    private bool pendingAttack = false;        // Đang chờ thực thi hit (được Animator báo)

    void Awake()
    {
        if (!attackerAnimator)
            attackerAnimator = GetComponentInParent<Animator>();
    }

    public void SetDamageAmount(float basic, float skill)
    {
        damageAmount = basic;
        skillDamageAmount = skill;
        // Có thể tắt object nếu muốn ẩn hitbox sau khi thiết lập
        // gameObject.SetActive(false);
    }

    public void SetUsingSkill()
    {
        usingSkill = true;
    }

    // Gọi từ Animation Event: đặt trong clip Attack/Skill
    // Ở thời điểm vung vũ khí chạm mục tiêu
    public void AttackHit()
    {
        pendingAttack = true;
    }

    void Update()
    {
        // Nếu animator đã gửi tín hiệu đánh
        if (pendingAttack)
        {
            pendingAttack = false; // reset

            // Quét collider trong phạm vi của hitbox (ví dụ BoxCollider2D của object này)
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                transform.position,
                GetComponent<BoxCollider2D>().size,
                0f,
                targetLayers);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                Stats stats = hit.GetComponent<Stats>();
                if (stats)
                {
                    float dmg = usingSkill ? skillDamageAmount : damageAmount;
                    stats.TakeDamage(dmg);
                    if(usingSkill) ApplyKnockback(hit.gameObject);
                    Debug.Log((usingSkill ? "Skill hit " : "Attack ") + hit.name);
                }
            }
        }
    }

    void ApplyKnockback(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        Rigidbody2D attackerRb = GetComponent<Rigidbody2D>();
        float attackerMass = attackerRb ? attackerRb.mass : 1f;

        Vector2 knockbackDirection = transform.right.normalized;
        float massFactor = targetRb.mass / attackerMass;
        float scalingFactor = 0.5f;
        float finalForce = knockbackForce * massFactor * scalingFactor;
        float knockbackDistance = finalForce * 0.1f;

        targetRb.MovePosition(targetRb.position + knockbackDirection * knockbackDistance);
        usingSkill = false; // reset sau khi dùng skill
    }

#if UNITY_EDITOR
    // Vẽ vùng quét trong Scene view
    void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, box.size);
        }
    }
#endif
}
