using UnityEngine;
using VContainer;
using Common.Interfaces;

public class ImprovedGrassCutter : MonoBehaviour
{
    [Header("Cutting Settings")]
    [SerializeField] private ComputeShader cutShader;
    [SerializeField] private float cutRadius = 1f;
    [SerializeField] private float cutDistance = 0.5f;
    [SerializeField] private bool enableCutting = true;

    [Inject] private IEventBus EventBus { get; set; }
    [Inject] private IGameStateManager GameStateManager { get; set; }

    private GPUGrassRenderer _grassRenderer;
    private int _kernel;
    private Vector3 _lastCutPosition;
    private bool _canCut = true;

    void Start()
    {
        InitializeCutter();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeCutter()
    {
        // Find grass renderer if not injected
        if (_grassRenderer == null)
        {
            _grassRenderer = FindObjectOfType<GPUGrassRenderer>();
        }

        if (cutShader != null)
        {
            _kernel = cutShader.FindKernel("CutGrass");
        }
        else
        {
            Debug.LogError("Cut shader not assigned!");
        }
    }

    private void SubscribeToEvents()
    {
        if (EventBus != null)
        {
            EventBus.Subscribe<PlayerMovedEvent>(OnPlayerMoved);
        }

        if (GameStateManager != null)
        {
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (EventBus != null)
        {
            EventBus.Unsubscribe<PlayerMovedEvent>(OnPlayerMoved);
        }

        if (GameStateManager != null)
        {
            GameStateManager.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void OnPlayerMoved(PlayerMovedEvent playerEvent)
    {
        if (!_canCut || !enableCutting) return;

        // Only cut if player moved enough distance
        if (Vector3.Distance(playerEvent.Position, _lastCutPosition) > cutDistance)
        {
            CutAtPosition(playerEvent.Position);
            _lastCutPosition = playerEvent.Position;
        }
    }

    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        _canCut = newState == GameState.Playing;
    }

    public void CutAtPosition(Vector3 worldPos)
    {
        if (_grassRenderer == null || cutShader == null) return;

        ComputeBuffer bladeBuffer = _grassRenderer.GetBladeBuffer();
        ComputeBuffer matrixBuffer = _grassRenderer.GetMatrixBuffer();

        if (bladeBuffer == null || matrixBuffer == null || _grassRenderer.GetBladeCount() == 0)
        {
            Debug.LogWarning("Grass buffers not ready, skipping cut.");
            return;
        }

        // Set up compute shader
        cutShader.SetBuffer(_kernel, "blades", bladeBuffer);
        cutShader.SetBuffer(_kernel, "_ChunkToWorldBuffer", matrixBuffer);
        cutShader.SetVector("cutCenter", worldPos);
        cutShader.SetFloat("cutRadius", cutRadius);
        cutShader.SetInt("count", _grassRenderer.GetBladeCount());

        // Dispatch the compute shader
        int threadGroups = Mathf.CeilToInt(_grassRenderer.GetBladeCount() / 256f);
        cutShader.Dispatch(_kernel, threadGroups, 1, 1);

        // Count blades cut (simplified estimation)
        int estimatedBladesCut = Mathf.RoundToInt(Mathf.PI * cutRadius * cutRadius * 10f);

        // Publish grass cut event
        EventBus?.Publish(new GrassCutEvent
        {
            Position = worldPos,
            Radius = cutRadius,
            BladesCut = estimatedBladesCut
        });

        Debug.Log($"Cut grass at {worldPos} with radius {cutRadius}, estimated {estimatedBladesCut} blades cut");
    }

    public void SetCuttingEnabled(bool enabled)
    {
        enableCutting = enabled;
    }

    public void SetCutRadius(float radius)
    {
        cutRadius = Mathf.Max(0.1f, radius);
    }

    public void SetCutDistance(float distance)
    {
        cutDistance = Mathf.Max(0.1f, distance);
    }

    // Manual cutting method for testing or special effects
    public void CutAtMousePosition()
    {
        if (!_canCut) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CutAtPosition(hit.point);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize cut radius in scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, cutRadius);
        
        if (_lastCutPosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_lastCutPosition, cutRadius);
        }
    }
}
