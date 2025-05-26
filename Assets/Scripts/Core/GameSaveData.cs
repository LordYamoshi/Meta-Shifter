namespace MetaBalance.Core
{
    /// <summary>
    /// Save data for the game
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        public int Week;
        public int Phase;
        public int ResearchPoints;
        public int CommunityPoints;
    }
}