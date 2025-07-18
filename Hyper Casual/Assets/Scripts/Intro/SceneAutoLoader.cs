using SceneManagement;
using UnityEngine;
using VContainer;

namespace Intro
{
    public class SceneAutoLoader : MonoBehaviour
    {
        [Inject] private SceneManager SceneManager { get; set; }
        
        [Header("Scene Loading Settings")]
        [SerializeField] private string sceneToLoad;
        [SerializeField] private float delayBeforeActivation = 1f;

        void Start()
        {
            var loadingParams = SceneLoadingParameters.CreateWithDelay(
                sceneName: sceneToLoad,
                delay: delayBeforeActivation
            );
            SceneManager.LoadScene(loadingParams);
        }
    }
}