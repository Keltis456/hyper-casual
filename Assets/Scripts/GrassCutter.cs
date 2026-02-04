using UnityEngine;
using VContainer;
using Common;
using Common.Interfaces;
using ILogger = Common.Interfaces.ILogger;

public class GrassCutter : MonoBehaviour
{
    [Header("Cutting Settings")]
    [SerializeField] private ComputeShader cutShader;
    [SerializeField] private bool enableCutting = true;

    [Inject] private IEventBus EventBus { get; set; }
    [Inject] private IGameStateManager GameStateManager { get; set; }
    [Inject] private IConfigurationService ConfigService { get; set; }
    [Inject] private ILogger Logger { get; set; }
    [Inject] private GPUGrassRenderer _grassRenderer { get; set; }
    
    private float CutRadius => ConfigService.GrassCutRadius;
    private float CutDistance => ConfigService.GrassCutDistance;

    private int _kernel;
    private Vector3 _lastCutPosition;
    private bool _canCut = true;

    void Start()
    {
        if (_grassRenderer == null)
        {
            Logger?.LogError($"{nameof(GrassCutter)}: {nameof(_grassRenderer)} not injected!");
        }
        
        if (EventBus == null)
        {
            Logger?.LogError($"{nameof(GrassCutter)}: {nameof(EventBus)} not injected!");
        }
        
        if (GameStateManager == null)
        {
            Logger?.LogError($"{nameof(GrassCutter)}: {nameof(GameStateManager)} not injected!");
        }
        
        if (ConfigService == null)
        {
            Logger?.LogError($"{nameof(GrassCutter)}: {nameof(ConfigService)} not injected!");
        }
        
        InitializeCutter();
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeCutter()
    {
        if (cutShader != null)
        {
            _kernel = cutShader.FindKernel("CutGrass");
        }
        else
        {
            Logger?.LogError($"{nameof(GrassCutter)}: Cut shader not assigned!");
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

        if (Vector3.Distance(playerEvent.Position, _lastCutPosition) > CutDistance)
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
            return;
        }

        cutShader.SetBuffer(_kernel, "blades", bladeBuffer);
        cutShader.SetBuffer(_kernel, "_ChunkToWorldBuffer", matrixBuffer);
        cutShader.SetVector("cutCenter", worldPos);
        cutShader.SetFloat("cutRadius", CutRadius);
        cutShader.SetInt("count", _grassRenderer.GetBladeCount());

        int threadGroups = Mathf.CeilToInt(_grassRenderer.GetBladeCount() / (float)GameConstants.ComputeThreadGroupSize);
        cutShader.Dispatch(_kernel, threadGroups, 1, 1);

        int estimatedBladesCut = Mathf.RoundToInt(Mathf.PI * CutRadius * CutRadius * 10f);

        EventBus?.Publish(new GrassCutEvent
        {
            Position = worldPos,
            Radius = CutRadius,
            BladesCut = estimatedBladesCut
        });

        Logger?.LogDebug($"Cut grass at {worldPos}, estimated {estimatedBladesCut} blades");
    }

    public void SetCuttingEnabled(bool enabled)
    {
        enableCutting = enabled;
    }

    public void CutAtMousePosition()
    {
        if (!_canCut) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CutAtPosition(hit.point);
        }
    }

    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(transform.position, CutRadius);
    //     
    //     if (_lastCutPosition != Vector3.zero)
    //     {
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawWireSphere(_lastCutPosition, CutRadius);
    //     }
    // }
}
