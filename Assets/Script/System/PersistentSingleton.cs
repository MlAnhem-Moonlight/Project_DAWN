using UnityEngine;

public class PersistentSingleton : MonoBehaviour
{
    public static PersistentSingleton Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // tránh bị tạo thêm khi Load scene mới
        }
    }
}
