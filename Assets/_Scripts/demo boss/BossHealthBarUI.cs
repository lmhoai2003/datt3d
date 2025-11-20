using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public Slider HealthSlider;
    public GameObject Container; // Để ẩn hiện cả thanh máu

    void Update()
    {
        // Kiểm tra cầu nối
        if (GameUIBridge.Instance == null) return;

        // 1. Ẩn/Hiện thanh máu tùy vào việc có Boss hay không
        bool hasBoss = GameUIBridge.Instance.HasBoss;
        if (Container.activeSelf != hasBoss)
        {
            Container.SetActive(hasBoss);
        }

        // 2. Nếu có Boss, cập nhật thanh trượt
        if (hasBoss)
        {
            float current = GameUIBridge.Instance.BossHP_Current;
            float max = GameUIBridge.Instance.BossHP_Max;

            // Tính phần trăm
            HealthSlider.value = current / max;
        }
    }
}