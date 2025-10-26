using UnityEngine;

public class ArcherStats : Stats
{
    [Header("Growth Config")]
    public float hpLinear = 25f;
    public float dmgMultiplier = 1.09f;
    public float atkSpdMultiplier = 1.06f;
    public float atkSpdCap = 3.5f;
    public float spdLinear = 0.12f;
    public float shieldLinear = 0.5f;
    public float shieldCap = 25f;
    public float skillCdMultiplier = 0.94f;
    public float skillCdMinFactor = 0.45f;
    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();

    }

    private void Start()
    {
        SetDmg(currentSkillDmg);
    }

    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP
        currentHP = Mathf.RoundToInt(baseStats.HP + hpLinear * (level - 1));

        // DMG (multiplicative)
        currentDMG = Mathf.RoundToInt(baseStats.DMG * Mathf.Pow(dmgMultiplier, level - 1));

        // AtkSpd (fast)
        currentAtkSpd = Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap);
        currentAtkSpd = Mathf.Round(currentAtkSpd * 100f) / 100f;

        // SPD
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (level - 1)), (int)shieldCap);

        // SkillCD (low base, reduced quickly)
        float minCD = baseStats.SkillCD * skillCdMinFactor;
        currentSkillCD = Mathf.Max(baseStats.SkillCD * Mathf.Pow(skillCdMultiplier, level - 1), minCD);
        currentSkillCD = Mathf.Round(currentSkillCD * 100f) / 100f;

        Debug.Log($"{gameObject.name} (Archer) | L{level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
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

