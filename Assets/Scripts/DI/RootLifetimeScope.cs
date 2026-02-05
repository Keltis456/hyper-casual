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
            builder.RegisterComponentOnNewGameObject<CoroutineRunner>(Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<SceneManager>(Lifetime.Singleton);
            
            builder.Register<ConfigurationService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LoggerService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<GameStateManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<ComponentFactory>(Lifetime.Singleton).AsImplementedInterfaces();
            
            builder.RegisterComponentOnNewGameObject<InputService>(Lifetime.Singleton).DontDestroyOnLoad()
                .AsImplementedInterfaces();
            
            builder.RegisterComponentOnNewGameObject<ObjectPoolService>(Lifetime.Singleton).DontDestroyOnLoad()
                .AsImplementedInterfaces();
        }
    }
}
