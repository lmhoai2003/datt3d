using Unity.Entities;

public partial class BossUISyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (GameUIBridge.Instance == null) return;

        // --- DÒNG MỚI THÊM VÀO ---
        // Ý nghĩa: "Đợi tất cả Job nào đang viết vào BossStats chạy xong đi, rồi tao mới đọc".
        EntityManager.CompleteDependencyBeforeRO<BossStats>(); 
        // -------------------------

        // Bây giờ đọc mới an toàn
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