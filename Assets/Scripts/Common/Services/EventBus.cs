using System;
using System.Collections.Generic;
using Common.Interfaces;
using UnityEngine;

namespace Common.Services
{
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            
            _eventHandlers[eventType].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var eventType = typeof(T);
            
            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType].Remove(handler);
                
                if (_eventHandlers[eventType].Count == 0)
                {
                    _eventHandlers.Remove(eventType);
                }
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            var eventType = typeof(T);
            
            if (!_eventHandlers.ContainsKey(eventType)) return;
            
            var handlers = _eventHandlers[eventType];
            
            // Create a copy to avoid modification during iteration
            var handlersCopy = new List<Delegate>(handlers);
            
            foreach (var handler in handlersCopy)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error executing event handler for {eventType.Name}: {e.Message}");
                }
            }
        }

        public void Clear()
        {
            _eventHandlers.Clear();
        }
    }
}
