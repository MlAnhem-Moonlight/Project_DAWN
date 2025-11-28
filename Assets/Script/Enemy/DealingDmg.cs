using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealingDmg : MonoBehaviour
{
    [Header("Damage Settings")]
    public float knockbackForce = 5f;
    public float damageAmount = 5f;
    public float skillDamageAmount = 10f;
    public float _atkSpd = 1f;
    public float _rageDuration = 5f;

    [Header("References")]
    public Animator attackerAnimator;
    public LayerMask targetLayers;

    [Header("Skill state")]
    [Tooltip("0: normal attack, 1: knockback, 2: rage, 3: damage over time")]
    public int usingSkill = 0;
    private bool pendingAttack = false;

    // Skill 3 variables
    private bool _isSkill3Active = false;          // skill3 bật/tắt
    public float skill3TickInterval = 0.5f;        // khoảng thời gian mỗi tick damage
    private Coroutine _skill3Coroutine;            // coroutine đang chạy

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
    }

    public void SetDamageAmount(float basic, float atkSpd)
    {
        damageAmount = basic;
        _atkSpd = atkSpd;
    }

    public void SetDamageAmount(float basic, float skill, float atkSpd, float rageDuration)
    {
        damageAmount = basic;
        skillDamageAmount = skill;
        _atkSpd = atkSpd;
        _rageDuration = rageDuration;
    }

    public void SetUsingSkill(int skill)
    {
        usingSkill = skill;
    }

    public void UsingSkill(int skill)
    {
        usingSkill = skill;
        pendingAttack = true;
    }

    // Gọi từ Animation Event
    public void AttackHit()
    {
        pendingAttack = true;
    }

    // === HÀM MỚI: BẬT/TẮT SKILL 3 ===
    public void SetSkill3Active(bool state)
    {
        _isSkill3Active = state;

        if (state)
        {
            if (_skill3Coroutine == null)
                _skill3Coroutine = StartCoroutine(Skill3DamageLoop());
        }
        else
        {
            if (_skill3Coroutine != null)
            {
                StopCoroutine(_skill3Coroutine);
                _skill3Coroutine = null;
            }
        }
    }

    IEnumerator Skill3DamageLoop()
    {
        while (_isSkill3Active)
        {
            ApplySkill3Damage();
            yield return new WaitForSeconds(skill3TickInterval);
        }
    }

    void ApplySkill3Damage()
    {
        var box = GetComponent<BoxCollider2D>();
        Vector2 center = (Vector2)transform.position + box.offset;
        Vector2 size = box.size;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetLayers);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            Stats stats = hit.GetComponent<Stats>();
            if (stats)
            {
                stats.TakeDamage(skillDamageAmount);
            }
        }
    }

    void LateUpdate()
    {
        if (pendingAttack)
        {
            pendingAttack = false;
            var box = GetComponent<BoxCollider2D>();
            Vector2 center = (Vector2)transform.position + box.offset;
            Vector2 size = box.size;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, targetLayers);

            foreach (var hit in hits)
            {
                Debug.Log(hit.name);
                if (hit.gameObject == gameObject) continue;
                Stats stats = hit.GetComponent<Stats>();
                if (stats)
                {
                    float dmg = usingSkill != 0 ? skillDamageAmount : damageAmount;
                   
                    switch (usingSkill)
                    {
                        case 1:
                            stats.TakeDamage(dmg);
                            ApplyKnockback(hit.gameObject);
                            break;
                        case 2:
                            RageSkill(_rageDuration);
                            stats.TakeDamage(dmg);
                            break;
                        case 3:
                            // Skill 3 bắt đầu được animation bật bằng SetSkill3Active(true)
                            // Không gây damage tức thời ở đây
                            break;
                        default:
                            stats.TakeDamage(dmg);
                            break;
                    }
                }
            }
        }
    }

    void RageSkill(float duration)
    {
        Rage(_atkSpd * 1.5f);
        StartCoroutine(RageDuration(duration));
    }

    IEnumerator RageDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        Rage(_atkSpd);
        usingSkill = 0;
    }

    void ApplyKnockback(GameObject target)
    {
        if(target.layer == LayerMask.NameToLayer("Construction")) return;
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        Rigidbody2D attackerRb = GetComponent<Rigidbody2D>();
        float attackerMass = attackerRb ? attackerRb.mass : 1f;

        Vector2 knockbackDirection = transform.right.normalized;
        float massFactor = targetRb.mass / attackerMass;
        float scalingFactor = 0.5f;
        float finalForce = knockbackForce * massFactor * scalingFactor;
        float knockbackDistance = finalForce * 0.7f;

        targetRb.MovePosition(targetRb.position + knockbackDirection * knockbackDistance);
        usingSkill = 0;
    }

    void Rage(float atkSpdBonus)
    {
        GetComponentInParent<Stats>()?.GetComponentInParent<Stats>().Rage(atkSpdBonus);
        float attackInterval = 1f / atkSpdBonus;
        float clipLength = 1f;

        foreach (var clip in attackerAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Attack")
            {
                clipLength = clip.length;
                break;
            }
        }

        float attackSpeedMultiplier = clipLength / attackInterval;
        attackerAnimator.SetFloat("AttackSpd", attackSpeedMultiplier);
        usingSkill = 0;
    }
}
