using System;
using Common.Interfaces;
using UnityEngine;

namespace Common.Services
{
    public class InputService : MonoBehaviour, IInputService
    {
        public event Action<Vector2> OnTouchStart;
        public event Action<Vector2> OnTouchMove;
        public event Action<Vector2> OnTouchEnd;
        public event Action<float> OnHorizontalInput;

        public bool IsTouching { get; private set; }
        public Vector2 TouchPosition { get; private set; }
        public float HorizontalInput { get; private set; }

        [Header("Input Settings")]
        [SerializeField] private float horizontalSensitivity = 1f;
        [SerializeField] private bool enableInput = true;

        private Vector2 _touchStartPosition;
        private bool _wasTouching;

        void Update()
        {
            if (!enableInput) return;

            HandleInput();
        }

        private void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            bool isMouseDown = Input.GetMouseButton(0);
            Vector2 mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                IsTouching = true;
                TouchPosition = mousePosition;
                _touchStartPosition = mousePosition;
                OnTouchStart?.Invoke(mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                IsTouching = false;
                OnTouchEnd?.Invoke(mousePosition);
                HorizontalInput = 0f;
                OnHorizontalInput?.Invoke(0f);
            }
            else if (isMouseDown && IsTouching)
            {
                TouchPosition = mousePosition;
                OnTouchMove?.Invoke(mousePosition);
                
                // Calculate horizontal input
                float deltaX = (mousePosition.x - _touchStartPosition.x) / Screen.width;
                HorizontalInput = deltaX * horizontalSensitivity;
                OnHorizontalInput?.Invoke(HorizontalInput);
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        IsTouching = true;
                        TouchPosition = touch.position;
                        _touchStartPosition = touch.position;
                        OnTouchStart?.Invoke(touch.position);
                        break;
                        
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (IsTouching)
                        {
                            TouchPosition = touch.position;
                            OnTouchMove?.Invoke(touch.position);
                            
                            // Calculate horizontal input
                            float deltaX = (touch.position.x - _touchStartPosition.x) / Screen.width;
                            HorizontalInput = deltaX * horizontalSensitivity;
                            OnHorizontalInput?.Invoke(HorizontalInput);
                        }
                        break;
                        
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        IsTouching = false;
                        OnTouchEnd?.Invoke(touch.position);
                        HorizontalInput = 0f;
                        OnHorizontalInput?.Invoke(0f);
                        break;
                }
            }
            else if (_wasTouching)
            {
                IsTouching = false;
                HorizontalInput = 0f;
                OnHorizontalInput?.Invoke(0f);
            }

            _wasTouching = IsTouching;
        }

        public void Enable()
        {
            enableInput = true;
        }

        public void Disable()
        {
            enableInput = false;
            IsTouching = false;
            HorizontalInput = 0f;
        }
    }
}
