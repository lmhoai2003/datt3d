using Unity.Entities;
using UnityEngine;

public class PlayerProjectileAuthoring : MonoBehaviour
{
    public float Speed = 20f;
    public float LifeTime = 3f;

    class Baker : Baker<PlayerProjectileAuthoring>
    {
        public override void Bake(PlayerProjectileAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ProjectileData { Speed = authoring.Speed, LifeTime = authoring.LifeTime });
            AddComponent(entity, new ProjectileTag());
            AddComponent(entity, new PlayerProjectileTag());
        }
    }
}