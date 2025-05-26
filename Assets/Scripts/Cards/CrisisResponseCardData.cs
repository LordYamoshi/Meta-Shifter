using UnityEngine;

namespace MetaBalance.Cards
{
    [CreateAssetMenu(fileName = "New Crisis Card", menuName = "Meta Balance/Cards/Crisis Response")]
    public class CrisisResponseCardData : CardData
    {
        [Header("Crisis Properties")]
        public CrisisType crisisType;
        [Range(1, 10)]
        public int responseEffectiveness = 5;
        
        public override CardEffect CreateEffect()
        {
            return new CrisisResponseEffect(this, crisisType, responseEffectiveness);
        }
    }
    
    public enum CrisisType
    {
        GameplayExploit,
        ServerIssue,
        ProPlayerControversy,
        DataBreach,
        MajorBug
    }
}