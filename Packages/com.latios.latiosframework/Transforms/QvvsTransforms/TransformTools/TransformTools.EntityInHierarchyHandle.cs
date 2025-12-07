using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        /// <summary>
        /// A handle into an entity within a hierarchy. This type allows for fast hierarchy traversal.
        /// </summary>
        public struct EntityInHierarchyHandle
        {
            internal NativeArray<EntityInHierarchy> m_hierarchy;
            internal int                            m_index;

            /// <summary>
            /// True if this handle is invalid.
            /// </summary>
            public bool isNull => !m_hierarchy.IsCreated;
            /// <summary>
            /// True if this handle refers to the root of the hierarchy. That is, this entity does not have
            /// a bloodParent. It is possible for an entity to not have an alive parent, but not be the root.
            /// </summary>
            public bool isRoot => m_index == 0 && m_hierarchy.IsCreated;
            /// <summary>
            /// The entity currently being referred to.
            /// </summary>
            public Entity entity => (isNull ? default : m_hierarchy[m_index].entity);
            /// <summary>
            /// Returns the handle that refers to the internal immediate parent of this current handle.
            /// That handle may or may not refer to a dead entity. If the current handle is the root,
            /// this returns a null handle.
            /// </summary>
            public EntityInHierarchyHandle bloodParent => (isRoot ? default : new EntityInHierarchyHandle
                                                               {
                                                                   m_hierarchy = m_hierarchy,
                                                                   m_index     = m_hierarchy[m_index].parentIndex
                                                               });
            /// <summary>
            /// Returns the handle that refers to the root of the hierarchy.
            /// </summary>
            public EntityInHierarchyHandle root => new EntityInHierarchyHandle
            {
                m_hierarchy = m_hierarchy,
                m_index     = 0,
            };
            /// <summary>
            /// Finds the alive parent of the entity, if one exists
            /// </summary>
            /// <param name="entityManager">The EntityManager this entity is managed by</param>
            /// <returns>The alive parent's handle if found, otherwise a null handle</returns>
            public EntityInHierarchyHandle FindParent(EntityManager entityManager)
            {
                if (isRoot)
                    return default;

                var bp = bloodParent;
                while (!bp.isRoot)
                {
                    if (entityManager.HasComponent<RootReference>(bp.entity))
                        return bp;
                    bp = bp.bloodParent;
                }
                if (entityManager.HasBuffer<EntityInHierarchy>(bp.entity))
                    return bp;
                return default;
            }

            /// <summary>
            /// Finds the alive parent of the entity, if one exists
            /// </summary>
            /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
            /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
            /// <returns>The alive parent's handle if found, otherwise a null handle</returns>
            public EntityInHierarchyHandle FindParent(ref ComponentLookup<RootReference>  rootReferenceLookupRO,
                                                      ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO)
            {
                if (isRoot)
                    return default;

                var bp = bloodParent;
                while (!bp.isRoot)
                {
                    if (rootReferenceLookupRO.HasComponent(bp.entity))
                        return bp;
                    bp = bp.bloodParent;
                }
                if (entityInHierarchyLookupRO.HasBuffer(bp.entity))
                    return bp;
                return default;
            }

            /// <summary>
            /// Finds the alive parent of the entity, if one exists
            /// </summary>
            /// <param name="worldTransformLookupRO">A readonly ComponentLookup to the WorldTransform component</param>
            /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
            /// <returns>The alive parent's handle if found, otherwise a null handle</returns>
            public EntityInHierarchyHandle FindParent(ref ComponentLookup<WorldTransform> worldTransformLookupRO,
                                                      ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO)
            {
                if (isRoot)
                    return default;

                var bp = bloodParent;
                while (!bp.isRoot)
                {
                    if (worldTransformLookupRO.HasComponent(bp.entity))
                        return bp;
                    bp = bp.bloodParent;
                }
                if (entityInHierarchyLookupRO.HasBuffer(bp.entity))
                    return bp;
                return default;
            }
        }

        /// <summary>
        /// Resolves the EntityInHierarchyHandle for the specified RootReference, allowing for fast hierarchy traversal.
        /// </summary>
        /// <param name="entityManager">The EntityManager which manages the entity this RootReference came from</param>
        /// <returns>An EntityInHierarchyHandle referring to the spot in the hierarchy that the entity this RootReference
        /// belongs to is located</returns>
        public static EntityInHierarchyHandle ToHandle(this RootReference rootRef, EntityManager entityManager)
        {
            DynamicBuffer<EntityInHierarchy> buffer;
            if (entityManager.HasBuffer<EntityInHierarchy>(rootRef.rootEntity))
                buffer = entityManager.GetBuffer<EntityInHierarchy>(rootRef.rootEntity);
            else
                buffer = entityManager.GetBuffer<EntityInHierarchyCleanup>(rootRef.rootEntity).Reinterpret<EntityInHierarchy>();
            return new EntityInHierarchyHandle
            {
                m_hierarchy = buffer.AsNativeArray(),
                m_index     = rootRef.indexInHierarchy
            };
        }

        /// <summary>
        /// Resolves the EntityInHierarchyHandle for the specified RootReference, allowing for fast hierarchy traversal.
        /// </summary>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        /// <returns>An EntityInHierarchyHandle referring to the spot in the hierarchy that the entity this RootReference
        /// belongs to is located</returns>
        public static EntityInHierarchyHandle ToHandle(this RootReference rootRef, ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                                       ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            if (!entityInHierarchyLookupRO.TryGetBuffer(rootRef.rootEntity, out var buffer))
                buffer = entityInHierarchyCleanupLookupRO[rootRef.rootEntity].Reinterpret<EntityInHierarchy>();
            return new EntityInHierarchyHandle
            {
                m_hierarchy = buffer.AsNativeArray(),
                m_index     = rootRef.indexInHierarchy
            };
        }

        /// <summary>
        /// Gets the root EntityInHierarchyHandle for the given buffer.
        /// </summary>
        public static EntityInHierarchyHandle GetRootHandle(this DynamicBuffer<EntityInHierarchy> entityInHierarchyBuffer)
        {
            return new EntityInHierarchyHandle
            {
                m_hierarchy = entityInHierarchyBuffer.AsNativeArray(),
                m_index     = 0
            };
        }

        /// <summary>
        /// Gets the root EntityInHierarchyHandle for the given buffer.
        /// </summary>
        public static EntityInHierarchyHandle GetRootHandle(this DynamicBuffer<EntityInHierarchyCleanup> entityInHierarchyBuffer)
        {
            return new EntityInHierarchyHandle
            {
                m_hierarchy = entityInHierarchyBuffer.AsNativeArray().Reinterpret<EntityInHierarchy>(),
                m_index     = 0
            };
        }
    }
}

