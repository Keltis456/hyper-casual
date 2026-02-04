using UnityEngine;
using Common.Interfaces;
using Common.Configuration;

namespace Common.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private const string CONFIG_RESOURCE_PATH = "GameConfig";
        
        public GameConfig GameConfig { get; private set; }

        public ConfigurationService()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            GameConfig = Resources.Load<GameConfig>(CONFIG_RESOURCE_PATH);
            
            if (GameConfig == null)
            {
                Debug.LogWarning($"GameConfig not found at Resources/{CONFIG_RESOURCE_PATH}. Creating default configuration.");
                GameConfig = ScriptableObject.CreateInstance<GameConfig>();
            }
            
            Debug.Log("Game configuration loaded successfully.");
        }

        public void SaveConfiguration()
        {
            // In a real project, you might want to save runtime changes to PlayerPrefs or a file
            // For now, we'll just save to PlayerPrefs for runtime settings
            PlayerPrefs.Save();
            Debug.Log("Configuration saved to PlayerPrefs.");
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (!HasKey(key))
                return defaultValue;

            var type = typeof(T);
            
            if (type == typeof(string))
                return (T)(object)PlayerPrefs.GetString(key, defaultValue?.ToString() ?? "");
            else if (type == typeof(int))
                return (T)(object)PlayerPrefs.GetInt(key, defaultValue is int intDefault ? intDefault : 0);
            else if (type == typeof(float))
                return (T)(object)PlayerPrefs.GetFloat(key, defaultValue is float floatDefault ? floatDefault : 0f);
            else if (type == typeof(bool))
                return (T)(object)(PlayerPrefs.GetInt(key, defaultValue is bool boolDefault && boolDefault ? 1 : 0) == 1);
            else
            {
                Debug.LogWarning($"Unsupported type {type} for configuration key {key}");
                return defaultValue;
            }
        }

        public void SetValue<T>(string key, T value)
        {
            var type = typeof(T);
            
            if (type == typeof(string))
                PlayerPrefs.SetString(key, value?.ToString() ?? "");
            else if (type == typeof(int))
                PlayerPrefs.SetInt(key, value is int intValue ? intValue : 0);
            else if (type == typeof(float))
                PlayerPrefs.SetFloat(key, value is float floatValue ? floatValue : 0f);
            else if (type == typeof(bool))
                PlayerPrefs.SetInt(key, value is bool boolValue && boolValue ? 1 : 0);
            else
            {
                Debug.LogWarning($"Unsupported type {type} for configuration key {key}");
                return;
            }
            
            Debug.Log($"Configuration value set: {key} = {value}");
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            Debug.Log($"Configuration key deleted: {key}");
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All configuration keys deleted.");
        }
    }
}
