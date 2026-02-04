using VContainer;
using VContainer.Unity;
using ShaveRunner;

namespace DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GPUGrassRenderer>();
            builder.RegisterComponentInHierarchy<GrassCutter>();
            builder.RegisterComponentInHierarchy<PlayerController>();
            builder.RegisterComponentInHierarchy<CameraController>();
            builder.RegisterComponentInHierarchy<LevelManager>();
        }
    }
}
