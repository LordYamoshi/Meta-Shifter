namespace MetaBalance.Core
{
    /// <summary>
    /// Event data for resource changes
    /// </summary>
    [System.Serializable]
    public class ResourceChangeEvent
    {
        public ResourceType ResourceType;
        public int NewValue;
        public int OldValue;
        public int GenerationRate;
    }
}