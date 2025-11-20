using Unity.Entities;
using Unity.Burst;

// 1. DATA: Chỉ số của Boss
public struct BossStats : IComponentData
{
    public float MaxHP;
    public float CurrentHP;
}

[BurstCompile]
public partial struct BossDamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        
        // Lấy Sổ ghi lệnh (ECB)
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (stats, entity) in SystemAPI.Query<RefRW<BossStats>>().WithEntityAccess())
        {
            // Trừ máu cực nhanh (50 máu/giây) để test cho lẹ
            stats.ValueRW.CurrentHP -= 10f * dt;

            // Kiểm tra chết
            if (stats.ValueRW.CurrentHP <= 0)
            {
                // XÓA SỔ BOSS
                ecb.DestroyEntity(entity);
            }
        }
    }
}