using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : Stats
{
    public float skillDmg = 6f;
    [Header("UI")]
    public Slider hpBar;

    private void Start()
    {
        SetDmg();
        hpBar = GameObject.Find("PlayerHP").GetComponent<Slider>();
        hpBar.maxValue = currentHP;
        hpBar.value = currentHP;
    }

    private void Update()
    {
        hpBar.value = currentHP;
    } 

    public void updateHP()
    {
        hpBar.maxValue = currentHP;
    }

}
