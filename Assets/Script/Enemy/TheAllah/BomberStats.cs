using UnityEngine;

public class BomberStats : Stats
{
    [Header("Level Settings")]
    public int level = 1;
    public int maxLevel = 8;

    [Header("Growth Config")]
    public float hpLinear = 80f;

    public float dmgMultiplier = 2.5f;

    public float atkSpdMultiplier = 1.02f;
    public float atkSpdCap = 1.5f;

    public float spdLinear = 0.08f;



    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
    }

    public void ApplyGrowth()
    {
        if (baseStats == null) return;

        // HP (trung bình, tăng đều)
        currentHP = Mathf.RoundToInt(baseStats.HP + hpLinear * (level - 1));

        // DMG (scale vừa, công trình có multiplier riêng)
        currentDMG = Mathf.RoundToInt(baseStats.DMG * Mathf.Pow(dmgMultiplier, level - 1));

        // AtkSpd (tăng chậm, cap 1.5)
        currentAtkSpd = Mathf.Round(Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap) * 100f) / 100f;

        // SPD (tăng đều, vừa phải)
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield (luôn = 0)
        currentShield = 0;

        // SkillCD (không thay đổi)
        currentSkillCD = baseStats.SkillCD;

        Debug.Log($"{gameObject.name} | Level {level} | HP {currentHP} | DMG {currentDMG} | AtkSpd {currentAtkSpd:F2} | SPD {currentSPD:F2} | Shield {currentShield}% | SkillCD {currentSkillCD:F2}s");
    }

    /// <summary>
    /// Tính damage khi tấn công công trình
    /// </summary>
    public float GetStructureDamage(float structureMultiplier = 2f)
    {
        return currentDMG * structureMultiplier;
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
