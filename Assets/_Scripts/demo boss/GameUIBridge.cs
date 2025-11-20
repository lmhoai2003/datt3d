using UnityEngine;

public class GameUIBridge : MonoBehaviour
{
    public static GameUIBridge Instance;
    [Header("Dữ liệu nhận từ ECS")]
    public float BossHP_Current;
    public float BossHP_Max;
    public bool HasBoss; 

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