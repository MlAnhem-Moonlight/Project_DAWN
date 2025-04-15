using BehaviorTree;
using System.Collections.Generic;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BridBehavior : Tree
{
    public float speed = 10f;
    public float attackRange = 2f;
    public float diveThreshold = 3f; // Khoảng cách để bắt đầu lao xuống
    public float minHeight = 2f; // Độ cao tối thiểu để vỗ cánh
    public UnityEngine.Animator animator;
    public UnityEngine.Transform target;
    private UnityEngine.Rigidbody2D rb;
    private BridMovement bridMovement;
    private BridCheckDistance bridCheckDistance;
    private BridDive bridDive;

    protected override Nodes SetupTree()
    {
        //target = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        rb = GetComponent<UnityEngine.Rigidbody2D>();

        if (rb == null)
        {
            UnityEngine.Debug.LogError("BridBehavior: Không tìm thấy Rigidbody2D!");
            return null;
        }


        BridCollisionHandler collisionHandler = gameObject.AddComponent<BridCollisionHandler>(); // Thêm script vào quái

        bridMovement = new BridMovement(transform, rb, speed, animator, target, minHeight);
        bridCheckDistance = new BridCheckDistance(transform, target, diveThreshold);
        bridDive = new BridDive(transform, rb, collisionHandler, target, speed * 2, animator);

        Nodes root = new Selector(new List<Nodes>
        {
            new Sequence(new List<Nodes>
            {
                bridCheckDistance,
                bridDive
            }),
            bridMovement
        });

            return root;
        }
}
