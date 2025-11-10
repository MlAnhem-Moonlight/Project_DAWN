using UnityEngine;
using System.Collections.Generic;

public class WaterTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _waterMask;
    [SerializeField] private GameObject _splashParticles;

    private EdgeCollider2D _edgeCollider;
    private InteractableWater _interactableWater;

    // Danh sách các object đã splash
    private HashSet<GameObject> _splashedObjects = new HashSet<GameObject>();

    private void Awake()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _interactableWater = GetComponent<InteractableWater>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu object đã splash rồi => bỏ qua
        if (_splashedObjects.Contains(collision.gameObject))
            return;

        if ((_waterMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            Rigidbody2D rb = collision.GetComponentInParent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 localPos = transform.localPosition;
                Vector2 hitObjectPos = collision.transform.position;
                Bounds hitObjectBounds = collision.bounds;

                Vector3 spawnPos = Vector3.zero;
                if (collision.transform.position.y >= _edgeCollider.points[1].y + _edgeCollider.offset.y + localPos.y)
                {
                    spawnPos = hitObjectPos - new Vector2(0f, hitObjectBounds.extents.y);
                }
                else
                {
                    spawnPos = hitObjectPos + new Vector2(0f, hitObjectBounds.extents.y);
                }

                Instantiate(_splashParticles, spawnPos, Quaternion.identity);

                int multiplier = rb.linearVelocity.y < 0 ? -1 : 1;

                float vel = rb.linearVelocity.y * _interactableWater.forceMultiplier;
                vel = Mathf.Clamp(Mathf.Abs(vel), 0f, _interactableWater.maxForce);
                vel *= multiplier;

                _interactableWater.Splash(collision, vel);

                // Ghi nhận object này đã splash
                _splashedObjects.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi object ra khỏi nước -> reset để lần sau có thể splash lại
        if (_splashedObjects.Contains(collision.gameObject))
        {
            _splashedObjects.Remove(collision.gameObject);
        }
    }
}
