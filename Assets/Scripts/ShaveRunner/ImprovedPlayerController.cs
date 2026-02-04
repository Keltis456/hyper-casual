using UnityEngine;
using VContainer;
using Common.Interfaces;

namespace ShaveRunner
{
    public class ImprovedPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float forwardSpeed = 5f;
        [SerializeField] private float horizontalSpeed = 10f;
        [SerializeField] private float laneLimit = 3f;
        [SerializeField] private float smoothing = 5f;

        [Inject] private IInputService InputService { get; set; }
        [Inject] private IEventBus EventBus { get; set; }
        [Inject] private IGameStateManager GameStateManager { get; set; }

        private Vector3 _previousPosition;
        private float _targetX;
        private bool _isMovementEnabled = true;

        void Start()
        {
            _previousPosition = transform.position;
            _targetX = transform.position.x;
            
            // Subscribe to input events
            if (InputService != null)
            {
                InputService.OnHorizontalInput += OnHorizontalInput;
            }
            
            // Subscribe to game state changes
            if (GameStateManager != null)
            {
                GameStateManager.OnStateChanged += OnGameStateChanged;
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from events
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
            
            _targetX = Mathf.Clamp(transform.position.x + input * horizontalSpeed, -laneLimit, laneLimit);
        }

        private void OnGameStateChanged(GameState previousState, GameState newState)
        {
            _isMovementEnabled = newState == GameState.Playing;
            
            if (!_isMovementEnabled)
            {
                _targetX = transform.position.x; // Stop horizontal movement
            }
        }

        private void MoveForward()
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }

        private void ApplyHorizontalMovement()
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * smoothing);
            transform.position = pos;
        }

        private void PublishMovementEvents()
        {
            Vector3 currentPosition = transform.position;
            
            if (Vector3.Distance(currentPosition, _previousPosition) > 0.1f)
            {
                EventBus?.Publish(new PlayerMovedEvent
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
            return forwardSpeed;
        }

        public Vector3 GetVelocity()
        {
            return Vector3.forward * forwardSpeed + Vector3.right * ((_targetX - transform.position.x) * smoothing);
        }
    }
}
