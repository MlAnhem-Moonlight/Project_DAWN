using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllyCard : MonoBehaviour
{
    public GameObject ally;
    public TextMeshProUGUI allyNameText;
    public Image allyPortraitImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayAlly()
    {
        if (ally != null)
        {
            Stats allyStats = ally.GetComponent<Stats>();
            if (allyStats != null)
            {
                allyNameText.text = ally.name.Replace("(Clone)", "").Trim() + "\nLv. " + allyStats.level.ToString();
                allyPortraitImage.sprite = allyStats.portrait;
            }
            else
            {
                Debug.LogWarning("Stats component not found on the ally GameObject.");
            }
        }
        else
        {
            Debug.LogWarning("Ally GameObject is not assigned.");
        }
    }
}
