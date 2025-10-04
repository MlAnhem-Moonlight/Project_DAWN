using UnityEngine;
using BehaviorTree;

public class SpearAttack : Nodes
{
    private float _range;
    private Transform _transform;
    private Animator _animator;


    public SpearAttack(float range, Transform transform)
    {
        _range = range;
        _transform = transform;
        _animator = transform.GetComponent<Animator>();
    }

    //node Running khi có enemy trong tầm (parent.GetData("target") != null) và chưa kết thúc animation
    //node Success khi kết thúc animation
    //node Failure khi không có enemy trong tầm
}
