using UnityEngine;

namespace ShaveRunner
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float forwardSpeed = 5f; // Forward movement speed
        public float horizontalSpeed = 10f; // Horizontal movement speed
        public float laneLimit = 3f; // How far left/right the player can move

        private float _inputStartX;
        private float _playerStartX;
        private bool _isDragging;

        void Update()
        {
            HandleInput();
            MoveForward();
        }

        // Handles touch or mouse input for horizontal movement
        void HandleInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            // Mouse input for editor/testing
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _inputStartX = Input.mousePosition.x;
                _playerStartX = transform.position.x;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
            else if (_isDragging && Input.GetMouseButton(0))
            {
                float delta = (Input.mousePosition.x - _inputStartX) / Screen.width;
                float targetX = _playerStartX + delta * horizontalSpeed;
                targetX = Mathf.Clamp(targetX, -laneLimit, laneLimit);
                Vector3 pos = transform.position;
                pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * horizontalSpeed);
                transform.position = pos;
            }
#else
        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _isDragging = true;
                _inputStartX = touch.position.x;
                _playerStartX = transform.position.x;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _isDragging = false;
            }
            else if (_isDragging && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
            {
                float delta = (touch.position.x - _inputStartX) / Screen.width;
                float targetX = _playerStartX + delta * horizontalSpeed;
                targetX = Mathf.Clamp(targetX, -laneLimit, laneLimit);
                Vector3 pos = transform.position;
                pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * horizontalSpeed);
                transform.position = pos;
            }
        }
#endif
        }

        // Moves the player forward automatically
        void MoveForward()
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }
    }
} 