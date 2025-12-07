using UnityEngine;

public class PanelOn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        ListAllyManage allyManage = FindAnyObjectByType<ListAllyManage>();
        if (allyManage != null)
        {
            allyManage.GenerateAllyCards();
        }
    }
}
