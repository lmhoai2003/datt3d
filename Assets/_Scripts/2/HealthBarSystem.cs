using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Collections;

// SystemBase chạy trên Main Thread để xử lý UI (GameObject)
public partial class HealthBarSystem : SystemBase
{
    private Camera _mainCamera;
    
    // DICTIONARY: Sổ quản lý. 
    // Key = Entity (Con quái), Value = GameObject (Thanh máu)
    private Dictionary<Entity, GameObject> _healthBars = new Dictionary<Entity, GameObject>();

    // Danh sách tạm để lưu những con quái cần xóa khỏi sổ
    private List<Entity> _entitiesToRemove = new List<Entity>();

    protected override void OnUpdate()
    {
        // 1. Cache Camera (Chỉ lấy 1 lần cho tối ưu)
        if (_mainCamera == null) 
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return; // Chưa có camera thì chưa làm gì
        }

        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(World.Unmanaged);

        _entitiesToRemove.Clear();

        // --- GIAI ĐOẠN 1: CẬP NHẬT CÁC THANH MÁU ĐANG SỐNG ---
        // Chỉ lặp qua những Entity CÒN TỒN TẠI và có dữ liệu HealthBarData
        foreach (var (transform, stats, barData, entity) in 
                 SystemAPI.Query<RefRO<LocalTransform>, RefRO<EnemyStats>, HealthBarData>()
                 .WithEntityAccess())
        {
            GameObject barInstance = null;

            // A. Kiểm tra/Tạo mới
            if (!_healthBars.TryGetValue(entity, out barInstance))
            {
                var canvas = GameObject.Find("Canvas");
                if (canvas != null && barData.BarPrefab != null)
                {
                    barInstance = Object.Instantiate(barData.BarPrefab, canvas.transform);
                    _healthBars.Add(entity, barInstance); // Ghi tên vào sổ
                }
            }

            // Nếu vẫn null (do lỗi setup), bỏ qua
            if (barInstance == null) continue;

            // B. Tính toán vị trí
            float3 worldPos = transform.ValueRO.Position + new float3(0, 2.5f, 0); // Cao hơn đầu 1 chút
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);
            
            // Ẩn thanh máu nếu nó lọt ra sau lưng Camera (z < 0)
            bool isVisible = screenPos.z > 0;
            if (barInstance.activeSelf != isVisible) barInstance.SetActive(isVisible);

            if (isVisible)
            {
                barInstance.transform.position = screenPos;
                
                // Cập nhật Slider
                var slider = barInstance.GetComponent<Slider>();
                if (slider != null)
                {
                    // Hardcode chia 10 (theo EnemyAuthoring), thực tế nên lưu MaxHP
                    slider.value = stats.ValueRO.LifeTime / 10.0f; 
                }
            }

            // C. Xử lý cái chết (Tuổi thọ hết)
            if (stats.ValueRO.LifeTime <= 0)
            {
                // Xóa UI
                Object.Destroy(barInstance);
                
                // Đánh dấu để xóa khỏi Dictionary
                _entitiesToRemove.Add(entity);

                // Xóa Entity khỏi thế giới ECS
                ecb.DestroyEntity(entity);
            }
        }

        // Dọn dẹp Dictionary (những con chết già)
        foreach (var entity in _entitiesToRemove)
        {
            _healthBars.Remove(entity);
        }

        // --- GIAI ĐOẠN 2: QUÉT DỌN RÁC (ZOMBIE UI CLEANUP) ---
        // Đây là phần thay thế cho script HealthBarSelfDestruct
        // Nhiệm vụ: Tìm những thanh máu mà Entity chủ nhân đã "chết bất đắc kỳ tử" (bị xóa bởi system khác)
        
        // Tạo danh sách tạm chứa các Key cần xóa
        var cleanUpList = new NativeList<Entity>(Allocator.Temp);

        foreach (var kvp in _healthBars)
        {
            Entity entity = kvp.Key;
            GameObject ui = kvp.Value;

            // Kiểm tra xem Entity này còn tồn tại trong thế giới ECS không?
            if (!SystemAPI.Exists(entity))
            {
                // Entity đã mất tích -> UI này là rác -> Xóa ngay
                if (ui != null) Object.Destroy(ui);
                cleanUpList.Add(entity);
            }
        }

        // Xóa key rác khỏi sổ
        foreach (var deadEntity in cleanUpList)
        {
            _healthBars.Remove(deadEntity);
        }
        cleanUpList.Dispose(); // Giải phóng bộ nhớ tạm
    }
}