using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerHarvestController : MonoBehaviour
{
    [Header("Selection")]
    public float interactionRadius = 1.5f;

    [Header("Highlight Settings")]
    public Material highlightMaterial;

    [Header("References")]
    public IngredientUI ingredientUI;

    private List<GameObject> nearbyTargets = new List<GameObject>();
    private int currentIndex = -1;

    private GameObject currentTarget;
    private Material originalMaterial;
    private SpriteRenderer targetRenderer;

    void Update()
    {
        UpdateNearbyTargets();
        HandleTargetSwitch();
    }

    void UpdateNearbyTargets()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        nearbyTargets.Clear();

        foreach (var hit in hits)
        {
            int layer = hit.gameObject.layer;
            if (layer == LayerMask.NameToLayer("Deer") ||
                layer == LayerMask.NameToLayer("Wolf"))
                continue;

            if (hit.GetComponent<Ingredient>() != null)
                nearbyTargets.Add(hit.gameObject);
        }

        if (currentTarget != null && !nearbyTargets.Contains(currentTarget))
            ClearSelection();
    }

    void HandleTargetSwitch()
    {
        if (nearbyTargets.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentIndex = (currentIndex + 1) % nearbyTargets.Count;
            SelectTarget(nearbyTargets[currentIndex]);
        }
        else if (currentTarget == null && nearbyTargets.Count > 0)
        {
            currentIndex = 0;
            SelectTarget(nearbyTargets[currentIndex]);
        }
    }

    void SelectTarget(GameObject newTarget)
    {
        if (currentTarget != null)
            ResetHighlight(currentTarget);

        currentTarget = newTarget;
        ApplyHighlight(currentTarget);

        if (ingredientUI != null)
            ingredientUI.ShowIngredients(currentTarget);
    }

    void ClearSelection()
    {
        if (currentTarget != null)
        {
            ResetHighlight(currentTarget);
            currentTarget = null;
            currentIndex = -1;
        }

        if (ingredientUI != null)
            ingredientUI.Clear();
    }

    void ApplyHighlight(GameObject target)
    {
        targetRenderer = target.GetComponent<SpriteRenderer>();
        if (targetRenderer != null && highlightMaterial != null)
        {
            originalMaterial = targetRenderer.material;
            targetRenderer.material = highlightMaterial;
        }
    }

    void ResetHighlight(GameObject target)
    {
        var renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null && originalMaterial != null)
            renderer.material = originalMaterial;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
