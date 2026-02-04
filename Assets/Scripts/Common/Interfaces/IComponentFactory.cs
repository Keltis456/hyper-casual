using UnityEngine;

namespace Common.Interfaces
{
    public interface IComponentFactory
    {
        void InjectDependencies(GameObject gameObject);
        void InjectDependencies(Component component);
    }
}
