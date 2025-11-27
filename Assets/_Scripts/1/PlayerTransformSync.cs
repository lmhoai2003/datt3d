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
    public float FireRate = 1.0f; 
    private float _nextFireTime = 0f; 

    private EntityManager _entityManager;
    private Entity _playerEntity;
    private bool _isDead = false;
    private bool _isInitialized = false;
    private bool _isShootButtonPressed = false;

    void Start()
    {
        try {
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
                CurrentHealth = MaxHealth, MaxHealth = MaxHealth, IsHit = false
            });
            if (HealthBar != null) { HealthBar.maxValue = MaxHealth; HealthBar.value = MaxHealth; }
            _isInitialized = true;
        } catch {}
    }

    public void OnShootButtonDown() { _isShootButtonPressed = true; }

    void Update()
    {
        if (!_isInitialized || _isDead) return;

        try
        {
            if (World.DefaultGameObjectInjectionWorld == null) return;
            if (!_entityManager.Exists(_playerEntity)) return;

            _entityManager.SetComponentData(_playerEntity, LocalTransform.FromPositionRotation(transform.position, transform.rotation));

            if (Input.GetMouseButtonDown(1) || _isShootButtonPressed) 
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
                _isShootButtonPressed = false;
            }

            var healthData = _entityManager.GetComponentData<PlayerHealthComponent>(_playerEntity);
            if (HealthBar != null) HealthBar.value = healthData.CurrentHealth;

            if (healthData.IsHit)
            {
                healthData.IsHit = false;
                _entityManager.SetComponentData(_playerEntity, healthData);

                if (healthData.CurrentHealth <= 0) Die();
                else if(PlayerAnimator != null) PlayerAnimator.SetTrigger("Hit");
            }
        }
        catch {}
    }

    void Die()
    {
        if (_isDead) return;
        _isDead = true;
        
        try {
            if (World.DefaultGameObjectInjectionWorld != null && _entityManager.Exists(_playerEntity))
                _entityManager.AddComponent<DeadTag>(_playerEntity);
        } catch {}

        if (PlayerAnimator != null) {
            PlayerAnimator.ResetTrigger("Hit"); PlayerAnimator.ResetTrigger("Attack"); PlayerAnimator.SetTrigger("Dead");
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; rb.isKinematic = true; }
        
        var moveScript = GetComponent<PlayerMobileMovement>(); 
        if (moveScript != null) moveScript.enabled = false;
    }

    // --- [MỚI] HÀM HỒI SINH ---
    public void Revive()
    {
        Debug.Log("PLAYER HỒI SINH!");
        _isDead = false;

        // 1. Reset ECS
        try
        {
            if (_entityManager.Exists(_playerEntity))
            {
                if (_entityManager.HasComponent<DeadTag>(_playerEntity))
                    _entityManager.RemoveComponent<DeadTag>(_playerEntity);

                var hp = _entityManager.GetComponentData<PlayerHealthComponent>(_playerEntity);
                hp.CurrentHealth = MaxHealth;
                _entityManager.SetComponentData(_playerEntity, hp);
                PlayerAnimator.SetTrigger("hoisinh");
            }
        }
        catch {}

        // 2. Reset Animation
        if (PlayerAnimator != null)
        {
            PlayerAnimator.ResetTrigger("Dead");
            PlayerAnimator.Play("Idle", 0, 0); 
        }

        // 3. Reset UI
        if (HealthBar != null) HealthBar.value = MaxHealth;

        // 4. Reset Vật lý & Di chuyển
        var rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; }

        var moveScript = GetComponent<PlayerMobileMovement>(); 
        if (moveScript != null) moveScript.enabled = true;
    }

    void OnDestroy()
    {
        try {
            if (!_isInitialized || World.DefaultGameObjectInjectionWorld == null) return;
            if (_entityManager.Exists(_playerEntity)) _entityManager.DestroyEntity(_playerEntity);
        } catch {}
    }
}