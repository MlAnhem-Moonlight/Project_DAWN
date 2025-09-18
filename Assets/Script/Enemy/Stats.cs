using UnityEngine;

public class Stats : MonoBehaviour
{
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

    private void Die()
    {
        Debug.Log($"{gameObject.name} đã chết!");
        // ở đây bạn có thể gọi animation, destroy, event...
    }

    public void SetDmg()
    {
        GetComponentInChildren<DealingDmg>()?.SetDamageAmount(currentDMG, currentDMG * 1.5f);
        Debug.Log("Activate: " + GetComponentInChildren<DealingDmg>());
    }

    public void Attack()
    {
        GetComponentInChildren<DealingDmg>()?.AttackHit();
        Debug.Log("Attack: " + GetComponentInChildren<DealingDmg>());
    }

    public void UseSkill()
    {
        GetComponentInChildren<DealingDmg>()?.SetUsingSkill();
        GetComponentInChildren<DealingDmg>()?.AttackHit();
        Debug.Log("Using skill: " + GetComponentInChildren<DealingDmg>());
    }
}
