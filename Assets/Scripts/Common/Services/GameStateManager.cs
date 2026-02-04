using System;
using Common.Interfaces;
using UnityEngine;

namespace Common.Services
{
    public class GameStateManager : IGameStateManager
    {
        public GameState CurrentState { get; private set; } = GameState.Menu;
        
        public event Action<GameState, GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            if (!CanTransitionTo(newState))
            {
                Debug.LogWarning($"Invalid state transition from {CurrentState} to {newState}");
                return;
            }

            var previousState = CurrentState;
            CurrentState = newState;
            
            Debug.Log($"Game state changed: {previousState} -> {newState}");
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
    }
}
