using UnityEngine;
using System;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Base ScriptableObject for all cards
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "Meta Balance/Card Data")]
    public abstract class CardData : ScriptableObject
    {
        [Header("Basic Info")]
        public string cardName;
        [TextArea(2, 4)]
        public string description;
        public CardType cardType;
        public CardRarity rarity;
        
        [Header("Costs")]
        public int researchPointCost;
        public int communityPointCost;
        
        [Header("Visual")]
        public Sprite cardArt;
        public Color cardColor = Color.white;
        
        /// <summary>
        /// Override this to define what the card does when played
        /// </summary>
        public abstract void PlayCard();
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