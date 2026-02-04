using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [Header("Game References")]
        [SerializeField] private GPUGrassRenderer grassRenderer;
        [SerializeField] private ImprovedGrassCutter grassCutter;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // Register game-specific components if they exist
            if (grassRenderer != null)
            {
                builder.RegisterInstance(grassRenderer);
            }
            
            if (grassCutter != null)
            {
                builder.RegisterInstance(grassCutter);
            }
            
            // Register game-specific services here
            // Example: builder.Register<ScoreService>(Lifetime.Scoped).AsImplementedInterfaces();
            // Example: builder.Register<PowerUpService>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
