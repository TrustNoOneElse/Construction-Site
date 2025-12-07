using System;
using System.Diagnostics;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    public static partial class TransformTools
    {
        public static unsafe void AddChild(this EntityManager em,
                                           Entity parent,
                                           Entity child,
                                           InheritanceFlags inheritanceFlags          = InheritanceFlags.Normal,
                                           bool transferLinkedEntityGroup = true)
        {
            CheckNotParentingChildToSelf(parent, child);
            var oldRootReference    = em.HasComponent<RootReference>(child) ? em.GetComponentData<RootReference>(child) : default;
            var parentRootReference = em.HasComponent<RootReference>(parent) ? em.GetComponentData<RootReference>(parent) : default;
            if (oldRootReference.rootEntity == Entity.Null)
            {
                // Try to add the RootReference to the new child. If the new child is dead, this will fail before we mess up the hierarchies further.
                em.AddComponent<RootReference>(child);
            }
            if (parentRootReference.rootEntity == Entity.Null)
            {
                // Try to add the buffer to the new parent. If the new parent is dead, this will fail before we mess up the hierarchies further.
                em.AddBuffer<EntityInHierarchy>(parent);
                parentRootReference.m_rootEntity = parent;
            }

            UnsafeList<(Entity, int)> descendantsToMove   = default;
            ThreadStackAllocator      tsa                 = default;
            bool                      removedChildFromOld = false;
            if (oldRootReference.rootEntity != Entity.Null && oldRootReference.rootEntity != parentRootReference.rootEntity)
            {
                // Remove the child from its old hierarchy.
                removedChildFromOld                             = true;
                var                              oldRootIsAlive = em.HasBuffer<EntityInHierarchy>(oldRootReference.rootEntity);
                DynamicBuffer<EntityInHierarchy> oldHierarchy;
                if (oldRootIsAlive)
                    oldHierarchy = em.GetBuffer<EntityInHierarchy>(oldRootReference.rootEntity, false);
                else
                    oldHierarchy        = em.GetBuffer<EntityInHierarchyCleanup>(oldRootReference.rootEntity, false).Reinterpret<EntityInHierarchy>();
                var oldChildInHierarchy = oldHierarchy[oldRootReference.indexInHierarchy];

                if (oldChildInHierarchy.childCount == 0)
                {
                    // Fast path, as there's no descendants to juggle.
                    oldHierarchy.RemoveAt(oldRootReference.indexInHierarchy);
                    var oldHierarchyArray    = oldHierarchy.AsNativeArray();
                    var oldParentInHierarchy = oldHierarchyArray[oldChildInHierarchy.parentIndex];
                    oldParentInHierarchy.m_childCount--;
                    oldHierarchyArray[oldChildInHierarchy.parentIndex] = oldParentInHierarchy;
                    for (int i = oldChildInHierarchy.parentIndex + 1; i < oldHierarchyArray.Length; i++)
                    {
                        var temp = oldHierarchyArray[i];
                        temp.m_firstChildIndex--;
                        if (i >= oldRootReference.indexInHierarchy && em.HasComponent<RootReference>(temp.entity))
                            em.SetComponentData(temp.entity, new RootReference { m_indexInHierarchy = i, m_rootEntity = oldRootReference.rootEntity });
                        oldHierarchyArray[i]                                                                          = temp;
                    }
                }
                else
                {
                    var oldHierarchyArray = oldHierarchy.AsNativeArray();
                    // Extract the subtree of the child from the old hierarchy
                    {
                        // Allocate the extracted subtree
                        var maxDescendantsCount = oldHierarchy.Length - oldRootReference.indexInHierarchy;
                        if (maxDescendantsCount > 512)
                        {
                            tsa               = ThreadStackAllocator.GetAllocator();
                            descendantsToMove = new UnsafeList<(Entity, int)>(tsa.Allocate<(Entity, int)>(maxDescendantsCount), maxDescendantsCount);
                        }
                        else
                        {
                            (Entity, int) * ptr = stackalloc (Entity, int)[maxDescendantsCount];
                            descendantsToMove   = new UnsafeList<(Entity, int)>(ptr, maxDescendantsCount);
                        }
                        descendantsToMove.Clear();  // The list initializer we are using sets both capacity and length.

                        descendantsToMove.Add((child, -1));
                        // The root is the first level. For each subsequent level, we iterate the entities added during the previous level.
                        // And then we add their children.
                        int firstParentInLevel               = 0;
                        int parentCountInLevel               = 1;
                        int firstParentHierarchyIndexInLevel = oldRootReference.indexInHierarchy;
                        while (parentCountInLevel > 0)
                        {
                            var firstParentInNextLevel               = descendantsToMove.Length;
                            var parentCountInNextLevel               = 0;
                            int firstParentHierarchyIndexInNextLevel = 0;
                            for (int parentIndex = 0; parentIndex < parentCountInLevel; parentIndex++)
                            {
                                var dstParentIndex    = parentIndex + firstParentInLevel;
                                var parentInHierarchy = oldHierarchyArray[firstParentHierarchyIndexInLevel + parentIndex];
                                if (parentIndex == 0)
                                    firstParentHierarchyIndexInNextLevel  = parentInHierarchy.firstChildIndex;
                                parentCountInLevel                       += parentInHierarchy.childCount;
                                for (int i = 0; i < parentInHierarchy.childCount; i++)
                                {
                                    descendantsToMove.Add((oldHierarchyArray[parentInHierarchy.firstChildIndex + i].entity, dstParentIndex));
                                }
                            }
                            firstParentInLevel               = firstParentInNextLevel;
                            parentCountInLevel               = parentCountInNextLevel;
                            firstParentHierarchyIndexInLevel = firstParentHierarchyIndexInNextLevel;
                        }
                    }
                    // Now remove the subtree from the old hierarchy
                    {
                        // Start by decreasing the old parent's child count
                        var oldParentInHierarchy = oldHierarchyArray[oldChildInHierarchy.parentIndex];
                        oldParentInHierarchy.m_childCount--;
                        oldHierarchyArray[oldChildInHierarchy.parentIndex] = oldParentInHierarchy;

                        // Next, offset any entity first child indices for entities that are prior to the removed child
                        for (int i = oldChildInHierarchy.parentIndex + 1; i < oldRootReference.indexInHierarchy; i++)
                        {
                            var temp = oldHierarchyArray[i];
                            temp.m_firstChildIndex--;
                            oldHierarchyArray[i] = temp;
                        }

                        // Filter out the subtree in order.
                        var dst   = oldRootReference.indexInHierarchy;
                        var match = 1;
                        for (int src = dst + 1; src < oldHierarchyArray.Length; src++)
                        {
                            var srcData = oldHierarchyArray[src];
                            if (srcData.entity == descendantsToMove[match].Item1)
                            {
                                match++;
                                continue;
                            }
                            srcData.m_firstChildIndex                                            -= src - dst;
                            oldHierarchyArray[dst]                                                = srcData;
                            em.SetComponentData(srcData.entity, new RootReference { m_rootEntity  = oldRootReference.m_rootEntity, m_indexInHierarchy = dst });
                            dst++;
                        }
                        oldHierarchy.Length = dst;
                    }
                }

                // Copy to cleanup if necessary
                if (oldRootIsAlive && em.HasBuffer<EntityInHierarchyCleanup>(oldRootReference.rootEntity))
                {
                    var cleanup = em.GetBuffer<EntityInHierarchyCleanup>(oldRootReference.rootEntity).Reinterpret<EntityInHierarchy>();
                    cleanup.Clear();
                    cleanup.AddRange(oldHierarchy.AsNativeArray());
                }
            }

            var                              newRootIsAlive = em.HasBuffer<EntityInHierarchy>(parentRootReference.rootEntity);
            DynamicBuffer<EntityInHierarchy> newHierarchy;
            if (newRootIsAlive)
                newHierarchy = em.GetBuffer<EntityInHierarchy>(parentRootReference.rootEntity, false);
            else
                newHierarchy = em.GetBuffer<EntityInHierarchyCleanup>(parentRootReference.rootEntity, false).Reinterpret<EntityInHierarchy>();

            bool childUsedToBeRootWithChildren = em.HasBuffer<EntityInHierarchy>(child);
            if (!childUsedToBeRootWithChildren && !descendantsToMove.IsCreated)
            {
                // Fast path. We only need to attach the single entity.
                if (newHierarchy.IsEmpty)
                {
                    // We just added this buffer. Initialize it directly with the new child.
                    newHierarchy.Add(new EntityInHierarchy
                    {
                        m_childCount       = 1,
                        m_descendantEntity = parent,
                        m_firstChildIndex  = 1,
                        m_flags            = InheritanceFlags.Normal,
                        m_parentIndex      = -1
                    });
                    newHierarchy.Add(new EntityInHierarchy
                    {
                        m_childCount       = 0,
                        m_descendantEntity = child,
                        m_firstChildIndex  = 2,
                        m_flags            = inheritanceFlags,
                        m_parentIndex      = 0
                    });
                }
                else
                {
                    ref var newParentInHierarchy = ref newHierarchy.ElementAt(parentRootReference.indexInHierarchy);
                    var     insertionPoint       = newParentInHierarchy.firstChildIndex + newParentInHierarchy.childCount;
                    newParentInHierarchy.m_childCount++;
                    if (insertionPoint == newHierarchy.Length)
                    {
                        newHierarchy.Add(new EntityInHierarchy
                        {
                            m_childCount       = 0,
                            m_descendantEntity = child,
                            m_firstChildIndex  = newHierarchy.Length + 1,
                            m_flags            = inheritanceFlags,
                            m_parentIndex      = parentRootReference.indexInHierarchy
                        });
                    }
                    {
                        var newFirstChildIndex = newHierarchy[insertionPoint].firstChildIndex;
                        newHierarchy.Insert(insertionPoint, new EntityInHierarchy
                        {
                            m_childCount       = 0,
                            m_descendantEntity = child,
                            m_firstChildIndex  = newFirstChildIndex,
                            m_flags            = inheritanceFlags,
                            m_parentIndex      = parentRootReference.indexInHierarchy
                        });
                        var newHierarchyArray = newHierarchy.AsNativeArray();
                        for (int i = parentRootReference.indexInHierarchy + 1; i < newHierarchyArray.Length; i++)
                        {
                            var temp = newHierarchyArray[i];
                            temp.m_firstChildIndex++;
                            newHierarchyArray[i] = temp;
                        }
                    }
                }

                // Either fix up LinkedEntityGroup, or append the Cleanup if necessary
                if (transferLinkedEntityGroup)
                {
                    DynamicBuffer<LinkedEntityGroup> newLeg;
                    if (em.HasBuffer<LinkedEntityGroup>(parentRootReference.rootEntity))
                        newLeg = em.GetBuffer<LinkedEntityGroup>(parentRootReference.rootEntity, false);
                    else
                    {
                        newLeg                                   = em.AddBuffer<LinkedEntityGroup>(parentRootReference.rootEntity);
                        newLeg.Add(new LinkedEntityGroup { Value = oldRootReference.rootEntity });
                    }
                    newLeg.Add(new LinkedEntityGroup { Value = child });

                    if (removedChildFromOld && em.HasBuffer<LinkedEntityGroup>(oldRootReference.rootEntity))
                    {
                        var oldLeg      = em.GetBuffer<LinkedEntityGroup>(oldRootReference.rootEntity, false);
                        var oldLegArray = oldLeg.AsNativeArray().Reinterpret<Entity>();
                        var index       = oldLegArray.IndexOf(child);
                        if (index >= 0)
                            oldLeg.RemoveAtSwapBack(index);
                    }
                }
                else if (newRootIsAlive)
                {
                    DynamicBuffer<EntityInHierarchyCleanup> cleanupBuffer;
                    if (em.HasBuffer<EntityInHierarchyCleanup>(parentRootReference.rootEntity))
                        cleanupBuffer = em.GetBuffer<EntityInHierarchyCleanup>(parentRootReference.rootEntity, false);
                    else
                        cleanupBuffer = em.AddBuffer<EntityInHierarchyCleanup>(parentRootReference.rootEntity);
                    var buffer        = cleanupBuffer.Reinterpret<EntityInHierarchy>();
                    buffer.Clear();
                    buffer.AddRange(newHierarchy.AsNativeArray());
                }
            }
            else
            {
                // Todo: Need to merge hierarchies, then process LEGs/Cleanups
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckNotParentingChildToSelf(Entity parent, Entity child)
        {
            if (parent == child)
                throw new ArgumentException("Cannot make an entity a child of itself");
        }
    }
}

