using UnityEngine;
using MetaBalance.Characters;

namespace MetaBalance.Cards
{
    [CreateAssetMenu(fileName = "New Balance Card", menuName = "Meta Balance/Cards/Balance Change")]
    public class BalanceChangeCardData : CardData
    {
        [Header("Balance Change Properties")]
        public CharacterType targetCharacterType;
        public CharacterStat targetStat;
        [Range(-50f, 50f)]
        public float percentageChange;
        
        public override CardEffect CreateEffect()
        {
            return new BalanceChangeEffect(this, targetCharacterType, targetStat, percentageChange);
        }
    }
}