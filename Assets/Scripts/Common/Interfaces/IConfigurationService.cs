using Common.Configuration;

namespace Common.Interfaces
{
    public interface IConfigurationService
    {
        GameConfig Config { get; }
        
        float PlayerForwardSpeed { get; }
        float PlayerHorizontalSpeed { get; }
        float PlayerLaneLimit { get; }
        float PlayerMovementSmoothing { get; }
        float GrassCutRadius { get; }
        float GrassCutDistance { get; }
        int ObjectPoolPreWarmCount { get; }
        
        void LoadConfiguration();
        void SaveConfiguration();
        T GetValue<T>(string key, T defaultValue = default);
        void SetValue<T>(string key, T value);
        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
    }
}
