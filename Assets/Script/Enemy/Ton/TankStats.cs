using UnityEngine;

public class TankStats : Stats
{
    [Header("Level Settings")]
    public int level = 1;              // level hiện tại
    public int maxLevel = 8;          // level tối đa (có thể chỉnh)

    [Header("Growth Parameters")]
    public float hpLinear = 100f;      // +200 HP mỗi level
    public float hpMultiplier = 1.03f; // HP scale %
    public float dmgLinear = 2f;
    public float atkSpdMultiplier = 1.01f;
    public float atkSpdCap = 1.1f;
    public float spdLinear = 0.05f;
    public float shieldLinear = 2f;
    public float shieldCap = 50f;
    public float skillCDMultiplier = 0.99f;
    public float skillCDMinFactor = 0.6f; // 60% của baseSkillCD

    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
        SetDmg();
    }

    /// <summary>
    /// Cập nhật chỉ số theo level hiện tại
    /// </summary>
    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP
        currentHP = Mathf.RoundToInt((baseStats.HP + hpLinear * (level - 1)) * Mathf.Pow(hpMultiplier, level - 1));

        // DMG
        currentDMG = Mathf.RoundToInt(baseStats.DMG + dmgLinear * (level - 1));

        // AtkSpd
        currentAtkSpd = Mathf.Round(Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap) * 100f) / 100f;

        // SPD
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (level - 1)), (int)shieldCap);

        // SkillCD
        float minSkillCD = baseStats.SkillCD * skillCDMinFactor;
        currentSkillCD = Mathf.Round(Mathf.Max(baseStats.SkillCD * Mathf.Pow(skillCDMultiplier, level - 1), minSkillCD) * 100f) / 100f;

        // SkillDMG
        currentSkillDmg = currentDMG * 1.5f;


        Debug.Log($"{gameObject.name} | Level {level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s | SkillDMG {currentSkillDmg:F2}");
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
