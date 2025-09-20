﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{

    public abstract class BhTree : MonoBehaviour
    {
        private Nodes _root = null;

        protected void Start()
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
    }

}

