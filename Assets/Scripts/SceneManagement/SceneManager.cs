using System.Collections;
using Common;
using UnityEngine;
using VContainer;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace SceneManagement
{
    public class SceneManager : MonoBehaviour
    {
        [Inject] private CoroutineRunner CoroutineRunner { get; set; }
        
        private GameParameters _currentParameters;
        private AsyncOperation _loadOperation;
        
        
        public void LoadScene(SceneLoadingParameters parameters)
        {
            _currentParameters = parameters.GameParameters;
            CoroutineRunner.StartCoroutine(LoadSceneCoroutine(parameters));
        }
        
        public GameParameters GetGameParameters()
        {
            var parameters = _currentParameters;
            _currentParameters = null; // Clear after getting to prevent stale data
            return parameters;
        }
        
        private IEnumerator LoadSceneCoroutine(SceneLoadingParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.SceneName))
            {
                Debug.LogError("Scene name cannot be null or empty!");
                yield break;
            }

            Debug.Log($"Starting to load scene: {parameters.SceneName}");

            // Start loading the scene asynchronously
            _loadOperation = USceneManager.LoadSceneAsync(parameters.SceneName);
            
            if (_loadOperation == null)
            {
                Debug.LogError($"Failed to start loading scene: {parameters.SceneName}");
                yield break;
            }

            // Prevent the scene from activating immediately
            _loadOperation.allowSceneActivation = false;

            // Wait until the scene is loaded (but not activated)
            while (!_loadOperation.isDone && _loadOperation.progress < 0.9f)
            {
                if (parameters.ShowProgress)
                {
                    var progress = _loadOperation.progress * 100f;
                    Debug.Log($"Loading progress: {progress:F1}%");
                }
                yield return null;
            }

            Debug.Log($"Scene {parameters.SceneName} loaded, waiting {parameters.DelayBeforeActivation} seconds before activation...");

            // Wait for the specified delay
            if (parameters.DelayBeforeActivation > 0f)
            {
                yield return new WaitForSeconds(parameters.DelayBeforeActivation);
            }

            _loadOperation.allowSceneActivation = true;
            Debug.Log($"Scene {parameters.SceneName} activated!");
        }
    }
} 