using System.Threading.Tasks;
using Common.Interfaces;
using VContainer;

namespace SceneManagement
{
    public class SceneManager
    {
        private readonly ILogger _logger;
        private readonly SceneLoaderAsync _asyncLoader;
        private GameParameters _currentParameters;
        
        public SceneManager(ILogger logger)
        {
            _logger = logger;
            _asyncLoader = new SceneLoaderAsync(logger);
        }
        
        public async Task LoadSceneAsync(SceneLoadingParameters parameters)
        {
            _currentParameters = parameters.GameParameters;
            await _asyncLoader.LoadSceneAsync(parameters);
        }
        
        public void LoadScene(SceneLoadingParameters parameters)
        {
            _ = LoadSceneAsync(parameters);
        }
        
        public GameParameters GetGameParameters()
        {
            var parameters = _currentParameters;
            _currentParameters = null;
            return parameters;
        }
    }
}
