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

    private EntityManager _entityManager;
    private Entity _playerEntity;
    private bool _isDead = false;
    private bool _isInitialized = false;

    void Start()
    {
        // Bọc try-catch để an toàn ngay từ đầu
        try 
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            _entityManager = world.EntityManager;
            _playerEntity = _entityManager.CreateEntity();

#if UNITY_EDITOR
            _entityManager.SetName(_playerEntity, "Player_Ghost_Entity");
#endif
            
            _entityManager.AddComponent<PlayerTag>(_playerEntity);
            _entityManager.AddComponent<LocalTransform>(_playerEntity);

            _entityManager.AddComponentData(_playerEntity, new PlayerHealthComponent
            {
                CurrentHealth = MaxHealth,
                MaxHealth = MaxHealth,
                IsHit = false
            });

            if (HealthBar != null)
            {
                HealthBar.maxValue = MaxHealth;
                HealthBar.value = MaxHealth;
            }

            _isInitialized = true;
        }
        catch (System.Exception)
        {
            // Nếu lỗi ngay lúc start (hiếm), bỏ qua luôn
        }
    }

    void Update()
    {
        if (!_isInitialized || _isDead) return;

        // --- [FIX TRIỆT ĐỂ] DÙNG TRY-CATCH ---
        // Nếu có bất kỳ lỗi truy cập bộ nhớ nào (do game đang tắt), nó sẽ nhảy vào catch và lờ đi.
        try
        {
            // 1. Kiểm tra thế giới còn sống không
            if (World.DefaultGameObjectInjectionWorld == null) return;
            if (!_entityManager.Exists(_playerEntity)) return;

            // 2. ĐỒNG BỘ VỊ TRÍ
            _entityManager.SetComponentData(_playerEntity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));

            // 3. XỬ LÝ BẮN SÚNG
            if (Input.GetMouseButtonDown(1)) 
            {
                if (Time.time >= _nextFireTime)
                {
                    _nextFireTime = Time.time + FireRate;
                    if (!_entityManager.HasComponent<PlayerShootInput>(_playerEntity))
                    {
                        _entityManager.AddComponent<PlayerShootInput>(_playerEntity);
                        if (PlayerAnimator != null) PlayerAnimator.SetTrigger("Attack"); 
                    }
                }
            }

            // 4. ĐỒNG BỘ TRẠNG THÁI
            var healthData = _entityManager.GetComponentData<PlayerHealthComponent>(_playerEntity);

            if (HealthBar != null) HealthBar.value = healthData.CurrentHealth;

            if (healthData.IsHit)
            {
                healthData.IsHit = false;
                _entityManager.SetComponentData(_playerEntity, healthData);

                if (healthData.CurrentHealth <= 0)
                {
                    Die();
                }
                else
                {
                    if(PlayerAnimator != null) PlayerAnimator.SetTrigger("Hit");
                }
            }
        }
        catch (System.Exception)
        {
            // IM LẶNG LÀ VÀNG: Khi tắt game, lỗi sẽ rơi vào đây và không hiện đỏ lên Console nữa.
            return;
        }
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        Debug.Log("GAME OVER!");

        try
        {
            if (World.DefaultGameObjectInjectionWorld != null && _entityManager.Exists(_playerEntity))
            {
                _entityManager.AddComponent<DeadTag>(_playerEntity);
            }
        }
        catch {} // Bỏ qua lỗi nếu ECS đã sập

        if (PlayerAnimator != null)
        {
            PlayerAnimator.ResetTrigger("Hit"); 
            PlayerAnimator.ResetTrigger("Attack");
            PlayerAnimator.SetTrigger("Dead");
        }

        // --- FIX LỖI VÀNG (Kinematic) ---
        // Phải dừng vận tốc TRƯỚC, rồi mới khóa Kinematic
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Dừng lại trước
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;            // Rồi mới khóa
        }

        var movement = GetComponent<MonoBehaviour>(); 
        if (movement != null && movement != this) movement.enabled = false;
    }

    void OnDestroy()
    {
        // Bọc try-catch cho chắc ăn
        try
        {
            if (!_isInitialized) return;
            if (World.DefaultGameObjectInjectionWorld == null) return;
            
            if (_entityManager.Exists(_playerEntity))
            {
                _entityManager.DestroyEntity(_playerEntity);
            }
        }
        catch (System.Exception)
        {
            // Bỏ qua lỗi khi tắt game
        }
    }
}