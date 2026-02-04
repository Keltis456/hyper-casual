using System;
using UnityEngine;

namespace Common.Interfaces
{
    public interface IInputService
    {
        event Action<Vector2> OnTouchStart;
        event Action<Vector2> OnTouchMove;
        event Action<Vector2> OnTouchEnd;
        event Action<float> OnHorizontalInput;
        
        bool IsTouching { get; }
        Vector2 TouchPosition { get; }
        float HorizontalInput { get; }
        
        void Enable();
        void Disable();
    }
}
