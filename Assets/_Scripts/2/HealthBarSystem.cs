using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 

public partial class HealthBarSystem : SystemBase
{
    private Camera _mainCamera;
    private Dictionary<Entity, GameObject> _healthBars = new Dictionary<Entity, GameObject>();
    private List<Entity> _entitiesToRemove = new List<Entity>();

    protected override void OnUpdate()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        
        // Lấy sổ ghi nợ (ECB) để xóa Entity an toàn
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(World.Unmanaged);

        _entitiesToRemove.Clear();

        // 1. VÒNG LẶP: Quét tất cả quái có dữ liệu HealthBarData
        foreach (var (transform, stats, barData, entity) in 
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<EnemyStats>, HealthBarData>()
                 .WithEntityAccess())
        {
            GameObject barInstance = null;

            // Kiểm tra trong sổ tay xem con này đã có thanh máu chưa?
            if (!_healthBars.TryGetValue(entity, out barInstance))
            {
                var canvas = GameObject.Find("Canvas"); 
                if (canvas != null && barData.BarPrefab != null)
                {
                    barInstance = Object.Instantiate(barData.BarPrefab, canvas.transform);
                    _healthBars.Add(entity, barInstance); // Ghi vào sổ ngay lập tức
                }
            }

            if (barInstance == null) continue;

            // --- LOGIC CẬP NHẬT ---
            // Cập nhật vị trí (Bay trên đầu quái)
            float3 worldPos = transform.ValueRO.Position + new float3(0, 2f, 0);
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
            
            barInstance.SetActive(screenPos.z > 0);
            if (screenPos.z > 0)
            {
                barInstance.transform.position = screenPos;
            }

            var slider = barInstance.GetComponent<Slider>();
            if (slider != null) 
            {
                // Giả sử MaxHP là 3 (theo LifeTime gốc)
                slider.value = stats.ValueRO.LifeTime / 3.0f;
            }
            
            // Gọi hàm "bơm tim" để UI không tự hủy (nếu bạn đã gắn script tự hủy)
            var selfDestruct = barInstance.GetComponent<HealthBarSelfDestruct>();
            if (selfDestruct != null) selfDestruct.OnUpdatePosition();

            // --- LOGIC XÓA ---
            // Nếu hết máu
            if (stats.ValueRO.LifeTime <= 0)
            {
                Object.Destroy(barInstance);
                
                // 2. Đánh dấu để xóa khỏi sổ tay tí nữa
                _entitiesToRemove.Add(entity);

                // 3. Xóa Entity (Ghi vào ECB)
                ecb.DestroyEntity(entity);
            }
        }

        // Dọn dẹp sổ tay: Xóa những con đã chết khỏi Dictionary
        foreach (var entity in _entitiesToRemove)
        {
            _healthBars.Remove(entity);
        }
    }
}