using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Stats : MonoBehaviour
{
    [Header("Level Settings")]
    public int level = 1;
    public int maxLevel = 8;

    [Header("Base Stats Reference")]
    public CharacterStatsSO baseStats;

    [Header("Runtime Stats")]
    public float currentHP;
    public float currentDMG;
    public float currentSPD;
    public float currentSkillCD;
    public float currentShield;
    public float currentAtkSpd;
    public float currentSkillDmg;
    public float currentSkillDuration;

    public void Awake()
    {
        if (baseStats != null)
        {
            // copy từ base sang runtime để không làm thay đổi asset gốc
            currentHP = baseStats.HP;
            currentDMG = baseStats.DMG;
            currentSPD = baseStats.SPD;
            currentSkillCD = baseStats.SkillCD;
            currentShield = baseStats.Shield;
            currentAtkSpd = baseStats.AtkSpd;
            currentSkillDmg = baseStats.DMG * 1.5f;
        }
    }

    /// <summary>
    /// Nhận damage, shield sẽ giảm % dmg
    /// </summary>
    public void TakeDamage(float damage)
    {
        float reducedDamage = damage * (1f - currentShield / 100f);
        currentHP -= reducedDamage;

        Debug.Log($"{gameObject.name} nhận {reducedDamage} dmg (gốc {damage}) | HP còn lại: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void ApplyDebuff(float value, float castDuration, int type)
    {
        switch (type)
        {
            case 0: // Giảm tốc độ di chuyển
                float baseSPD = currentSPD; // lưu giá trị gốc
                StartCoroutine(DebuffRoutine(
                    () => { currentSPD -= value * currentSPD; if (currentSPD < 0) currentSPD = 0; },
                    () => { currentSPD = baseSPD; },   // trả về giá trị gốc
                    castDuration,
                    $"{gameObject.name} bị giảm tốc độ"
                ));
                break;

            case 1: // Phá giáp
                float baseShield = currentShield; // lưu giá trị gốc
                StartCoroutine(DebuffRoutine(
                    () => { currentShield -= currentShield * value; },
                    () => { currentShield = baseShield; },
                    castDuration,
                    $"{gameObject.name} bị phá giáp"
                ));
                break;

            case 2: // Giảm sát thương
                float baseDMG = currentDMG; // lưu giá trị gốc
                StartCoroutine(DebuffRoutine(
                    () => { currentDMG -= value; if (currentDMG < 0) currentDMG = 0; },
                    () => { currentDMG = baseDMG; },
                    castDuration,
                    $"{gameObject.name} bị giảm sát thương"
                ));
                break;

            case 3: // Giảm tốc độ đánh
                float baseAtkSpd = currentAtkSpd; // lưu giá trị gốc
                StartCoroutine(DebuffRoutine(
                    () => { currentAtkSpd -= value * currentAtkSpd; if (currentAtkSpd < 0) currentAtkSpd = 0; },
                    () => { currentAtkSpd = baseAtkSpd; },
                    castDuration,
                    $"{gameObject.name} bị giảm tốc độ đánh"
                ));
                break;

            default:
                Debug.LogWarning("Loại debuff không hợp lệ");
                break;
        }
    }

    private IEnumerator DebuffRoutine(System.Action apply, System.Action revert, float duration, string log)
    {
        apply?.Invoke();
        //Debug.Log($"{log} trong {duration}s");
        yield return new WaitForSeconds(duration);
        revert?.Invoke();
        //Debug.Log($"{gameObject.name} hết debuff");
    }


    private void Die()
    {
        Debug.Log($"{gameObject.name} đã chết!");
        gameObject.SetActive(false);
        // ở đây bạn có thể gọi animation, destroy, event...
    }

    public void Rage(float atkSpdIncrease)
    {
        currentAtkSpd = atkSpdIncrease;
        //Debug.Log($"{gameObject.name} tăng tốc độ đánh : {currentAtkSpd}");
    }

    public void SetDmg()
    {
        GetComponentInChildren<DealingDmg>()?.SetDamageAmount(currentDMG, currentAtkSpd);

    }

    public void SetDmg(float skillDmg)
    {
        GetComponentInChildren<DealingDmg>()?.SetDamageAmount(currentDMG, skillDmg, currentAtkSpd);

    }

    public void SetDmg(float skillDmg, float duration)
    {
        GetComponentInChildren<DealingDmg>()?.SetDamageAmount(currentDMG, skillDmg, currentAtkSpd, duration);

    }

    public void SetDmgDur(float duration)
    {
        GetComponentInChildren<DealingDmg>()?.SetDamageAmount(currentDMG, currentDMG * 1.5f, currentAtkSpd, duration);

    }

    public void Attack()
    {
        GetComponentInChildren<DealingDmg>()?.AttackHit();
        //Debug.Log("Attack");
    }

    public void UseSkill()
    {
        GetComponentInChildren<DealingDmg>()?.SetUsingSkill(1);
        GetComponentInChildren<DealingDmg>()?.AttackHit();

    }

    public void UseSkill1()
    {
        GetComponentInChildren<DealingDmg>()?.SetUsingSkill(2);
        GetComponentInChildren<DealingDmg>()?.AttackHit();

    }

}
