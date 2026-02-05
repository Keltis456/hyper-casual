using System;
using UnityEngine;
using VContainer;
using Common;
using Common.Interfaces;
using ILogger = Common.Interfaces.ILogger;

namespace ShaveRunner
{
    public class PlayerController : MonoBehaviour
    {
        [Inject] private IInputService InputService { get; set; }
        [Inject] private IGameStateManager GameStateManager { get; set; }
        [Inject] private IConfigurationService ConfigService { get; set; }
        [Inject] private ILogger Logger { get; set; }

        public event Action<PlayerMovedEvent> OnPlayerMoved;

        private Vector3 _previousPosition;
        private float _targetX;
        private bool _isMovementEnabled = true;
        
        private float ForwardSpeed => ConfigService?.PlayerForwardSpeed ?? 5f;
        private float HorizontalSpeed => ConfigService?.PlayerHorizontalSpeed ?? 10f;
        private float LaneLimit => ConfigService?.PlayerLaneLimit ?? 3f;
        private float Smoothing => ConfigService?.PlayerMovementSmoothing ?? 5f;

        void Start()
        {
            _previousPosition = transform.position;
            _targetX = transform.position.x;
            
            if (InputService == null)
            {
                Logger?.LogError($"{nameof(PlayerController)}: {nameof(InputService)} not injected!");
            }
            else
            {
                InputService.OnHorizontalInput += OnHorizontalInput;
            }
            
            if (GameStateManager == null)
            {
                Logger?.LogError($"{nameof(PlayerController)}: {nameof(GameStateManager)} not injected!");
            }
            else
            {
                GameStateManager.OnStateChanged += OnGameStateChanged;
            }
            
            if (ConfigService == null)
            {
                Logger?.LogError($"{nameof(PlayerController)}: {nameof(ConfigService)} not injected!");
            }
        }

        void OnDestroy()
        {
            if (InputService != null)
            {
                InputService.OnHorizontalInput -= OnHorizontalInput;
            }
            
            if (GameStateManager != null)
            {
                GameStateManager.OnStateChanged -= OnGameStateChanged;
            }
        }

        void Update()
        {
            if (!_isMovementEnabled) return;

            MoveForward();
            ApplyHorizontalMovement();
            PublishMovementEvents();
        }

        private void OnHorizontalInput(float input)
        {
            if (!_isMovementEnabled) return;
            
            _targetX = Mathf.Clamp(transform.position.x + input * HorizontalSpeed, -LaneLimit, LaneLimit);
        }

        private void OnGameStateChanged(GameState previousState, GameState newState)
        {
            _isMovementEnabled = newState == GameState.Playing;
            
            if (!_isMovementEnabled)
            {
                _targetX = transform.position.x;
            }
        }

        private void MoveForward()
        {
            transform.Translate(Vector3.forward * ForwardSpeed * Time.deltaTime);
        }

        private void ApplyHorizontalMovement()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * Smoothing);
            transform.position = pos;
        }

        private void PublishMovementEvents()
        {
            Vector3 currentPosition = transform.position;
            
            if (Vector3.Distance(currentPosition, _previousPosition) > GameConstants.MovementEventThreshold)
            {
                OnPlayerMoved?.Invoke(new PlayerMovedEvent
                {
                    Position = currentPosition,
                    PreviousPosition = _previousPosition
                });
                
                _previousPosition = currentPosition;
            }
        }

        public void SetMovementEnabled(bool enabled)
        {
            _isMovementEnabled = enabled;
        }

        public float GetCurrentSpeed()
        {
            return ForwardSpeed;
        }

        public Vector3 GetVelocity()
        {
            return Vector3.forward * ForwardSpeed + Vector3.right * ((_targetX - transform.position.x) * Smoothing);
        }
    }

    public class PlayerMovedEvent
    {
        public Vector3 Position { get; set; }
        public Vector3 PreviousPosition { get; set; }
    }
}
