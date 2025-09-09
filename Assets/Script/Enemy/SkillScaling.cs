using UnityEngine;

public static class SkillScaling
{
    // Tank skill: Knockback + Damage
    public static float GetTankSkillDamage(float baseSkillDMG, int level)
    {
        return Mathf.Round(baseSkillDMG * Mathf.Pow(1.15f, level - 1));
    }
    public static float GetTankKnockbackDistance() => 3f;

    // Mage skill: Debuff AoE
    public static float GetMageDebuffPercent(int level)
    {
        return 10f + 3f * (level - 1); // %
    }
    public static float GetMageDebuffDuration(int level)
    {
        return Mathf.Round((2f + 0.3f * (level - 1)) * 100f) / 100f; // làm tròn 2 số thập phân
    }
    public static float GetMageDebuffRange() => 5f;

    // Melee DPS skill: Frenzy
    public static float GetMeleeFrenzyMultiplier() => 3f;
    public static float GetMeleeFrenzyDuration(int level)
    {
        return Mathf.Round((2f + 0.5f * (level - 1)) * 100f) / 100f;
    }
}
