using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI; 

public class PlayerTransformSync : MonoBehaviour
{
    [Header("Cài đặt chỉ số")]
    public float MaxHealth = 100f;
    public Animator PlayerAnimator; 

    [Header("UI")]
    public Slider HealthBar; 

    [Header("Cài đặt tấn công")]
    [Tooltip("Thời gian chờ giữa 2 lần bắn (giây)")]
    public float FireRate = 1.0f; 
    private float _nextFireTime = 0f; 

    // Các biến nội bộ
    private EntityManager _entityManager;
    private Entity _playerEntity;
    private bool _isDead = false;

    void Start()
    {
        // 1. Khởi tạo Entity đại diện cho Player
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _playerEntity = _entityManager.CreateEntity();

        // Đặt tên để dễ Debug
#if UNITY_EDITOR
        _entityManager.SetName(_playerEntity, "Player_Ghost_Entity");
#endif
        
        // 2. Gắn các Component cơ bản
        _entityManager.AddComponent<PlayerTag>(_playerEntity);
        _entityManager.AddComponent<LocalTransform>(_playerEntity);

        // 3. Gắn Component Máu
        _entityManager.AddComponentData(_playerEntity, new PlayerHealthComponent
        {
            CurrentHealth = MaxHealth,
            MaxHealth = MaxHealth,
            IsHit = false
        });

        // --- MỚI: Cài đặt thanh máu ban đầu ---
        if (HealthBar != null)
        {
            HealthBar.maxValue = MaxHealth;
            HealthBar.value = MaxHealth;
        }
    }

    void Update()
    {
        // Nếu Entity bị hủy đột ngột thì dừng lại
        if (!_entityManager.Exists(_playerEntity)) return;

        // --- A. ĐỒNG BỘ VỊ TRÍ (GameObject -> ECS) ---
        // Chỉ cập nhật khi còn sống
        if (!_isDead)
        {
            _entityManager.SetComponentData(_playerEntity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));
        }

        // --- B. XỬ LÝ BẮN SÚNG (INPUT) ---
        // Nhấn chuột phải (1) và chưa chết
        if (Input.GetMouseButtonDown(1) && !_isDead) 
        {
            // Kiểm tra Cooldown (Thời gian chờ)
            if (Time.time >= _nextFireTime)
            {
                // Cập nhật lần bắn tiếp theo
                _nextFireTime = Time.time + FireRate;

                // Gửi tín hiệu "Bắn" vào ECS (Thêm component PlayerShootInput)
                if (!_entityManager.HasComponent<PlayerShootInput>(_playerEntity))
                {
                    _entityManager.AddComponent<PlayerShootInput>(_playerEntity);
                    
                    // Chạy Animation tấn công ngay lập tức
                    if (PlayerAnimator != null) PlayerAnimator.SetTrigger("Attack"); 
                }
            }
        }

        // --- C. ĐỒNG BỘ TRẠNG THÁI (ECS -> GameObject) ---
        var healthData = _entityManager.GetComponentData<PlayerHealthComponent>(_playerEntity);

        // --- MỚI: Cập nhật thanh máu liên tục ---
        if (HealthBar != null)
        {
            HealthBar.value = healthData.CurrentHealth;
        }

        // Kiểm tra xem có bị trúng đạn không
        if (healthData.IsHit)
        {
            // 1. Reset cờ IsHit trong ECS để không bị lặp
            healthData.IsHit = false;
            _entityManager.SetComponentData(_playerEntity, healthData);

            // 2. Kiểm tra máu để xử lý Chết hoặc Hit
            if (healthData.CurrentHealth <= 0)
            {
                if (!_isDead) Die();
            }
            else if (!_isDead)
            {
                // Chỉ chạy anim Hit khi còn sống
                if(PlayerAnimator != null) PlayerAnimator.SetTrigger("Hit");
            }
        }
    }

    // Hàm xử lý cái chết
    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        Debug.Log("GAME OVER!");

        // 1. Báo cho ECS biết là đã chết (để Quái ngừng bắn)
        if (_entityManager.Exists(_playerEntity))
        {
            _entityManager.AddComponent<DeadTag>(_playerEntity);
        }

        // 2. Xử lý Animation
        if (PlayerAnimator != null)
        {
            // Hủy bỏ lệnh Hit/Attack đang chờ (nếu có) để ưu tiên chết
            PlayerAnimator.ResetTrigger("Hit"); 
            PlayerAnimator.ResetTrigger("Attack");
            
            // Chạy anim Chết
            PlayerAnimator.SetTrigger("Dead");
        }

        // 3. Xử lý Vật lý (Để xác nằm yên trên sàn, không trôi)
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; 
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 4. Tắt khả năng di chuyển (Thay tên script di chuyển của bạn vào đây)
        // Ví dụ: var movement = GetComponent<PlayerMovement>();
        // if (movement != null) movement.enabled = false;
    }

    // Dọn dẹp khi tắt game hoặc hủy Player
    void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld == null || !World.DefaultGameObjectInjectionWorld.IsCreated) return;
        
        if (_entityManager.Exists(_playerEntity))
        {
            _entityManager.DestroyEntity(_playerEntity);
        }
    }
}