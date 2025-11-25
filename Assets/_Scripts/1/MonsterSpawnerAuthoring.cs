using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class MonsterSpawnerAuthoring : MonoBehaviour
{
    public GameObject MonsterPrefab; 
    public int Count = 100;          
    public float Radius = 20f;      

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

[Unity.Burst.BurstCompile]
public partial struct MonsterSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        // --- DÒNG QUAN TRỌNG MỚI THÊM ---
        // Yêu cầu: Phải tìm thấy ít nhất 1 cái MonsterSpawnerComponent thì mới được chạy OnUpdate
        // Nếu SubScene chưa load xong -> System sẽ kiên nhẫn chờ.
        state.RequireForUpdate<MonsterSpawnerComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // Khi code chạy vào đây nghĩa là ĐÃ tìm thấy Spawner rồi.
        
        // Tắt system ngay để chỉ spawn 1 lần duy nhất
        state.Enabled = false;

        foreach (var spawner in SystemAPI.Query<RefRW<MonsterSpawnerComponent>>())
        {
            // Reset Random Seed mỗi lần chơi lại để vị trí quái thay đổi khác đi
            // Dùng thời gian hiện tại làm seed
            spawner.ValueRW.RandomSeed = Random.CreateFromIndex((uint)System.DateTime.Now.Millisecond);

            for (int i = 0; i < spawner.ValueRO.Count; i++)
            {
                var newMonster = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                
                // Random vị trí
                float2 randCircle = spawner.ValueRW.RandomSeed.NextFloat2Direction() * spawner.ValueRW.RandomSeed.NextFloat(0, spawner.ValueRO.Radius);
                
                state.EntityManager.SetComponentData(newMonster, Unity.Transforms.LocalTransform.FromPosition(new float3(randCircle.x, 0, randCircle.y)));
            }
        }
    }
}