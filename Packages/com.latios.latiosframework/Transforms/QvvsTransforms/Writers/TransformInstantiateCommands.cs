using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Latios.Transforms
{
    /// <summary>
    /// An IInstantiateCommand to set the root transform of the solo entity or instantiated hierarchy.
    /// This sets both the WorldTransform and TickedWorldTransform depending on which are present.
    /// </summary>
    [BurstCompile]
    public struct WorldTransformCommand : IInstantiateCommand
    {
        public WorldTransformCommand(TransformQvvs newWorldTransform)
        {
            this.newWorldTransform = newWorldTransform;
        }

        public TransformQvvs newWorldTransform;

        public FunctionPointer<IInstantiateCommand.OnPlayback> GetFunctionPointer()
        {
            return BurstCompiler.CompileFunctionPointer<IInstantiateCommand.OnPlayback>(OnPlayback);
        }

        [MonoPInvokeCallback(typeof(IInstantiateCommand.OnPlayback))]
        [BurstCompile]
        static void OnPlayback(ref IInstantiateCommand.Context context)
        {
            var entities = context.entities;
            var em       = context.entityManager;
            for (int i = 0; i < entities.Length; i++)
            {
                var entity    = entities[i];
                var transform = context.ReadCommand<WorldTransformCommand>(i);
                if (em.HasComponent<WorldTransform>(entity))
                    TransformTools.SetWorldTransform(entity, transform.newWorldTransform, em);
                if (em.HasComponent<TickedWorldTransform>(entity))
                    TransformTools.SetTickedWorldTransform(entity, transform.newWorldTransform, em);
            }
        }
    }
}

