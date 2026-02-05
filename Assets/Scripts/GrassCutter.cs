using UnityEngine;
using VContainer;
using Common;
using Common.Interfaces;
using ShaveRunner;
using ILogger = Common.Interfaces.ILogger;

public class GrassCutter : MonoBehaviour
{
    [Header("Cutting Settings")]
    [SerializeField] private ComputeShader cutShader;
    [SerializeField] private bool enableCutting = true;

    [Inject] private PlayerController PlayerController { get; set; }
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
        
        if (PlayerController == null)
        {
            Logger?.LogError($"{nameof(GrassCutter)}: {nameof(PlayerController)} not injected!");
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
        if (PlayerController != null)
        {
            PlayerController.OnPlayerMoved += OnPlayerMoved;
        }
        
        if (GameStateManager != null)
        {
            GameStateManager.OnStateChanged += OnGameStateChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (PlayerController != null)
        {
            PlayerController.OnPlayerMoved -= OnPlayerMoved;
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
        
        Logger?.LogDebug($"Cut grass at {worldPos}, estimated {estimatedBladesCut} blades");
    }
}
