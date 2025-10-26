using UnityEngine;

public enum Type
{
    SetPositionArchery,
}
public class Mics : MonoBehaviour
{
    public Type type;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        switch(type)
        {
            case Type.SetPositionArchery:
                transform.position = new Vector3(
                    GetComponentInParent<ArcherBehavior>().target
                        ? GetComponentInParent<ArcherBehavior>().target.transform.position.x
                        : 0f,
                    transform.position.y,
                    transform.position.z
                );

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
