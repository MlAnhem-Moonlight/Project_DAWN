using System.Collections.Generic;
using UnityEngine;

public static class TargetSelector
{
    /// <summary>
    /// Tìm target gần nhất, EXCLUDE default target
    /// </summary>
    public static Transform GetClosestTarget(Transform origin, float range, string layerHuman, string layerConstruction)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin.position, range);
        
        //Debug.Log($"[TargetSelector] OverlapCircle found {hitColliders.Length} colliders at {origin.name}, range={range}");

        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.activeInHierarchy)
                continue;

            int hitLayer = hitCollider.gameObject.layer;
            int layerHumanInt = LayerMask.NameToLayer(layerHuman);
            int layerConstructionInt = LayerMask.NameToLayer(layerConstruction);

            // ✅ Chỉ detect Human layer (exclude Construction/DefaultTarget)
            if (hitLayer != layerHumanInt)
                continue;

            Stats stats = hitCollider.gameObject.GetComponent<Stats>();

            if (stats == null)
            {
             //   Debug.Log($"    ⚠️ {hitCollider.name} không có Stats component!");
                continue;
            }

            if (stats.currentHP <= 0)
            {
             //   Debug.Log($"    ⚠️ {hitCollider.name} đã chết (HP={stats.currentHP})");
                continue;
            }

            float distance = Vector3.Distance(origin.position, hitCollider.transform.position);
            //Debug.Log($"    ✅ {hitCollider.name} hợp lệ (Distance: {distance:F2})");

            if (distance < closestDistance)
            {
                closestTarget = hitCollider.transform;
                closestDistance = distance;
            }
        }

        //Debug.Log($"Closest Target: {(closestTarget != null ? closestTarget.name : "None")}");
        return closestTarget;
    }

    /// <summary>
    /// Overload: Có default target parameter
    /// </summary>
    public static Transform GetClosestTarget(Transform origin, float range, string layerHuman, string layerConstruction, Transform defaultTarget)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin.position, range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.gameObject.activeInHierarchy)
                continue;

            if (hitCollider.gameObject.layer == LayerMask.NameToLayer(layerHuman) ||
                hitCollider.gameObject.layer == LayerMask.NameToLayer(layerConstruction))
            {
                Stats stats = hitCollider.gameObject.GetComponent<Stats>();

                if (stats != null && stats.currentHP > 0)
                {
                    float distance = Vector3.Distance(origin.position, hitCollider.transform.position);
                    if (hitCollider.transform == defaultTarget)
                    {
                        closestTarget = defaultTarget;
                        break;
                    }
                    else if (distance < closestDistance)
                    {
                        closestTarget = hitCollider.transform;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestTarget ?? defaultTarget;
    }
}
