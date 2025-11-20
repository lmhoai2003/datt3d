using Unity.Entities;

// Phải dùng SystemBase vì cần truy cập vào biến static của MonoBehaviour (Bridge)
public partial class BossUISyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (GameUIBridge.Instance == null) return;

        if (SystemAPI.TryGetSingleton<BossStats>(out BossStats bossStats))
        {
            GameUIBridge.Instance.HasBoss = true;
            GameUIBridge.Instance.BossHP_Current = bossStats.CurrentHP;
            GameUIBridge.Instance.BossHP_Max = bossStats.MaxHP;
        }
        else
        {
            GameUIBridge.Instance.HasBoss = false;
            GameUIBridge.Instance.BossHP_Current = 0;
        }
    }
}