using BehaviorTree;
using System.Collections.Generic;

public class WolfBehavior : Tree
{
    public float speed = 5f;
    public float chaseSpeed = 7f;
    public float attackSpeed = 2.5f;
    public float attackRange = 2f;



    animalState wolfState = animalState.Wander;
    protected override Nodes SetupTree()
    {
        Nodes root = new Selector(new List<Nodes>
        {
            // thêm script di chuyển, có thể chuyển từ wander(speed 5f) sang hunt(chaseSpeed 7f)
            // thêm script tấn công
            // thêm script chết
        });
        return root;
    }
}
