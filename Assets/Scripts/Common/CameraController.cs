using UnityEngine;
using VContainer;
using Common.Interfaces;
using ShaveRunner;
using ILogger = Common.Interfaces.ILogger;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;

    [Inject] private PlayerController player { get; set; }
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

        playerTransform = player.transform;
        offset = transform.position - playerTransform.position;
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            playerTransform.position.z + offset.z
        );
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
} 