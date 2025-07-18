// using Games.Core;
using SceneManagement;
// using Streak;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CoroutineRunner = Common.CoroutineRunner;

namespace DI
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<CoroutineRunner>(Lifetime.Singleton).DontDestroyOnLoad();
            builder.Register<SceneManager>(Lifetime.Singleton);
            // builder.Register<StreakManager>(Lifetime.Singleton);
            //
            // builder.Register<GameResultNotifier>(Lifetime.Singleton).AsImplementedInterfaces();
            // builder.Register<ScoreChangesNotifier>(Lifetime.Singleton).AsImplementedInterfaces();
            //
            // builder.Register<GameResultControllerMock>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
