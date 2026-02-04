using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class ObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<GameObject> _pool;
    
        public ObjectPool(GameObject prefab, Transform parent, int initialSize)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new Stack<GameObject>(initialSize);
        
            // Pre-instantiate objects
            for (var i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }
    
        public GameObject Get()
        {
            if (_pool.Count <= 0) CreateNewObject();
            var obj = _pool.Pop();
            obj.SetActive(true);
            return obj;
        }
    
        public void ReturnToPool(GameObject obj)
        {
            if (!_parent) return;
            if (!obj) return;
        
            obj.SetActive(false);
            obj.transform.SetParent(_parent);
            if (!_pool.Contains(obj))
                _pool.Push(obj);

        }
    
        private void CreateNewObject()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _pool.Push(obj);
        }
    }
} 