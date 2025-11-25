using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections; 
using UnityEngine;

public partial struct MonsterSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // 1. SETUP SCORE
        if (!SystemAPI.HasSingleton<GameScore>())
        {
            var scoreEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(scoreEntity, new GameScore { Value = 0 });
        }
        RefRW<GameScore> gameScore = SystemAPI.GetSingletonRW<GameScore>();

        // 2. TÌM PLAYER
        float3 playerPos = float3.zero;
        bool playerFound = false;

        foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>().WithNone<DeadTag>())
        {
            playerPos = transform.ValueRO.Position;
            playerFound = true;
            break; 
        }

        // --- CHUẨN BỊ DỮ LIỆU ĐỂ NÉ NHAU ---
        // Để quái né nhau, ta cần biết vị trí của TẤT CẢ con quái.
        // Ta lưu tạm vị trí bọn nó vào một danh sách (NativeList) để tra cứu cho nhanh.
        var queryMonster = SystemAPI.QueryBuilder().WithAll<MonsterTag, LocalTransform>().WithNone<DeadTag>().Build();
        var allMonsterPositions = queryMonster.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        // 3. LOGIC QUÁI (Di chuyển & Né nhau)
        int index = 0; // Biến đếm để biết mình là con số mấy trong danh sách
        foreach (var (transform, props, health, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<MonsterProperties>, RefRW<MonsterHealth>>()
                                                                   .WithEntityAccess()
                                                                   .WithAll<MonsterTag>()
                                                                   .WithNone<DeadTag>())
        {
            if (health.ValueRO.Current <= 0)
            {
                ecb.AddComponent<DeadTag>(entity);
                ecb.AddComponent(entity, new DeathTimer { Value = 3.0f }); 
                gameScore.ValueRW.Value += 100;
                index++; 
                continue;
            }

            if (!playerFound) { index++; continue; }

            float distToPlayer = math.distance(transform.ValueRO.Position, playerPos);
            
            // B. TÍNH TOÁN DI CHUYỂN (CÓ NÉ NHAU)
            if (distToPlayer <= props.ValueRO.DetectionRange)
            {
                float3 finalDir = math.normalize(playerPos - transform.ValueRO.Position);
                float3 separationDir = float3.zero;
                float separationRadius = 1.5f; // Bán kính "vùng riêng tư" của quái (khoảng 1.5m)
                int neighborsCount = 0;

                for (int i = 0; i < allMonsterPositions.Length; i++)
                {
                    if (i == index) continue;

                    float3 neighborPos = allMonsterPositions[i].Position;
                    float distToNeighbor = math.distance(transform.ValueRO.Position, neighborPos);
                    if (distToNeighbor < separationRadius && distToNeighbor > 0.1f)
                    {
                        float3 pushDir = math.normalize(transform.ValueRO.Position - neighborPos);
                        separationDir += pushDir / distToNeighbor;
                        neighborsCount++;
                    }
                }

                if (neighborsCount > 0)
                {
                    finalDir += separationDir * 5f; // Nhân 1.5 để ưu tiên né hơn là đi
                    finalDir = math.normalize(finalDir);
                }

                // C. THỰC HIỆN DI CHUYỂN
                finalDir.y = 0;
                if (!math.all(finalDir == 0))
                {
                    var targetRot = quaternion.LookRotation(finalDir, math.up());
                    transform.ValueRW.Rotation = math.slerp(transform.ValueRO.Rotation, targetRot, 10f * dt);
                }

                if (distToPlayer > props.ValueRO.AttackDistance || neighborsCount > 0)
                {
                    transform.ValueRW.Position += finalDir * props.ValueRO.MoveSpeed * dt;
                }

                // D. BẮN ĐẠN
                props.ValueRW.FireTimer -= dt;
                if (props.ValueRW.FireTimer <= 0 && distToPlayer <= props.ValueRO.DetectionRange)
                {
                    props.ValueRW.FireTimer = props.ValueRO.FireRate;
                    props.ValueRW.IsAttacking = true; 

                    var newBullet = ecb.Instantiate(props.ValueRO.ProjectilePrefab);
                    float3 spawnPos = transform.ValueRO.Position + math.forward(transform.ValueRO.Rotation) * 1.2f;
                    spawnPos.y += 1.0f;

                    // --- ĐOẠN CODE SỬA ĐỔI ---
                    // 1. Tạo transform tạm thời
                    var bulletTrans = LocalTransform.FromPositionRotation(spawnPos, transform.ValueRO.Rotation);
                    
                    // 2. Ép kích thước nhỏ lại (Bạn chỉnh số 0.2f này to nhỏ tùy ý)
                    bulletTrans.Scale = 0.2f; 

                    // 3. Gán vào viên đạn
                    ecb.SetComponent(newBullet, bulletTrans);
                    // -------------------------
                }
            }
            index++;
        }

        // Xóa danh sách tạm sau khi dùng xong để giải phóng bộ nhớ
        allMonsterPositions.Dispose();

        // 4. LOGIC HỦY XÁC CHẾT (Giữ nguyên)
        foreach (var (timer, visual, entity) in SystemAPI.Query<RefRW<DeathTimer>, MonsterVisualObj>().WithEntityAccess().WithAll<DeadTag>())
        {
            timer.ValueRW.Value -= dt;
            if (timer.ValueRW.Value <= 0)
            {
                if (visual.VisualObject != null) Object.Destroy(visual.VisualObject);
                ecb.DestroyEntity(entity);
            }
        }
    }
}