using SceneManagement;
using UnityEngine;
using VContainer;
using System.Threading.Tasks;

namespace Intro
{
    public class SceneAutoLoader : MonoBehaviour
    {
        [Inject] private SceneManager SceneManager { get; set; }
        
        [Header("Scene Loading Settings")]
        [SerializeField] private string sceneToLoad;
        [SerializeField] private float delayBeforeActivation = 1f;

        async void Start()
        {
            var loadingParams = SceneLoadingParameters.CreateWithDelay(
                sceneName: sceneToLoad,
                delay: delayBeforeActivation
            );
            await SceneManager.LoadSceneAsync(loadingParams);
        }
    }
}