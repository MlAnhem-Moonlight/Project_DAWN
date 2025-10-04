using BehaviorTree;
using System.Collections.Generic;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BridBehavior : BhTree
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

    private void FindTarget()
    {
        // Tìm tất cả các GameObject trong scene
        UnityEngine.GameObject[] allObjects = UnityEngine.GameObject.FindObjectsByType<UnityEngine.GameObject>(UnityEngine.FindObjectsInactive.Exclude, UnityEngine.FindObjectsSortMode.None);

        // Lọc đối tượng theo layer "Human" hoặc "Construction"
        var filteredObjects = new List<UnityEngine.GameObject>();
        foreach (var obj in allObjects)
        {
            if (obj.layer == UnityEngine.LayerMask.NameToLayer("Human") || obj.layer == UnityEngine.LayerMask.NameToLayer("Construction"))
            {
                filteredObjects.Add(obj);
            }
        }

        // Chọn ngẫu nhiên một đối tượng từ danh sách lọc
        if (filteredObjects.Count > 0)
        {
            UnityEngine.GameObject randomObject = filteredObjects[UnityEngine.Random.Range(0, filteredObjects.Count)];
            target = randomObject.transform; // Lưu transform của đối tượng được chọn
            UnityEngine.Debug.Log("Đã tìm thấy mục tiêu ngẫu nhiên: " + randomObject.name);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Không tìm thấy đối tượng nào với layer 'Human' hoặc 'Construction'.");
        }// Tìm mục tiêu trong scene
    }

    protected override Nodes SetupTree()
    {
        //target = UnityEngine.GameObject.FindGameObjectWithTag("DefaultTarget").transform;
        speed = GetComponent<BomberStats>() ? GetComponent<BomberStats>().currentSPD : 10f;
        rb = GetComponent<UnityEngine.Rigidbody2D>();
        if(target == null)
        {
            FindTarget(); // Tìm mục tiêu nếu chưa có
        }
        if (rb == null)
        {
            UnityEngine.Debug.LogError("BridBehavior: Không tìm thấy Rigidbody2D!");
            return null;
        }


        //BridCollisionHandler collisionHandler = gameObject.AddComponent<BridCollisionHandler>(); // Thêm script vào quái

        //bridMovement = new BridMovement(transform, rb, speed, animator, target, minHeight);
        //bridCheckDistance = new BridCheckDistance(transform, target, diveThreshold);
        //bridDive = new BridDive(transform, rb, collisionHandler, target, speed * 2, animator);

        Nodes root = new Selector(new List<Nodes>
                {
                    new Sequence(new List<Nodes>
                    {
                        new BridCheckDistance(transform, target, diveThreshold),
                        new BridDive(transform, rb, animator)
                    }),
                    new BridMovement(transform, rb, speed, animator, target, minHeight),
                });

        return root;
    }
}
