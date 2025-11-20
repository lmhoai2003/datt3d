using Unity.Entities;

// Phải dùng SystemBase vì cần truy cập vào biến static của MonoBehaviour (Bridge)
public partial class BossUISyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Kiểm tra xem Cầu Nối có tồn tại không
        if (GameUIBridge.Instance == null) return;

        // Cố gắng tìm con Boss duy nhất trong ECS (Singleton Entity)
        if (SystemAPI.TryGetSingleton<BossStats>(out BossStats bossStats))
        {
            // TÌM THẤY BOSS -> Bắn dữ liệu sang cầu nối
            GameUIBridge.Instance.HasBoss = true;
            GameUIBridge.Instance.BossHP_Current = bossStats.CurrentHP;
            GameUIBridge.Instance.BossHP_Max = bossStats.MaxHP;
        }
        else
        {
            // KHÔNG THẤY BOSS
            GameUIBridge.Instance.HasBoss = false;
            GameUIBridge.Instance.BossHP_Current = 0;
        }
    }
}