using BehaviorTree;
using UnityEngine;

/// <summary>
/// Di chuyển Archer đến checkpoint (mỗi Archer có checkpoint riêng).
/// Khi tới nơi (trong phạm vi _stopDistance), ẩn checkpoint và trả về SUCCESS.
/// </summary>
public class ArcherMovement : Nodes
{
    private readonly Transform _transform;
    private Transform _checkpoint;

    private Transform _waypoint;   // chỉ dùng 1 waypoint
    private Transform _startPos;
    private Transform _endPos;
    private Vector3 destination;
    private float _waitTime = 12f, _waitCounter = 0f;
    private bool _waiting = true;
    

    private readonly float _speed;
    private readonly float _stopDistance;
    private readonly float _attackRange;
    private AnimationController _controller;

    public ArcherMovement(Transform transform, Transform checkpoint, Transform startPos, Transform endPos, Transform waypoint, 
                            float speed, float stopDistance, float attackRange)
    {
        _transform = transform;
        _checkpoint = checkpoint;
        _speed = speed;
        _stopDistance = stopDistance;
        _attackRange = attackRange;
        _startPos = startPos;
        _endPos = endPos;
        _waypoint = waypoint;
        _controller = _transform.gameObject.GetComponent<AnimationController>();
        MoveWaypointToNewPosition();
    }

    public void SetCheckPoint(Transform checkpoint)
    {
        _checkpoint = checkpoint;
    }

    public override NodeState Evaluate()
    {
        var archer = _transform.GetComponent<ArcherBehavior>();
        // Nếu Archer đang ở trạng thái trung lập thì di ngẫu nhiên
        if (archer.spearState == AllyState.Neutral)
        {
            RandomMovement();
            state = NodeState.FAILURE;
            return state;
        }

        // Nếu chưa có checkpoint thì không di chuyển
        if (_checkpoint == null)
        {
            GameObject cpObj = GameObject.Find($"{_transform.name}_CheckPoint");
            if (cpObj != null)
                _checkpoint = cpObj.transform;
            else
            {
                state = NodeState.SUCCESS;
                return state;
            }
        }

        // Nếu đang tấn công hoặc dùng kỹ năng thì dừng di chuyển
        if (archer.currentState == AnimatorState.Attack || archer.currentState == AnimatorState.UsingSkill)
        {
            return NodeState.SUCCESS;
        }

        Vector3 targetPos = new Vector3(_checkpoint.position.x, _transform.position.y, _transform.position.z);
        float distance = Vector2.Distance(_transform.position, targetPos);

        // Nếu đã đến vị trí checkpoint hoặc có mục tiêu
        if (distance <= _stopDistance || archer.target != null)
        {
            if (_checkpoint.gameObject.activeSelf)
                _checkpoint.gameObject.SetActive(false); // Ẩn checkpoint khi đã tới nơi

            return state = NodeState.SUCCESS;
        }

        // Nếu chưa đến checkpoint thì di chuyển
        CheckMovement(targetPos, "Run2", "Run2 1", 0f);
        _transform.position = Vector3.MoveTowards(
            _transform.position,
            targetPos,
            _speed * Time.deltaTime
        );

        if (!_checkpoint.gameObject.activeSelf)
            _checkpoint.gameObject.SetActive(true); // Hiện checkpoint nếu bị ẩn

        return state = NodeState.FAILURE;
    }

    private void CheckMovement(Vector3 targetPos, string leftAnim, string rightAnim, float crossFade = 0.1f)
    {
        if (_transform.position.x - targetPos.x > 0)
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), leftAnim, crossFade);
        else
            _controller.ChangeAnimation(_transform.GetComponent<Animator>(), rightAnim, crossFade);
    }

    private void RandomMovement()
    {
        if (_waiting)
        {
            _waitCounter += Time.deltaTime;
            if (_waitCounter >= _waitTime)
            {
                _waiting = false;

            }
        }
        else
        {
            if (Mathf.Abs(_transform.position.x - destination.x) < 0.5f)
            {
                // Đặt NPC tại waypoint (chặn rung)
                //_transform.position = new Vector3(_waypoint.position.x, _transform.position.y, _transform.position.z);


                _waitCounter = 0f;
                _waiting = true;

                // Random Idle hoặc Walk
                ChooseNextState();


            }
            else
            {
                if (_transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.Running
                    || _transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.Walk) // Walk
                {
                    _transform.position = Vector3.MoveTowards(
                        _transform.position,
                        new Vector3(destination.x, _transform.position.y, _transform.position.z),
                        _speed * Time.deltaTime
                    );
                }
                if (_transform.GetComponent<ArcherBehavior>().currentState == AnimatorState.Idle)
                    CheckMovement(destination, "Run2", "Run2 1", 0.2f);

            }
        }
    }

    private void MoveWaypointToNewPosition()
    {
        if (_waypoint == null) return;

        int childCount = _waypoint.childCount;
        if (childCount == 0) return;

        // Random 1 child trong waypoint
        int randomIndex = Random.Range(0, childCount);
        Transform randomChild = _waypoint.GetChild(randomIndex);

        // Set vị trí của object đến child
        destination = randomChild.position;

    }


    private void ChooseNextState(int choice = -1)
    {
        if (choice == -1)
            choice = Random.Range(0, 2);// 0 = Idle, 1 = Walk

        if (choice == 0) // Idle
        {
            CheckMovement(_waypoint.position, "Idle 1", "Idle 0", 0.2f);
            _waitTime = Random.Range(5f, 12f); // Idle lâu
        }
        else // Walk
        {
            CheckMovement(_waypoint.position, "Run2 1", "Run2");
            _waitTime = 1f;
            // Ngay khi chọn Walk thì random vị trí waypoint mới
            MoveWaypointToNewPosition();
        }
    }
}
