using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

// 1. DATA
public struct EnemyStats : IComponentData
{
    public float MoveSpeed;
    public float LifeTime;
}

// Đây là Component dạng Class (Managed), chứa tham chiếu đến UI thật
public class HealthBarData : IComponentData
{
    public GameObject BarInstance; // thanh máu
    public GameObject BarPrefab;   // Cái khuôn để tạo ra nố
}

// 2. AUTHORING (ĐÂY LÀ CÁI TÊN BẠN PHẢI TÌM TRONG ADD COMPONENT)
public class EnemyAuthoring : MonoBehaviour
{
    public float Speed = 5f;
    public float LifeTime = 3f;
    public GameObject HealthBarPrefab;
    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemyStats
            {
                MoveSpeed = authoring.Speed,
                LifeTime = authoring.LifeTime
            });

            // Thêm Data UI (MỚI) - Dùng AddComponentObject cho Class
            AddComponentObject(entity, new HealthBarData
            {
                BarPrefab = authoring.HealthBarPrefab,
                BarInstance = null
            });
        }
    }
}

// 3. SYSTEM
[BurstCompile]
public partial struct EnemySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        new EnemyJob
        {
            DeltaTime = dt,
            Ecb = ecb
        }.ScheduleParallel();
    }
}

// 4. JOB
[BurstCompile]
public partial struct EnemyJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter Ecb;

    void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref LocalTransform transform, ref EnemyStats stats)
    {
        transform.Position.y -= stats.MoveSpeed * DeltaTime; // tạo đất ảo cho quái rơi xuống
        float groundLevel = 0f;
        if (transform.Position.y < groundLevel)
        {
            transform.Position.y = groundLevel;
        }

        stats.LifeTime -= DeltaTime;

        // if (stats.LifeTime <= 0)
        // {
        //     Ecb.DestroyEntity(sortKey, entity);
        // }
    }


}