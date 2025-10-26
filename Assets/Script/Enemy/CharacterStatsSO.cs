using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Base Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Base Stats")]
    public float HP = 100f;
    public float DMG = 10f;
    public float SPD = 5f;
    public float SkillCD = 3f;
    [Range(0, 100)] public float Shield = 0f;   // % giảm dmg (0–100)
    public float AtkSpd = 1f;
    public float atkRange = 1f;
}
