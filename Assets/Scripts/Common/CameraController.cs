using UnityEngine;
using VContainer;
using Common.Interfaces;
using ShaveRunner;
using ILogger = Common.Interfaces.ILogger;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;

    [Inject] private PlayerController player { get; set; }
    [Inject] private IEventBus EventBus { get; set; }
    [Inject] private ILogger Logger { get; set; }

    private Vector3 offset;
    private Transform playerTransform;

    void Start()
    {
        if (player == null)
        {
            Logger?.LogError($"{nameof(CameraController)}: {nameof(player)} not injected!");
            enabled = false;
            return;
        }

        if (EventBus == null)
        {
            Logger?.LogError($"{nameof(CameraController)}: {nameof(EventBus)} not injected!");
        }

        playerTransform = player.transform;
        offset = transform.position - playerTransform.position;
        
        if (EventBus != null)
        {
            EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
        }
    }

    void OnDestroy()
    {
        if (EventBus != null)
        {
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
        }
    }

    private void OnPlayerMoved(PlayerMovedEvent playerEvent)
    {
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            playerEvent.Position.z + offset.z
        );
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
} 