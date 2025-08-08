using UnityEngine;
using System;

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

    [Serializable]
    public struct IngredientEntry
    {
        public IngredientType type;
        public int quantity;
    }

    [Header("Danh sách nguyên liệu và số lượng")]
    public IngredientEntry[] ingredients;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
