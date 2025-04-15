using UnityEngine;

public class BridCollisionHandler : MonoBehaviour
{
    public System.Action OnBridCollision; // Sự kiện va chạm

    private void OnCollisionEnter2D(Collision2D collision)
    {
            OnBridCollision?.Invoke(); // Gửi tín hiệu đến BridDive
    }
}
