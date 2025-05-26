using UnityEngine;

namespace MetaBalance.Core
{
    /// <summary>
    /// Represents the current game state
    /// </summary>
    [System.Serializable]
    public class GameState
    {
        public int CurrentWeek;
        public GamePhase CurrentPhase;
    }
}