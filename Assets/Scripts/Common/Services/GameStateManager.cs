using System;
using Common.Interfaces;

namespace Common.Services
{
    public class GameStateManager : IGameStateManager, IDisposable
    {
        private readonly ILogger _logger;
        private bool _disposed = false;
        
        public GameState CurrentState { get; private set; } = GameState.Menu;
        
        public event Action<GameState, GameState> OnStateChanged;

        public GameStateManager(ILogger logger)
        {
            _logger = logger;
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            if (!CanTransitionTo(newState))
            {
                _logger?.LogWarning($"Invalid state transition from {CurrentState} to {newState}");
                return;
            }

            var previousState = CurrentState;
            CurrentState = newState;
            
            _logger?.LogInfo($"Game state changed: {previousState} -> {newState}");
            OnStateChanged?.Invoke(previousState, newState);
        }

        public bool CanTransitionTo(GameState targetState)
        {
            return targetState switch
            {
                GameState.Menu => CurrentState != GameState.Loading,
                GameState.Playing => CurrentState is GameState.Menu or GameState.Paused or GameState.Loading,
                GameState.Paused => CurrentState == GameState.Playing,
                GameState.GameOver => CurrentState == GameState.Playing,
                GameState.Loading => true,
                _ => false
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            OnStateChanged = null;
            _disposed = true;
        }
    }
}
