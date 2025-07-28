using SceneManagement;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CoroutineRunner = Common.CoroutineRunner;
using Common.Interfaces;
using Common.Services;

namespace DI
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Core Services
            builder.RegisterComponentOnNewGameObject<CoroutineRunner>(Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<SceneManager>(Lifetime.Singleton);
            
            // Configuration System
            builder.Register<ConfigurationService>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // Game State Management
            builder.Register<GameStateManager>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // Event System
            builder.Register<EventBus>(Lifetime.Singleton).AsImplementedInterfaces();
            
            // Input System
            builder.RegisterComponentOnNewGameObject<InputService>(Lifetime.Singleton).DontDestroyOnLoad()
                .AsImplementedInterfaces();
                
            
            // Object Pooling
            builder.RegisterComponentOnNewGameObject<ObjectPoolService>(Lifetime.Singleton).DontDestroyOnLoad()
                .AsImplementedInterfaces();
            
            // TODO: Add analytics, save system, audio manager, etc.
            // builder.Register<AnalyticsService>(Lifetime.Singleton).AsImplementedInterfaces();
            // builder.Register<SaveService>(Lifetime.Singleton).AsImplementedInterfaces();
            // builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
