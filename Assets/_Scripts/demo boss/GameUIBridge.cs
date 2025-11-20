using UnityEngine;

public class GameUIBridge : MonoBehaviour
{
    // Singleton: Để ai cũng gọi được bằng GameUIBridge.Instance
    public static GameUIBridge Instance;

    // Dữ liệu cần hiển thị ra ngoài
    [Header("Dữ liệu nhận từ ECS")]
    public float BossHP_Current;
    public float BossHP_Max;
    public bool HasBoss; // Có Boss đang sống không?

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}