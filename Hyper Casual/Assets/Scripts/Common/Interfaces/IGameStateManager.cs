using System;

namespace Common.Interfaces
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Loading
    }

    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        event Action<GameState, GameState> OnStateChanged;
        
        void ChangeState(GameState newState);
        bool CanTransitionTo(GameState targetState);
    }
}
