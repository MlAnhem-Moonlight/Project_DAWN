using System.Collections.Generic;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    [Header("Danh sách tất cả panel cần quản lý")]
    public List<GameObject> panels = new List<GameObject>();

    /// <summary>
    /// Bật panel được chọn và ẩn toàn bộ panel khác.
    /// Gắn vào sự kiện OnClick() của Button, truyền panel cần bật vào.
    /// </summary>
    public void ShowOnly(GameObject panelToShow)
    {
        foreach (var panel in panels)
        {
            if (panel == null) continue;
            panel.SetActive(panel == panelToShow);
        }

        // Dừng thời gian game (nếu là menu tạm dừng)
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Tắt tất cả menu, quay lại gameplay.
    /// </summary>
    public void ResumeGame()
    {
        foreach (var panel in panels)
        {
            if (panel == null) continue;
            panel.SetActive(false);
        }

        // Tiếp tục thời gian game
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Thoát khỏi game (hoạt động khác nhau giữa Editor và Build).
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
