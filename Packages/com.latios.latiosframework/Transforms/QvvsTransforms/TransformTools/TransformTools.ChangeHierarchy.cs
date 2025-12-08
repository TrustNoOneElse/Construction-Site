using System;
using System.Diagnostics;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Exposed;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        #region API
        /// <summary>
        /// Assigns a new parent to the entity, updating all hierarchy information between the two entities involved
        /// </summary>
        /// <param name="parent">The target parent</param>
        /// <param name="child">The entity which should have its parent assigned</param>
        /// <param name="inheritanceFlags">The inheritance flags the child will use</param>
        /// <param name="transferLinkedEntityGroup">If the child entity is a standalone entity or is a hierarchy root,
        /// then its entire LinkedEntityGroup (or itself if it doesn't have one) will be appended to the destination
        /// hierarchy root. If the entity is already a child of a different hierarchy, than only entities within its
        /// subtree which are in its original hierarchy's LinkedEntityGroup will be transferred. If the child already
        /// belonged to the destination hierarchy, the LinkedEntityGroup buffer will not be touched.</param>
        public static unsafe void AddChild(this EntityManager em,
                                           Entity parent,
                                           Entity child,
                                           InheritanceFlags inheritanceFlags          = InheritanceFlags.Normal,
                                           bool transferLinkedEntityGroup = true)
        {
            CheckChangeParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);

            bool parentHasRootRef   = em.HasComponent<RootReference>(parent);
            bool childHasRootRef    = em.HasComponent<RootReference>(parent);
            bool parentHasHierarchy = !parentHasRootRef && em.HasBuffer<EntityInHierarchy>(parent);
            bool childHasHierarchy  = !childHasRootRef && em.HasBuffer<EntityInHierarchy>(child);

            if (!parentHasRootRef && !childHasRootRef && !parentHasHierarchy && !childHasHierarchy)
                AddSoloChildToSoloParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (parentHasHierarchy && !childHasRootRef && !childHasHierarchy)
                AddSoloChildToRootParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (parentHasRootRef && !childHasRootRef && !childHasHierarchy)
                AddSoloChildToInternalParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (!parentHasRootRef && !parentHasHierarchy && childHasHierarchy)
                AddRootChildToSoloParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (parentHasHierarchy && childHasHierarchy)
                AddRootChildToRootParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (parentHasRootRef && childHasHierarchy)
                AddRootChildToInternalParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (!parentHasRootRef && !parentHasHierarchy && childHasRootRef)
                AddInternalChildToSoloParent(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            else if (parentHasHierarchy && childHasRootRef)
            {
                var childRootRef = em.GetComponentData<RootReference>(child);
                if (childRootRef.rootEntity == parent)
                    AddInternalChildToRootParentSameRoot(em, parent, child, inheritanceFlags);
                else
                    AddInternalChildToRootParentSeparateRoot(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            }
            else if (parentHasRootRef && childHasRootRef)
            {
                var childRootRef  = em.GetComponentData<RootReference>(child);
                var parentRootRef = em.GetComponentData<RootReference>(parent);
                if (childRootRef.rootEntity == parentRootRef.rootEntity)
                    AddInternalChildToInternalParentSameRoot(em, parent, child, inheritanceFlags);
                else
                    AddInternalChildToInernalParentSeparateRoot(em, parent, child, inheritanceFlags, transferLinkedEntityGroup);
            }

            if ((inheritanceFlags & InheritanceFlags.CopyParent) == InheritanceFlags.CopyParent)
            {
                // Todo: Set WorldTransform of child and propagate.
            }
        }
        #endregion

        #region Processes
        static unsafe void AddSoloChildToSoloParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            ConvertSoloArchetypeToRootArchetype(em, parent, createOrAppendLEG, false);
            ConvertSoloArchetypeToChildArchetype(em, parent, child, flags);

            em.SetComponentData(child, new RootReference { m_rootEntity = parent, m_indexInHierarchy = 1 });
            var hierarchy                                                                            = em.GetBuffer<EntityInHierarchy>(parent);
            hierarchy.Add(new EntityInHierarchy
            {
                m_childCount       = 1,
                m_descendantEntity = parent,
                m_firstChildIndex  = 1,
                m_flags            = InheritanceFlags.Normal,
                m_parentIndex      = -1
            });
            hierarchy.Add(new EntityInHierarchy
            {
                m_childCount       = 0,
                m_descendantEntity = child,
                m_firstChildIndex  = 2,
                m_flags            = flags,
                m_parentIndex      = 0
            });

            if (createOrAppendLEG)
                em.GetBuffer<LinkedEntityGroup>(parent).Add(new LinkedEntityGroup { Value = child });
            else
            {
                var cleanup = em.GetBuffer<EntityInHierarchyCleanup>(parent).Reinterpret<EntityInHierarchy>();
                cleanup.AddRange(hierarchy.AsNativeArray());
            }
        }

        static unsafe void AddSoloChildToRootParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            ConvertSoloArchetypeToChildArchetype(em, parent, child, flags);

            var hierarchy = em.GetBuffer<EntityInHierarchy>(parent);
            AddSingleChild(em, hierarchy, 0, child, flags);

            if (createOrAppendLEG)
            {
                var dstLeg = GetOrCreateLEG(em, parent);
                if (em.HasBuffer<LinkedEntityGroup>(child))
                {
                    var srcLeg = em.GetBuffer<LinkedEntityGroup>(child).Reinterpret<Entity>();
                    // Ugly copy because Unity safety flags this
                    for (int i = 0; i < srcLeg.Length; i++)
                        dstLeg.Add(srcLeg[i]);
                }
                else
                    dstLeg.Add(child);
            }
            else
            {
                GetOrAddAndCopyCleanup(em, parent);
            }
        }

        static unsafe void AddSoloChildToInternalParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            ConvertSoloArchetypeToChildArchetype(em, parent, child, flags);

            var rootRef   = em.GetComponentData<RootReference>(parent);
            var hierarchy = GetHierarchy(em, rootRef.rootEntity, out var rootIsAlive);
            AddSingleChild(em, hierarchy, rootRef.indexInHierarchy, child, flags);

            if (createOrAppendLEG)
            {
                var dstLeg = GetOrCreateLEG(em, rootRef.rootEntity);
                AppendChildRootLEG(em, dstLeg, child);
            }
            else if (rootIsAlive)
            {
                GetOrAddAndCopyCleanup(em, rootRef.rootEntity);
            }
        }

        static unsafe void AddRootChildToSoloParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            bool childHasCleanup = em.HasBuffer<EntityInHierarchyCleanup>(child);
            ConvertSoloArchetypeToRootArchetype(em, parent, createOrAppendLEG, childHasCleanup);
            em.AddComponent<RootReference>(child);

            var dstHierarchy = em.GetBuffer<EntityInHierarchy>(parent);
            var srcHierarchy = em.GetBuffer<EntityInHierarchy>(child);
            dstHierarchy.ResizeUninitialized(srcHierarchy.Length + 1);
            var dst = dstHierarchy.AsNativeArray();
            var src = srcHierarchy.AsNativeArray();
            dst[0]  = new EntityInHierarchy
            {
                m_childCount       = 1,
                m_descendantEntity = parent,
                m_firstChildIndex  = 1,
                m_flags            = InheritanceFlags.Normal,
                m_parentIndex      = -1,
            };
            for (int i = 0; i < src.Length; i++)
            {
                var temp = src[i];
                temp.m_parentIndex++;
                temp.m_firstChildIndex++;
                em.SetComponentData(temp.entity, new RootReference { m_indexInHierarchy = i + 1, m_rootEntity = parent});
                dst[i + 1]                                                                                    = temp;
            }

            if (childHasCleanup || !createOrAppendLEG)
                GetOrAddAndCopyCleanup(em, parent);
            if (createOrAppendLEG)
            {
                var dstLeg = GetOrCreateLEG(em, parent);
                AppendChildRootLEG(em, dstLeg, child);
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup, LinkedEntityGroup>());
            }
            else
            {
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
            }
        }

        static unsafe void AddRootChildToRootParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            bool childHasCleanup = em.HasBuffer<EntityInHierarchyCleanup>(child);
            em.AddComponent<RootReference>(child);

            var dstHierarchy = em.GetBuffer<EntityInHierarchy>(parent);
            var srcHierarchy = em.GetBuffer<EntityInHierarchy>(child).AsNativeArray().AsReadOnlySpan();

            InsertSubtree(em, dstHierarchy, 0, srcHierarchy, flags);

            if (childHasCleanup || !createOrAppendLEG)
                GetOrAddAndCopyCleanup(em, parent);
            if (createOrAppendLEG)
            {
                var dstLeg = GetOrCreateLEG(em, parent);
                AppendChildRootLEG(em, dstLeg, child);
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup, LinkedEntityGroup>());
            }
            else
            {
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
            }
        }

        static unsafe void AddRootChildToInternalParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            bool childHasCleanup = em.HasBuffer<EntityInHierarchyCleanup>(child);
            em.AddComponent<RootReference>(child);

            var rootRef      = em.GetComponentData<RootReference>(parent);
            var dstHierarchy = GetHierarchy(em, rootRef.rootEntity, out var rootIsAlive);
            var srcHierarchy = em.GetBuffer<EntityInHierarchy>(child).AsNativeArray().AsReadOnlySpan();

            InsertSubtree(em, dstHierarchy, rootRef.indexInHierarchy, srcHierarchy, flags);

            if (rootIsAlive)
            {
                if (childHasCleanup || !createOrAppendLEG)
                    GetOrAddAndCopyCleanup(em, parent);
                if (createOrAppendLEG)
                {
                    var dstLeg = GetOrCreateLEG(em, parent);
                    AppendChildRootLEG(em, dstLeg, child);
                }
            }
            if (createOrAppendLEG)
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup, LinkedEntityGroup>());
            else
                em.RemoveComponent(child, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
        }

        static unsafe void AddInternalChildToSoloParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            ConvertSoloArchetypeToRootArchetype(em, parent, createOrAppendLEG, false);

            var childRootRef    = em.GetComponentData<RootReference>(child);
            var childHierarchy  = GetHierarchy(em, childRootRef.rootEntity, out var childRootIsAlive);
            var parentHierarchy = em.GetBuffer<EntityInHierarchy>(parent);

            if (childHierarchy[childRootRef.indexInHierarchy].childCount == 0)
            {
                RemoveSingleDescendant(em, childHierarchy, childRootRef.indexInHierarchy);
                parentHierarchy.ResizeUninitialized(2);
                parentHierarchy[0] = new EntityInHierarchy
                {
                    m_childCount       = 1,
                    m_descendantEntity = parent,
                    m_firstChildIndex  = 1,
                    m_flags            = InheritanceFlags.Normal,
                    m_parentIndex      = -1,
                };
                parentHierarchy[1] = new EntityInHierarchy
                {
                    m_childCount       = 0,
                    m_descendantEntity = child,
                    m_firstChildIndex  = 2,
                    m_flags            = flags,
                    m_parentIndex      = 0
                };

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredLeg = false;
                if (childRootIsAlive && createOrAppendLEG)
                {
                    var childLeg = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                    var index    = childLeg.AsNativeArray().IndexOf(child);
                    if (index >= 0)
                    {
                        childLeg.RemoveAtSwapBack(index);
                        GetOrCreateLEG(em, parent).Add(child);
                        transferredLeg = true;
                    }
                }
                if (!transferredLeg || em.HasBuffer<EntityInHierarchyCleanup>(parent))
                {
                    GetOrAddAndCopyCleanup(em, parent);
                }
            }
            else
            {
                var tsa     = ThreadStackAllocator.GetAllocator();
                var subtree = ExtractSubtree(ref tsa, childHierarchy.AsNativeArray().AsReadOnlySpan(), childRootRef.indexInHierarchy);
                RemoveSubtree(em, childHierarchy, childRootRef.indexInHierarchy, subtree);
                parentHierarchy.ResizeUninitialized(childHierarchy.Length + 1);
                var dst = parentHierarchy.AsNativeArray();
                var src = childHierarchy.AsNativeArray();
                dst[0]  = new EntityInHierarchy
                {
                    m_childCount       = 1,
                    m_descendantEntity = parent,
                    m_firstChildIndex  = 1,
                    m_flags            = InheritanceFlags.Normal,
                    m_parentIndex      = -1,
                };
                for (int i = 0; i < src.Length; i++)
                {
                    var temp = src[i];
                    temp.m_parentIndex++;
                    temp.m_firstChildIndex++;
                    em.SetComponentData(temp.entity, new RootReference { m_indexInHierarchy = i + 1, m_rootEntity = parent });
                    dst[i + 1]                                                                                    = temp;
                }

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredAllLeg = false;
                if (childRootIsAlive && createOrAppendLEG)
                {
                    bool hadLeg       = em.HasBuffer<LinkedEntityGroup>(parent);
                    var  dstLeg       = GetOrCreateLEG(em, parent);
                    var  childLeg     = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                    transferredAllLeg = true;
                    for (int i = 0; i < subtree.Length; i++)
                    {
                        var candidate = subtree[i].entity;
                        // Todo: Optimize this.
                        var index = childLeg.AsNativeArray().IndexOf(candidate);
                        if (index >= 0)
                        {
                            childLeg.RemoveAtSwapBack(index);
                            dstLeg.Add(candidate);
                        }
                        else
                            transferredAllLeg = false;
                    }
                    // If we added the LEG for nothing, undo that.
                    if (dstLeg.Length == 1 && !hadLeg)
                        em.RemoveComponent<LinkedEntityGroup>(parent);
                }
                if (!transferredAllLeg || em.HasBuffer<EntityInHierarchyCleanup>(parent))
                {
                    GetOrAddAndCopyCleanup(em, parent);
                }

                tsa.Dispose();
            }
        }

        static unsafe void AddInternalChildToRootParentSameRoot(EntityManager em, Entity parent, Entity child, InheritanceFlags flags)
        {
            var childRootRef = em.GetComponentData<RootReference>(child);
            var hierarchy    = GetHierarchy(em, parent, out var rootIsAlive);
            if (hierarchy[childRootRef.indexInHierarchy].childCount == 0)
            {
                RemoveSingleDescendant(em, hierarchy, childRootRef.indexInHierarchy);
                AddSingleChild(em, hierarchy, 0, child, flags);
            }
            else
            {
                var tsa     = ThreadStackAllocator.GetAllocator();
                var subtree = ExtractSubtree(ref tsa, hierarchy.AsNativeArray().AsReadOnlySpan(), childRootRef.indexInHierarchy);
                RemoveSubtree(em, hierarchy, childRootRef.indexInHierarchy, subtree);
                InsertSubtree(em, hierarchy, 0, subtree, flags);
                tsa.Dispose();
            }
        }

        static unsafe void AddInternalChildToRootParentSeparateRoot(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            var childRootRef    = em.GetComponentData<RootReference>(child);
            var childHierarchy  = GetHierarchy(em, childRootRef.rootEntity, out var childRootIsAlive);
            var parentHierarchy = em.GetBuffer<EntityInHierarchy>(parent);
            if (childHierarchy[childRootRef.indexInHierarchy].childCount == 0)
            {
                RemoveSingleDescendant(em, childHierarchy, childRootRef.indexInHierarchy);
                AddSingleChild(em, parentHierarchy, 0, child, flags);

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredLeg = false;
                if (childRootIsAlive && createOrAppendLEG)
                {
                    if (em.HasBuffer<LinkedEntityGroup>(childRootRef.rootEntity))
                    {
                        var childLeg = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                        var index    = childLeg.AsNativeArray().IndexOf(child);
                        if (index >= 0)
                        {
                            childLeg.RemoveAtSwapBack(index);
                            GetOrCreateLEG(em, parent).Add(child);
                            transferredLeg = true;
                        }
                    }
                }
                if (!transferredLeg || em.HasBuffer<EntityInHierarchyCleanup>(parent))
                {
                    GetOrAddAndCopyCleanup(em, parent);
                }
            }
            else
            {
                var tsa     = ThreadStackAllocator.GetAllocator();
                var subtree = ExtractSubtree(ref tsa, childHierarchy.AsNativeArray().AsReadOnlySpan(), childRootRef.indexInHierarchy);
                RemoveSubtree(em, childHierarchy, childRootRef.indexInHierarchy, subtree);
                InsertSubtree(em, parentHierarchy, 0, subtree, flags);

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredAllLeg = false;
                if (childRootIsAlive && createOrAppendLEG)
                {
                    if (em.HasBuffer<LinkedEntityGroup>(childRootRef.rootEntity))
                    {
                        bool hadLeg       = em.HasBuffer<LinkedEntityGroup>(parent);
                        var  dstLeg       = GetOrCreateLEG(em, parent);
                        var  childLeg     = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                        transferredAllLeg = true;
                        for (int i = 0; i < subtree.Length; i++)
                        {
                            var candidate = subtree[i].entity;
                            // Todo: Optimize this.
                            var index = childLeg.AsNativeArray().IndexOf(candidate);
                            if (index >= 0)
                            {
                                childLeg.RemoveAtSwapBack(index);
                                dstLeg.Add(candidate);
                            }
                            else
                                transferredAllLeg = false;
                        }
                        // If we added the LEG for nothing, undo that.
                        if (dstLeg.Length == 1 && !hadLeg)
                            em.RemoveComponent<LinkedEntityGroup>(parent);
                    }
                }
                if (!transferredAllLeg || em.HasBuffer<EntityInHierarchyCleanup>(parent))
                {
                    GetOrAddAndCopyCleanup(em, parent);
                }

                tsa.Dispose();
            }
        }

        static unsafe void AddInternalChildToInternalParentSameRoot(EntityManager em, Entity parent, Entity child, InheritanceFlags flags)
        {
            var childRootRef = em.GetComponentData<RootReference>(child);
            var hierarchy    = GetHierarchy(em, childRootRef.rootEntity, out var rootIsAlive);
            if (hierarchy[childRootRef.indexInHierarchy].childCount == 0)
            {
                RemoveSingleDescendant(em, hierarchy, childRootRef.indexInHierarchy);
                var parentRootRef = em.GetComponentData<RootReference>(parent);
                AddSingleChild(em, hierarchy, parentRootRef.indexInHierarchy, child, flags);
            }
            else
            {
                var tsa     = ThreadStackAllocator.GetAllocator();
                var subtree = ExtractSubtree(ref tsa, hierarchy.AsNativeArray().AsReadOnlySpan(), childRootRef.indexInHierarchy);
                RemoveSubtree(em, hierarchy, childRootRef.indexInHierarchy, subtree);

                var parentRootRef = em.GetComponentData<RootReference>(parent);
                InsertSubtree(em, hierarchy, parentRootRef.indexInHierarchy, subtree, flags);
                tsa.Dispose();
            }
        }

        static unsafe void AddInternalChildToInernalParentSeparateRoot(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool createOrAppendLEG)
        {
            var childRootRef    = em.GetComponentData<RootReference>(child);
            var parentRootRef   = em.GetComponentData<RootReference>(parent);
            var childHierarchy  = GetHierarchy(em, childRootRef.rootEntity, out var childRootIsAlive);
            var parentHierarchy = GetHierarchy(em, parentRootRef.rootEntity, out var parentRootIsAlive);
            if (childHierarchy[childRootRef.indexInHierarchy].childCount == 0)
            {
                RemoveSingleDescendant(em, childHierarchy, childRootRef.indexInHierarchy);
                AddSingleChild(em, parentHierarchy, parentRootRef.indexInHierarchy, child, flags);

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredLeg = false;
                if (parentRootIsAlive && childRootIsAlive && createOrAppendLEG)
                {
                    if (em.HasBuffer<LinkedEntityGroup>(childRootRef.rootEntity))
                    {
                        var childLeg = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                        var index    = childLeg.AsNativeArray().IndexOf(child);
                        if (index >= 0)
                        {
                            childLeg.RemoveAtSwapBack(index);
                            GetOrCreateLEG(em, parentRootRef.rootEntity).Add(child);
                            transferredLeg = true;
                        }
                    }
                }
                if (!transferredLeg || em.HasBuffer<EntityInHierarchyCleanup>(parentRootRef.rootEntity))
                {
                    GetOrAddAndCopyCleanup(em, parentRootRef.rootEntity);
                }
            }
            else
            {
                var tsa     = ThreadStackAllocator.GetAllocator();
                var subtree = ExtractSubtree(ref tsa, childHierarchy.AsNativeArray().AsReadOnlySpan(), childRootRef.indexInHierarchy);
                RemoveSubtree(em, childHierarchy, childRootRef.indexInHierarchy, subtree);
                InsertSubtree(em, parentHierarchy, parentRootRef.indexInHierarchy, subtree, flags);

                if (childHierarchy.Length == 1)
                {
                    em.RemoveComponent(childRootRef.rootEntity, new TypePack<EntityInHierarchy, EntityInHierarchyCleanup>());
                }

                bool transferredAllLeg = false;
                if (parentRootIsAlive && childRootIsAlive && createOrAppendLEG)
                {
                    if (em.HasBuffer<LinkedEntityGroup>(childRootRef.rootEntity))
                    {
                        bool hadLeg       = em.HasBuffer<LinkedEntityGroup>(parentRootRef.rootEntity);
                        var  dstLeg       = GetOrCreateLEG(em, parentRootRef.rootEntity);
                        var  childLeg     = em.GetBuffer<LinkedEntityGroup>(childRootRef.rootEntity).Reinterpret<Entity>();
                        transferredAllLeg = true;
                        for (int i = 0; i < subtree.Length; i++)
                        {
                            var candidate = subtree[i].entity;
                            // Todo: Optimize this.
                            var index = childLeg.AsNativeArray().IndexOf(candidate);
                            if (index >= 0)
                            {
                                childLeg.RemoveAtSwapBack(index);
                                dstLeg.Add(candidate);
                            }
                            else
                                transferredAllLeg = false;
                        }
                        // If we added the LEG for nothing, undo that.
                        if (dstLeg.Length == 1 && !hadLeg)
                            em.RemoveComponent<LinkedEntityGroup>(parentRootRef.rootEntity);
                    }
                }
                if (!transferredAllLeg || em.HasBuffer<EntityInHierarchyCleanup>(parentRootRef.rootEntity))
                {
                    GetOrAddAndCopyCleanup(em, parentRootRef.rootEntity);
                }

                tsa.Dispose();
            }
        }
        #endregion

        #region Algorithms
        static void ConvertSoloArchetypeToChildArchetype(EntityManager em, Entity parent, Entity child, InheritanceFlags flags)
        {
            ComponentTypeSet addToChild  = new TypePack<WorldTransform, RootReference>();
            bool             childHadLtw = em.HasComponent<WorldTransform>(child);
            em.AddComponent(child, addToChild);
            if (!childHadLtw)
                em.SetComponentData(child, new WorldTransform { worldTransform = TransformQvvs.identity });
            if ((flags & InheritanceFlags.CopyParent) == InheritanceFlags.CopyParent)
                em.SetComponentData(child, em.GetComponentData<WorldTransform>(parent));
            else if (!childHadLtw)
            {
                var parentTransform                    = em.GetComponentData<WorldTransform>(parent);
                parentTransform.worldTransform.stretch = 1f;
                em.SetComponentData(child, parentTransform);
            }
        }

        static void ConvertSoloArchetypeToRootArchetype(EntityManager em, Entity root, bool requireLEG, bool requireCleanup)
        {
            ComponentTypeSet addToRoot;
            if (requireCleanup)
            {
                if (requireLEG)
                    addToRoot = new TypePack<WorldTransform, EntityInHierarchy, LinkedEntityGroup, EntityInHierarchyCleanup>();
                else
                    addToRoot = new TypePack<WorldTransform, EntityInHierarchy, EntityInHierarchyCleanup>();
            }
            else
            {
                if (requireLEG)
                    addToRoot = new TypePack<WorldTransform, EntityInHierarchy, LinkedEntityGroup>();
                else
                    addToRoot = new TypePack<WorldTransform, EntityInHierarchy, EntityInHierarchyCleanup>();
            }
            bool parentHadLtw = em.HasComponent<WorldTransform>(root);
            em.AddComponent(root, addToRoot);
            if (!parentHadLtw)
                em.SetComponentData(root, new WorldTransform { worldTransform = TransformQvvs.identity });
        }

        static DynamicBuffer<Entity> GetOrCreateLEG(EntityManager em, Entity entity)
        {
            if (em.HasBuffer<LinkedEntityGroup>(entity))
                return em.GetBuffer<LinkedEntityGroup>(entity).Reinterpret<Entity>();
            else
            {
                var result = em.AddBuffer<LinkedEntityGroup>(entity).Reinterpret<Entity>();
                result.Add(entity);
                return result;
            }
        }

        static void AppendChildRootLEG(EntityManager em, DynamicBuffer<Entity> dstLeg, Entity childToTransfer)
        {
            if (em.HasBuffer<LinkedEntityGroup>(childToTransfer))
            {
                var srcLeg = em.GetBuffer<LinkedEntityGroup>(childToTransfer).Reinterpret<Entity>();
                // Ugly copy because Unity safety flags this
                for (int i = 0; i < srcLeg.Length; i++)
                    dstLeg.Add(srcLeg[i]);
            }
            else
                dstLeg.Add(childToTransfer);
        }

        static void GetOrAddAndCopyCleanup(EntityManager em, Entity root)
        {
            DynamicBuffer<EntityInHierarchyCleanup> cleanupBuffer;
            if (em.HasBuffer<EntityInHierarchyCleanup>(root))
                cleanupBuffer = em.GetBuffer<EntityInHierarchyCleanup>(root);
            else
                cleanupBuffer = em.AddBuffer<EntityInHierarchyCleanup>(root);
            var cleanup       = cleanupBuffer.Reinterpret<EntityInHierarchy>();
            cleanup.Clear();
            cleanup.AddRange(em.GetBuffer<EntityInHierarchy>(root).AsNativeArray());
        }

        static DynamicBuffer<EntityInHierarchy> GetHierarchy(EntityManager em, Entity root, out bool rootIsAlive)
        {
            if (em.HasBuffer<EntityInHierarchy>(root))
            {
                rootIsAlive = true;
                return em.GetBuffer<EntityInHierarchy>(root);
            }
            rootIsAlive = false;
            return em.GetBuffer<EntityInHierarchyCleanup>(root).Reinterpret<EntityInHierarchy>();
        }

        static unsafe ReadOnlySpan<EntityInHierarchy> ExtractSubtree(ref ThreadStackAllocator tsa, ReadOnlySpan<EntityInHierarchy> srcHierarchy, int subtreeRootIndex)
        {
            var maxDescendantsCount = srcHierarchy.Length - subtreeRootIndex;
            var extractionList      = new UnsafeList<EntityInHierarchy>(tsa.Allocate<EntityInHierarchy>(maxDescendantsCount), maxDescendantsCount);

            extractionList.Clear();  // The list initializer we are using sets both capacity and length.

            //descendantsToMove.Add((child, -1));
            extractionList.Add(new EntityInHierarchy
            {
                m_descendantEntity = srcHierarchy[subtreeRootIndex].entity,
                m_parentIndex      = -1,
                m_childCount       = srcHierarchy[subtreeRootIndex].childCount,
                m_firstChildIndex  = int.MaxValue,
                m_flags            = default
            });
            // The root is the first level. For each subsequent level, we iterate the entities added during the previous level.
            // And then we add their children.
            int firstParentInLevel               = 0;
            int parentCountInLevel               = 1;
            int firstParentHierarchyIndexInLevel = subtreeRootIndex;
            while (parentCountInLevel > 0)
            {
                var firstParentInNextLevel               = extractionList.Length;
                var parentCountInNextLevel               = 0;
                int firstParentHierarchyIndexInNextLevel = 0;
                for (int parentIndex = 0; parentIndex < parentCountInLevel; parentIndex++)
                {
                    var dstParentIndex    = parentIndex + firstParentInLevel;
                    var parentInHierarchy = srcHierarchy[firstParentHierarchyIndexInLevel + parentIndex];
                    if (parentIndex == 0)
                        firstParentHierarchyIndexInNextLevel  = parentInHierarchy.firstChildIndex;
                    parentCountInLevel                       += parentInHierarchy.childCount;
                    for (int i = 0; i < parentInHierarchy.childCount; i++)
                    {
                        var oldElement               = srcHierarchy[parentInHierarchy.firstChildIndex + i];
                        oldElement.m_firstChildIndex = int.MaxValue;
                        oldElement.m_parentIndex     = dstParentIndex;
                        extractionList.Add(oldElement);
                    }
                }
                firstParentInLevel               = firstParentInNextLevel;
                parentCountInLevel               = parentCountInNextLevel;
                firstParentHierarchyIndexInLevel = firstParentHierarchyIndexInNextLevel;
            }

            var result = new Span<EntityInHierarchy>(extractionList.Ptr, extractionList.Length);
            for (int i = 1; i < result.Length; i++)
            {
                var     child           = result[i];
                ref var firstChildIndex = ref result[child.parentIndex].m_firstChildIndex;
                firstChildIndex         = math.min(firstChildIndex, i);
            }
            return result;
        }

        static unsafe void RemoveSingleDescendant(EntityManager em, DynamicBuffer<EntityInHierarchy> hierarchy, int indexToRemove)
        {
            var parentIndex = hierarchy[indexToRemove].parentIndex;
            hierarchy.RemoveAt(indexToRemove);

            var hierarchyArray       = hierarchy.AsNativeArray();
            var root                 = hierarchyArray[0].entity;
            var oldParentInHierarchy = hierarchyArray[parentIndex];
            oldParentInHierarchy.m_childCount--;
            hierarchyArray[parentIndex] = oldParentInHierarchy;

            for (int i = parentIndex + 1; i < hierarchyArray.Length; i++)
            {
                var temp = hierarchyArray[i];
                temp.m_firstChildIndex--;
                if (i >= indexToRemove && em.HasComponent<RootReference>(temp.entity))
                    em.SetComponentData(temp.entity, new RootReference { m_indexInHierarchy = i, m_rootEntity = root });
                hierarchyArray[i]                                                                             = temp;
            }
        }

        static unsafe void RemoveSubtree(EntityManager em,
                                         DynamicBuffer<EntityInHierarchy> hierarchyToRemoveFrom,
                                         int subtreeRootIndex,
                                         ReadOnlySpan<EntityInHierarchy>  extractedSubtree)
        {
            // Start by decreasing the old parent's child count
            var oldHierarchyArray    = hierarchyToRemoveFrom.AsNativeArray();
            var subtreeParentIndex   = oldHierarchyArray[subtreeRootIndex].parentIndex;
            var oldParentInHierarchy = oldHierarchyArray[subtreeParentIndex];
            oldParentInHierarchy.m_childCount--;
            oldHierarchyArray[subtreeParentIndex] = oldParentInHierarchy;

            // Next, offset any entity first child indices for entities that are prior to the removed child
            for (int i = subtreeParentIndex + 1; i < subtreeRootIndex; i++)
            {
                var temp = oldHierarchyArray[i];
                temp.m_firstChildIndex--;
                oldHierarchyArray[i] = temp;
            }

            // Filter out the subtree in order.
            var dst   = subtreeRootIndex;
            var match = 1;
            var root  = oldHierarchyArray[0].entity;
            for (int src = dst + 1; src < oldHierarchyArray.Length; src++)
            {
                var srcData = oldHierarchyArray[src];
                if (srcData.entity == extractedSubtree[match].entity)
                {
                    match++;
                    continue;
                }
                srcData.m_firstChildIndex                                            -= src - dst;
                oldHierarchyArray[dst]                                                = srcData;
                em.SetComponentData(srcData.entity, new RootReference { m_rootEntity  = root, m_indexInHierarchy = dst });
                dst++;
            }
            hierarchyToRemoveFrom.Length = dst;
        }

        static unsafe void AddSingleChild(EntityManager em, DynamicBuffer<EntityInHierarchy> hierarchy, int parentIndex, Entity child, InheritanceFlags flags)
        {
            var     root                 = hierarchy[0].entity;
            ref var newParentInHierarchy = ref hierarchy.ElementAt(parentIndex);
            var     insertionPoint       = newParentInHierarchy.firstChildIndex + newParentInHierarchy.childCount;
            newParentInHierarchy.m_childCount++;
            if (insertionPoint == hierarchy.Length)
            {
                hierarchy.Add(new EntityInHierarchy
                {
                    m_childCount       = 0,
                    m_descendantEntity = child,
                    m_firstChildIndex  = insertionPoint + 1,
                    m_flags            = flags,
                    m_parentIndex      = parentIndex
                });
                em.SetComponentData(child, new RootReference { m_rootEntity = root, m_indexInHierarchy = insertionPoint });
            }
            else
            {
                var newFirstChildIndex = hierarchy[insertionPoint].firstChildIndex;
                hierarchy.Insert(insertionPoint, new EntityInHierarchy
                {
                    m_childCount       = 0,
                    m_descendantEntity = child,
                    m_firstChildIndex  = newFirstChildIndex,
                    m_flags            = flags,
                    m_parentIndex      = parentIndex
                });
                var hierarchyArray = hierarchy.AsNativeArray().AsSpan();
                for (int i = parentIndex + 1; i < hierarchyArray.Length; i++)
                {
                    ref var element = ref hierarchyArray[i];
                    element.m_firstChildIndex++;
                    if (element.parentIndex >= insertionPoint)
                        element.m_parentIndex++;
                    em.SetComponentData(element.entity, new RootReference { m_rootEntity = root, m_indexInHierarchy = i });
                }
            }
        }

        static unsafe void InsertSubtree(EntityManager em,
                                         DynamicBuffer<EntityInHierarchy> hierarchy,
                                         int parentIndex,
                                         ReadOnlySpan<EntityInHierarchy>  subtree,
                                         InheritanceFlags flags)
        {
            var hierarchyOriginalLength  = hierarchy.Length;
            hierarchy.Length            += subtree.Length;
            ref var parentInHierarchy    = ref hierarchy.ElementAt(parentIndex);
            var     insertionPoint       = parentInHierarchy.firstChildIndex + parentInHierarchy.childCount;
            parentInHierarchy.m_childCount++;
            var hierarchyArray = hierarchy.AsNativeArray().AsSpan();
            var root           = hierarchyArray[0].entity;
            var child          = subtree[0].entity;
            if (insertionPoint == hierarchyOriginalLength)
            {
                // We are appending the new child to the end, which means we can just copy the whole hierarchy.
                var childElement                                                   = subtree[0];
                childElement.m_parentIndex                                         = parentIndex;
                childElement.m_firstChildIndex                                    += insertionPoint;
                childElement.m_flags                                               = flags;
                em.SetComponentData(child, new RootReference { m_indexInHierarchy  = insertionPoint, m_rootEntity = root });
                hierarchyArray[insertionPoint]                                     = childElement;
                for (int i = 1; i < subtree.Length; i++)
                {
                    childElement                                                       = subtree[i];
                    childElement.m_parentIndex                                        += insertionPoint;
                    childElement.m_firstChildIndex                                    += insertionPoint;
                    em.SetComponentData(child, new RootReference { m_indexInHierarchy  = insertionPoint + i, m_rootEntity = root });
                    hierarchyArray[insertionPoint + i]                                 = childElement;
                }
            }
            else
            {
                // Move elements starting at the insertion point to the back
                for (int i = hierarchyOriginalLength - 1; i >= insertionPoint; i--)
                {
                    var src             = i;
                    var dst             = src + subtree.Length;
                    hierarchyArray[dst] = hierarchyArray[src];
                }

                // Adjust first child index of parents preceeding the inserted child
                int existingChildrenToAdd = 0;
                for (int i = parentIndex + 1; i < insertionPoint; i++)
                {
                    var parentElement = hierarchyArray[i];
                    parentElement.m_firstChildIndex++;
                    existingChildrenToAdd += parentElement.childCount;
                    hierarchyArray[i]      = parentElement;
                }

                // Add the new child
                var newChildElement                                               = subtree[0];
                newChildElement.m_parentIndex                                     = parentIndex;
                newChildElement.m_firstChildIndex                                 = insertionPoint + existingChildrenToAdd;
                newChildElement.m_flags                                           = flags;
                int newChildrenToAdd                                              = newChildElement.childCount;
                em.SetComponentData(child, new RootReference { m_indexInHierarchy = insertionPoint, m_rootEntity = root });
                hierarchyArray[insertionPoint]                                    = newChildElement;

                // Merge the hierarchies by alternating based on accumulated children batches
                int existingChildrenParentShift = 0;
                int existingChildrenChildShift  = 1 + newChildrenToAdd;
                int existingChildRunningIndex   = insertionPoint + subtree.Length;
                int newChildrenLastAdded        = 1;
                int newChildrenParentShift      = insertionPoint;
                int newChildrenChildShift       = insertionPoint + existingChildrenToAdd;
                int newChildRunningIndex        = 1;
                int runningDst                  = insertionPoint + 1;

                while (newChildrenToAdd + existingChildrenToAdd > 0)
                {
                    int nextExistingChildrenToAdd = 0;
                    for (int i = 0; i < existingChildrenToAdd; i++)
                    {
                        var existingElement                                                                 = hierarchyArray[existingChildRunningIndex];
                        existingElement.m_parentIndex                                                      += existingChildrenParentShift;
                        existingElement.m_firstChildIndex                                                  += existingChildrenChildShift;
                        nextExistingChildrenToAdd                                                          += existingElement.childCount;
                        em.SetComponentData(existingElement.entity, new RootReference { m_indexInHierarchy  = runningDst, m_rootEntity = root });
                        hierarchyArray[runningDst]                                                          = existingElement;
                        existingChildRunningIndex++;
                        runningDst++;
                    }
                    newChildrenChildShift       += nextExistingChildrenToAdd;
                    existingChildrenParentShift += newChildrenLastAdded;

                    int nextNewChildrenToAdd = 0;
                    for (int i = 0; i < newChildrenToAdd; i++)
                    {
                        var newElement                                                                 = subtree[newChildRunningIndex];
                        newElement.m_parentIndex                                                      += newChildrenParentShift;
                        newElement.m_firstChildIndex                                                  += newChildrenChildShift;
                        nextNewChildrenToAdd                                                          += newElement.childCount;
                        em.SetComponentData(newElement.entity, new RootReference { m_indexInHierarchy  = runningDst, m_rootEntity = root });
                        hierarchyArray[runningDst]                                                     = newElement;
                        newChildRunningIndex++;
                        runningDst++;
                    }
                    existingChildrenChildShift += nextNewChildrenToAdd;
                    newChildrenParentShift     += existingChildrenToAdd;
                    newChildrenLastAdded        = newChildrenToAdd;
                    newChildrenToAdd            = nextNewChildrenToAdd;
                    existingChildrenToAdd       = nextExistingChildrenToAdd;
                }
            }
        }
        #endregion

        #region
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckChangeParent(EntityManager em, Entity parent, Entity child, InheritanceFlags flags, bool transferLEG)
        {
            if (parent == child)
                throw new ArgumentException("Cannot make an entity a child of itself");
            if (!em.Exists(parent))
                throw new ArgumentException("The parent does not exist");
            if (em.GetChunk(parent).Archetype.IsCleanup())
                throw new ArgumentException("The parent has been destroyed");
            if (!em.Exists(child))
                throw new ArgumentException("The child does not exist");
            if (em.GetChunk(child).Archetype.IsCleanup())
                throw new ArgumentException("The child has been destroyed");
            if (transferLEG && em.HasComponent<RootReference>(parent))
            {
                var rootRef = em.GetComponentData<RootReference>(parent);
                if (em.GetChunk(rootRef.rootEntity).Archetype.IsCleanup())
                    throw new InvalidOperationException(
                        $"Cannot transfer LinkedEntityGroup to a new hierarchy whose root has been destroyed. Root: {rootRef.rootEntity.ToFixedString()}");
            }
        }
        #endregion
    }
}

