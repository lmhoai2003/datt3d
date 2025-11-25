using Unity.Entities;

// Lưu dữ liệu máu của Player trong thế giới ECS
public struct PlayerHealthComponent : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public bool IsHit; // Cờ báo hiệu "Vừa bị trúng đạn" để GameObject biết mà diễn Animation
}