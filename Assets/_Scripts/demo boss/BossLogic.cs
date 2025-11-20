// using Unity.Entities;
// using Unity.Burst;

// // 1. DATA: Chỉ số của Boss
// public struct BossStats : IComponentData
// {
//     public float MaxHP;
//     public float CurrentHP;
// }

// [BurstCompile]
// public partial struct BossDamageSystem : ISystem
// {
//     [BurstCompile]
//     public void OnUpdate(ref SystemState state)
//     {
//         float dt = SystemAPI.Time.DeltaTime;
        
//         var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
//         var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

//         foreach (var (stats, entity) in SystemAPI.Query<RefRW<BossStats>>().WithEntityAccess())
//         {
//             stats.ValueRW.CurrentHP -= 10f * dt;

//             if (stats.ValueRW.CurrentHP <= 0)
//             {
//                 ecb.DestroyEntity(entity);
//             }
//         }
//     }
// }

using Unity.Entities;
using Unity.Burst;
using System.Diagnostics;

// 1. DATA (Giữ nguyên)
public struct BossStats : IComponentData
{
    public float MaxHP;
    public float CurrentHP;
}

// 2. SYSTEM (Người quản lý)
[BurstCompile]
public partial struct BossDamageSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        
        // BƯỚC 1: Lấy Sổ ghi lệnh (ECB)
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        
        // QUAN TRỌNG: Phải chuyển thành .AsParallelWriter() để nhiều luồng cùng ghi được
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        // BƯỚC 2: Lên lịch Job (Schedule)
        new BossDamageJob
        {
            DeltaTime = dt,
            Ecb = ecb
        }.ScheduleParallel(); // Chạy song song trên nhiều lõi CPU
    }
}

// 3. JOB (Công nhân xử lý)
[BurstCompile]
public partial struct BossDamageJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter Ecb; 

    void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref BossStats stats)
    {
        stats.CurrentHP -= 10f * DeltaTime;
        if (stats.CurrentHP <= 0)
        {
            Ecb.DestroyEntity(sortKey, entity);
            // Debug.WriteLine("Boss defeated!");
        }
    }
}