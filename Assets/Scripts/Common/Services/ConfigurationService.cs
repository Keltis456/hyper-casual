using UnityEngine;
using Common.Interfaces;
using Common.Configuration;

namespace Common.Services
{
    public class ConfigurationService : IConfigurationService
    {
        public GameConfig Config { get; private set; }

        public float PlayerForwardSpeed => Config?.playerForwardSpeed ?? 5f;
        public float PlayerHorizontalSpeed => Config?.playerHorizontalSpeed ?? 10f;
        public float PlayerLaneLimit => Config?.playerLaneLimit ?? 3f;
        public float PlayerMovementSmoothing => Config?.playerMovementSmoothing ?? 5f;
        public float GrassCutRadius => Config?.grassCutRadius ?? 1f;
        public float GrassCutDistance => Config?.grassCutDistance ?? 0.5f;
        public int ObjectPoolPreWarmCount => Config?.objectPoolPreWarmCount ?? 10;

        public ConfigurationService()
        {
            LoadConfiguration();
        }

        public void LoadConfiguration()
        {
            Config = Resources.Load<GameConfig>(GameConstants.ConfigResourcePath);
            
            if (Config == null)
            {
                Debug.LogWarning($"GameConfig not found at Resources/{GameConstants.ConfigResourcePath}. Using default values.");
                Config = ScriptableObject.CreateInstance<GameConfig>();
            }
        }

        public void SaveConfiguration()
        {
            PlayerPrefs.Save();
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
