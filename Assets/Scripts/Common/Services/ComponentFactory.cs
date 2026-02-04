using UnityEngine;
using VContainer;
using Common.Interfaces;
using System;
using ILogger = Common.Interfaces.ILogger;

namespace Common.Services
{
    public class ComponentFactory : IComponentFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly ILogger _logger;

        public ComponentFactory(IObjectResolver resolver, ILogger logger)
        {
            _resolver = resolver;
            _logger = logger;
        }

        public void InjectDependencies(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            var components = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var component in components)
            {
                try
                {
                    _resolver.Inject(component);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Failed to inject dependencies for {component.GetType().Name}: {ex.Message}");
                }
            }
        }

        public void InjectDependencies(Component component)
        {
            if (component == null) return;
            
            try
            {
                _resolver.Inject(component);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to inject dependencies for {component.GetType().Name}: {ex.Message}");
            }
        }
    }
}
