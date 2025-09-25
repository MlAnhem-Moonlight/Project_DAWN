using UnityEngine;

/// <summary>
/// Tách các unit layer "Demon" để tránh chồng chéo nhau (Separation Steering).
/// Khoảng cách kiểm tra thay đổi theo LOD dựa vào khoảng cách tới Camera.
/// Gắn script này cho mỗi Demon unit có Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SeparationSteering : MonoBehaviour
{
    [Header("LOD Settings")]
    [Tooltip("Khoảng cách camera gần nhất (để tính LOD).")]
    public float minCameraDistance = 5f;

    [Tooltip("Khoảng cách camera xa nhất (để tính LOD).")]
    public float maxCameraDistance = 30f;

    [Tooltip("Bán kính tách khi gần camera.")]
    public float minCheckRadius = 1f;

    [Tooltip("Bán kính tách khi xa camera.")]
    public float maxCheckRadius = 5f;

    [Header("Steering Settings")]
    [Tooltip("Lực đẩy tách.")]
    public float separationStrength = 3f;

    [Tooltip("Khoảng cách lý tưởng muốn giữ giữa các Demon.")]
    public float desiredSeparation = 0.8f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Tính bán kính kiểm tra theo LOD
        float cameraDist = Vector3.Distance(Camera.main.transform.position, transform.position);
        float t = Mathf.InverseLerp(minCameraDistance, maxCameraDistance, cameraDist);
        float checkRadius = Mathf.Lerp(minCheckRadius, maxCheckRadius, t);

        // Quét các collider Demon xung quanh
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);
        Vector2 separationForce = Vector2.zero;
        int count = 0;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Chỉ tách đối tượng cùng layer "Demon"
            if (hit.gameObject.layer == gameObject.layer && hit.gameObject.GetComponent<Rigidbody2D>())
            {
                Vector2 diff = (Vector2)(transform.position - hit.transform.position);
                float distance = diff.magnitude;

                if (distance > 0 && distance < desiredSeparation)
                {
                    //Debug.Log($"Separation applied between {gameObject.name} and {hit.gameObject.name}");
                    // Càng gần càng đẩy mạnh
                    diff = diff.normalized / distance;
                    separationForce += diff;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separationForce /= count;
            // Áp dụng lực steering
            rb.AddForce(separationForce * separationStrength);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn kiểm tra trong Scene View (sử dụng bán kính hiện tại nếu đang chạy)
        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            float cameraDist = Vector3.Distance(Camera.main.transform.position, transform.position);
            float t = Mathf.InverseLerp(minCameraDistance, maxCameraDistance, cameraDist);
            float checkRadius = Mathf.Lerp(minCheckRadius, maxCheckRadius, t);
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, minCheckRadius);
        }
    }
#endif
}
