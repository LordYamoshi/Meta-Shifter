using System.Collections.Generic;

namespace MetaBalance.Events
{
    /// <summary>
    /// Option for resolving an event
    /// </summary>
    [System.Serializable]
    public class EventOption
    {
        public string optionText;
        [UnityEngine.TextArea(2, 4)]
        public string resultText;
        
        [UnityEngine.Header("Effects")]
        [UnityEngine.Range(-25f, 25f)]
        public float playerSatisfactionEffect;
        public List<CharacterEffect> characterEffects = new List<CharacterEffect>();
        
        [UnityEngine.Header("Resource Costs/Rewards")]
        public int researchPointsCost;
        public int communityPointsCost;
        public int researchPointsReward;
        public int communityPointsReward;
    }
}