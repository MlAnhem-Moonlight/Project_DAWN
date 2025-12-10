using System.Collections.Generic;
using UnityEngine;

public class ListAllyManage : MonoBehaviour
{
    public GameObject rootObject;     // Chứa toàn bộ Ally đã spawn
    public GameObject allyCardPrefab; // Prefab của AllyCard UI
    public Transform cardParentUI;    // Content Panel để chứa các card

    public List<GameObject> activeAllies = new List<GameObject>();

    [ContextMenu("Generate Ally Cards")]
    public void GenerateAllyCards()
    {
        // Clear list & UI cũ
        activeAllies.Clear();

  

        foreach (Transform child in rootObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeAllies.Add(child.gameObject);
            }
        }

        // Xóa card cũ trên UI
        foreach (Transform c in cardParentUI)
            Destroy(c.gameObject);

        // Tạo card mới
        foreach (GameObject ally in activeAllies)
        {
            GameObject newCard = Instantiate(allyCardPrefab, cardParentUI);

            AllyCard card = newCard.GetComponent<AllyCard>();
            card.ally = ally;   // Gán ally vào card
            card.DisplayAlly(); // Hiển thị UI
        }

        Debug.Log("Tạo xong " + activeAllies.Count + " Ally Cards.");
    }

    private void OnEnable()
    {
        GenerateAllyCards();
    }
}
