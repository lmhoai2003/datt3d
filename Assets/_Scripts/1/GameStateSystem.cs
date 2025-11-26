using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public partial struct GameStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. TẠO DỮ LIỆU BAN ĐẦU
        if (!SystemAPI.HasSingleton<GameStateData>())
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new GameStateData 
            { 
                CurrentState = GameState.WaitingToStart, 
                GameTimer = 0,
                HasSpawned = false 
            });
        }

        RefRW<GameStateData> stateData = SystemAPI.GetSingletonRW<GameStateData>();

        // --- [SỬA ĐỔI] ---
        // Nếu đang chờ Start -> Dừng lại ngay, không làm gì hết.
        // Việc chuyển trạng thái sẽ do nút bấm UI (GameUIManager) lo.
        if (stateData.ValueRO.CurrentState == GameState.WaitingToStart) return; 
        // -----------------

        // --- LOGIC KHI GAME ĐANG CHẠY ---
        float dt = SystemAPI.Time.DeltaTime;
        stateData.ValueRW.GameTimer += dt;

        if (stateData.ValueRO.CurrentState != GameState.Playing) return;

        if (stateData.ValueRO.GameTimer < 3.0f) return;

        // 2. CHECK THUA
        var playerDeadQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag, DeadTag>().Build();
        if (playerDeadQuery.CalculateEntityCount() > 0)
        {
            stateData.ValueRW.CurrentState = GameState.Lost;
            return;
        }

        // 3. CHECK THẮNG
        var monsterAliveQuery = SystemAPI.QueryBuilder().WithAll<MonsterTag>().WithNone<DeadTag>().Build();
        int aliveCount = monsterAliveQuery.CalculateEntityCount();

        if (aliveCount > 0) stateData.ValueRW.HasSpawned = true;

        if (aliveCount == 0 && stateData.ValueRO.HasSpawned)
        {
            stateData.ValueRW.CurrentState = GameState.Won;
        }
    }
}