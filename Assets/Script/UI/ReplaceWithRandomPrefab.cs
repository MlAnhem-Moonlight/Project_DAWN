using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MultiUIRandomizer : MonoBehaviour
{
    [System.Serializable]
    public class UIGroup
    {
        public RectTransform targetImage;   // vị trí/size cũ
        public GameObject[] prefabs;        // danh sách prefab cho riêng target này
        [HideInInspector] public GameObject instance; // prefab đang dùng
    }
    [Range(0, 100)]
    public int ReRollCount = 2;
    public UIGroup[] groups;   // danh sách target + prefab list
    [Range(1, 3)]
    public int TowerLevel = 1;
    [Header("UI Stats")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI statText;
    public TextMeshProUGUI unitname;
    public TextMeshProUGUI reqDetail;

    [Header("Spawned Object")]
    public GameObject selected;
    public GameObject cardSelected;
    public GameObject rpUI;

    public EnemySpawner allySpawner;

    public void SetSelected(GameObject obj, GameObject obj1)
    {
        selected = obj;
        cardSelected = obj1;
        rpUI = cardSelected.transform.Find("OutOfStock").gameObject;
    }

    public void HireHero()
    {
        
        if (selected == null || cardSelected == null)
        {
            Debug.LogWarning("No selected object to hire");
            return;
        }
        
        if (allySpawner != null)
        {
            allySpawner.SpawnAlly(selected.name.Replace("(Clone)", "").Trim(), selected.GetComponent<Stats>().level);
        }
        else Debug.LogWarning("Ally Spawner is null");
        cardSelected.GetComponentInParent<Button>().interactable = false;
        rpUI.GetComponent<TextMeshProUGUI>().enabled = true;
        selected = cardSelected = rpUI = null;
    }

    void Start()
    {
        RandomizeAll();
    }

    private void Update()
    {
        if (GetComponent<UnityEngine.UI.Button>() != null)
        {
            if (ReRollCount > 0 && GetComponent<UnityEngine.UI.Button>().interactable == false)
            {

                GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }
    }

    public void ClearText()
    {
        levelText.text = "";
        statText.text = "";
        unitname.text = "";
        reqDetail.text = "";
    }

    [ContextMenu("Replace Prefab")]
    public void RandomizeAll()
    {
        if (ReRollCount > 0) ReRollCount--;

        levelText.text = "";
        statText.text = "";
        unitname.text = "";
        reqDetail.text = "";
        
        foreach (var group in groups)
        {
            ReplacePrefab(group);
        }
        if (ReRollCount <= 0)
        {
            Debug.LogWarning("No More Roll Left");
            if (GetComponent<UnityEngine.UI.Button>() != null)
            {
                GetComponent<UnityEngine.UI.Button>().interactable = false;
            }
        }
        
    }
    public void ReplacePrefab(UIGroup group)
    {
        Debug.Log("Replacing prefab for " + group.targetImage.name);
        // Xoá prefab cũ nếu có
        if (group.instance != null)
            Destroy(group.instance);

        // Random 1 prefab từ group.prefabs
        int index = Random.Range(0, group.prefabs.Length);
        GameObject prefab = group.prefabs[index];

        // Instantiate vào đúng Canvas Parent
        group.instance = Instantiate(prefab, group.targetImage.parent);

        // Copy toàn bộ layout từ target
        RectTransform newRT = group.instance.GetComponent<RectTransform>();
        RectTransform oldRT = group.targetImage;

        newRT.anchorMin = oldRT.anchorMin;
        newRT.anchorMax = oldRT.anchorMax;
        newRT.pivot = oldRT.pivot;
        newRT.sizeDelta = oldRT.sizeDelta;
        newRT.anchoredPosition = oldRT.anchoredPosition;
        newRT.localRotation = oldRT.localRotation;
        newRT.localScale = oldRT.localScale;
        switch(TowerLevel)
        {
            case 1:
                RandomStats(prefab, 3);
                break;
            case 2:
                RandomStats(prefab, 6);
                break;
            case 3:
                RandomStats(prefab, 8);
                break;
        }
        

        group.targetImage.gameObject.SetActive(false); // ẩn target cũ
        group.targetImage = newRT; // cập nhật target mới

    }

    private void RandomStats(GameObject prefab, int maxLevel)
    {
        if(prefab.GetComponent<Stats>() != null)
        {
            prefab.GetComponent<Stats>().level = Random.Range(1, maxLevel);
            prefab.GetComponent<Stats>().ApplyGrowth();
        }
    }

}
