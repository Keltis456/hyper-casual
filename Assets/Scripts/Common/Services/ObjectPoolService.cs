using System.Collections.Generic;
using Common.Interfaces;
using UnityEngine;
using VContainer;

namespace Common.Services
{
    public class ObjectPoolService : MonoBehaviour, IObjectPoolService
    {
        [Inject] private IComponentFactory ComponentFactory { get; set; }
        
        private readonly Dictionary<int, ObjectPool> _pools = new();
        private readonly Dictionary<int, HashSet<GameObject>> _activeObjects = new();

        public T Get<T>(T prefab, Transform parent = null) where T : Component
        {
            var poolKey = prefab.GetInstanceID();
            
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new ObjectPool(prefab.gameObject, parent ?? transform, 0);
                _activeObjects[poolKey] = new HashSet<GameObject>();
            }

            var obj = _pools[poolKey].Get();
            _activeObjects[poolKey].Add(obj);
            
            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj.GetComponent<T>();
        }

        public void Return<T>(T instance) where T : Component
        {
            if (instance == null) return;
            Return(instance.gameObject);
        }

        public void Return(GameObject instance)
        {
            if (instance == null) return;

            // Find which pool this object belongs to
            foreach (var kvp in _activeObjects)
            {
                if (kvp.Value.Contains(instance))
                {
                    kvp.Value.Remove(instance);
                    _pools[kvp.Key].ReturnToPool(instance);
                    return;
                }
            }

            Destroy(instance);
        }

        public void PreWarm<T>(T prefab, int count, Transform parent = null) where T : Component
        {
            var poolKey = prefab.GetInstanceID();
            
            if (!_pools.ContainsKey(poolKey))
            {
                _pools[poolKey] = new ObjectPool(prefab.gameObject, parent ?? transform, count);
                _activeObjects[poolKey] = new HashSet<GameObject>();
            }
        }

        public void ClearPool<T>(T prefab) where T : Component
        {
            var poolKey = prefab.GetInstanceID();
            
            if (_pools.ContainsKey(poolKey))
            {
                // Return all active objects first
                var activeObjects = new List<GameObject>(_activeObjects[poolKey]);
                foreach (var obj in activeObjects)
                {
                    Return(obj);
                }
                
                _pools.Remove(poolKey);
                _activeObjects.Remove(poolKey);
            }
        }

        public void ClearAllPools()
        {
            // Return all active objects
            foreach (var activeSet in _activeObjects.Values)
            {
                var activeObjects = new List<GameObject>(activeSet);
                foreach (var obj in activeObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }
            
            _pools.Clear();
            _activeObjects.Clear();
        }

        public int GetPoolSize<T>(T prefab) where T : Component
        {
            var poolKey = prefab.GetInstanceID();
            return _activeObjects.ContainsKey(poolKey) ? _activeObjects[poolKey].Count : 0;
        }

        public int GetActiveCount<T>(T prefab) where T : Component
        {
            var poolKey = prefab.GetInstanceID();
            return _activeObjects.ContainsKey(poolKey) ? _activeObjects[poolKey].Count : 0;
        }

        void OnDestroy()
        {
            ClearAllPools();
        }
    }
}
