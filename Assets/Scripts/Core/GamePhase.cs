namespace MetaBalance.Core
{
    /// <summary>
    /// Game phases
    /// </summary>
    public enum GamePhase
    {
        Planning,       // Select cards to play
        Implementation, // Apply card effects
        Feedback,       // See effects of changes
        Event           // Handle random events
    }
}