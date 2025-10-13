using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{

    public abstract class BhTree : MonoBehaviour
    {
        public Nodes _root = null;

        protected virtual void Start()
        {
            _root = SetupTree();
        }

        private void Update()
        {
            if (_root != null) _root.Evaluate();
        }
        protected abstract Nodes SetupTree();

        public void SetupAttackSpeed(Animator animator, float attackSpeed)
        {
            float attackInterval = 1f / attackSpeed;
            // Lấy độ dài clip gốc
            float clipLength = 1f;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "Attack") // đúng tên clip
                {
                    clipLength = clip.length;
                    Debug.Log($"Found Attack clip length: {clipLength}s");
                    break;
                }
            }

            // Công thức: cần tốc độ gấp clipLength/attackInterval
            float attackSpeedMultiplier = clipLength / attackInterval;

            // Gán vào parameter thay vì animator.speed
            animator.SetFloat("AttackSpd", attackSpeedMultiplier);

            //Debug.Log($"ClipLength={clipLength:F2}s, AttackInterval={attackInterval:F2}s, " +
            //          $"AttackSpeedMul={attackSpeedMultiplier:F2}");
        }

        public void SetupAnimatorSpeed(Animator animator, float attackSpeed, string clipName)
        {
            float attackInterval = 1f / attackSpeed;
            // Lấy độ dài clip gốc
            float clipLength = 1f;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName) // đúng tên clip
                {
                    clipLength = clip.length;
                    Debug.Log($"Found {clipName} clip length: {clipLength}s");
                    break;
                }
            }

            // Công thức: cần tốc độ gấp clipLength/attackInterval
            float attackSpeedMultiplier = clipLength / attackInterval;

            // Gán vào parameter thay vì animator.speed
            animator.SetFloat(clipName + "Spd", attackSpeedMultiplier);

            //Debug.Log($"ClipLength={clipLength:F2}s, AttackInterval={attackInterval:F2}s, " +
            //          $"AttackSpeedMul={attackSpeedMultiplier:F2}");
        }

        public void SetupAnimatorSpeedDirect(Animator animator, float attackSpeed, string clipName, string parameter)
        {
            float attackInterval = 1f / attackSpeed;
            // Lấy độ dài clip gốc
            float clipLength = 1f;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName) // đúng tên clip
                {
                    clipLength = clip.length;
                    Debug.Log($"Found {clipName} clip length: {clipLength}s");
                    break;
                }
            }

            // Công thức: cần tốc độ gấp clipLength/attackInterval
            float attackSpeedMultiplier = clipLength / attackInterval;

            // Gán vào parameter thay vì animator.speed
            animator.SetFloat(parameter, attackSpeedMultiplier);

            //Debug.Log($"ClipLength={clipLength:F2}s, AttackInterval={attackInterval:F2}s, " +
            //          $"AttackSpeedMul={attackSpeedMultiplier:F2}");
        }
    }

}

