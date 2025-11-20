using Unity.Entities;
using UnityEngine;

public class BossAuthoring : MonoBehaviour
{
    public float MaxHP = 1000f;

    class Baker : Baker<BossAuthoring>
    {
        public override void Bake(BossAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BossStats
            {
                MaxHP = authoring.MaxHP,
                CurrentHP = authoring.MaxHP
            });
        }
    }
}