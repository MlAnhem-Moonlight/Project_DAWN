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
        playerMovement.setState(state);
    }

    public void Attack()
    {
        GetComponentInChildren<DealingDmg>().AttackHit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
