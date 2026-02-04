using System.Threading.Tasks;
using Common.Interfaces;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    public class SceneLoaderAsync
    {
        private readonly ILogger _logger;

        public SceneLoaderAsync(ILogger logger)
        {
            _logger = logger;
        }

        public async Task LoadSceneAsync(string sceneName, float delayBeforeActivation = 0f)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                _logger?.LogError("Scene name cannot be null or empty");
                return;
            }

            var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                _logger?.LogError($"Failed to start loading scene: {sceneName}");
                return;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                await Task.Yield();
            }

            if (delayBeforeActivation > 0f)
            {
                await Task.Delay((int)(delayBeforeActivation * 1000));
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            _logger?.LogInfo($"Scene {sceneName} loaded successfully");
        }

        public async Task LoadSceneAsync(SceneLoadingParameters parameters)
        {
            await LoadSceneAsync(parameters.SceneName, parameters.DelayBeforeActivation);
        }
    }
}
