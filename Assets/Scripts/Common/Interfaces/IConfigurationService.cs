using Common.Configuration;

namespace Common.Interfaces
{
    public interface IConfigurationService
    {
        GameConfig GameConfig { get; }
        void LoadConfiguration();
        void SaveConfiguration();
        T GetValue<T>(string key, T defaultValue = default);
        void SetValue<T>(string key, T value);
        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
    }
}
