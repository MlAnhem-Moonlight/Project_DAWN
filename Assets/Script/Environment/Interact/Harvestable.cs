using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Harvestable : MonoBehaviour
{
    [HideInInspector]
    public bool isHarvested = false; // đã bị thu hoạch hay đang bận
    public float harvestTime = 2f; // thời gian thu hoạch

    // Tùy: expose offset để đặt UI
    public Transform uiAnchor;


    // Gọi khi thu hoạch hoàn thành
    public void CompleteHarvest()
    {
        if (isHarvested) return;
        isHarvested = true;

        // Lấy Ingredient component (nếu có) và add vào manager
        Ingredient ingredient = GetComponent<Ingredient>();
        IngridientManager manager = Object.FindFirstObjectByType<IngridientManager>();
        if (ingredient != null && manager != null && ingredient.ingredients != null)
        {
            foreach (var entry in ingredient.ingredients)
            {
                manager.AddIngredient(entry.type, entry.quantity);
            }
        }

        // Hành động sau khi thu hoạch xong
        // ví dụ: disable object hoặc play animation
        gameObject.SetActive(false);
    }

    public Vector3 GetUIWorldPosition()
    {
        if (uiAnchor != null)
        {
            Debug.Log($"Using UI anchor position: {uiAnchor.position}");
            return uiAnchor.position;
        }
        Debug.Log($"Using transform position for UI: {transform.position}");
        return transform.position + Vector3.up * 1.2f;
    }
}
