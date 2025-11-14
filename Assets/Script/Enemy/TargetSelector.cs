using System.Collections.Generic;
using UnityEngine;

public static class TargetSelector
{
    /// <summary>
    /// Finds the closest target within a specified range.
    /// </summary>
    /// <param name="origin">The origin transform to calculate distances from.</param>
    /// <param name="range">The range within which to search for targets.</param>
    /// <param name="layerHuman">The layer name for human targets.</param>
    /// <param name="layerConstruction">The layer name for construction targets.</param>
    /// <param name="defaultTarget">The default target to return if no other target is found.</param>
    /// <returns>The closest target transform or the default target if no valid target is found.</returns>
    public static Transform GetClosestTarget(Transform origin, float range, string layerHuman, string layerConstruction, Transform defaultTarget)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin.position, range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            // Kiểm tra object có active không
            if (!hitCollider.gameObject.activeInHierarchy)
                continue;

            if (hitCollider.gameObject.layer == LayerMask.NameToLayer(layerHuman) ||
                hitCollider.gameObject.layer == LayerMask.NameToLayer(layerConstruction) ||
                hitCollider.transform == defaultTarget)
            {
                Stats stats = hitCollider.gameObject.GetComponent<Stats>();

                // Kiểm tra Stats có tồn tại và HP > 0 không
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

    public static Transform GetClosestTarget(Transform origin, float range, string layerHuman, string layerConstruction)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(origin.position, range);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            // Kiểm tra object có active không
            if (!hitCollider.gameObject.activeInHierarchy)
                continue;

            if (hitCollider.gameObject.layer == LayerMask.NameToLayer(layerHuman) ||
                hitCollider.gameObject.layer == LayerMask.NameToLayer(layerConstruction))
            {
                Stats stats = hitCollider.gameObject.GetComponent<Stats>();

                // Kiểm tra Stats có tồn tại và HP > 0 không
                if (stats != null && stats.currentHP > 0)
                {
                    float distance = Vector3.Distance(origin.position, hitCollider.transform.position);

                    if (distance < closestDistance)
                    {
                        closestTarget = hitCollider.transform;
                        closestDistance = distance;
                    }
                }
            }
        }

        return closestTarget;
    }
}
