using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowStatsUI : MonoBehaviour
{
    public TextMeshProUGUI levelText, statText, reqText, Unitname;
    public GameObject selectedHolder;

    private UnitCostWrapper costData; // dữ liệu JSON load vào

    void Start()
    {
        LoadJson();
    }

    private void LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UnitReq");
        if (jsonFile != null)
        {
            // 1. Load raw List
            UnitCostWrapperRaw raw = JsonUtility.FromJson<UnitCostWrapperRaw>(jsonFile.text);

            if (raw == null || raw.Units == null)
            {
                Debug.LogError("Load JSON thất bại hoặc dữ liệu rỗng!");
                return;
            }

            // 2. Chuyển sang Dictionary thủ công
            costData = new UnitCostWrapper();
            costData.Units = new Dictionary<string, UnitCostItem>();
            foreach (var u in raw.Units)
            {
                UnitCostItem item = new UnitCostItem()
                {
                    MeatPerRound = u.MeatPerRound,
                    Lv1 = u.Lv1,
                    Lv2 = u.Lv2,
                    Lv3 = u.Lv3,
                    Lv4 = u.Lv4,
                    Lv5 = u.Lv5,
                    Lv6 = u.Lv6,
                    Lv7 = u.Lv7,
                    Lv8 = u.Lv8
                };
                costData.Units.Add(u.name, item);
            }

            Debug.Log("UnitReq.json Loaded! Total Units: " + costData.Units.Count);
        }
        else
        {
            Debug.LogError("Không tìm thấy UnitReq.json trong Resources!");
        }
    }

    public void ChooseCard()
    {
        Stats stats = GetComponentInChildren<Stats>();

        if (stats != null)
        {
            if (selectedHolder.GetComponent<MultiUIRandomizer>() != null)
                selectedHolder.GetComponent<MultiUIRandomizer>().SetSelected(stats.gameObject, this.gameObject);

            ShowStats(stats.gameObject);
        }
        else Debug.Log("No Stats Component Found");
    }

    private void ShowStats(GameObject prefab)
    {
        Stats stats = prefab.GetComponent<Stats>();
        if (stats == null) return;

        string cleanName = prefab.name.Replace("(Clone)", "").Trim();
        Unitname.text = cleanName;
        levelText.text = $"Level: {stats.level}";

        // ========= SHOW STATS =========
        statText.text =
            $"<sprite name=\"Hp_Icon\"> : {stats.currentHP}  <sprite name=\"Dmg_Icon\"> : {stats.currentDMG}\n" +
            $"<sprite name=\"Spd_Icon\"> : {stats.currentSPD} <sprite name=\"Atk_Spd_Icon\"> : {stats.currentAtkSpd}\n" +
            $"<sprite name=\"SkillCD_Icon\"> : {stats.currentSkillCD} <sprite name=\"Shield_Icon\"> : {stats.currentShield}\n" +
            $"<sprite name=\"Atk_Range_Icon\"> : {stats.currentAtkRange} <sprite name=\"Skill_Dmg_Icon\"> : {stats.currentSkillDmg}\n\n";

        // ========= SHOW COST FROM JSON =========
        if (costData != null && costData.Units.ContainsKey(cleanName))
        {
            UnitCostItem unit = costData.Units[cleanName];

            UnitCostLevel lvCost = GetCostByLevel(unit, stats.level);

            if (lvCost != null)
            {
                reqText.text =
                    $"Requirement :\n" +
                    $"<sprite name=\"Iron\"> : {lvCost.iron}  <sprite name=\"Stone\"> : {lvCost.stone}\n" +
                    $"<sprite name=\"Gold\"> : {lvCost.gold}  <sprite name=\"Wood\"> : {lvCost.wood}\n" +
                    $"<sprite name=\"Meat\"> :{unit.MeatPerRound}/ Round:";
            }
        }
        else
        {
            statText.text += "<color=red>Không có dữ liệu cost trong JSON</color>";
        }
    }

    private UnitCostLevel GetCostByLevel(UnitCostItem unit, int level)
    {
        switch (level)
        {
            case 1: return unit.Lv1;
            case 2: return unit.Lv2;
            case 3: return unit.Lv3;
            case 4: return unit.Lv4;
            case 5: return unit.Lv5;
            case 6: return unit.Lv6;
            case 7: return unit.Lv7;
            case 8: return unit.Lv8;
            default: return unit.Lv1;
        }
    }
}
