using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        /// <summary>
        /// Computes the local transform of the entity. If the entity does not have a parent, then this
        /// is identical to the WorldTransform without stretch.
        /// </summary>
        /// <param name="entity">The entity to compute the local transform for.</param>
        /// <param name="entityManager">The EntityManager which manages the entity</param>
        /// <param name="parentTransform">The parent's world-space transform for convenience, identity if there was no parent</param>
        /// <returns>The local position, rotation, and uniform scale of the entity</returns>
        public static TransformQvs LocalTransformFrom(Entity entity, EntityManager entityManager, out TransformQvvs parentTransform)
        {
            var worldTransform = entityManager.GetComponentData<WorldTransform>(entity);
            if (entityManager.HasComponent<RootReference>(entity))
            {
                var rootReference = entityManager.GetComponentData<RootReference>(entity);
                var handle        = rootReference.ToHandle(entityManager);
                var parentHandle  = handle.FindParent(entityManager);
                if (!parentHandle.isNull)
                {
                    parentTransform = entityManager.GetComponentData<WorldTransform>(parentHandle.entity).worldTransform;
                    return qvvs.inversemul(in parentTransform, in worldTransform.worldTransform);
                }
            }
            parentTransform = TransformQvvs.identity;
            return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
        }

        /// <summary>
        /// Computes the local transform of the entity. If the entity does not have a parent, then this
        /// is identical to the WorldTransform without stretch.
        /// </summary>
        /// <param name="entity">The entity to compute the local transform for.</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="worldTransformLookupRO">A readonly ComponentLookup to the WorldTransform component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        /// <param name="parentTransform">The parent's world-space transform for convenience, identity if there was no parent</param>
        /// <returns>The local position, rotation, and uniform scale of the entity</returns>
        public static TransformQvs LocalTransformFrom(Entity entity,
                                                      EntityStorageInfoLookup entityStorageInfoLookup,
                                                      ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                      ref ComponentLookup<WorldTransform>        worldTransformLookupRO,
                                                      ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                      ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO,
                                                      out TransformQvvs parentTransform)
        {
            var worldTransform = worldTransformLookupRO[entity];
            if (rootReferenceLookupRO.HasComponent(entity))
            {
                var rootReference = rootReferenceLookupRO[entity];
                var handle        = rootReference.ToHandle(ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
                var parentHandle  = handle.FindParent(entityStorageInfoLookup);
                if (!parentHandle.isNull)
                {
                    parentTransform = worldTransformLookupRO[parentHandle.entity].worldTransform;
                    return qvvs.inversemul(in parentTransform, in worldTransform.worldTransform);
                }
            }
            parentTransform = TransformQvvs.identity;
            return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
        }

        /// <summary>
        /// Computes the local transform of the entity. If the entity does not have a parent, then this
        /// is identical to the WorldTransform without stretch.
        /// </summary>
        /// <param name="handle">The hierarchy handle to compute the local transform for.</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="worldTransformLookupRO">A readonly ComponentLookup to the WorldTransform component</param>
        /// <param name="parentTransform">The parent's world-space transform for convenience, identity if there was no parent</param>
        /// <returns>The local position, rotation, and uniform scale of the entity</returns>
        public static TransformQvs LocalTransformFrom(EntityInHierarchyHandle handle,
                                                      EntityStorageInfoLookup entityStorageInfoLookup,
                                                      ref ComponentLookup<WorldTransform> worldTransformLookupRO,
                                                      out TransformQvvs parentTransform)
        {
            var worldTransform = worldTransformLookupRO[handle.entity];
            if (!handle.isRoot)
            {
                var parentHandle = handle.FindParent(entityStorageInfoLookup);
                if (!parentHandle.isNull)
                {
                    parentTransform = worldTransformLookupRO[parentHandle.entity].worldTransform;
                    return qvvs.inversemul(in parentTransform, in worldTransform.worldTransform);
                }
            }
            parentTransform = TransformQvvs.identity;
            return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
        }
    }
}

