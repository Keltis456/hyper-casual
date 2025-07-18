namespace SceneManagement
{
    public class SceneLoadingParameters
    {
        public string SceneName { get; set; }
        public float DelayBeforeActivation { get; set; }
        public bool ShowProgress { get; set; }
        public GameParameters GameParameters { get; set; }
        
        public static SceneLoadingParameters CreateWithDelay(string sceneName, float delay)
        {
            return new SceneLoadingParameters
            {
                SceneName = sceneName,
                DelayBeforeActivation = delay,
                ShowProgress = false
            };
        }
        
        public static SceneLoadingParameters CreateForGame(string sceneName, GameParameters gameParameters, float delay = 0f)
        {
            return new SceneLoadingParameters
            {
                SceneName = sceneName,
                DelayBeforeActivation = delay,
                ShowProgress = false,
                GameParameters = gameParameters
            };
        }
    }
} 