using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public partial struct GameStateSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // 1. TẠO SINGLETON STATE 
        if (!SystemAPI.HasSingleton<GameStateData>())
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new GameStateData 
            { 
                CurrentState = GameState.Playing,
                GameTimer = 0,
                HasSpawned = false 
            });
        }

        RefRW<GameStateData> stateData = SystemAPI.GetSingletonRW<GameStateData>();
        float dt = SystemAPI.Time.DeltaTime;
        stateData.ValueRW.GameTimer += dt;

        if (stateData.ValueRO.CurrentState != GameState.Playing) return;

        // 2. KIỂM TRA THUA 
        var playerDeadQuery = SystemAPI.QueryBuilder().WithAll<PlayerTag, DeadTag>().Build();
        if (playerDeadQuery.CalculateEntityCount() > 0)
        {
            stateData.ValueRW.CurrentState = GameState.Lost;
            return;
        }

        // 3. KIỂM TRA QUÁI & THẮNG
        // Đếm số quái CÒN SỐNG
        var monsterAliveQuery = SystemAPI.QueryBuilder()
                                            .WithAll<MonsterTag>()
                                            .WithNone<DeadTag>()
                                            .Build();
        
        int aliveCount = monsterAliveQuery.CalculateEntityCount();


        if (aliveCount > 0)
        {
            stateData.ValueRW.HasSpawned = true;
        }

        // Chỉ thắng khi: (Số quái = 0) VÀ (Đã từng sinh quái)
        if (aliveCount == 0 && stateData.ValueRO.HasSpawned == true)
        {
            stateData.ValueRW.CurrentState = GameState.Won;
        }
    }
}