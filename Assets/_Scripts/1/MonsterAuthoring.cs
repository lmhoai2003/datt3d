using Unity.Entities;
using UnityEngine;

// File: MonsterAuthoring.cs
public class MonsterAuthoring : MonoBehaviour
{
    [Header("Chỉ số Quái")]
    public float MoveSpeed = 4f;
    public float DetectionRange = 5f;
    public float AttackDistance = 3.5f;
    public float Health = 100f;
    public float FireRate = 1.5f;

    [Header("Prefab Đạn (Đã gắn ProjectileAuthoring)")]
    public GameObject ProjectilePrefab;

    class Baker : Baker<MonsterAuthoring>
    {
        public override void Bake(MonsterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Gắn Tag
            AddComponent(entity, new MonsterTag());
            
            // Gắn Máu
            AddComponent(entity, new MonsterHealth
            {
                Current = authoring.Health,
                Max = authoring.Health
            });

            // Gắn thuộc tính di chuyển/bắn
            AddComponent(entity, new MonsterProperties
            {
                MoveSpeed = authoring.MoveSpeed,
                DetectionRange = authoring.DetectionRange,
                AttackDistance = authoring.AttackDistance,
                FireRate = authoring.FireRate,
                FireTimer = 0f,
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}