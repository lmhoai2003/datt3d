using Unity.Entities;
using UnityEngine; // Bắt buộc có để dùng GameObject

// File: MonsterComponents.cs
// Cập nhật: Thêm WaitingToStart và PlayerHealthComponent

// --- PHẦN 1: TAG (NHÃN) ---
public struct MonsterTag : IComponentData { }
public struct ProjectileTag : IComponentData { }
public struct DeadTag : IComponentData { } // Gắn cho ai đã chết

// Lưu ý: Nếu bạn đã định nghĩa 'PlayerTag' bên file PlayerComponents.cs rồi
// thì hãy XÓA dòng dưới này đi nhé. Nếu chưa thì giữ lại.
public struct PlayerTag : IComponentData { } 

// Tag đánh dấu đây là đạn do Player bắn ra
public struct PlayerProjectileTag : IComponentData { }

// (Tùy chọn) Tag đánh dấu đạn do Monster bắn ra
public struct MonsterProjectileTag : IComponentData { }


// --- PHẦN 2: DỮ LIỆU PLAYER (Bổ sung cái này vì bạn đang thiếu) ---
public struct PlayerHealthComponent : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public bool IsHit;
}

public struct PlayerShootingData : IComponentData
{
    public Entity BulletPrefab; 
}

public struct PlayerShootInput : IComponentData { }

public struct PlayerBulletConfig : IComponentData
{
    public Entity BulletPrefab;
}


// --- PHẦN 3: DỮ LIỆU QUÁI VẬT ---

// Chỉ số hành vi & Tấn công
public struct MonsterProperties : IComponentData
{
    public float MoveSpeed;
    public float DetectionRange;      // Tầm phát hiện
    public float AttackDistance;      // Khoảng cách dừng lại bắn
    public float FireRate;            // Tốc độ bắn
    public float FireTimer;           // Bộ đếm thời gian
    public Entity ProjectilePrefab;   // Entity viên đạn
    public bool IsAttacking;          // Cờ báo hiệu đang tấn công
}

// Máu của quái
public struct MonsterHealth : IComponentData
{
    public float Current;
    public float Max;
    public bool IsHit;                // Cờ báo hiệu vừa trúng đạn
}

// Liên kết với GameObject Visual (Hybrid)
public class MonsterVisualObj : IComponentData
{
    public GameObject VisualObject;
}

// Bộ đếm thời gian hủy xác (Delay sau khi chết)
public struct DeathTimer : IComponentData
{
    public float Value;
}


// --- PHẦN 4: DỮ LIỆU VIÊN ĐẠN ---
public struct ProjectileData : IComponentData
{
    public float Speed;
    public float LifeTime;
}


// --- PHẦN 5: GAME STATE (TRẠNG THÁI GAME) ---

public struct GameScore : IComponentData
{
    public int Value;
}

public enum GameState
{
    WaitingToStart, 
    Playing, 
    Won,     
    Lost    
}
public struct GameStateData : IComponentData
{
    public GameState CurrentState;
    public float GameTimer; 
    public bool HasSpawned;
}