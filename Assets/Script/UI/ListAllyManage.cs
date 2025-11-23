using System.Collections.Generic;
using UnityEngine;

public class ListAllyManage : MonoBehaviour
{
    public GameObject rootObject; // object cha chứa tất cả ally đã spawn
    public GameObject allyCard;
    public List<GameObject> activeAllies = new List<GameObject>();

    void Start()
    {
        //ListActiveChildren();
    }
    [ContextMenu("List Active Children")]
    public void ListActiveChildren()
    {
        foreach (Transform child in rootObject.transform)
        {
            if (child.gameObject.activeSelf) // hoặc activeInHierarchy
            {
                activeAllies.Add(child.gameObject);
                Debug.Log("Child đang bật: " + child.name);
            }
        }
    }
}
