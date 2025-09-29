using UnityEngine;

public class SpearStats : Stats
{
    public float hpLinear = 40f;
    public float dmgMultiplier = 1.08f;
    public float atkSpdMultiplier = 1.03f;
    public float atkSpdCap = 2.2f;
    public float spdLinear = 0.08f;
    public float shieldLinear = 1f;
    public float shieldCap = 20f;
    public float skillCdMultiplier = 0.96f;
    public float skillCdMinFactor = 0.6f;

    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
        
    }

    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP
        currentHP = Mathf.RoundToInt(baseStats.HP + hpLinear * (level - 1));

        // DMG (multiplicative)
        currentDMG = Mathf.RoundToInt(baseStats.DMG * Mathf.Pow(dmgMultiplier, level - 1));

        // AtkSpd
        currentAtkSpd = Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap);
        currentAtkSpd = Mathf.Round(currentAtkSpd * 100f) / 100f;

        // SPD
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (level - 1)), (int)shieldCap);

        // SkillCD
        float minCD = baseStats.SkillCD * skillCdMinFactor;
        currentSkillCD = Mathf.Max(baseStats.SkillCD * Mathf.Pow(skillCdMultiplier, level - 1), minCD);
        currentSkillCD = Mathf.Round(currentSkillCD * 100f) / 100f;

        SetDmg(currentDMG*2,3f);
        Debug.Log($"{gameObject.name} (Spear) | L{level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
    }

    /// <summary>
    /// Nâng cấp level (nếu chưa max)
    /// </summary>
    [ContextMenu("Level Up")]
    public void LevelUp()
    {
        if (level < maxLevel)
        {
            level++;
            ApplyGrowth();
        }
    }
}
