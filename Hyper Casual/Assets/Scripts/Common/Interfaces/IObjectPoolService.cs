using UnityEngine;

namespace Common.Interfaces
{
    public interface IObjectPoolService
    {
        T Get<T>(T prefab, Transform parent = null) where T : Component;
        void Return<T>(T instance) where T : Component;
        void Return(GameObject instance);
        void PreWarm<T>(T prefab, int count, Transform parent = null) where T : Component;
        void ClearPool<T>(T prefab) where T : Component;
        void ClearAllPools();
        int GetPoolSize<T>(T prefab) where T : Component;
        int GetActiveCount<T>(T prefab) where T : Component;
    }
}
