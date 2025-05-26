using UnityEngine;

namespace MetaBalance.Cards
{
    [CreateAssetMenu(fileName = "New Meta Shift Card", menuName = "Meta Balance/Cards/Meta Shift")]
    public class MetaShiftCardData : CardData
    {
        [Header("Meta Shift Properties")]
        public MetaShiftType metaShiftType;
        [Range(1, 5)]
        public int shiftPower = 3;
        
        public override CardEffect CreateEffect()
        {
            return new MetaShiftEffect(this, metaShiftType, shiftPower);
        }
    }
    
    public enum MetaShiftType
    {
        MapPool,
        GameMode,
        ItemBalance,
        SeasonsChange,
        TournamentFormat
    }
}