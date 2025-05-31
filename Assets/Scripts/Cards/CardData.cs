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
    
    /// <summary>
    /// Balance change cards that modify character stats
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceCard", menuName = "Meta Balance/Balance Card")]
    public class BalanceChangeCard : CardData
    {
        [Header("Balance Change Settings")]
        public Characters.CharacterType targetCharacter;
        public Characters.CharacterStat targetStat;
        [Range(-50f, 50f)]
        public float percentageChange;
        
        public override void PlayCard()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager != null)
            {
                characterManager.ModifyStat(targetCharacter, targetStat, percentageChange);
                Debug.Log($"Applied {percentageChange:+0.0;-0.0}% to {targetCharacter} {targetStat}");
            }
        }
    }
    
    /// <summary>
    /// Meta shift cards that affect multiple characters
    /// </summary>
    [CreateAssetMenu(fileName = "MetaShiftCard", menuName = "Meta Balance/Meta Shift Card")]
    public class MetaShiftCard : CardData
    {
        [Header("Meta Shift Settings")]
        public MetaShiftType shiftType;
        [Range(1, 10)]
        public int effectPower = 5;
        
        public override void PlayCard()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            switch (shiftType)
            {
                case MetaShiftType.MapPool:
                    // Favor tanks and supports
                    characterManager.ModifyStat(Characters.CharacterType.Tank, Characters.CharacterStat.WinRate, effectPower);
                    characterManager.ModifyStat(Characters.CharacterType.Support, Characters.CharacterStat.WinRate, effectPower * 0.8f);
                    characterManager.ModifyStat(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, -effectPower * 0.5f);
                    break;
                    
                case MetaShiftType.GameMode:
                    // Favor damage dealers
                    characterManager.ModifyStat(Characters.CharacterType.Mage, Characters.CharacterStat.WinRate, effectPower);
                    characterManager.ModifyStat(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, effectPower * 0.7f);
                    break;
                    
                case MetaShiftType.ItemBalance:
                    // Small changes to all characters
                    foreach (Characters.CharacterType type in Enum.GetValues(typeof(Characters.CharacterType)))
                    {
                        float change = UnityEngine.Random.Range(-effectPower * 0.5f, effectPower * 0.5f);
                        characterManager.ModifyStat(type, Characters.CharacterStat.WinRate, change);
                    }
                    break;
            }
            
            Debug.Log($"Applied {shiftType} meta shift with power {effectPower}");
        }
    }
    
    /// <summary>
    /// Community cards that affect popularity
    /// </summary>
    [CreateAssetMenu(fileName = "CommunityCard", menuName = "Meta Balance/Community Card")]
    public class CommunityCard : CardData
    {
        [Header("Community Settings")]
        public bool affectsAllCharacters = true;
        public Characters.CharacterType specificCharacter;
        [Range(-20f, 20f)]
        public float popularityChange = 10f;
        
        public override void PlayCard()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            if (affectsAllCharacters)
            {
                foreach (Characters.CharacterType type in Enum.GetValues(typeof(Characters.CharacterType)))
                {
                    characterManager.ModifyStat(type, Characters.CharacterStat.Popularity, popularityChange);
                }
                Debug.Log($"Applied {popularityChange:+0.0;-0.0}% popularity to all characters");
            }
            else
            {
                characterManager.ModifyStat(specificCharacter, Characters.CharacterStat.Popularity, popularityChange);
                Debug.Log($"Applied {popularityChange:+0.0;-0.0}% popularity to {specificCharacter}");
            }
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