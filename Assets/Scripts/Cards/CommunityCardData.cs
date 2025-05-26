using UnityEngine;

namespace MetaBalance.Cards
{
    [CreateAssetMenu(fileName = "New Community Card", menuName = "Meta Balance/Cards/Community")]
    public class CommunityCardData : CardData
    {
        [Header("Community Properties")]
        public CommunityActionType actionType;
        [Range(1, 10)]
        public int communityImpact = 5;
        
        public override CardEffect CreateEffect()
        {
            return new CommunityEffect(this, actionType, communityImpact);
        }
    }
    
    public enum CommunityActionType
    {
        DeveloperUpdate,
        Survey,
        EngagementCampaign,
        ContentCreatorSpotlight,
        CommunityEvent
    }
}