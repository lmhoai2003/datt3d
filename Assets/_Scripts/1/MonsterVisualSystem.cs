using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public partial class MonsterVisualSystem : SystemBase
{
    private GameObject _visualPrefab;

    protected override void OnStartRunning()
    {
        _visualPrefab = Resources.Load<GameObject>("MonsterVisualPrefab");
        if (_visualPrefab == null) Debug.LogError("LỖI: Không tìm thấy 'MonsterVisualPrefab' trong Resources!");
    }

    protected override void OnUpdate()
    {
        if (_visualPrefab == null) return;

        // --- BƯỚC 1: TẠO "PHIẾU YÊU CẦU" (ECB) ---
        // Lấy hệ thống CommandBuffer để ghi lệnh
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);

        // --- BƯỚC 2: SINH RA VISUAL ---
        Entities.WithAll<MonsterTag>().WithNone<MonsterVisualObj>().ForEach((Entity entity, in LocalTransform trans) =>
        {
            // Tạo GameObject (Cái này an toàn vì nó là của Unity thường, không ảnh hưởng ECS)
            var go = Object.Instantiate(_visualPrefab, trans.Position, trans.Rotation);

            // --- SỬA LỖI Ở ĐÂY ---
            // Cũ (Lỗi): EntityManager.AddComponentData(entity, ...); -> Thay đổi cấu trúc khi đang lặp -> LỖI
            // Mới (Đúng): Dùng ecb để ghi lệnh "Lát nữa hãy thêm component này vào nhé"
            ecb.AddComponent(entity, new MonsterVisualObj { VisualObject = go });

        }).WithoutBurst().Run();

        // --- BƯỚC 3: CẬP NHẬT (Phần này không thay đổi cấu trúc nên giữ nguyên) ---
        Entities.ForEach((MonsterVisualObj visual, ref LocalTransform trans, ref MonsterHealth health, ref MonsterProperties props) =>
        {
            if (visual.VisualObject == null) return;

            // Đồng bộ vị trí
            visual.VisualObject.transform.position = trans.Position;
            visual.VisualObject.transform.rotation = trans.Rotation;

            // Đồng bộ Animation
            var sync = visual.VisualObject.GetComponent<MonsterVisualSync>();
            if (sync != null)
            {
                bool isDead = health.Current <= 0;

                // TRUYỀN THÊM health.Current VÀO ĐÂY
                sync.UpdateVisual(health.Current, health.IsHit, props.IsAttacking, isDead);
            }

            // Reset cờ
            if (health.IsHit) health.IsHit = false;
            if (props.IsAttacking) props.IsAttacking = false;

        }).WithoutBurst().Run();
    }
}