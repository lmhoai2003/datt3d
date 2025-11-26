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
                                            .WithAll<PlayerShootInput>() 
                                            .WithEntityAccess())
        {
            // A. Sinh đạn
            var newBullet = ecb.Instantiate(bulletPrefab);

            // B. Đặt vị trí (Trước mặt 1m, cao 1.2m)
            float3 spawnPos = transform.ValueRO.Position 
                            + (transform.ValueRO.Forward() * 1.0f) 
                            + (transform.ValueRO.Right() * 0.2f); // <--- LỆCH PHẢI 0.2f
            
            spawnPos.y += 1.2f;

            // --- [SỬA ĐOẠN NÀY ĐỂ CHỈNH CỠ ĐẠN] ---
            
            // 1. Tạo transform tạm thời với vị trí và góc xoay
            var bulletTrans = LocalTransform.FromPositionRotation(spawnPos, transform.ValueRO.Rotation);
            
            // 2. Ép kích thước nhỏ lại (Ví dụ: 0.2f = 20% kích thước gốc)
            bulletTrans.Scale = 0.5f; 

            // 3. Gán vào viên đạn
            ecb.SetComponent(newBullet, bulletTrans);
            // --------------------------------------

            // C. Xóa tín hiệu bắn
            ecb.RemoveComponent<PlayerShootInput>(entity);
        }
    }
}