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
            new Sequence(new List<Nodes>
            {
                new CheckInRange(transform, attackRange, animator, "Human", "Deer"),// thêm code thay đổi trạng thái chuyển từ wander sang hunt
                new EnvMovement(transform, chaseSpeed, 10f, null, false),// sai logic, do cây sẽ khởi tạo các node cùng lúc nên biến cần thay đổi trong node chứ không phải trong khởi tạo
                new WolfAttack(),
            }),
            new EnvMovement(transform, speed, 10f, startArea, endArea, true),
        });
        return root;
    }
}
