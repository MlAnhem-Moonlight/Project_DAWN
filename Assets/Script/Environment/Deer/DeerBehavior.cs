using BehaviorTree;
using System.Collections.Generic;

public class DeerBehavior : Tree
{
    public float speed = 4f;
    public float runSpeed = 6f;
    public float rangeDetection = 8f;

    public UnityEngine.Transform startArea, endArea;
    public UnityEngine.Animator animator;

    protected override Nodes SetupTree()
    {
        startArea = UnityEngine.GameObject.Find("Start").transform;
        endArea = UnityEngine.GameObject.Find("End").transform;
        Nodes root = new Selector(new List<Nodes>
        {

            new DeerMovement(transform, speed, runSpeed, rangeDetection, startArea, endArea, animator),
        });
        return root;
    }

}
