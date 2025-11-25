using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyCard : MonoBehaviour
{
    public GameObject ally;
    public TextMeshProUGUI allyNameText;
    public Image allyPortraitImage;

    [Header("Ally Stats UI")]
    public TextMeshProUGUI Stat;
    public TextMeshProUGUI Req;
    public Image allyPortrait;

    private static UnitUpgradeData upgradeData;
    private UnitUpgradeLevel upgradeCost;

    void Start()
    {
        Stat = GameObject.Find("AllyStatsDetail").GetComponent<TextMeshProUGUI>();
        Req = GameObject.Find("AllyReqUpgradeText").GetComponent<TextMeshProUGUI>();
        allyPortrait = GameObject.Find("AllyPortrait").GetComponent<Image>();
    }

    public void UpgradeAlly()
    {
        Stats allyStats = ally.GetComponent<Stats>();
        allyStats.level++;
        allyStats.ApplyGrowth();
    }    

    void LoadUpgradeData()
    {
        if (upgradeData != null) return;

        TextAsset file = Resources.Load<TextAsset>("UnitUpgradeCost");
        if (file == null)
        {
            Debug.LogError("Không tìm thấy UnitUpgradeCost.json trong Resources!");
            return;
        }

        upgradeData = JsonUtility.FromJson<UnitUpgradeData>(file.text);
    }

    UnitUpgradeLevel GetUpgradeCost(string unitName, int currentLevel)
    {
        string key = $"Lv{currentLevel}_{currentLevel + 1}";

        foreach (var unit in upgradeData.Units)
        {
            if (unit.name == unitName)
            {
                foreach (var u in unit.upgrade)
                {
                    if (u.key == key)
                    {
                        // chuyển UpgradeKeyValue -> UnitUpgradeLevel
                        return new UnitUpgradeLevel
                        {
                            wood = u.wood,
                            stone = u.stone,
                            iron = u.iron,
                            gold = u.gold
                        };
                    }
                }

                Debug.LogWarning("Không có dữ liệu upgrade: " + key);
            }
        }

        return null;
    }


    public void DisplayAlly()
    {
        if (ally != null)
        {
            Stats allyStats = ally.GetComponent<Stats>();
            if (allyStats != null)
            {
                allyNameText.text = ally.name.Replace("(Clone)", "").Trim() + "\nLv. " + allyStats.level;
                allyPortraitImage.sprite = allyStats.portrait;
            }
            else
            {
                Debug.LogWarning("Stats component không tồn tại.");
            }
        }
        else
        {
            Debug.LogWarning("Ally GameObject chưa được gán.");
        }
    }

    public void SelectedCard()
    {
        LoadUpgradeData();

        Stats currentAllyStats = ally.GetComponent<Stats>();

        // Clone stats
        Stats nextLevelStats = currentAllyStats.Clone();
        nextLevelStats.level++;
        nextLevelStats.ApplyGrowth();

        string cleanName = ally.name.Replace("(Clone)", "").Trim();
        upgradeCost = GetUpgradeCost(cleanName, currentAllyStats.level);

        UpgradeManager.Instance.SetSelectedAlly(ally, currentAllyStats, upgradeCost, this);
        string errorMsg = (currentAllyStats.level >= 8) ? "<color=red>Max level reached!</color>" : "<color=red>No upgrade data</color>";
        string reqText = (upgradeCost != null)
            ? "Requirement: \n" +
              $"  <sprite name=\"Wood\"> {upgradeCost.wood}    <sprite name=\"Stone\"> {upgradeCost.stone}    \n" +
              $"  <sprite name=\"Iron\"> {upgradeCost.iron}    <sprite name=\"Gold\"> {upgradeCost.gold}"
            : errorMsg;

        Stat.text =
            $"Level : {currentAllyStats.level} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.level}</color>\n" +
            $"<sprite name=\"Hp_Icon\"> {currentAllyStats.currentHP} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentHP}</color>      " +
            $"<sprite name=\"Dmg_Icon\"> {currentAllyStats.currentDMG} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentDMG}</color>\n" +
            $"<sprite name=\"Spd_Icon\"> {currentAllyStats.currentSPD} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentSPD}</color>      " +
            $"<sprite name=\"Atk_Spd_Icon\"> {currentAllyStats.currentAtkSpd} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentAtkSpd}</color>\n" +
            $"<sprite name=\"SkillCD_Icon\"> {currentAllyStats.currentSkillCD} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentSkillCD}</color>      " +
            $"<sprite name=\"Shield_Icon\"> {currentAllyStats.currentShield} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentShield}</color>\n" +
            $"<sprite name=\"Atk_Range_Icon\"> {currentAllyStats.currentAtkRange} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentAtkRange}</color>      " +
            $"<sprite name=\"Skill_Dmg_Icon\"> {currentAllyStats.currentSkillDmg} <sprite name=\"Arrow_Right_Icon\"> <color=#00FF00>{nextLevelStats.currentSkillDmg}</color>";

        Req.text = reqText;
        allyPortrait.sprite = currentAllyStats.portrait;
    }
}
