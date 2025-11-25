using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine; 

[BurstCompile]
public partial struct PlayerShootingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. Kiểm tra xem Kho đạn đã được tạo chưa
        if (!SystemAPI.HasSingleton<PlayerBulletConfig>()) return;

        // 2. Lấy Prefab đạn từ Kho
        Entity bulletPrefab = SystemAPI.GetSingleton<PlayerBulletConfig>().BulletPrefab;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // 3. Tìm Player nào đang bấm nút (Có tag PlayerShootInput)
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                                            .WithAll<PlayerShootInput>() // Chỉ lấy ai đang bấm nút
                                            .WithEntityAccess())
        {
            // Debug.Log("System: Bùm! Đang sinh đạn..."); 

            // A. Sinh đạn
            var newBullet = ecb.Instantiate(bulletPrefab);

            // B. Đặt vị trí (Trước mặt 1m, cao 1m)
            float3 spawnPos = transform.ValueRO.Position + transform.ValueRO.Forward() * 1.0f;
            spawnPos.y += 1.0f;

            ecb.SetComponent(newBullet, LocalTransform.FromPositionRotation(spawnPos, transform.ValueRO.Rotation));

            // C. Xóa tín hiệu bắn (để không bắn liên thanh)
            ecb.RemoveComponent<PlayerShootInput>(entity);
        }
    }
}