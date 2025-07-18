using System;

namespace SceneManagement
{
    [Serializable]
    public class GameParameters
    {
        public int Goal { get; set; }
        public string ReturnScene { get; set; } = "Map";
    }
} 