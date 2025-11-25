using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class MonsterSpawnerAuthoring : MonoBehaviour
{
    public GameObject MonsterPrefab; // Kéo Prefab Monster vào đây
    public int Count = 100;          // Số lượng: 100
    public float Radius = 20f;       // Phạm vi sinh

    class Baker : Baker<MonsterSpawnerAuthoring>
    {
        public override void Bake(MonsterSpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new MonsterSpawnerComponent
            {
                Prefab = GetEntity(authoring.MonsterPrefab, TransformUsageFlags.Dynamic),
                Count = authoring.Count,
                Radius = authoring.Radius,
                RandomSeed = Random.CreateFromIndex(1)
            });
        }
    }
}

// Data Component cho Spawner
public struct MonsterSpawnerComponent : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float Radius;
    public Random RandomSeed;
}

// System để chạy việc sinh quái (chỉ chạy 1 lần lúc đầu)
[Unity.Burst.BurstCompile] // Thêm dòng này để tối ưu tốc độ spawn
public partial struct MonsterSpawnerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // Tắt system sau lần chạy đầu tiên để không spawn liên tục
        state.Enabled = false;

        foreach (var spawner in SystemAPI.Query<RefRW<MonsterSpawnerComponent>>())
        {
            for (int i = 0; i < spawner.ValueRO.Count; i++)
            {
                // 1. Sinh ra entity
                var newMonster = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);

                // 2. Random vị trí
                float2 randCircle = spawner.ValueRW.RandomSeed.NextFloat2Direction() * spawner.ValueRW.RandomSeed.NextFloat(0, spawner.ValueRO.Radius);
                
                float3 position = new float3(randCircle.x, 0, randCircle.y);

                // 3. Đặt vị trí
                state.EntityManager.SetComponentData(newMonster, Unity.Transforms.LocalTransform.FromPosition(position));
            }
        }
    }
}