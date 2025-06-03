using UnityEngine;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Configuration settings for the balance calculation system
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceCalculationSettings", menuName = "Meta Balance/Balance Calculation Settings")]
    public class BalanceCalculationSettings : ScriptableObject
    {
        [Header("Win Rate Calculation")]
        [Range(0f, 1f)]
        [Tooltip("How quickly win rates transition to new values (0 = instant, 1 = very slow)")]
        public float winRateTransitionSpeed = 0.3f;
        
        [Range(0f, 10f)]
        [Tooltip("Maximum random variance applied to win rates each calculation")]
        public float maxRandomVariance = 3f;
        
        [Header("Power Level Weights")]
        [Range(0f, 1f)]
        [Tooltip("Weight of Health stat in power calculation")]
        public float healthWeight = 0.25f;
        
        [Range(0f, 1f)]
        [Tooltip("Weight of Damage stat in power calculation")]
        public float damageWeight = 0.35f;
        
        [Range(0f, 1f)]
        [Tooltip("Weight of Speed stat in power calculation")]
        public float speedWeight = 0.2f;
        
        [Range(0f, 1f)]
        [Tooltip("Weight of Utility stat in power calculation")]
        public float utilityWeight = 0.2f;
        
        [Header("Matchup System")]
        [Range(0f, 20f)]
        [Tooltip("Maximum win rate modifier from character matchups")]
        public float maxMatchupModifier = 8f;
        
        [Range(0f, 1f)]
        [Tooltip("How much matchups affect win rates vs base power")]
        public float matchupInfluence = 0.4f;
        
        [Header("Popularity Effects")]
        [Range(0f, 10f)]
        [Tooltip("Maximum win rate penalty for very popular characters")]
        public float popularityPenalty = 4f;
        
        [Range(0f, 10f)]
        [Tooltip("Maximum win rate bonus for unpopular characters")]
        public float unpopularityBonus = 3f;
        
        [Range(0f, 100f)]
        [Tooltip("Popularity threshold above which penalties apply")]
        public float highPopularityThreshold = 70f;
        
        [Range(0f, 100f)]
        [Tooltip("Popularity threshold below which bonuses apply")]
        public float lowPopularityThreshold = 30f;
        
        [Header("Meta Health")]
        [Range(0f, 50f)]
        [Tooltip("Ideal win rate range around 50% for balanced characters")]
        public float idealWinRateRange = 5f;
        
        [Range(0f, 50f)]
        [Tooltip("Ideal popularity range for diverse character usage")]
        public float idealPopularityRange = 25f;
        
        [Header("Advanced Settings")]
        [Range(1, 10)]
        [Tooltip("Number of calculation iterations for stability")]
        public int calculationIterations = 3;
        
        [Range(0f, 1f)]
        [Tooltip("How much previous calculations influence new ones")]
        public float calculationMomentum = 0.2f;
        
        [Header("Character Archetype Modifiers")]
        [Tooltip("Base win rate modifiers for each character type")]
        public CharacterArchetypeModifiers archetypeModifiers;
        
        /// <summary>
        /// Initialize default values
        /// </summary>
        public void InitializeDefaults()
        {
            winRateTransitionSpeed = 0.3f;
            maxRandomVariance = 3f;
            
            healthWeight = 0.25f;
            damageWeight = 0.35f;
            speedWeight = 0.2f;
            utilityWeight = 0.2f;
            
            maxMatchupModifier = 8f;
            matchupInfluence = 0.4f;
            
            popularityPenalty = 4f;
            unpopularityBonus = 3f;
            highPopularityThreshold = 70f;
            lowPopularityThreshold = 30f;
            
            idealWinRateRange = 5f;
            idealPopularityRange = 25f;
            
            calculationIterations = 3;
            calculationMomentum = 0.2f;
            
            // Initialize archetype modifiers
            if (archetypeModifiers == null)
            {
                archetypeModifiers = new CharacterArchetypeModifiers();
            }
        }
        
        /// <summary>
        /// Get total weight (should equal 1.0 for balanced calculation)
        /// </summary>
        public float GetTotalWeight()
        {
            return healthWeight + damageWeight + speedWeight + utilityWeight;
        }
        
        /// <summary>
        /// Normalize weights to ensure they sum to 1.0
        /// </summary>
        [ContextMenu("Normalize Weights")]
        public void NormalizeWeights()
        {
            float total = GetTotalWeight();
            if (total > 0f)
            {
                healthWeight /= total;
                damageWeight /= total;
                speedWeight /= total;
                utilityWeight /= total;
            }
        }
        
        /// <summary>
        /// Reset to default balanced values
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            InitializeDefaults();
        }
    }
    
    /// <summary>
    /// Archetype-specific modifiers for character balance
    /// </summary>
    [System.Serializable]
    public class CharacterArchetypeModifiers
    {
        [Header("Base Win Rate Modifiers")]
        [Range(-10f, 10f)]
        public float warriorModifier = 0f;
        
        [Range(-10f, 10f)]
        public float mageModifier = 0f;
        
        [Range(-10f, 10f)]
        public float supportModifier = -2f; // Supports often have lower win rates but high impact
        
        [Range(-10f, 10f)]
        public float tankModifier = 1f; // Tanks slightly easier to play
        
        [Header("Popularity Modifiers")]
        [Range(-20f, 20f)]
        public float warriorPopularityMod = 5f; // Warriors tend to be popular
        
        [Range(-20f, 20f)]
        public float magePopularityMod = 0f;
        
        [Range(-20f, 20f)]
        public float supportPopularityMod = -10f; // Support often less popular
        
        [Range(-20f, 20f)]
        public float tankPopularityMod = -5f;
        
        /// <summary>
        /// Get win rate modifier for a character type
        /// </summary>
        public float GetWinRateModifier(CharacterType type)
        {
            return type switch
            {
                CharacterType.Warrior => warriorModifier,
                CharacterType.Mage => mageModifier,
                CharacterType.Support => supportModifier,
                CharacterType.Tank => tankModifier,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Get popularity modifier for a character type
        /// </summary>
        public float GetPopularityModifier(CharacterType type)
        {
            return type switch
            {
                CharacterType.Warrior => warriorPopularityMod,
                CharacterType.Mage => magePopularityMod,
                CharacterType.Support => supportPopularityMod,
                CharacterType.Tank => tankPopularityMod,
                _ => 0f
            };
        }
    }
}