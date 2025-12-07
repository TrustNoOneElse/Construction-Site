using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        public static partial class Unsafe
        {
            /// <summary>
            /// Computes the local transform of the entity. If the entity does not have a parent, then this
            /// is identical to the WorldTransform without stretch.
            /// </summary>
            /// <param name="rootReference">The RootReference obtained from chunk iteration</param>
            /// <param name="worldTransform">The WorldTransform of the entity possessing the RootReference, obtained from chunk iteration</param>
            /// <param name="entityManager">The EntityManager which manages the entity</param>
            /// <returns>The local position, rotation, and uniform scale of the entity</returns>
            public static TransformQvs LocalTransformFrom(in RootReference rootReference, in WorldTransform worldTransform, EntityManager entityManager)
            {
                var handle       = rootReference.ToHandle(entityManager);
                var parentHandle = handle.FindParent(entityManager);
                if (!parentHandle.isNull)
                {
                    var parentTransform = entityManager.GetComponentData<WorldTransform>(parentHandle.entity);
                    return qvvs.inversemul(in parentTransform.worldTransform, in worldTransform.worldTransform);
                }
                return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
            }

            /// <summary>
            /// Computes the local transform of the entity. If the entity does not have a parent, then this
            /// is identical to the WorldTransform without stretch.
            /// </summary>
            /// <param name="rootReference">The RootReference obtained from chunk iteration</param>
            /// <param name="worldTransform">The WorldTransform of the entity possessing the RootReference, obtained from chunk iteration</param>
            /// <param name="worldTransformLookupRO">A readonly ComponentLookup to the WorldTransform component</param>
            /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
            /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
            /// <returns>The local position, rotation, and uniform scale of the entity</returns>
            public static TransformQvs LocalTransformFrom(in RootReference rootReference,
                                                          in WorldTransform worldTransform,
                                                          ref ComponentLookup<WorldTransform>        worldTransformLookupRO,
                                                          ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                          ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
            {
                var handle       = rootReference.ToHandle(ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
                var parentHandle = handle.FindParent(ref worldTransformLookupRO, ref entityInHierarchyLookupRO);
                if (!parentHandle.isNull)
                {
                    var parentTransform = worldTransformLookupRO[parentHandle.entity];
                    return qvvs.inversemul(in parentTransform.worldTransform, in worldTransform.worldTransform);
                }
                return new TransformQvs(worldTransform.position, worldTransform.rotation, worldTransform.scale);
            }
        }
    }
}

