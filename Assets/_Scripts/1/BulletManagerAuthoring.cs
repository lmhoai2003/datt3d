using Unity.Entities;
using UnityEngine;

public class BulletManagerAuthoring : MonoBehaviour
{
    public GameObject PlayerBulletPrefab; // Kéo Prefab đạn Player vào đây

    class Baker : Baker<BulletManagerAuthoring>
    {
        public override void Bake(BulletManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            // Lưu Prefab đạn vào Component toàn cục
            AddComponent(entity, new PlayerBulletConfig
            {
                BulletPrefab = GetEntity(authoring.PlayerBulletPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}