using System;

namespace Common.Interfaces
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : class;
        void Unsubscribe<T>(Action<T> handler) where T : class;
        void Publish<T>(T eventData) where T : class;
        void Clear();
    }

    // Common game events
    public class PlayerMovedEvent
    {
        public UnityEngine.Vector3 Position { get; set; }
        public UnityEngine.Vector3 PreviousPosition { get; set; }
    }

    public class GrassCutEvent
    {
        public UnityEngine.Vector3 Position { get; set; }
        public float Radius { get; set; }
        public int BladesCut { get; set; }
    }

    public class LevelProgressEvent
    {
        public float Progress { get; set; }
        public float Distance { get; set; }
    }

    public class GameOverEvent
    {
        public float FinalScore { get; set; }
        public float Distance { get; set; }
        public bool IsWin { get; set; }
    }

    public class ObjectShavedEvent
    {
        public UnityEngine.Vector3 Position { get; set; }
        public string ObjectType { get; set; }
    }
}
