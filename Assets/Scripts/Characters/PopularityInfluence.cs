using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Handles how character popularity affects win rates and meta balance
    /// </summary>
    public class PopularityInfluence : MonoBehaviour
    {
        private CharacterManager characterManager;
        private BalanceCalculationSettings settings;
        
        // Popularity tracking
        private Dictionary<CharacterType, float> previousPopularity = new Dictionary<CharacterType, float>();
        private Dictionary<CharacterType, float> popularityTrends = new Dictionary<CharacterType, float>();
        
        public void Initialize(CharacterManager manager, BalanceCalculationSettings calculationSettings)
        {
            characterManager = manager;
            settings = calculationSettings;
            
            // Initialize popularity tracking
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                previousPopularity[type] = 50f;
                popularityTrends[type] = 0f;
            }
        }
        
        /// <summary>
        /// Apply popularity effects to win rates
        /// </summary>
        public Dictionary<CharacterType, float> ApplyPopularityEffects(
            Dictionary<CharacterType, float> baseWinRates,
            Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            var adjustedWinRates = new Dictionary<CharacterType, float>();
            
            // Update popularity trends
            UpdatePopularityTrends(characterStats);
            
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float baseWinRate = baseWinRates.GetValueOrDefault(character, 50f);
                float popularity = characterStats[character].GetValueOrDefault(CharacterStat.Popularity, 50f);
                
                // Calculate popularity effects
                float popularityEffect = CalculatePopularityEffect(character, popularity);
                float trendEffect = CalculateTrendEffect(character);
                float metaCounterEffect = CalculateMetaCounterEffect(character, characterStats);
                
                float totalEffect = popularityEffect + trendEffect + metaCounterEffect;
                float adjustedWinRate = baseWinRate + totalEffect;
                
                adjustedWinRates[character] = adjustedWinRate;
                
                Debug.Log($"🌟 {character}: Base {baseWinRate:F1}% + Popularity {totalEffect:F1}% = {adjustedWinRate:F1}%");
            }
            
            return adjustedWinRates;
        }
        
        private void UpdatePopularityTrends(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float currentPopularity = characterStats[character].GetValueOrDefault(CharacterStat.Popularity, 50f);
                float prevPopularity = previousPopularity.GetValueOrDefault(character, 50f);
                
                // Calculate trend (positive = gaining popularity, negative = losing)
                float trend = currentPopularity - prevPopularity;
                
                // Smooth the trend with momentum
                float currentTrend = popularityTrends.GetValueOrDefault(character, 0f);
                popularityTrends[character] = Mathf.Lerp(currentTrend, trend, 0.3f);
                
                // Update previous popularity
                previousPopularity[character] = currentPopularity;
            }
        }
        
        private float CalculatePopularityEffect(CharacterType character, float popularity)
        {
            float effect = 0f;
            
            // High popularity penalty (overcentralization)
            if (popularity > settings.highPopularityThreshold)
            {
                float excess = popularity - settings.highPopularityThreshold;
                float maxExcess = 100f - settings.highPopularityThreshold;
                float penaltyRatio = excess / maxExcess;
                
                effect -= settings.popularityPenalty * penaltyRatio;
                
                // Additional scaling penalty for extreme popularity
                if (popularity > 85f)
                {
                    effect -= (popularity - 85f) * 0.2f;
                }
            }
            // Low popularity bonus (underrepresented characters)
            else if (popularity < settings.lowPopularityThreshold)
            {
                float deficit = settings.lowPopularityThreshold - popularity;
                float maxDeficit = settings.lowPopularityThreshold;
                float bonusRatio = deficit / maxDeficit;
                
                effect += settings.unpopularityBonus * bonusRatio;
            }
            
            return effect;
        }
        
        private float CalculateTrendEffect(CharacterType character)
        {
            float trend = popularityTrends.GetValueOrDefault(character, 0f);
            
            // Trending characters get slight penalties (meta adaptation)
            // Declining characters get slight bonuses (hidden power)
            return -trend * 0.1f;
        }
        
        private float CalculateMetaCounterEffect(CharacterType character, 
            Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            // Characters that counter popular characters should get win rate bonuses
            float counterBonus = 0f;
            
            foreach (CharacterType opponent in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (character == opponent) continue;
                
                float opponentPopularity = characterStats[opponent].GetValueOrDefault(CharacterStat.Popularity, 50f);
                
                // Only consider popular opponents
                if (opponentPopularity > 60f)
                {
                    float matchupAdvantage = GetMatchupAdvantage(character, opponent);
                    
                    if (matchupAdvantage > 0f)
                    {
                        // Weight by opponent popularity
                        float weight = (opponentPopularity - 60f) / 40f; // 0-1 scale
                        counterBonus += matchupAdvantage * weight * 0.5f;
                    }
                }
            }
            
            return Mathf.Clamp(counterBonus, 0f, 3f);
        }
        
        private float GetMatchupAdvantage(CharacterType character, CharacterType opponent)
        {
            // Get matchup data from MatchupMatrix if available
            var matchupMatrix = GetComponent<MatchupMatrix>();
            if (matchupMatrix != null)
            {
                return matchupMatrix.GetMatchupRating(character, opponent);
            }
            
            // Fallback to basic archetype matchups
            return CalculateBasicMatchup(character, opponent);
        }
        
        private float CalculateBasicMatchup(CharacterType attacker, CharacterType defender)
        {
            return (attacker, defender) switch
            {
                (CharacterType.Warrior, CharacterType.Mage) => 3f,
                (CharacterType.Warrior, CharacterType.Support) => 4f,
                (CharacterType.Warrior, CharacterType.Tank) => -2f,
                (CharacterType.Mage, CharacterType.Warrior) => -3f,
                (CharacterType.Mage, CharacterType.Support) => 2f,
                (CharacterType.Mage, CharacterType.Tank) => 1f,
                (CharacterType.Support, CharacterType.Warrior) => -4f,
                (CharacterType.Support, CharacterType.Mage) => -2f,
                (CharacterType.Support, CharacterType.Tank) => 3f,
                (CharacterType.Tank, CharacterType.Warrior) => 2f,
                (CharacterType.Tank, CharacterType.Mage) => -1f,
                (CharacterType.Tank, CharacterType.Support) => -3f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// Simulate popularity changes based on win rate and other factors
        /// </summary>
        public void UpdatePopularityBasedOnPerformance(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (!characterStats.ContainsKey(character)) continue;
                
                float currentPopularity = characterStats[character].GetValueOrDefault(CharacterStat.Popularity, 50f);
                float winRate = characterStats[character].GetValueOrDefault(CharacterStat.WinRate, 50f);
                
                // Calculate popularity change factors
                float performanceInfluence = CalculatePerformanceInfluence(winRate);
                float archetypeAppeal = CalculateArchetypeAppeal(character);
                float metaAdaptation = CalculateMetaAdaptation(character, characterStats);
                
                // Apply changes
                float popularityChange = (performanceInfluence + archetypeAppeal + metaAdaptation) * 0.3f;
                float newPopularity = Mathf.Clamp(currentPopularity + popularityChange, 0f, 100f);
                
                // Update popularity in character stats
                characterStats[character][CharacterStat.Popularity] = newPopularity;
                
                if (Mathf.Abs(popularityChange) > 0.5f)
                {
                    Debug.Log($"📈 {character} popularity: {currentPopularity:F1}% → {newPopularity:F1}% ({popularityChange:+0.1;-0.1})");
                }
            }
        }
        
        private float CalculatePerformanceInfluence(float winRate)
        {
            // Higher win rates gradually increase popularity
            // Lower win rates gradually decrease popularity
            float deviation = winRate - 50f;
            return deviation * 0.2f; // 60% win rate = +2 popularity per update
        }
        
        private float CalculateArchetypeAppeal(CharacterType character)
        {
            // Different archetypes have different baseline appeal
            float baseAppeal = character switch
            {
                CharacterType.Warrior => 0.5f,    // Generally popular
                CharacterType.Mage => 0.2f,       // Moderate appeal
                CharacterType.Support => -0.8f,   // Often less popular
                CharacterType.Tank => -0.3f,      // Slightly less popular
                _ => 0f
            };
            
            // Add some randomness for meta shifts
            return baseAppeal + Random.Range(-0.3f, 0.3f);
        }
        
        private float CalculateMetaAdaptation(CharacterType character, 
            Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            // Characters that counter the current meta should gain popularity
            float metaAdaptation = 0f;
            
            // Find most popular character
            CharacterType mostPopular = CharacterType.Warrior;
            float highestPopularity = 0f;
            
            foreach (var kvp in characterStats)
            {
                float pop = kvp.Value.GetValueOrDefault(CharacterStat.Popularity, 50f);
                if (pop > highestPopularity)
                {
                    highestPopularity = pop;
                    mostPopular = kvp.Key;
                }
            }
            
            // If this character counters the most popular character
            if (mostPopular != character && highestPopularity > 65f)
            {
                float matchupAdvantage = GetMatchupAdvantage(character, mostPopular);
                if (matchupAdvantage > 1f)
                {
                    metaAdaptation = matchupAdvantage * 0.3f;
                }
            }
            
            return metaAdaptation;
        }
        
        /// <summary>
        /// Get popularity trend for a character
        /// </summary>
        public float GetPopularityTrend(CharacterType character)
        {
            return popularityTrends.GetValueOrDefault(character, 0f);
        }
        
        /// <summary>
        /// Calculate overall meta diversity based on popularity distribution
        /// </summary>
        public float CalculateMetaDiversity(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            var popularities = new List<float>();
            
            foreach (var kvp in characterStats)
            {
                popularities.Add(kvp.Value.GetValueOrDefault(CharacterStat.Popularity, 50f));
            }
            
            if (popularities.Count == 0) return 50f;
            
            // Calculate standard deviation of popularities
            float mean = popularities.Average();
            float variance = popularities.Sum(p => Mathf.Pow(p - mean, 2)) / popularities.Count;
            float standardDeviation = Mathf.Sqrt(variance);
            
            // Convert to diversity score (lower deviation = higher diversity)
            float diversityScore = Mathf.Max(0f, 100f - (standardDeviation * 2f));
            
            return diversityScore;
        }
        
        /// <summary>
        /// Get the most and least popular characters
        /// </summary>
        public (CharacterType most, CharacterType least) GetPopularityExtremes(
            Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            CharacterType mostPopular = CharacterType.Warrior;
            CharacterType leastPopular = CharacterType.Warrior;
            float highestPop = 0f;
            float lowestPop = 100f;
            
            foreach (var kvp in characterStats)
            {
                float pop = kvp.Value.GetValueOrDefault(CharacterStat.Popularity, 50f);
                
                if (pop > highestPop)
                {
                    highestPop = pop;
                    mostPopular = kvp.Key;
                }
                
                if (pop < lowestPop)
                {
                    lowestPop = pop;
                    leastPopular = kvp.Key;
                }
            }
            
            return (mostPopular, leastPopular);
        }
        
        // Debug methods
        [ContextMenu("📊 Debug: Show Popularity Analysis")]
        public void DebugShowPopularityAnalysis()
        {
            if (characterManager == null) return;
            
            Debug.Log("=== 📊 POPULARITY ANALYSIS ===");
            
            var characterStats = characterManager.GetAllStats();
            var extremes = GetPopularityExtremes(characterStats);
            float diversity = CalculateMetaDiversity(characterStats);
            
            Debug.Log($"Most Popular: {extremes.most}");
            Debug.Log($"Least Popular: {extremes.least}");
            Debug.Log($"Meta Diversity: {diversity:F1}/100");
            
            Debug.Log("\nPopularity Trends:");
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float popularity = characterManager.GetStat(character, CharacterStat.Popularity);
                float trend = GetPopularityTrend(character);
                string trendDirection = trend > 0.5f ? "📈 RISING" : trend < -0.5f ? "📉 FALLING" : "➡️ STABLE";
                
                Debug.Log($"  {character}: {popularity:F1}% {trendDirection} ({trend:+0.1;-0.1})");
            }
        }
        
        [ContextMenu("🔄 Debug: Simulate Popularity Update")]
        public void DebugSimulatePopularityUpdate()
        {
            if (characterManager == null) return;
            
            var characterStats = characterManager.GetAllStats();
            
            Debug.Log("=== 🔄 SIMULATING POPULARITY UPDATE ===");
            Debug.Log("Before:");
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float pop = characterManager.GetStat(character, CharacterStat.Popularity);
                Debug.Log($"  {character}: {pop:F1}%");
            }
            
            UpdatePopularityBasedOnPerformance(characterStats);
            
            Debug.Log("After:");
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float pop = characterStats[character][CharacterStat.Popularity];
                Debug.Log($"  {character}: {pop:F1}%");
            }
        }
        
        [ContextMenu("⚖️ Debug: Show Popularity Effects")]
        public void DebugShowPopularityEffects()
        {
            if (characterManager == null) return;
            
            Debug.Log("=== ⚖️ POPULARITY EFFECTS ON WIN RATES ===");
            
            var characterStats = characterManager.GetAllStats();
            var baseWinRates = characterManager.GetAllWinRates();
            var adjustedWinRates = ApplyPopularityEffects(baseWinRates, characterStats);
            
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float baseRate = baseWinRates.GetValueOrDefault(character, 50f);
                float adjustedRate = adjustedWinRates.GetValueOrDefault(character, 50f);
                float effect = adjustedRate - baseRate;
                
                string effectType = effect > 0.5f ? "BONUS" : effect < -0.5f ? "PENALTY" : "NEUTRAL";
                Debug.Log($"{character}: {baseRate:F1}% → {adjustedRate:F1}% ({effect:+0.1;-0.1} {effectType})");
            }
        }
    }
}