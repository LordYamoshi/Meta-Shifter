using UnityEngine;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Base ScriptableObject for all card data
    /// </summary>
    [CreateAssetMenu(fileName = "New Card", menuName = "MetaBalance/Card")]
    public abstract class CardData : ScriptableObject
    {
        [Header("Card Info")]
        public string cardName;
        [TextArea(2, 4)]
        public string description;
        public CardType cardType;
        public CardRarity rarity;
        
        [Header("Visuals")]
        public Sprite cardArt;
        public Color cardColor = Color.white;
        
        [Header("Costs")]
        public int researchPointCost;
        public int communityPointCost;
        
        /// <summary>
        /// Creates an effect instance for this card
        /// </summary>
        public abstract CardEffect CreateEffect();
    }
}