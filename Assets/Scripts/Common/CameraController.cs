using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Reference to the player transform
    public float smoothSpeed = 5f; // How smoothly the camera follows

    private Vector3 offset; // Initial offset from the player

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("CameraController: Player reference not set.");
            enabled = false;
            return;
        }
        // Calculate initial offset
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;
        // Only follow the player's z position, keep x and y offset
        Vector3 targetPos = new Vector3(
            transform.position.x, // Keep current x
            transform.position.y, // Keep current y
            player.position.z + offset.z // Follow player's z
        );
        // Smoothly interpolate to the target position
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
} 