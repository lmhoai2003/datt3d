using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public Slider HealthSlider;
    public GameObject Container; // Để ẩn hiện cả thanh máu

    void Update()
    {
        if (GameUIBridge.Instance == null) return;

        bool hasBoss = GameUIBridge.Instance.HasBoss;
        if (Container.activeSelf != hasBoss)
        {
            Container.SetActive(hasBoss);
        }

        if (hasBoss)
        {
            float current = GameUIBridge.Instance.BossHP_Current;
            float max = GameUIBridge.Instance.BossHP_Max;

            HealthSlider.value = current / max;
        }
    }
}