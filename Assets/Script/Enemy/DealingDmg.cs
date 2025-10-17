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
    public float _atkSpd = 1f;                     // Tốc độ đánh
    public float _rageDuration = 5f;             // Thời gian tăng tốc độ đánh khi dùng skill

    [Header("References")]
    public Animator attackerAnimator;          // Animator của nhân vật
    public LayerMask targetLayers;             // Layer mục tiêu (VD: Human)

    [Header("Skill state")]
    public int usingSkill = 0;
    private bool pendingAttack = false;        // Đang chờ thực thi hit (được Animator báo)

    void Awake()
    {
        if (!attackerAnimator)
            attackerAnimator = GetComponentInParent<Animator>();
    }

    public void SetDamageAmount(float basic, float skill, float atkSpd)
    {
        damageAmount = basic;
        skillDamageAmount = skill;
        _atkSpd = atkSpd;

        // Có thể tắt object nếu muốn ẩn hitbox sau khi thiết lập
        // gameObject.SetActive(false);
    }

    public void SetDamageAmount(float basic,float atkSpd)
    {
        damageAmount = basic;
        _atkSpd = atkSpd;

        // Có thể tắt object nếu muốn ẩn hitbox sau khi thiết lập
        // gameObject.SetActive(false);
    }

    public void SetDamageAmount(float basic, float skill, float atkSpd, float rageDuration)
    {
        damageAmount = basic;
        skillDamageAmount = skill;
        _atkSpd = atkSpd;
        _rageDuration = rageDuration;
        // Có thể tắt object nếu muốn ẩn hitbox sau khi thiết lập
        // gameObject.SetActive(false);
    }

    public void SetUsingSkill(int skill)
    {
        usingSkill = skill;
    }

    // Gọi từ Animation Event: đặt trong clip Attack/Skill
    // Ở thời điểm vung vũ khí chạm mục tiêu
    public void AttackHit()
    {
        pendingAttack = true;
    }

    void Update()
    {
        //// Nếu animator đã gửi tín hiệu đánh
        //if (pendingAttack)
        //{
        //    pendingAttack = false; // reset

        //    var box = GetComponent<BoxCollider2D>();
        //    Vector2 center = (Vector2)transform.position + box.offset;
        //    Vector2 size = box.size;
        //    Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetLayers);
        //    //// Quét collider trong phạm vi của hitbox (ví dụ BoxCollider2D của object này)
        //    //Collider2D[] hits = Physics2D.OverlapBoxAll(
        //    //    transform.position,
        //    //    GetComponent<BoxCollider2D>().size,
        //    //    0f,
        //    //    targetLayers);
        //    //Debug.Log($"Detected {hits.Length} hits");
        //    foreach (var hit in hits)
        //    {
        //        Debug.Log($"Hit: {hit.name} on layer {LayerMask.LayerToName(hit.gameObject.layer)}");
        //        if (hit.gameObject == gameObject) continue;
                
        //        Stats stats = hit.GetComponent<Stats>();
        //        if (stats)
        //        {
        //            float dmg = usingSkill != 0 ? skillDamageAmount : damageAmount;
        //            stats.TakeDamage(dmg); //Dealing damage
        //            switch (usingSkill)
        //            {
        //                case 1:
        //                    ApplyKnockback(hit.gameObject);
        //                    break;
        //                case 2:
        //                    RageSkill(_rageDuration);
        //                    break;
        //                default:

        //                    break;
        //            }
        //            //Debug.Log((usingSkill != 0 ? "Skill hit " : "Attack hit") + hit.name);
        //        }
        //    }
        //}
    }

    void LateUpdate()
    {
        // Nếu animator đã gửi tín hiệu đánh
        if (pendingAttack)
        {
            pendingAttack = false; // reset

            var box = GetComponent<BoxCollider2D>();
            Vector2 center = (Vector2)transform.position + box.offset;
            Vector2 size = box.size;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetLayers);
            //// Quét collider trong phạm vi của hitbox (ví dụ BoxCollider2D của object này)
            //Collider2D[] hits = Physics2D.OverlapBoxAll(
            //    transform.position,
            //    GetComponent<BoxCollider2D>().size,
            //    0f,
            //    targetLayers);
            //Debug.Log($"Detected {hits.Length} hits");
            foreach (var hit in hits)
            {
                //Debug.Log($"Hit: {hit.name} on layer {LayerMask.LayerToName(hit.gameObject.layer)}");
                if (hit.gameObject == gameObject) continue;

                Stats stats = hit.GetComponent<Stats>();
                if (stats)
                {
                    float dmg = usingSkill != 0 ? skillDamageAmount : damageAmount;
                    stats.TakeDamage(dmg); //Dealing damage
                    switch (usingSkill)
                    {
                        case 1:
                            ApplyKnockback(hit.gameObject);
                            break;
                        case 2:
                            RageSkill(_rageDuration);
                            break;
                        default:

                            break;
                    }
                    //Debug.Log((usingSkill != 0 ? "Skill hit " : "Attack hit") + hit.name);
                }
            }
        }
    }


    void RageSkill(float duration)
    {
        //Debug.Log("Rage skill");
        Rage(_atkSpd * 1.5f);
        StartCoroutine(RageDuration(duration));
    }

    IEnumerator RageDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Rage(_atkSpd);
        usingSkill = 0;
        //Debug.Log("End Rage");
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
        float knockbackDistance = finalForce * 0.7f;
        //Debug.Log($"Knockback {target.name}: Force={finalForce}, Distance={knockbackDistance}");
        targetRb.MovePosition(targetRb.position + knockbackDirection * knockbackDistance);
        usingSkill = 0; // reset sau khi dùng skill
    }

    void Rage(float atkSpdBonus)
    {
        GetComponentInParent<Stats>()?.GetComponentInParent<Stats>().Rage(atkSpdBonus);
        float attackInterval = 1f / atkSpdBonus;
        // Lấy độ dài clip gốc
        float clipLength = 1f;
        foreach (var clip in attackerAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack") // đúng tên clip
            {
                clipLength = clip.length;
                break;
            }
        }
        // Công thức: cần tốc độ gấp clipLength/attackInterval
        float attackSpeedMultiplier = clipLength / attackInterval;

        // Gán vào parameter thay vì animator.speed
        attackerAnimator.SetFloat("AttackSpd", attackSpeedMultiplier);

        usingSkill = 0; // reset sau khi dùng skill
    }


#if UNITY_EDITOR
    // Vẽ vùng quét trong Scene view
    void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        Vector2 center = (Vector2)transform.position + box.offset;
        Vector2 size = box.size;
        if (box)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
    }
#endif
}
