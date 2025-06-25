using BehaviorTree;
using System.Collections.Generic;

public class WolfBehavior : Tree
{
    public float speed = 5f;
    public float chaseSpeed = 7f;
    public float attackSpeed = 2.5f;
    public float attackRange = 2f;

    public UnityEngine.Transform startArea, endArea;

    public UnityEngine.Animator animator;



    protected override Nodes SetupTree()
    {
        Nodes root = new Selector(new List<Nodes>
        {
            // thêm script di chuyển, có thể chuyển từ wander(speed 5f) sang hunt(chaseSpeed 7f)
            new Sequence(new List<Nodes>
            {
                new CheckInRange(transform, attackRange, animator, "Human", "Deer"),
                //chuyển trạng thái sang hunt(chaseSpeed 7f)
            }),
            // wander(speed 5f)
            new EnvMovement(transform, speed, 10f, null, startArea, endArea),
        });
        return root;
    }
}
