using UnityEngine;

public class KnightStats : Stats
{
    // Growth params (tweakable in Inspector)
    public float hpScalePerLevel = 0.18f;
    public float dmgLinear = 3f;
    public float atkSpdPerLevel = 0.015f;
    public float atkSpdCap = 1.0f;
    public float spdLinear = 0.04f;
    public float shieldLinear = 6f;
    public float shieldCap = 75f;
    public float skillCdReducePerLevel = 0.04f;
    public float skillCdMinFactor = 0.65f;

    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();

    }

    private void Start()
    {
        SetDmg();
    }
    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP (scale % per level)
        currentHP = Mathf.RoundToInt(baseStats.HP * (1f + hpScalePerLevel * (level - 1)));

        // DMG (linear)
        currentDMG = Mathf.RoundToInt(baseStats.DMG + dmgLinear * (level - 1));

        // AtkSpd (small multiplicative increase, low cap)
        currentAtkSpd = Mathf.Min(baseStats.AtkSpd * (1f + atkSpdPerLevel * (level - 1)), atkSpdCap);
        currentAtkSpd = Mathf.Round(currentAtkSpd * 100f) / 100f;

        // SPD (move)
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield %
        currentShield = Mathf.Min(Mathf.RoundToInt(baseStats.Shield + shieldLinear * (level - 1)), (int)shieldCap);

        // SkillCD (reduce per level, min factor)
        float minCD = baseStats.SkillCD * skillCdMinFactor;
        currentSkillCD = Mathf.Max(baseStats.SkillCD * (1f - skillCdReducePerLevel * (level - 1)), minCD);
        currentSkillCD = Mathf.Round(currentSkillCD * 100f) / 100f;

        Debug.Log($"{gameObject.name} (Knight) | L{level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
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
