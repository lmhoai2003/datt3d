using Unity.Entities;
using UnityEngine;

public class BulletManagerAuthoring : MonoBehaviour
{
    public GameObject PlayerBulletPrefab; 

    class Baker : Baker<BulletManagerAuthoring>
    {
        public override void Bake(BulletManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PlayerBulletConfig
            {
                BulletPrefab = GetEntity(authoring.PlayerBulletPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}