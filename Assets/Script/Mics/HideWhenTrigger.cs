using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HideWhenTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D called with tag: " + other.tag);
        try
        {
            // Kiểm tra tag của vật thể kích hoạt
            if (other.CompareTag("Enemy") || other.CompareTag("Ally") || other.CompareTag("Player"))
            {
                // Thay đổi màu của Tilemap để ẩn nó
                GetComponent<Tilemap>().color = new Color(1f, 1f, 1f, 0f); // Sử dụng giá trị từ 0-1
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occurred in OnTriggerEnter2D: " + e.Message);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D called with tag: " + other.tag);
        try
        {
            // Kiểm tra tag của vật thể kích hoạt
            if (other.CompareTag("Enemy") || other.CompareTag("Ally") || other.CompareTag("Player"))
            {
                // Thay đổi màu của Tilemap để hiện lại nó
                GetComponent<Tilemap>().color = new Color(1f, 1f, 1f, 1f); // Sử dụng giá trị từ 0-1
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error occurred in OnTriggerExit2D: " + e.Message);
        }
    }
}
