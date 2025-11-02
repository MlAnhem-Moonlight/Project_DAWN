using UnityEngine;

public class SpawnPrediction : MonoBehaviour
{
    [Header("Type Spawn")]
    [Tooltip("1 : GA; 2: CMA-ES; 3: DE")]
    public int type = 1;

    private ResourceAllocationGA ga;
    private ResourceAllocationDE de;
    private ResourceAllocationCMAES cmaes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ga = GetComponent<ResourceAllocationGA>();
        de = GetComponent<ResourceAllocationDE>();
        cmaes = GetComponent<ResourceAllocationCMAES>();

        switch(type)
        {
            case 1:
                ga.RunGA();
                break;
            case 2:
                cmaes.RunCMAES();
                break;
            case 3:
                de.RunDE();
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
