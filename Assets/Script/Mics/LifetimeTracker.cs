using UnityEngine;

public class LifetimeTracker : MonoBehaviour
{
    private float _spawnTime;    // Thời điểm GameObject được tạo ra
    private float _lifeTime;     // Tổng thời gian tồn tại tính đến hiện tại
    private bool _isTracking = true;

    // Bạn có thể gắn ID hoặc loại unit nếu cần
    [Header("Thông tin thống kê")]
    public string unitName = "UnknownUnit";
    public bool logOnDestroy = true; // In log khi bị hủy

    void Start()
    {
        _spawnTime = Time.time;
    }

    void Update()
    {
        if (_isTracking)
        {
            _lifeTime = Time.time - _spawnTime;
        }
    }

    // Dừng theo dõi tạm thời (nếu muốn)
    public void PauseTracking() => _isTracking = false;
    public void ResumeTracking() => _isTracking = true;

    // Trả về thời gian sống hiện tại
    public float GetLifeTime() => _lifeTime;

    // Khi GameObject bị destroy
    private void OnDisable()
    {
        if (logOnDestroy)
        {
            Debug.Log($"🕒 {unitName} đã tồn tại trong {_lifeTime:F2} giây.");
        }
    }
}
