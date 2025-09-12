using System.Collections.Generic;
using UnityEngine;

public class TurnPrefab : MonoBehaviour
{
    [Header("Danh sách prefab cần bật/tắt")]
    public List<GameObject> prefabs = new List<GameObject>();
    public List<GameObject> projectiles = new List<GameObject>();

    private int index = 0;
    // Bật tất cả
    public void TurnOnAll()
    {
        foreach (var obj in prefabs)
            if (obj) obj.SetActive(true);
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
}
