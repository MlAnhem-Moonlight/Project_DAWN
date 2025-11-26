using UnityEngine;

public class StateController : MonoBehaviour
{
    public Movement playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void ChangeState(AnimatorState state)
    {
        Debug.Log($"State changed to: {state}");
        playerMovement.setState(state);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
