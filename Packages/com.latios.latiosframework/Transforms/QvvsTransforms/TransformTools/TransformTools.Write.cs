using System;
using Unity.Entities;
using Unity.Mathematics;

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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position = newLocalPosition;
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
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position = newLocalPosition;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position = newLocalPosition;
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position = newLocalPosition;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation = newLocalRotation;
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
                                            ref ComponentLookup<WorldTransform> transformLookupRW,
                                            ref EntityStorageInfoLookup entityStorageInfoLookup,
                                            ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                            ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                            ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = newLocalRotation;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation = newLocalRotation;
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
                                                  ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                  ref EntityStorageInfoLookup entityStorageInfoLookup,
                                                  ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                                  ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                                  ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                var currentTransform = transformLookupRW[entity].worldTransform;
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale = newLocalScale;
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
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale = newLocalScale;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale = newLocalScale;
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
                                               ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                               ref EntityStorageInfoLookup entityStorageInfoLookup,
                                               ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                               ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                               ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale = newLocalScale;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleSet
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
                                             ref ComponentLookup<WorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup,
                                             ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                             ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
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
                entityManager.SetComponentData(entity, new TickedWorldTransform() { worldTransform = newLocalTransform });
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.stretch = newStretch;
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
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch = newStretch;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.stretch = newStretch;
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch = newStretch;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position = newWorldPosition;
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
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position = newWorldPosition;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position = newWorldPosition;
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position = newWorldPosition;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation = newWorldRotation;
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
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = newWorldRotation;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation = newWorldRotation;
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = newWorldRotation;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale = newWorldScale;
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
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale = newWorldScale;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
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
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.scale = newWorldScale;
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
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale = newWorldScale;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
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
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleSet
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion
        
        #region Apply Local Position Delta
        /// <summary>
        /// Applies a delta to the local position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalPositionDelta(Entity entity, float3 positionDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull) {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position += positionDelta;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyLocalPositionDelta(handle, positionDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalPositionDelta(EntityInHierarchyHandle handle, float3 positionDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the local position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyLocalPositionDelta(Entity entity,
                                         float3 positionDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position += positionDelta;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyLocalPositionDelta(handle, positionDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyLocalPositionDelta(EntityInHierarchyHandle handle,
                                                   float3 positionDelta,
                                                   ref ComponentLookup<WorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked local position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalPositionDelta(Entity entity, float3 positionDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position += positionDelta;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedLocalPositionDelta(handle, positionDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalPositionDelta(EntityInHierarchyHandle handle, float3 positionDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked local position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedLocalPositionDelta(Entity entity,
                                         float3 positionDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position += positionDelta;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedLocalPositionDelta(handle, positionDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked local position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedLocalPositionDelta(EntityInHierarchyHandle handle,
                                                         float3 positionDelta,
                                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
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
        /// Applies a delta to the local rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalRotationDelta(Entity entity, quaternion rotationDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull) {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyLocalRotationDelta(handle, rotationDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalRotationDelta(EntityInHierarchyHandle handle, quaternion rotationDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the local rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyLocalRotationDelta(Entity entity,
                                         quaternion rotationDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyLocalRotationDelta(handle, rotationDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyLocalRotationDelta(EntityInHierarchyHandle handle,
                                                   quaternion rotationDelta,
                                                   ref ComponentLookup<WorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        /// <summary>
        /// Applies a delta to the ticked local rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalRotationDelta(Entity entity, quaternion rotationDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedLocalRotationDelta(handle, rotationDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalRotationDelta(EntityInHierarchyHandle handle, quaternion rotationDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked local rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedLocalRotationDelta(Entity entity,
                                         quaternion rotationDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedLocalRotationDelta(handle, rotationDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked local rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedLocalRotationDelta(EntityInHierarchyHandle handle,
                                                         quaternion rotationDelta,
                                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Local Scale Delta
        /// <summary>
        /// Applies a delta to the local scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalScaleDelta(Entity entity, float scaleDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale *= scaleDelta;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyLocalScaleDelta(handle, scaleDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalScaleDelta(EntityInHierarchyHandle handle, float scaleDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the local scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyLocalScaleDelta(Entity entity,
                                         float scaleDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale *= scaleDelta;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyLocalScaleDelta(handle, scaleDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyLocalScaleDelta(EntityInHierarchyHandle handle,
                                                float scaleDelta,
                                                ref ComponentLookup<WorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked local scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalScaleDelta(Entity entity, float scaleDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.scale += scaleDelta;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedLocalScaleDelta(handle, scaleDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalScaleDelta(EntityInHierarchyHandle handle, float scaleDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked local scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedLocalScaleDelta(Entity entity,
                                         float scaleDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale *= scaleDelta;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedLocalScaleDelta(handle, scaleDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked local scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedLocalScaleDelta(EntityInHierarchyHandle handle,
                                                      float scaleDelta,
                                                      ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                      ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply Local Transform Delta
        /// <summary>
        /// Applies a delta to the local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalTransformDelta(Entity entity, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                TransformQvvs newTransform = qvvs.mul(transformDelta, currentTransform);;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = newTransform });
                return;
            }
            ApplyLocalTransformDelta(handle, transformDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the local transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyLocalTransformDelta(EntityInHierarchyHandle handle, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the local transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyLocalTransformDelta(Entity entity,
                                         in TransformQvvs transformDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform = qvvs.mul(transformDelta, currentTransform);
                transformLookupRW[entity] = new WorldTransform { worldTransform = newTransform };
                return;
            }
            ApplyLocalTransformDelta(handle, in transformDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the local transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose local transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyLocalTransformDelta(EntityInHierarchyHandle handle,
                                                    in TransformQvvs transformDelta,
                                                    ref ComponentLookup<WorldTransform> transformLookupRW,
                                                    ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
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
        /// Applies a delta to the ticked local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalTransformDelta(Entity entity, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                TransformQvvs newTransform = qvvs.mul(transformDelta, currentTransform);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = newTransform });
                return;
            }
            ApplyTickedLocalTransformDelta(handle, transformDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked local transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedLocalTransformDelta(EntityInHierarchyHandle handle, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.LocalTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked local transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked local transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedLocalTransformDelta(Entity entity,
                                         in TransformQvvs transformDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform =  qvvs.mul(transformDelta, currentTransform);
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = newTransform };
                return;
            }
            ApplyTickedLocalTransformDelta(handle, in transformDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked local transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked local transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedLocalTransformDelta(EntityInHierarchyHandle handle,
                                                          in TransformQvvs transformDelta,
                                                          ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
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

        #region Apply Stretch Delta
        /// <summary>
        /// Applies a delta to the stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the stretch for</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyStretchDelta(Entity entity, float3 stretchDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.stretch *= stretchDelta;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyStretchDelta(handle, stretchDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should receive the delta</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyStretchDelta(EntityInHierarchyHandle handle, float3 stretchDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the stretch for</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyStretchDelta(Entity entity,
                                         float3 stretchDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch *= stretchDelta;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyStretchDelta(handle, stretchDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose stretch should receive the delta</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyStretchDelta(EntityInHierarchyHandle handle,
                                             float3 stretchDelta,
                                             ref ComponentLookup<WorldTransform> transformLookupRW,
                                             ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked stretch for</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedStretchDelta(Entity entity, float3 stretchDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.stretch *= stretchDelta;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedStretchDelta(handle, stretchDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should receive the delta</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedStretchDelta(EntityInHierarchyHandle handle, float3 stretchDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked stretch of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked stretch for</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedStretchDelta(Entity entity,
                                         float3 stretchDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.stretch *= stretchDelta;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedStretchDelta(handle, stretchDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked stretch of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked stretch should receive the delta</param>
        /// <param name="stretchDelta">The stretch delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedStretchDelta(EntityInHierarchyHandle handle,
                                                   float3 stretchDelta,
                                                   ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { stretch = stretchDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.StretchDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Position Delta
        /// <summary>
        /// Applies a delta to the world position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldPositionDelta(Entity entity, float3 positionDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.position += positionDelta;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyWorldPositionDelta(handle, positionDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldPositionDelta(EntityInHierarchyHandle handle, float3 positionDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the world position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyWorldPositionDelta(Entity entity,
                                         float3 positionDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position += positionDelta;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyWorldPositionDelta(handle, positionDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyWorldPositionDelta(EntityInHierarchyHandle handle,
                                                   float3 positionDelta,
                                                   ref ComponentLookup<WorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked world position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldPositionDelta(Entity entity, float3 positionDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.position += positionDelta;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedWorldPositionDelta(handle, positionDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldPositionDelta(EntityInHierarchyHandle handle, float3 positionDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldPositionDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked world position of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world position for</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedWorldPositionDelta(Entity entity,
                                         float3 positionDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.position += positionDelta;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedWorldPositionDelta(handle, positionDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked world position of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world position should receive the delta</param>
        /// <param name="positionDelta">The position delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedWorldPositionDelta(EntityInHierarchyHandle handle,
                                                         float3 positionDelta,
                                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { position = positionDelta } };
            Span<Propagate.WriteCommand> commands   =
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
        /// Applies a delta to the world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldRotationDelta(Entity entity, quaternion rotationDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return; 
            }
            ApplyWorldRotationDelta(handle, rotationDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldRotationDelta(EntityInHierarchyHandle handle, quaternion rotationDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyWorldRotationDelta(Entity entity,
                                         quaternion rotationDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyWorldRotationDelta(handle, rotationDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyWorldRotationDelta(EntityInHierarchyHandle handle,
                                                   quaternion rotationDelta,
                                                   ref ComponentLookup<WorldTransform> transformLookupRW,
                                                   ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldRotationDelta(Entity entity, quaternion rotationDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedWorldRotationDelta(handle, rotationDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldRotationDelta(EntityInHierarchyHandle handle, quaternion rotationDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked world rotation of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world rotation for</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedWorldRotationDelta(Entity entity,
                                         quaternion rotationDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.rotation = math.mul(rotationDelta, currentTransform.rotation);
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedWorldRotationDelta(handle, rotationDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked world rotation of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world rotation should receive the delta</param>
        /// <param name="rotationDelta">The rotation delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedWorldRotationDelta(EntityInHierarchyHandle handle,
                                                         quaternion rotationDelta,
                                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                         ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { rotation = rotationDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldRotationDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Scale Delta
        /// <summary>
        /// Applies a delta to the world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldScaleDelta(Entity entity, float scaleDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<WorldTransform>(entity).worldTransform;
                currentTransform.scale *= scaleDelta;
                entityManager.SetComponentData(entity, new WorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyWorldScaleDelta(handle, scaleDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldScaleDelta(EntityInHierarchyHandle handle, float scaleDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyWorldScaleDelta(Entity entity,
                                         float scaleDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale *= scaleDelta;
                transformLookupRW[entity] = new WorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyWorldScaleDelta(handle, scaleDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyWorldScaleDelta(EntityInHierarchyHandle handle,
                                                float scaleDelta,
                                                ref ComponentLookup<WorldTransform> transformLookupRW,
                                                ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        
        /// <summary>
        /// Applies a delta to the ticked world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldScaleDelta(Entity entity, float scaleDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                currentTransform.scale *= scaleDelta;
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = currentTransform });
                return;
            }
            ApplyTickedWorldScaleDelta(handle, scaleDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldScaleDelta(EntityInHierarchyHandle handle, float scaleDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked world scale of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world scale for</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedWorldScaleDelta(Entity entity,
                                         float scaleDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                currentTransform.scale *= scaleDelta;
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = currentTransform };
                return;
            }
            ApplyTickedWorldScaleDelta(handle, scaleDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked world scale of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world scale should receive the delta</param>
        /// <param name="scaleDelta">The scale delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedWorldScaleDelta(EntityInHierarchyHandle handle,
                                                      float scaleDelta,
                                                      ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                      ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { new TransformQvvs { scale = scaleDelta } };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldScaleDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref LookupTickedWorldTransform.From(ref transformLookupRW),
                                        ref EsilAlive.From(ref entityStorageInfoLookup));
        }
        #endregion

        #region Apply World Transform Delta
        /// <summary>
        /// Applies a delta to the world transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldTransformDelta(Entity entity, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                
                return;
            }
            ApplyWorldTransformDelta(handle, transformDelta, entityManager);
        }
        
        /// <summary>
        /// Applies a delta to the world transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyWorldTransformDelta(EntityInHierarchyHandle handle, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new EntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the world transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the world transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyWorldTransformDelta(Entity entity,
                                         in TransformQvvs transformDelta,
                                         ref ComponentLookup<WorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform = qvvs.mul(transformDelta, currentTransform);
                transformLookupRW[entity] = new WorldTransform { worldTransform = newTransform };
                return;
            }
            ApplyWorldTransformDelta(handle, in transformDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the world transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose world transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyWorldTransformDelta(EntityInHierarchyHandle handle,
                                                    in TransformQvvs transformDelta,
                                                    ref ComponentLookup<WorldTransform> transformLookupRW,
                                                    ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
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
        /// Applies a delta to the ticked world transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldTransformDelta(Entity entity, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            var handle = GetHierarchyHandle(entity, entityManager);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = entityManager.GetComponentData<TickedWorldTransform>(entity).worldTransform;
                TransformQvvs newTransform = qvvs.mul(transformDelta, currentTransform);
                entityManager.SetComponentData(entity, new TickedWorldTransform { worldTransform = newTransform });
                return;
            }
            ApplyTickedWorldTransformDelta(handle, transformDelta, entityManager);
        }

        /// <summary>
        /// Applies a delta to the ticked world transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="entityManager">The EntityManager used to perform the write operations</param>
        public static void ApplyTickedWorldTransformDelta(EntityInHierarchyHandle handle, in TransformQvvs transformDelta, EntityManager entityManager)
        {
            if (handle.isCopyParent)
                return;
            var                          lookup     = new TickedEntityManagerAccess(entityManager);
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
            Span<Propagate.WriteCommand> commands   =
                stackalloc Propagate.WriteCommand[] { new Propagate.WriteCommand
                                                      {
                                                          indexInHierarchy = handle.indexInHierarchy,
                                                          writeType        = Propagate.WriteCommand.WriteType.WorldTransformDelta
                                                      } };
            Propagate.WriteAndPropagate(handle.m_hierarchy, transforms, commands, ref lookup, ref lookup);
        }
        
        /// <summary>
        /// Applies a delta to the ticked world transform of an entity
        /// </summary>
        /// <param name="entity">The entity to apply the delta to the ticked world transform for</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        /// <param name="rootReferenceLookupRO">A readonly ComponentLookup to the RootReference component</param>
        /// <param name="entityInHierarchyLookupRO">A readonly BufferLookup to the EntityInHierarchy dynamic buffer</param>
        /// <param name="entityInHierarchyCleanupLookupRO">A readonly BufferLookup to the EntityInHierarchyCleanup dynamic buffer</param>
        public static void ApplyTickedWorldTransformDelta(Entity entity,
                                         in TransformQvvs transformDelta,
                                         ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                         ref EntityStorageInfoLookup entityStorageInfoLookup,
                                         ref ComponentLookup<RootReference> rootReferenceLookupRO,
                                         ref BufferLookup<EntityInHierarchy> entityInHierarchyLookupRO,
                                         ref BufferLookup<EntityInHierarchyCleanup> entityInHierarchyCleanupLookupRO)
        {
            var handle = GetHierarchyHandle(entity, ref rootReferenceLookupRO, ref entityInHierarchyLookupRO, ref entityInHierarchyCleanupLookupRO);
            if (handle.isNull)
            {
                TransformQvvs currentTransform = transformLookupRW[entity].worldTransform;
                TransformQvvs newTransform  = qvvs.mul(transformDelta, currentTransform);
                transformLookupRW[entity] = new TickedWorldTransform { worldTransform = newTransform };
                return;
            }
            ApplyTickedWorldTransformDelta(handle, in transformDelta, ref transformLookupRW, ref entityStorageInfoLookup);
        }

        /// <summary>
        /// Applies a delta to the ticked world transform (position, rotation, scale) of an entity.
        /// </summary>
        /// <param name="handle">The hierarchy handle representing the entity whose ticked world transform should receive the delta</param>
        /// <param name="transformDelta">The transform delta to apply</param>
        /// <param name="transformLookupRW">A write-accessible ComponentLookup. Writing to multiple entities within the same hierarchy from different threads is not safe!</param>
        /// <param name="entityStorageInfoLookup">An EntityStorageInfoLookup from the same world the hierarchy belongs to</param>
        public static void ApplyTickedWorldTransformDelta(EntityInHierarchyHandle handle,
                                                          in TransformQvvs transformDelta,
                                                          ref ComponentLookup<TickedWorldTransform> transformLookupRW,
                                                          ref EntityStorageInfoLookup entityStorageInfoLookup)
        {
            if (handle.isCopyParent)
                return;
            Span<TransformQvvs>          transforms = stackalloc TransformQvvs[] { transformDelta };
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
    }
}

