using UnityEngine;

public class ParentInheritance : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<SpriteRenderer>().color = transform.parent.GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().sortingOrder = transform.parent.GetComponent<SpriteRenderer>().sortingOrder + 1;
        GetComponent<SpriteRenderer>().sortingLayerName = transform.parent.GetComponent<SpriteRenderer>().sortingLayerName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
