using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using Unity.Transforms;

// 1. DATA
public struct SpawnerData : IComponentData
{
    public Entity PrefabToSpawn;
    public int Count;
}

// 2. AUTHORING
public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab; 
    public int Count = 1000;  

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SpawnerData
            {
                PrefabToSpawn = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Count = authoring.Count
            });
        }
    }
}

// 3. SYSTEM: Sinh quái 1 lần duy nhất
[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false; 
        var spawner = SystemAPI.GetSingleton<SpawnerData>();
        
        // Tạo mảng Entity
        var instances = state.EntityManager.Instantiate(spawner.PrefabToSpawn, spawner.Count, Allocator.Temp);
        
        // Random vị trí cho vui mắt
        var rand = new Random(123);
        foreach (var entity in instances)
        {
            var pos = new float3(rand.NextFloat(-50, 50), rand.NextFloat(10, 50), rand.NextFloat(-50, 50));
            
            // SỬA Ở ĐÂY: Tạo Transform với vị trí, nhưng chỉnh Scale nhỏ lại
            var transform = LocalTransform.FromPosition(pos);
            transform.Scale = 0.02f; 
            
            state.EntityManager.SetComponentData(entity, transform);
        }
    }
    
}