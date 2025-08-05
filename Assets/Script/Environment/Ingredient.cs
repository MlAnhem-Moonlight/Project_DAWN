using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public enum IngredientType
    {
        Wood,
        Stone,
        Gold,
        Iron,
        Meat
    }

    [Header("Tên nguyên liệu")]
    public IngredientType ingredientName;

    [Header("Số lượng nguyên liệu")]
    public int value = 1; // Số lượng nguyên liệu mặc định là 1

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
