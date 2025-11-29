using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        public static TransformQvs LocalTransformFrom(Entity entity, EntityManager entityManager)
        {
            var worldTransform = entityManager.GetComponentData<WorldTransform>(entity);
            if (entityManager.HasComponent<RootReference>(entity))
            {
                var                              rootReference = entityManager.GetComponentData<RootReference>(entity);
                DynamicBuffer<EntityInHierarchy> hierarchy;
                if (entityManager.HasBuffer<EntityInHierarchy>(rootReference.rootEntity))
                    hierarchy = entityManager.GetBuffer<EntityInHierarchy>(rootReference.rootEntity, true);
                else
                    hierarchy       = entityManager.GetBuffer<EntityInHierarchyCleanup>(rootReference.rootEntity, true).Reinterpret<EntityInHierarchy>();
                var parentIndex     = hierarchy[rootReference.indexInHierarchy].parentIndex;
                var parentEntity    = hierarchy[parentIndex].entity;
                var parentTransform = entityManager.GetComponentData<WorldTransform>(parentEntity);
                return qvvs.inversemul(in parentTransform.worldTransform, in worldTransform.worldTransform);
            }
            return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
        }
    }
}

