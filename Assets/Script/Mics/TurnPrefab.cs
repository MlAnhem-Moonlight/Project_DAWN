using System.Collections.Generic;
using UnityEngine;

public class TurnPrefab : MonoBehaviour
{
    [Header("GameObject Stats")]
    public GameObject owner;

    [Header("Danh sách prefab cần bật/tắt")]
    public List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> projectiles = new List<GameObject>();

    [Header("Animator")]
    public float duration = 2f; // Duration in seconds
    public Animator animator;
    public string clipName = "CastingSpell";
    public bool isMage = false;

    private int index = 0;

    private void OnEnable()
    {
        duration = owner.GetComponent<Stats>().currentSkillDuration;
        if (isMage) SetupAnimatorDuration(animator, duration, clipName);
    }

    // Bật tất cả
    public void TurnOnAll()
    {
        foreach (var obj in prefabs)
            if (obj) obj.SetActive(true);
    }

    public void TurnOffSelf()
    {
        gameObject.SetActive(false);
    }

    // Tắt tất cả
    public void TurnOffAll()
    {
        foreach (var obj in prefabs)
            if (obj) obj.SetActive(false);
    }

    // Animator có thể gọi và truyền Index (int)
    public void TurnOnByIndex(int index)
    {
        if (index >= 0 && index < prefabs.Count && prefabs[index])
            prefabs[index].SetActive(true);
    }

    public void TurnOffByIndex(int index)
    {
        if (index >= 0 && index < prefabs.Count && prefabs[index])
            prefabs[index].SetActive(false);
    }

    // Animator có thể gọi và truyền Name (string)
    public void TurnOnByName(string prefabName)
    {
        foreach (var obj in prefabs)
            if (obj && obj.name == prefabName)
                obj.SetActive(true);
    }

    public void TurnOffByName(string prefabName)
    {
        foreach (var obj in prefabs)
            if (obj && obj.name == prefabName)
                obj.SetActive(false);
    }

    public void TurnOnProjectiles()
    {

        projectiles[index].SetActive(true);
        //index = index == projectiles.Count ? 0 : index++;
        index = (index + 1) % projectiles.Count;

    }
    public void TurnOffAllProjectiles()
    {
        foreach (var obj in projectiles)
            if (obj) obj.SetActive(false);
    }

    public void GetAnimDuration()
    {
        animator.SetFloat("CastingSpellEffectSpd", 1f);
    }

    public void SetupAnimatorDuration(Animator animator, float speed, string clipName)
    {

        // Lấy độ dài clip gốc
        float clipLength = 1f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName) // đúng tên clip
            {
                clipLength = clip.length;
                Debug.Log($"Found {clipName} clip length: {clipLength}s");
                break;
            }
        }

        // Công thức: cần animator chạy với tốc độ này
        float attackSpeedMultiplier = clipLength / speed;

        // Gán vào parameter thay vì animator.speed
        animator.SetFloat(clipName + "Spd", attackSpeedMultiplier);

        //Debug.Log($"ClipLength={clipLength:F2}s, AttackInterval={attackInterval:F2}s, " +
        //          $"AttackSpeedMul={attackSpeedMultiplier:F2}");
    }


}
