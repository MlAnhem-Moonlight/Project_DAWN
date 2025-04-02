using UnityEngine;

public class BuildConstruction : MonoBehaviour
{
    public bool isBuilt = false;
    public GameObject construction;
    public int constructionHP = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && isBuilt == false)
        {
            isBuilt = true;
            construction.SetActive(true);
            GetComponent<SpriteRenderer>().enabled = false;
        }
        if(isBuilt == true && constructionHP <= 0)
        {
            construction.SetActive(false);
            GetComponent<SpriteRenderer>().enabled = true;
            isBuilt = false;
        }
    }
}
