using UnityEngine;

public class MageStats : Stats
{
    [Header("Growth Config")]
    public float hpLinear = 30f;
    public float dmgMultiplier = 1.2f;
    public float atkSpdMultiplier = 1.03f;
    public float atkSpdCap = 2.5f;
    public float spdLinear = 0.12f;
    public float shieldLinear = 0.5f;
    public float shieldCap = 30f;
    public float skillCDMultiplier = 0.97f;
    public float skillCDMinFactor = 0.5f;


    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
    }

    public override void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP
        currentHP = Mathf.RoundToInt(baseStats.HP + hpLinear * (level - 1));

        // DMG
        currentDMG = Mathf.RoundToInt(baseStats.DMG * Mathf.Pow(dmgMultiplier, level - 1));

        // AtkSpd
        currentAtkSpd = Mathf.Round(Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap) * 100f) / 100f;

        // SPD
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (level - 1)), shieldCap);

        // SkillCD
        float minSkillCD = baseStats.SkillCD * skillCDMinFactor;
        currentSkillCD = Mathf.Round(Mathf.Max(baseStats.SkillCD * Mathf.Pow(skillCDMultiplier, level - 1), minSkillCD) * 100f) / 100f;

        //SkillDmg
        currentSkillDmg = 10f + 3 * (level - 1);
        //SkillDuration
        currentSkillDuration = 2f + 0.3f * (level - 1);

        SetDmg();
        // Debug log
        //Debug.Log($"{gameObject.name} | Level {level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
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
