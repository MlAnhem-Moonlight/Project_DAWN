using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;              // Tốc độ bay
    public float lifeTime = 5f;            // Tự hủy nếu không va

    [Header("Vị trí và hướng ban đầu")]
    public Transform _startPosition; // Có thể set trong Inspector hoặc code
    public Quaternion _startRotation;


    [Header("Hướng bay (-1 = Trái, 1 = Phải)")]
    [Range(-1f, 1f)]
    public int direction = 1;   // chỉ nên set -1 hoặc 1

    public Animator animator;
    public string animationClipName = "Attack";

    private void OnEnable()
    {
        // Khi bật lại, đưa về vị trí gốc và hướng gốc
        transform.position = _startPosition.position;
        transform.rotation = Quaternion.Euler(
            _startRotation.eulerAngles.x,
            _startRotation.eulerAngles.y,
            _startRotation.eulerAngles.z * direction
        );
        Invoke(nameof(DisableProjectile), lifeTime);
        direction = animator.GetFloat(animationClipName) >= 0 ? 1 : -1;

        // Có thể thêm direction = transform.forward nếu cần.
    }

    private void Start()
    {
        // Nếu sau lifeTime không trúng gì, tự ẩn
        Invoke(nameof(DisableProjectile), lifeTime);
    }

    private void Update()
    { 
        // Bảo đảm direction luôn chỉ là -1 hoặc 1
        //direction = animator.GetFloat(animationClipName) >= 0 ? 1 : -1;
        int dir = direction >= 0 ? 1 : -1;

        // Di chuyển thẳng trên trục X
        transform.position += Vector3.right * dir * speed * Time.deltaTime;
    }


    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra layer bằng tên
        if (other.gameObject.layer == LayerMask.NameToLayer("Human") ||
            other.gameObject.layer == LayerMask.NameToLayer("Construction"))
        {
            DisableProjectile();
        }
    }

    public void DisableProjectile()
    {
        // Hủy Invoke để không gọi lại
        CancelInvoke(nameof(DisableProjectile));
        // Tắt GameObject (có thể dùng object pool)
        gameObject.SetActive(false);
        // Reset vị trí khi bật lại sẽ được OnEnable xử lý
    }
}
