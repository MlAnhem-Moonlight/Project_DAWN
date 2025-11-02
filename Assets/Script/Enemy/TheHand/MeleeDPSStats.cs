using UnityEngine;

public class MeleeDPSStats : Stats
{
    [Header("Growth Config")]
    public float hpLinear = 25f;

    public float dmgMultiplier = 1.02f;

    public float atkSpdMultiplier = 1.04f;
    public float atkSpdCap = 3.0f;

    public float spdLinear = 0.10f;

    public float skillCDMultiplier = 0.985f;
    public float skillCDMinFactor = 0.7f;

    protected new void Awake()
    {
        base.Awake();
        ApplyGrowth();
    }

    //public void Start()
    //{
        
    //    //Debug.Log("Done");
    //}


    public override void ApplyGrowth()
    {
        
        if (baseStats == null) return;

        // HP (rất thấp)
        currentHP = Mathf.RoundToInt(baseStats.HP + hpLinear * (level - 1));

        // DMG (scale mạnh)
        currentDMG = Mathf.RoundToInt(baseStats.DMG * Mathf.Pow(dmgMultiplier, level - 1));

        // AtkSpd (tăng nhanh, cap 3.0)
        currentAtkSpd = Mathf.Round(Mathf.Min(baseStats.AtkSpd * Mathf.Pow(atkSpdMultiplier, level - 1), atkSpdCap) * 100f) / 100f;

        // SPD (tăng đều, khá nhanh)
        currentSPD = Mathf.Round((baseStats.SPD + spdLinear * (level - 1)) * 100f) / 100f;

        // Shield (hầu như = 0)
        currentShield = baseStats.Shield;

        // SkillCD (giảm dần, cap 70%)
        float minSkillCD = baseStats.SkillCD * skillCDMinFactor;
        currentSkillCD = Mathf.Round(Mathf.Max(baseStats.SkillCD * Mathf.Pow(skillCDMultiplier, level - 1), minSkillCD) * 100f) / 100f;
        SetDmgDur(2 + 0.5f * (level - 1));
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
