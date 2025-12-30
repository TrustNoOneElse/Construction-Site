using System;
using Unity.Entities;
using Unity.Mathematics;

// Todo:
// 1) Use RefRW on lookups that both read and write WorldTransform.
// 2) Update parameter descriptions (mainly the deltas, which should use the action words translate, rotate, ect)
// 3) Implement inverse transforms
// 4) Implement TransformsComponentLookup and TransformsKey variants (already done for SetWorldTransform)
// 5) Support ComponentBroker (already done for SetWorldTransform)
// 6) Probably split this into separate files per #region in a folder called Write

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
        /// <param name="componentBroker">A ComponentBroker with write access to WorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetWorldTransform(Entity entity, in TransformQvvs newWorldTransform, ref ComponentBroker componentBroker)
        {
            var handle = GetHierarchyHandle(entity, ref componentBroker);
            if (handle.isNull)
            {
                componentBroker.GetRW<WorldTransform>(entity).ValueRW.worldTransform = newWorldTransform;
                return;
            }
            SetWorldTransform(handle, in newWorldTransform, ref componentBroker);
        }

        /// <summary>
        /// Sets the WorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the WorldTransform for</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="componentBroker">A ComponentBroker with write access to WorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetWorldTransform(Entity entity, in TransformQvvs newWorldTransform, TransformsKey key, ref ComponentBroker componentBroker)
        {
            var handle = GetHierarchyHandle(entity, ref componentBroker);
            if (handle.isNull)
            {
                componentBroker.GetRW<WorldTransform>(entity, key).ValueRW.worldTransform = newWorldTransform;
                return;
            }
            SetWorldTransform(handle, in newWorldTransform, ref componentBroker);
        }

        /// <summary>
        /// Sets the WorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose WorldTransform should be replaced</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="componentBroker">A ComponentBroker with write access to WorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newWorldTransform, ref ComponentBroker componentBroker)
        {
            if (handle.isCopyParent)
                return;
            ref var                      lookup     = ref ComponentBrokerAccess.From(ref componentBroker);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the WorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose WorldTransform should be replaced</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="componentBroker">A ComponentBroker with write access to WorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newWorldTransform, TransformsKey key, ref ComponentBroker componentBroker)
        {
            if (handle.isCopyParent)
                return;
            key.Validate(handle.root.entity);
            ref var                      lookup     = ref ComponentBrokerParallelAccess.From(ref componentBroker);
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
        /// Sets the WorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the WorldTransform for</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="transformLookupRW">A TransformsComponentLookup for parallel write access when the hierarchy is safe to access</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetWorldTransform(Entity entity,
                                             in TransformQvvs newWorldTransform,
                                             TransformsKey key,
                                             ref TransformsComponentLookup<WorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference>            rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy>           entityInHierarchyLookupRO,
                                             ref BufferLookup<EntityInHierarchyCleanup>    entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW.GetCheckedLookup(handle.root.entity, key)[entity] = new WorldTransform { worldTransform = newWorldTransform };
                return;
            }
            SetWorldTransform(handle, in newWorldTransform, ref transformLookupRW.GetCheckedLookup(entity, key), ref entityStorageInfoLookup);
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
        /// Sets the WorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose WorldTransform should be replaced</param>
        /// <param name="newWorldTransform">The new WorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="transformLookupRW">A TransformsComponentLookup for parallel write access when the hierarchy is safe to access</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetWorldTransform(EntityInHierarchyHandle handle,
                                             in TransformQvvs newWorldTransform,
                                             TransformsKey key,
                                             ref TransformsComponentLookup<WorldTransform> transformLookupRW,
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
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW.GetCheckedLookup(handle.root.entity, key)),
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
        /// <param name="componentBroker">A ComponentBroker with write access to TickedWorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetTickedWorldTransform(Entity entity, in TransformQvvs newTickedWorldTransform, ref ComponentBroker componentBroker)
        {
            var handle = GetHierarchyHandle(entity, ref componentBroker);
            if (handle.isNull)
            {
                componentBroker.GetRW<TickedWorldTransform>(entity).ValueRW.worldTransform = newTickedWorldTransform;
                return;
            }
            SetTickedWorldTransform(handle, in newTickedWorldTransform, ref componentBroker);
        }

        /// <summary>
        /// Sets the TickedWorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the TickedWorldTransform for</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="componentBroker">A ComponentBroker with write access to TickedWorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetTickedWorldTransform(Entity entity, in TransformQvvs newTickedWorldTransform, TransformsKey key, ref ComponentBroker componentBroker)
        {
            var handle = GetHierarchyHandle(entity, ref componentBroker);
            if (handle.isNull)
            {
                componentBroker.GetRW<TickedWorldTransform>(entity, key).ValueRW.worldTransform = newTickedWorldTransform;
                return;
            }
            SetTickedWorldTransform(handle, in newTickedWorldTransform, ref componentBroker);
        }

        /// <summary>
        /// Sets the TickedWorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose TickedWorldTransform should be replaced</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="componentBroker">A ComponentBroker with write access to TickedWorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetTickedWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newTickedWorldTransform, ref ComponentBroker componentBroker)
        {
            if (handle.isCopyParent)
                return;
            ref var                      lookup     = ref TickedComponentBrokerAccess.From(ref componentBroker);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newTickedWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the TickedWorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose TickedWorldTransform should be replaced</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="componentBroker">A ComponentBroker with write access to TickedWorldTransform and read access to RootReference, EntityInHierarchy, and EntityInHierarchyCleanup</param>
        public static void SetTickedWorldTransform(EntityInHierarchyHandle handle, in TransformQvvs newTickedWorldTransform, TransformsKey key, ref ComponentBroker componentBroker)
        {
            if (handle.isCopyParent)
                return;
            key.Validate(handle.root.entity);
            ref var                      lookup     = ref TickedComponentBrokerParallelAccess.From(ref componentBroker);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newTickedWorldTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand {
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
        /// Sets the TickedWorldTransform of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the TickedWorldTransform for</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="transformLookupRW">A TransformsComponentLookup for parallel write access when the hierarchy is safe to access</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedWorldTransform(Entity entity,
                                                   in TransformQvvs newTickedWorldTransform,
                                                   TransformsKey key,
                                                   ref TransformsComponentLookup<TickedWorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                   ref ComponentLookup<RootReference>                  rootReferenceLookupRO,
                                                   ref BufferLookup<EntityInHierarchy>                 entityInHierarchyLookupRO,
                                                   ref BufferLookup<EntityInHierarchyCleanup>          entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW.GetCheckedLookup(handle.root.entity, key)[entity] = new TickedWorldTransform { worldTransform = newTickedWorldTransform };
                return;
            }
            SetTickedWorldTransform(handle, in newTickedWorldTransform, ref transformLookupRW.GetCheckedLookup(entity, key), ref entityStorageInfoLookup);
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

        /// <summary>
        /// Sets the TickedWorldTransform for the entity corresponding to the specified hierarchy handle.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose TickedWorldTransform should be replaced</param>
        /// <param name="newTickedWorldTransform">The new TickedWorldTransform value</param>
        /// <param name="key">A key to ensure the hierarchy is safe to access</param>
        /// <param name="transformLookupRW">A TransformsComponentLookup for parallel write access when the hierarchy is safe to access</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedWorldTransform(EntityInHierarchyHandle handle,
                                                   in TransformQvvs newTickedWorldTransform,
                                                   TransformsKey key,
                                                   ref TransformsComponentLookup<TickedWorldTransform> transformLookupRW,
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
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands,
                                        ref LookupTickedWorldTransform.From(ref transformLookupRW.GetCheckedLookup(handle.root.entity, key)),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set World Position
        /// <summary>
        /// Sets the world position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world position for</param>
        /// <param name="newWorldPosition">The new position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldPosition(Entity entity, float3 newWorldPosition, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position                                                  = newWorldPosition;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetWorldPosition(handle, newWorldPosition, entityManager);
        }

        /// <summary>
        /// Sets the world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should be set</param>
        /// <param name="newWorldPosition">The new world position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldPosition(EntityInHierarchyHandle handle, float3 newWorldPosition, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newWorldPosition } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the world position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world position for</param>
        /// <param name="newWorldPosition">The new position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetWorldPosition(Entity entity,
                                            float3 newWorldPosition,
                                            ref ComponentLookup<WorldTransform>        transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position      = newWorldPosition;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetWorldPosition(handle, newWorldPosition, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should be set</param>
        /// <param name="newWorldPosition">The new world position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetWorldPosition(EntityInHierarchyHandle handle,
                                            float3 newWorldPosition,
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newWorldPosition } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked world position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world position for</param>
        /// <param name="newWorldPosition">The new position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldPosition(Entity entity, float3 newWorldPosition, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position                                                        = newWorldPosition;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedWorldPosition(handle, newWorldPosition, entityManager);
        }

        /// <summary>
        /// Sets the ticked world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should be set</param>
        /// <param name="newWorldPosition">The new world position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldPosition(EntityInHierarchyHandle handle, float3 newWorldPosition, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newWorldPosition } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked world position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world position for</param>
        /// <param name="newWorldPosition">The new position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedWorldPosition(Entity entity,
                                                  float3 newWorldPosition,
                                                  ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                  ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                  ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                  ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position      = newWorldPosition;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedWorldPosition(handle, newWorldPosition, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should be set</param>
        /// <param name="newWorldPosition">The new world position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedWorldPosition(EntityInHierarchyHandle handle,
                                                  float3 newWorldPosition,
                                                  ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newWorldPosition } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set World Rotation
        /// <summary>
        /// Sets the world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world rotation for</param>
        /// <param name="newWorldRotation">The new rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldRotation(Entity entity, quaternion newWorldRotation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                  = newWorldRotation;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetWorldRotation(handle, newWorldRotation, entityManager);
        }

        /// <summary>
        /// Sets the world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should be set</param>
        /// <param name="newWorldRotation">The new world rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldRotation(EntityInHierarchyHandle handle, quaternion newWorldRotation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newWorldRotation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world rotation for</param>
        /// <param name="newWorldRotation">The new rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetWorldRotation(Entity entity,
                                            quaternion newWorldRotation,
                                            ref ComponentLookup<WorldTransform>        transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = newWorldRotation;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetWorldRotation(handle, newWorldRotation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should be set</param>
        /// <param name="newWorldRotation">The new world rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetWorldRotation(EntityInHierarchyHandle handle,
                                            quaternion newWorldRotation,
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newWorldRotation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world rotation for</param>
        /// <param name="newWorldRotation">The new rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldRotation(Entity entity, quaternion newWorldRotation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                        = newWorldRotation;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedWorldRotation(handle, newWorldRotation, entityManager);
        }

        /// <summary>
        /// Sets the ticked world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should be set</param>
        /// <param name="newWorldRotation">The new world rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldRotation(EntityInHierarchyHandle handle, quaternion newWorldRotation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newWorldRotation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world rotation for</param>
        /// <param name="newWorldRotation">The new rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedWorldRotation(Entity entity,
                                                  quaternion newWorldRotation,
                                                  ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                  ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                  ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                  ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = newWorldRotation;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedWorldRotation(handle, newWorldRotation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should be set</param>
        /// <param name="newWorldRotation">The new world rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedWorldRotation(EntityInHierarchyHandle handle,
                                                  quaternion newWorldRotation,
                                                  ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newWorldRotation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set World Scale
        /// <summary>
        /// Sets the world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world scale for</param>
        /// <param name="newWorldScale">The new scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldScale(Entity entity, float newWorldScale, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale                                                     = newWorldScale;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetWorldScale(handle, newWorldScale, entityManager);
        }

        /// <summary>
        /// Sets the world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should be set</param>
        /// <param name="newWorldScale">The new world scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetWorldScale(EntityInHierarchyHandle handle, float newWorldScale, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newWorldScale } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to set the world scale for</param>
        /// <param name="newWorldScale">The new scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetWorldScale(Entity entity,
                                         float newWorldScale,
                                         ref ComponentLookup<WorldTransform>        transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale         = newWorldScale;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetWorldScale(handle, newWorldScale, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should be set</param>
        /// <param name="newWorldScale">The new world scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetWorldScale(EntityInHierarchyHandle handle,
                                         float newWorldScale,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newWorldScale } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world scale for</param>
        /// <param name="newWorldScale">The new scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldScale(Entity entity, float newWorldScale, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.scale                                                           = newWorldScale;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedWorldScale(handle, newWorldScale, entityManager);
        }

        /// <summary>
        /// Sets the ticked world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should be set</param>
        /// <param name="newWorldScale">The new world scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedWorldScale(EntityInHierarchyHandle handle, float newWorldScale, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newWorldScale } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked world scale for</param>
        /// <param name="newWorldScale">The new scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedWorldScale(Entity entity,
                                               float newWorldScale,
                                               ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                               ref EntityStorageInfoLookup entityStorageInfoLookup,
                                               ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                               ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                               ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale         = newWorldScale;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedWorldScale(handle, newWorldScale, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should be set</param>
        /// <param name="newWorldScale">The new world scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedWorldScale(EntityInHierarchyHandle handle,
                                               float newWorldScale,
                                               ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                               ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newWorldScale } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set Stretch
        /// <summary>
        /// Sets the stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to set the stretch for</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetStretch(Entity entity, float3 newStretch, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.stretch                                                   = newStretch;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetStretch(handle, newStretch, entityManager);
        }

        /// <summary>
        /// Sets the stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should be set</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetStretch(EntityInHierarchyHandle handle, float3 newStretch, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = newStretch } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to set the stretch for</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetStretch(Entity entity,
                                      float3 newStretch,
                                      ref ComponentLookup<WorldTransform>        transformLookupRW,
                                      ref EntityStorageInfoLookup entityStorageInfoLookup,
                                      ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                      ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                      ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch       = newStretch;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetStretch(handle, newStretch, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should be set</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetStretch(EntityInHierarchyHandle handle,
                                      float3 newStretch,
                                      ref ComponentLookup<WorldTransform> transformLookupRW,
                                      ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = newStretch } };
            Span<Propagate.WriteCommand> commands                                                              =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked stretch for</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedStretch(Entity entity, float3 newStretch, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.stretch                                                         = newStretch;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedStretch(handle, newStretch, entityManager);
        }

        /// <summary>
        /// Sets the ticked stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should be set</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedStretch(EntityInHierarchyHandle handle, float3 newStretch, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = newStretch } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked stretch for</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedStretch(Entity entity,
                                            float3 newStretch,
                                            ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch       = newStretch;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedStretch(handle, newStretch, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should be set</param>
        /// <param name="newStretch">The new stretch value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedStretch(EntityInHierarchyHandle handle,
                                            float3 newStretch,
                                            ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = newStretch } };
            Span<Propagate.WriteCommand> commands                                                              =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set Local Transform
        /// <summary>
        /// Sets the local transform  of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local transform for</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalTransform(Entity entity, in TransformQvvs newLocalTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = newLocalTransform });
                return;
            }
            SetLocalTransform(handle, in newLocalTransform, entityManager);
        }

        /// <summary>
        /// Sets the local transform  of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should be set</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalTransform(EntityInHierarchyHandle handle, in TransformQvvs newLocalTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newLocalTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the local transform  of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local transform for</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetLocalTransform(Entity entity,
                                             in TransformQvvs newLocalTransform,
                                             ref ComponentLookup<WorldTransform>        transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                             ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW[entity] = new WorldTransform { worldTransform = newLocalTransform };
                return;
            }
            SetLocalTransform(handle, in newLocalTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the local transform  of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should be set</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetLocalTransform(EntityInHierarchyHandle handle,
                                             in TransformQvvs newLocalTransform,
                                             ref ComponentLookup<WorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newLocalTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked local transform for</param>
        /// <param name="newLocalTransform">The new transform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalTransform(Entity entity, in TransformQvvs newLocalTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                entityManager.SetComponentData(entity, new TickedWorldTransform() {
                    worldTransform = newLocalTransform
                });
                return;
            }
            SetTickedLocalTransform(handle, newLocalTransform, entityManager);
        }

        /// <summary>
        /// Sets the ticked local transform of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should be set</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalTransform(EntityInHierarchyHandle handle, in TransformQvvs newLocalTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newLocalTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked local transform for</param>
        /// <param name="newLocalTransform">The new transform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedLocalTransform(Entity entity,
                                                   in TransformQvvs newLocalTransform,
                                                   ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                   ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                   ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                   ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = newLocalTransform };
                return;
            }
            SetTickedLocalTransform(handle, in newLocalTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked local transform  of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should be set</param>
        /// <param name="newLocalTransform">The new local transform value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedLocalTransform(EntityInHierarchyHandle handle,
                                                   in TransformQvvs newLocalTransform,
                                                   ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { newLocalTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set Local Position
        /// <summary>
        /// Sets the local position of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local position for</param>
        /// <param name="newLocalPosition">The new local position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalPosition(Entity entity, float3 newLocalPosition, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position                                                  = newLocalPosition;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetLocalPosition(handle, newLocalPosition, entityManager);
        }

        /// <summary>
        /// Sets the local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should be set</param>
        /// <param name="newLocalPosition">The new local position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalPosition(EntityInHierarchyHandle handle, float3 newLocalPosition, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newLocalPosition } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the local position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the local position for</param>
        /// <param name="newLocalPosition">The new position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetLocalPosition(Entity entity,
                                            float3 newLocalPosition,
                                            ref ComponentLookup<WorldTransform>        transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position      = newLocalPosition;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetLocalPosition(handle, newLocalPosition, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should be set</param>
        /// <param name="newLocalPosition">The new local position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetLocalPosition(EntityInHierarchyHandle handle,
                                            float3 newLocalPosition,
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newLocalPosition } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked local position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked local position for</param>
        /// <param name="newLocalPosition">The new position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalPosition(Entity entity, float3 newLocalPosition, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position                                                        = newLocalPosition;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedLocalPosition(handle, newLocalPosition, entityManager);
        }

        /// <summary>
        /// Sets the ticked local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should be set</param>
        /// <param name="newLocalPosition">The new local position value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalPosition(EntityInHierarchyHandle handle, float3 newLocalPosition, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newLocalPosition } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked local position of an entity
        /// </summary>
        /// <param name="entity">The entity to set the ticked local position for</param>
        /// <param name="newLocalPosition">The new position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedLocalPosition(Entity entity,
                                                  float3 newLocalPosition,
                                                  ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                  ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                  ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                  ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position      = newLocalPosition;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedLocalPosition(handle, newLocalPosition, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should be set</param>
        /// <param name="newLocalPosition">The new local position value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedLocalPosition(EntityInHierarchyHandle handle,
                                                  float3 newLocalPosition,
                                                  ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = newLocalPosition } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set Local Rotation
        /// <summary>
        /// Sets the local rotation of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local rotation for</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalRotation(Entity entity, quaternion newLocalRotation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                  = newLocalRotation;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetLocalRotation(handle, newLocalRotation, entityManager);
        }

        /// <summary>
        /// Sets the local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should be set</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalRotation(EntityInHierarchyHandle handle, quaternion newLocalRotation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newLocalRotation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the local rotation of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local rotation for</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetLocalRotation(Entity entity,
                                            quaternion newLocalRotation,
                                            ref ComponentLookup<WorldTransform>        transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = newLocalRotation;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetLocalRotation(handle, newLocalRotation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should be set</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetLocalRotation(EntityInHierarchyHandle handle,
                                            quaternion newLocalRotation,
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newLocalRotation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked local rotation of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the ticked local rotation for</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalRotation(Entity entity, quaternion newLocalRotation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                        = newLocalRotation;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedLocalRotation(handle, newLocalRotation, entityManager);
        }

        /// <summary>
        /// Sets the ticked local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should be set</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalRotation(EntityInHierarchyHandle handle, quaternion newLocalRotation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newLocalRotation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked local rotation of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the ticked local rotation for</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedLocalRotation(Entity entity,
                                                  quaternion newLocalRotation,
                                                  ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                  ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                  ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                  ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                var currentTransform      = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = newLocalRotation;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedLocalRotation(handle, newLocalRotation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should be set</param>
        /// <param name="newLocalRotation">The new local rotation value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedLocalRotation(EntityInHierarchyHandle handle,
                                                  quaternion newLocalRotation,
                                                  ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = newLocalRotation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Set Local Scale
        /// <summary>
        /// Sets the local scale of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local scale for</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalScale(Entity entity, float newLocalScale, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale                                                     = newLocalScale;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetLocalScale(handle, newLocalScale, entityManager);
        }

        /// <summary>
        /// Sets the local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local scale should be set</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetLocalScale(EntityInHierarchyHandle handle, float newLocalScale, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newLocalScale } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the local scale of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the local scale for</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetLocalScale(Entity entity,
                                         float newLocalScale,
                                         ref ComponentLookup<WorldTransform>        transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale         = newLocalScale;
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            SetLocalScale(handle, newLocalScale, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local scale should be set</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetLocalScale(EntityInHierarchyHandle handle,
                                         float newLocalScale,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newLocalScale } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Sets the ticked local scale of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the ticked local scale for</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalScale(Entity entity, float newLocalScale, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale                                                     = newLocalScale;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            SetTickedLocalScale(handle, newLocalScale, entityManager);
        }

        /// <summary>
        /// Sets the ticked local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local scale should be set</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void SetTickedLocalScale(EntityInHierarchyHandle handle, float newLocalScale, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newLocalScale } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Sets the ticked local scale of an entity.
        /// </summary>
        /// <param name="entity">The entity to set the ticked local scale for</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void SetTickedLocalScale(Entity entity,
                                               float newLocalScale,
                                               ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                               ref EntityStorageInfoLookup entityStorageInfoLookup,
                                               ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                               ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                               ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale         = newLocalScale;
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            SetTickedLocalScale(handle, newLocalScale, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Sets the ticked local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local scale should be set</param>
        /// <param name="newLocalScale">The new local scale value</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void SetTickedLocalScale(EntityInHierarchyHandle handle,
                                               float newLocalScale,
                                               ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                               ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = newLocalScale } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Transform Delta
        /// <summary>
        /// Multiplies the entity's current WorldTransform by the specified transform and assigns the
        /// result to the entity's WorldTransform. The expression is as follows:
        /// worldTransform = qvvs.mul(appliedTransform, worldTransform);
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world transform for</param>
        /// <param name="appliedTransform">The transform to multiply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformWorld(Entity entity, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
                return;
            TransformWorld(handle, appliedTransform, entityManager);
        }

        /// <summary>
        /// Multiplies the entity's current WorldTransform by the specified transform and assigns the
        /// result to the entity's WorldTransform. The expression is as follows:
        /// worldTransform = qvvs.mul(appliedTransform, worldTransform);
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformWorld(EntityInHierarchyHandle handle, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Multiplies the entity's current WorldTransform by the specified transform and assigns the
        /// result to the entity's WorldTransform. The expression is as follows:
        /// worldTransform = qvvs.mul(appliedTransform, worldTransform);
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TransformWorld(Entity entity,
                                          in TransformQvvs appliedTransform,
                                          ref ComponentLookup<WorldTransform>        transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup,
                                          ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                          ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                          ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform     = qvvs.mul(appliedTransform, currentTransform);
                transformLookupRW[entity]      = new WorldTransform { worldTransform = newTransform };
                return;
            }
            TransformWorld(handle, in appliedTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Multiplies the entity's current WorldTransform by the specified transform and assigns the
        /// result to the entity's WorldTransform. The expression is as follows:
        /// worldTransform = qvvs.mul(appliedTransform, worldTransform);
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TransformWorld(EntityInHierarchyHandle handle,
                                          in TransformQvvs appliedTransform,
                                          ref ComponentLookup<WorldTransform> transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Multiplies the entity's current TickedWorldTransform by the specified transform and assigns the
        /// result to the entity's TickedWorldTransform. The expression is as follows:
        /// tickedWorldTransform = qvvs.mul(appliedTransform, tickedWorldTransform);
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformTickedWorld(Entity entity, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                TransformQvvs newTransform                                                       = qvvs.mul(appliedTransform, currentTransform);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = newTransform });
                return;
            }
            TransformTickedWorld(handle, appliedTransform, entityManager);
        }

        /// <summary>
        /// Multiplies the entity's current TickedWorldTransform by the specified transform and assigns the
        /// result to the entity's TickedWorldTransform. The expression is as follows:
        /// tickedWorldTransform = qvvs.mul(appliedTransform, tickedWorldTransform);
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformTickedWorld(EntityInHierarchyHandle handle, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Multiplies the entity's current TickedWorldTransform by the specified transform and assigns the
        /// result to the entity's TickedWorldTransform. The expression is as follows:
        /// tickedWorldTransform = qvvs.mul(appliedTransform, tickedWorldTransform);
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TransformTickedWorld(Entity entity,
                                                in TransformQvvs appliedTransform,
                                                ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform     = qvvs.mul(appliedTransform, currentTransform);
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = newTransform };
                return;
            }
            TransformTickedWorld(handle, in appliedTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Multiplies the entity's current TickedWorldTransform by the specified transform and assigns the
        /// result to the entity's TickedWorldTransform. The expression is as follows:
        /// tickedWorldTransform = qvvs.mul(appliedTransform, tickedWorldTransform);
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TransformTickedWorld(EntityInHierarchyHandle handle,
                                                in TransformQvvs appliedTransform,
                                                ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Position Delta
        /// <summary>
        /// Moves the entity by the specified translation in world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateWorld(Entity entity, float3 translation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                              = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position                                                  += translation;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform  = currentTransform });
                return;
            }
            TranslateWorld(handle, translation, entityManager);
        }

        /// <summary>
        /// Moves the entity by the specified translation in world-space
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateWorld(EntityInHierarchyHandle handle, float3 translation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TranslateWorld(Entity entity,
                                          float3 translation,
                                          ref ComponentLookup<WorldTransform>        transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup,
                                          ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                          ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                          ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.position      += translation;
                transformLookupRW[entity]       = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            TranslateWorld(handle, translation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in world-space
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TranslateWorld(EntityInHierarchyHandle handle,
                                          float3 translation,
                                          ref ComponentLookup<WorldTransform> transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateTickedWorld(Entity entity, float3 translation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                    = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position                                                        += translation;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform  = currentTransform });
                return;
            }
            TranslateTickedWorld(handle, translation, entityManager);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked world-space
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateTickedWorld(EntityInHierarchyHandle handle, float3 translation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TranslateTickedWorld(Entity entity,
                                                float3 translation,
                                                ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.position      += translation;
                transformLookupRW[entity]       = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            TranslateTickedWorld(handle, translation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked world-space
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TranslateTickedWorld(EntityInHierarchyHandle handle,
                                                float3 translation,
                                                ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Rotation Delta
        /// <summary>
        /// Rotates the entity by the specified rotation in world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateWorld(Entity entity, quaternion rotationToApply, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                  = math.mul(rotationToApply, currentTransform.rotation);
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            RotateWorld(handle, rotationToApply, entityManager);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in world-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateWorld(EntityInHierarchyHandle handle, quaternion rotationToApply, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void RotateWorld(Entity entity,
                                       quaternion rotationToApply,
                                       ref ComponentLookup<WorldTransform>        transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup,
                                       ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                       ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                       ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = math.mul(rotationToApply, currentTransform.rotation);
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            RotateWorld(handle, rotationToApply, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in world-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void RotateWorld(EntityInHierarchyHandle handle,
                                       quaternion rotationToApply,
                                       ref ComponentLookup<WorldTransform> transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateTickedWorld(Entity entity, quaternion rotationToApply, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                        = math.mul(rotationToApply, currentTransform.rotation);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            RotateTickedWorld(handle, rotationToApply, entityManager);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked world-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateTickedWorld(EntityInHierarchyHandle handle, quaternion rotationToApply, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked world-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void RotateTickedWorld(Entity entity,
                                             quaternion rotationToApply,
                                             ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                             ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = math.mul(rotationToApply, currentTransform.rotation);
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            RotateTickedWorld(handle, rotationToApply, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked world-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void RotateTickedWorld(EntityInHierarchyHandle handle,
                                             quaternion rotationToApply,
                                             ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Scale Delta
        /// <summary>
        /// Scales the entity by the specified factor
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world scale for</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void Scale(Entity entity, float scaleFactor, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                              = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale                                                     *= scaleFactor;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform  = currentTransform });
                return;
            }
            Scale(handle, scaleFactor, entityManager);
        }

        /// <summary>
        /// Scales the entity by the specified factor.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should receive the delta</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void Scale(EntityInHierarchyHandle handle, float scaleFactor, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleFactor } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.ScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Scales the entity by the specified factor
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world scale for</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void Scale(Entity entity,
                                 float scaleFactor,
                                 ref ComponentLookup<WorldTransform>        transformLookupRW,
                                 ref EntityStorageInfoLookup entityStorageInfoLookup,
                                 ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                 ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                 ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.scale         *= scaleFactor;
                transformLookupRW[entity]       = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            Scale(handle, scaleFactor, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Scales the entity by the specified factor
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should receive the delta</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void Scale(EntityInHierarchyHandle handle,
                                 float scaleFactor,
                                 ref ComponentLookup<WorldTransform> transformLookupRW,
                                 ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleFactor } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.ScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Scales the entity by the specified factor for the tick
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world scale for</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ScaleTicked(Entity entity, float scaleFactor, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                    = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.scale                                                           *= scaleFactor;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform  = currentTransform });
                return;
            }
            ScaleTicked(handle, scaleFactor, entityManager);
        }

        /// <summary>
        /// Scales the entity by the specified factor for the tick
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should receive the delta</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ScaleTicked(EntityInHierarchyHandle handle, float scaleFactor, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleFactor } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.ScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Scales the entity by the specified factor for the tick
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world scale for</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ScaleTicked(Entity entity,
                                       float scaleFactor,
                                       ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup,
                                       ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                       ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                       ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.scale         *= scaleFactor;
                transformLookupRW[entity]       = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ScaleTicked(handle, scaleFactor, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Scales the entity by the specified factor for the tick
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should receive the delta</param>
        /// <param name="scaleFactor">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ScaleTicked(EntityInHierarchyHandle handle,
                                       float scaleFactor,
                                       ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleFactor } };
            Span<Propagate.WriteCommand> commands                                                            =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.ScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Stretch Delta
        /// <summary>
        /// Stretches the entity by the specified factors along each axis
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the stretch for</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void Stretch(Entity entity, float3 stretchFactors, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                              = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.stretch                                                   *= stretchFactors;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform  = currentTransform });
                return;
            }
            Stretch(handle, stretchFactors, entityManager);
        }

        /// <summary>
        /// Stretches the entity by the specified factors along each axis.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should receive the delta</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void Stretch(EntityInHierarchyHandle handle, float3 stretchFactors, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchFactors } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Stretches the entity by the specified factors along each axis
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the stretch for</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void Stretch(Entity entity,
                                   float3 stretchFactors,
                                   ref ComponentLookup<WorldTransform>        transformLookupRW,
                                   ref EntityStorageInfoLookup entityStorageInfoLookup,
                                   ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                   ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                   ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.stretch       *= stretchFactors;
                transformLookupRW[entity]       = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            Stretch(handle, stretchFactors, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Stretches the entity by the specified factors along each axis.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should receive the delta</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void Stretch(EntityInHierarchyHandle handle,
                                   float3 stretchFactors,
                                   ref ComponentLookup<WorldTransform> transformLookupRW,
                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchFactors } };
            Span<Propagate.WriteCommand> commands                                                              =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Stretches the entity by the specified factors for the tick
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked stretch for</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void StretchTicked(Entity entity, float3 stretchFactors, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                    = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.stretch                                                         *= stretchFactors;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform  = currentTransform });
                return;
            }
            StretchTicked(handle, stretchFactors, entityManager);
        }

        /// <summary>
        /// Stretches the entity by the specified factors for the tick.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should receive the delta</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void StretchTicked(EntityInHierarchyHandle handle, float3 stretchFactors, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchFactors } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Stretches the entity by the specified factors for the tick
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked stretch for</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void StretchTicked(Entity entity,
                                         float3 stretchFactors,
                                         ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.stretch       *= stretchFactors;
                transformLookupRW[entity]       = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            StretchTicked(handle, stretchFactors, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Stretches the entity by the specified factors for the tick.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should receive the delta</param>
        /// <param name="stretchFactors">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void StretchTicked(EntityInHierarchyHandle handle,
                                         float3 stretchFactors,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchFactors } };
            Span<Propagate.WriteCommand> commands                                                              =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Local Transform Delta
        /// <summary>
        /// Multiplies the entity's current local transform by the specified transform, and sets the entity's
        /// local transform to the result. The expression is as follows:
        /// localTransform = qvvs.mul(appliedTransform, localTransform)
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformLocal(Entity entity, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                TransformQvvs newTransform                                                 = qvvs.mul(appliedTransform, currentTransform);;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = newTransform });
                return;
            }
            TransformLocal(handle, appliedTransform, entityManager);
        }

        /// <summary>
        /// Multiplies the entity's current local transform by the specified transform, and sets the entity's
        /// local transform to the result. The expression is as follows:
        /// localTransform = qvvs.mul(appliedTransform, localTransform)
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformLocal(EntityInHierarchyHandle handle, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Multiplies the entity's current local transform by the specified transform, and sets the entity's
        /// local transform to the result. The expression is as follows:
        /// localTransform = qvvs.mul(appliedTransform, localTransform)
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TransformLocal(Entity entity,
                                          in TransformQvvs appliedTransform,
                                          ref ComponentLookup<WorldTransform>        transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup,
                                          ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                          ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                          ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform     = qvvs.mul(appliedTransform, currentTransform);
                transformLookupRW[entity]      = new WorldTransform { worldTransform = newTransform };
                return;
            }
            TransformLocal(handle, in appliedTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Multiplies the entity's current local transform by the specified transform, and sets the entity's
        /// local transform to the result. The expression is as follows:
        /// localTransform = qvvs.mul(appliedTransform, localTransform)
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TransformLocal(EntityInHierarchyHandle handle,
                                          in TransformQvvs appliedTransform,
                                          ref ComponentLookup<WorldTransform> transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Multiplies the entity's current ticked local transform by the specified transform, and sets the entity's
        /// ticked local transform to the result. The expression is as follows:
        /// tickedLocalTransform = qvvs.mul(appliedTransform, tickedLocalTransform)
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformTickedLocal(Entity entity, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                TransformQvvs newTransform                                                       = qvvs.mul(appliedTransform, currentTransform);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = newTransform });
                return;
            }
            TransformTickedLocal(handle, appliedTransform, entityManager);
        }

        /// <summary>
        /// Multiplies the entity's current ticked local transform by the specified transform, and sets the entity's
        /// ticked local transform to the result. The expression is as follows:
        /// tickedLocalTransform = qvvs.mul(appliedTransform, tickedLocalTransform)
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TransformTickedLocal(EntityInHierarchyHandle handle, in TransformQvvs appliedTransform, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Multiplies the entity's current ticked local transform by the specified transform, and sets the entity's
        /// ticked local transform to the result. The expression is as follows:
        /// tickedLocalTransform = qvvs.mul(appliedTransform, tickedLocalTransform)
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local transform for</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TransformTickedLocal(Entity entity,
                                                in TransformQvvs appliedTransform,
                                                ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform     = qvvs.mul(appliedTransform, currentTransform);
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = newTransform };
                return;
            }
            TransformTickedLocal(handle, in appliedTransform, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Multiplies the entity's current ticked local transform by the specified transform, and sets the entity's
        /// ticked local transform to the result. The expression is as follows:
        /// tickedLocalTransform = qvvs.mul(appliedTransform, tickedLocalTransform)
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should receive the delta</param>
        /// <param name="appliedTransform">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TransformTickedLocal(EntityInHierarchyHandle handle,
                                                in TransformQvvs appliedTransform,
                                                ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { appliedTransform };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Local Position Delta
        /// <summary>
        /// Moves the entity by the specified translation in local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateLocal(Entity entity, float3 translation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                              = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position                                                  += translation;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform  = currentTransform });
                return;
            }
            TranslateLocal(handle, translation, entityManager);
        }

        /// <summary>
        /// Moves the entity by the specified translation in local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateLocal(EntityInHierarchyHandle handle, float3 translation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TranslateLocal(Entity entity,
                                          float3 translation,
                                          ref ComponentLookup<WorldTransform>        transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup,
                                          ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                          ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                          ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.position      += translation;
                transformLookupRW[entity]       = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            TranslateLocal(handle, translation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TranslateLocal(EntityInHierarchyHandle handle,
                                          float3 translation,
                                          ref ComponentLookup<WorldTransform> transformLookupRW,
                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateTickedLocal(Entity entity, float3 translation, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                    = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position                                                        += translation;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform  = currentTransform });
                return;
            }
            TranslateTickedLocal(handle, translation, entityManager);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void TranslateTickedLocal(EntityInHierarchyHandle handle, float3 translation, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local position for</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void TranslateTickedLocal(Entity entity,
                                                float3 translation,
                                                ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                                ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                                ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform  = transformLookupRW[entity].worldTransform;
                currentTransform.position      += translation;
                transformLookupRW[entity]       = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            TranslateTickedLocal(handle, translation, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Moves the entity by the specified translation in ticked local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should receive the delta</param>
        /// <param name="translation">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void TranslateTickedLocal(EntityInHierarchyHandle handle,
                                                float3 translation,
                                                ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = translation } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Local Rotation Delta
        /// <summary>
        /// Rotates the entity by the specified rotation in local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateLocal(Entity entity, quaternion rotationToApply, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                             = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                  = math.mul(rotationToApply, currentTransform.rotation);
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            RotateLocal(handle, rotationToApply, entityManager);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateLocal(EntityInHierarchyHandle handle, quaternion rotationToApply, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void RotateLocal(Entity entity,
                                       quaternion rotationToApply,
                                       ref ComponentLookup<WorldTransform>        transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup,
                                       ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                       ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                       ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = math.mul(rotationToApply, currentTransform.rotation);
                transformLookupRW[entity]      = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            RotateLocal(handle, rotationToApply, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void RotateLocal(EntityInHierarchyHandle handle,
                                       quaternion rotationToApply,
                                       ref ComponentLookup<WorldTransform> transformLookupRW,
                                       ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        /// <summary>
        /// Rotates the entity by the specified rotation in ticked local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateTickedLocal(Entity entity, quaternion rotationToApply, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform                                                   = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation                                                        = math.mul(rotationToApply, currentTransform.rotation);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            RotateTickedLocal(handle, rotationToApply, entityManager);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void RotateTickedLocal(EntityInHierarchyHandle handle, quaternion rotationToApply, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked local-space
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local rotation for</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void RotateTickedLocal(Entity entity,
                                             quaternion rotationToApply,
                                             ref ComponentLookup<TickedWorldTransform>  transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference>         rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy>        entityInHierarchyLookupRO,
                                             ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation      = math.mul(rotationToApply, currentTransform.rotation);
                transformLookupRW[entity]      = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            RotateTickedLocal(handle, rotationToApply, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Rotates the entity by the specified rotation in ticked local-space.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should receive the delta</param>
        /// <param name="rotationToApply">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void RotateTickedLocal(EntityInHierarchyHandle handle,
                                             quaternion rotationToApply,
                                             ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationToApply } };
            Span<Propagate.WriteCommand> commands                                                               =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion
    }
}

