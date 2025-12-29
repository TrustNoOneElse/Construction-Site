using System;
using Unity.Entities;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        #region Set WorldTransform
        /// <summary>
        /// Sets the WorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the WorldTransform for</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldTransform(Entity entity, in TransformQvvs newWorldTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = newWorldTransform });
                return;
            }
            SetWorldTransform(handle, in newWorldTransform, entityManager);
        }

        /// <summary>
        /// Sets the WorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose WorldTransform should be replaced</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newWorldTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the WorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the WorldTransform for</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetWorldTransform(Entity entity,
                                             in TransformQvvs newWorldTransform,
                                             ref ComponentLookup<WorldTransform>        transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                             ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW[entity] = new WorldTransform { worldTransform = newWorldTransform };
                return;
            }
            SetWorldTransform(handle, in newWorldTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the WorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose WorldTransform should be replaced</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetWorldTransform(EntityInHierarchyHandle handle,
                                             in TransformQvvs newWorldTransform,
                                             ref ComponentLookup<WorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the TickedWorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the TickedWorldTransform for</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldTransform(Entity entity, in TransformQvvs newTickedWorldTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = newTickedWorldTransform });
                return;
            }
            SetTickedWorldTransform(handle, in newTickedWorldTransform, entityManager);
        }

        /// <summary>
        /// Sets the TickedWorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose TickedWorldTransform should be replaced</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newTickedWorldTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newTickedWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the TickedWorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the TickedWorldTransform for</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedWorldTransform(Entity entity,
                                                   in TransformQvvs newTickedWorldTransform,
                                                   ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                   ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                   ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                   ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = newTickedWorldTransform };
                return;
            }
            SetTickedWorldTransform(handle, in newTickedWorldTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the TickedWorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose TickedWorldTransform should be replaced</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedWorldTransform(EntityInHierarchyHandle handle,
                                                   in TransformQvvs newTickedWorldTransform,
                                                   ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newTickedWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion
    }
}

