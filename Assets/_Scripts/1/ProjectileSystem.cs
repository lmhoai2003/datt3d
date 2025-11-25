using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine; // Dùng cho Debug.Log

[BurstCompile]
public partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        // ------------------------------------------------------------------
        // PHẦN 1: CHUẨN BỊ DỮ LIỆU PLAYER (Để đạn quái biết đường bắn)
        // ------------------------------------------------------------------
        float3 playerPos = float3.zero;
        Entity playerEntity = Entity.Null;
        bool playerFound = false;

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                                            .WithEntityAccess()
                                            .WithAll<PlayerTag>()
                                            .WithNone<DeadTag>()) // Không tìm nếu Player chết
        {
            playerPos = transform.ValueRO.Position;
            playerEntity = entity;
            playerFound = true;
            break;
        }

        // ------------------------------------------------------------------
        // PHẦN 2: XỬ LÝ RIÊNG CHO ĐẠN CỦA PLAYER (HOMING + VA CHẠM QUÁI)
        // ------------------------------------------------------------------
        // Chỉ lấy những viên đạn có 'PlayerProjectileTag'
        foreach (var (trans, data, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ProjectileData>>()
                                                 .WithEntityAccess()
                                                 .WithAll<ProjectileTag, PlayerProjectileTag>())
        {
            // A. LOGIC HOMING (Tự tìm quái)
            float3 bulletPos = trans.ValueRO.Position;
            float3 targetPos = float3.zero;
            bool foundTarget = false;
            float minDist = 15f; // Tầm tìm mục tiêu

            // Quét tìm quái gần nhất
            foreach (var (mTrans, mHealth) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MonsterHealth>>()
                                                       .WithAll<MonsterTag>()
                                                       .WithNone<DeadTag>())
            {
                float d = math.distance(bulletPos, mTrans.ValueRO.Position);
                if (d < minDist)
                {
                    minDist = d;
                    targetPos = mTrans.ValueRO.Position;
                    targetPos.y += 1.0f; // Nhắm vào ngực
                    foundTarget = true;
                }
            }

            // Bẻ lái
            if (foundTarget)
            {
                float3 dirToTarget = math.normalize(targetPos - bulletPos);
                quaternion targetRot = quaternion.LookRotation(dirToTarget, math.up());
                trans.ValueRW.Rotation = math.slerp(trans.ValueRO.Rotation, targetRot, 10f * dt);
            }

            // B. DI CHUYỂN
            trans.ValueRW.Position += trans.ValueRO.Forward() * data.ValueRO.Speed * dt;

            // C. VA CHẠM VỚI QUÁI (Logic va chạm riêng)
            if (foundTarget && minDist < 1.5f) // Nếu gần quái < 1.5m
            {
                // Tìm lại quái đó để trừ máu (Đoạn này làm tắt cho gọn)
                foreach (var (mTrans, mHealth, mEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<MonsterHealth>>().WithEntityAccess().WithAll<MonsterTag>())
                {
                    if (math.distance(bulletPos, mTrans.ValueRO.Position) < 1.5f)
                    {
                        mHealth.ValueRW.Current -= 20f;
                        mHealth.ValueRW.IsHit = true;
                        break; // Trúng 1 con thôi
                    }
                }
                ecb.DestroyEntity(entity); // Hủy đạn
                continue; // Xong viên này
            }

            // D. TUỔI THỌ
            data.ValueRW.LifeTime -= dt;
            if (data.ValueRW.LifeTime <= 0) ecb.DestroyEntity(entity);
        }

        // ------------------------------------------------------------------
        // PHẦN 3: XỬ LÝ CÁC LOẠI ĐẠN CÒN LẠI (ĐẠN QUÁI / ĐẠN THƯỜNG)
        // ------------------------------------------------------------------
        // Query tất cả ProjectileTag NHƯNG loại trừ PlayerProjectileTag ra
        foreach (var (trans, data, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<ProjectileData>>()
                                                 .WithEntityAccess()
                                                 .WithAll<ProjectileTag>()
                                                 .WithNone<PlayerProjectileTag>()) // <--- QUAN TRỌNG: Không xử lý lại đạn Player
        {
            // A. DI CHUYỂN (Bay thẳng)
            trans.ValueRW.Position += trans.ValueRO.Forward() * data.ValueRO.Speed * dt;

            // B. VA CHẠM VỚI PLAYER (Logic cũ của bạn)
            if (playerFound)
            {
                float distanceToPlayer = math.distance(trans.ValueRO.Position, playerPos);
                if (distanceToPlayer < 1.0f) // Nếu trúng Player
                {
                    if (SystemAPI.HasComponent<PlayerHealthComponent>(playerEntity))
                    {
                        var hp = SystemAPI.GetComponent<PlayerHealthComponent>(playerEntity);
                        hp.CurrentHealth -= 10f;
                        hp.IsHit = true;
                        SystemAPI.SetComponent(playerEntity, hp);
                    }
                    ecb.DestroyEntity(entity);
                    continue;
                }
            }

            // C. TUỔI THỌ
            data.ValueRW.LifeTime -= dt;
            if (data.ValueRW.LifeTime <= 0) ecb.DestroyEntity(entity);
        }
    }
}