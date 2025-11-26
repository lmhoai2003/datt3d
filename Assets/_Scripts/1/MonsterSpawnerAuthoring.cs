using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class MonsterSpawnerAuthoring : MonoBehaviour
{
    public GameObject MonsterPrefab; 
    public int Count = 50;           // (Ví dụ số lượng 50)
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

// [Unity.Burst.BurstCompile] <--- XÓA DÒNG NÀY ĐI VÌ TA DÙNG SYSTEM.DATETIME
public partial struct MonsterSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MonsterSpawnerComponent>();
        // Yêu cầu thêm cái này để lấy dữ liệu trạng thái
        state.RequireForUpdate<GameStateData>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var gameState = SystemAPI.GetSingleton<GameStateData>();
        
        // --- NẾU ĐANG CHỜ THÌ KHÔNG LÀM GÌ CẢ ---
        if (gameState.CurrentState == GameState.WaitingToStart) return;
        // ----------------------------------------

        // state.Enabled = false;
        // Khi code chạy vào đây nghĩa là ĐÃ tìm thấy Spawner rồi.
        
        // Tắt system ngay để chỉ spawn 1 lần duy nhất
        state.Enabled = false;

        foreach (var spawner in SystemAPI.Query<RefRW<MonsterSpawnerComponent>>())
        {
            // Reset Random Seed theo thời gian thực để vị trí quái thay đổi khác đi mỗi lần chơi
            // System.DateTime chỉ chạy được khi KHÔNG CÓ BurstCompile
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