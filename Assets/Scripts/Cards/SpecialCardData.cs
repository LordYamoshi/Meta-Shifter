using UnityEngine;

namespace MetaBalance.Cards
{
    [CreateAssetMenu(fileName = "New Special Card", menuName = "Meta Balance/Cards/Special")]
    public class SpecialCardData : CardData
    {
        [Header("Special Properties")]
        public SpecialActionType actionType;
        [Range(1, 20)]
        public int impactMagnitude = 10;
        
        public override CardEffect CreateEffect()
        {
            return new SpecialEffect(this, actionType, impactMagnitude);
        }
    }
    
    public enum SpecialActionType
    {
        CompleteOverhaul,
        NewGameMode,
        ProCircuitAnnouncement,
        MajorContentUpdate,
        CrossoverEvent
    }
}