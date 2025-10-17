using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightStats : Stats
{
    [Header("Growth Config")]
    // Linear + exponential HP growth (suitable for Tank)
    public float hpLinear = 200f;          // +hp per level (linear)
    public float hpMultiplier = 1.03f;     // multiplicative factor per level (e.g. 1.03)

    // Optional dmg growth (linear)
    public float dmgLinear = 5f;

    // AtkSpd growth (multiplicative, small), capped
    public float atkSpdMultiplier = 1.01f;
    public float atkSpdCap = 1.0f;

    // Movement speed linear
    public float spdLinear = 0.02f;

    // Shield growth (linear %), cap
    public float shieldLinear = 6f;
    public float shieldCap = 75f;

    // Skill cooldown reduction (multiplicative or power)
    public float skillCDMultiplier = 0.96f;    // per level multiplicative
    public float skillCDMinFactor = 0.65f;     // don't go below 65% of base

    [Header("Skill Settings")]
    public float skillDuration = 4f;
    public float synergyRadius = 2.5f;
    public LayerMask allyLayer;

    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
    }

    private void Start()
    {
        SetDmg();
    }

    public void UsingSkill()
    {
        StartCoroutine(ApplySynergyShield());
    }

    private IEnumerator ApplySynergyShield()
    {
        // Tính % tăng shield theo level
        float selfBuff = 0.10f + 0.02f * (level - 1); // 10% -> 24%
        float allyBuff = 0.05f + 0.01f * (level - 1); // 5% -> 12%

        // Shield gốc
        float originalShield = currentShield;

        // Tăng cho chính Knight (dùng phần trăm của giá trị hiện tại)
        int selfShieldBonus = Mathf.RoundToInt(originalShield * selfBuff);
        currentShield = Mathf.Min(originalShield + selfShieldBonus, (int)shieldCap);
        Debug.Log($"{name} dùng kỹ năng! +{selfShieldBonus}% Shield (tổng {currentShield}%) trong {skillDuration}s");

        // Tăng cho đồng minh xung quanh
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, synergyRadius, allyLayer);
        List<Stats> buffedAllies = new List<Stats>();

        foreach (var ally in allies)
        {
            if (ally.gameObject == gameObject) continue; // bỏ qua chính mình
            Stats allyStats = ally.GetComponent<Stats>();
            if (allyStats != null)
            {
                int allyShieldBonus = Mathf.RoundToInt(allyStats.currentShield * allyBuff);
                allyStats.currentShield = Mathf.Min(allyStats.currentShield + allyShieldBonus, (int)shieldCap);
                buffedAllies.Add(allyStats);

                Debug.Log($"→ {ally.name} được cộng +{allyShieldBonus}% Shield (tổng {allyStats.currentShield}%)");
            }
        }

        // Đợi hết thời gian skill
        yield return new WaitForSeconds(skillDuration);

        // Hết hiệu lực: trả về giá trị cũ
        currentShield = originalShield;
        foreach (var ally in buffedAllies)
        {
            if (ally != null)
            {
                int reduce = Mathf.RoundToInt(ally.currentShield * allyBuff / (1f + allyBuff));
                ally.currentShield = Mathf.Max(ally.currentShield - reduce, 0);
            }
        }

        Debug.Log($"{name} synergy skill kết thúc, Shield trở lại bình thường.");
    }

    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        int lv = Mathf.Clamp(level, 1, maxLevel);

        // HP: (baseHP + linear*(lv-1)) * (hpMultiplier^(lv-1))
        float linearPart = baseStats.HP + hpLinear * (lv - 1);
        float multPart = Mathf.Pow(hpMultiplier, (lv - 1));
        currentHP = Mathf.RoundToInt(linearPart * multPart);

        // DMG: linear growth for Knight (tweakable)
        currentDMG = Mathf.RoundToInt(baseStats.DMG + dmgLinear * (lv - 1));

        // AtkSpd: multiplicative small increase, capped
        currentAtkSpd = Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, (lv - 1)), atkSpdCap);
        currentAtkSpd = Mathf.Round(currentAtkSpd * 100f) / 100f;

        // SPD: linear
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (lv - 1)) * 100f) / 100f;

        // Shield: linear percent, cap
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (lv - 1)), (int)shieldCap);

        // SkillCD: multiplicative reduction, don't go below min factor
        float minCD = baseStats.SkillCD * skillCDMinFactor;
        float cd = baseStats.SkillCD * Mathf.Pow(skillCDMultiplier, (lv - 1));
        currentSkillCD = Mathf.Max(cd, minCD);
        currentSkillCD = Mathf.Round(currentSkillCD * 100f) / 100f;

        Debug.Log($"{gameObject.name} (Knight) | L{lv} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
    }

    [ContextMenu("Level Up")]
    public void LevelUp()
    {
        if (level < maxLevel)
        {
            level++;
            ApplyGrowth();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, synergyRadius);
    }
}
