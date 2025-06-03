using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Handles complex meta calculations for realistic win rate determination
    /// </summary>
    public class MetaCalculator : MonoBehaviour
    {
        private CharacterManager characterManager;
        private BalanceCalculationSettings settings;
        
        // Meta state tracking
        private Dictionary<CharacterType, float> previousWinRates = new Dictionary<CharacterType, float>();
        private float metaShiftIntensity = 0f;
        private int calculationCycle = 0;
        
        public void Initialize(CharacterManager manager, BalanceCalculationSettings calculationSettings)
        {
            characterManager = manager;
            settings = calculationSettings;
            
            // Initialize previous win rates
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                previousWinRates[type] = 50f;
            }
        }
        
        /// <summary>
        /// Calculate base power levels for all characters
        /// </summary>
        public Dictionary<CharacterType, float> CalculateBasePowerLevels(
            Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            var powerLevels = new Dictionary<CharacterType, float>();
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (characterStats.ContainsKey(type))
                {
                    float power = CalculateCharacterPower(type, characterStats[type]);
                    powerLevels[type] = power;
                }
                else
                {
                    powerLevels[type] = 50f; // Default
                }
            }
            
            // Convert power levels to win rates relative to average
            return ConvertPowerToWinRates(powerLevels);
        }
        
        private float CalculateCharacterPower(CharacterType type, Dictionary<CharacterStat, float> stats)
        {
            float health = stats.GetValueOrDefault(CharacterStat.Health, 50f);
            float damage = stats.GetValueOrDefault(CharacterStat.Damage, 50f);
            float speed = stats.GetValueOrDefault(CharacterStat.Speed, 50f);
            float utility = stats.GetValueOrDefault(CharacterStat.Utility, 50f);
            
            // Advanced power calculation considering character archetype
            float basePower = CalculateWeightedPower(health, damage, speed, utility);
            
            // Apply archetype-specific calculations
            float archetypePower = ApplyArchetypeModifiers(type, basePower, health, damage, speed, utility);
            
            // Apply synergy bonuses/penalties
            float synergyPower = ApplySynergyModifiers(type, health, damage, speed, utility);
            
            return (basePower * 0.6f) + (archetypePower * 0.3f) + (synergyPower * 0.1f);
        }
        
        private float CalculateWeightedPower(float health, float damage, float speed, float utility)
        {
            return (health * settings.healthWeight) +
                   (damage * settings.damageWeight) +
                   (speed * settings.speedWeight) +
                   (utility * settings.utilityWeight);
        }
        
        private float ApplyArchetypeModifiers(CharacterType type, float basePower, 
            float health, float damage, float speed, float utility)
        {
            return type switch
            {
                CharacterType.Warrior => CalculateWarriorPower(health, damage, speed, utility),
                CharacterType.Mage => CalculateMagePower(health, damage, speed, utility),
                CharacterType.Support => CalculateSupportPower(health, damage, speed, utility),
                CharacterType.Tank => CalculateTankPower(health, damage, speed, utility),
                _ => basePower
            };
        }
        
        private float CalculateWarriorPower(float health, float damage, float speed, float utility)
        {
            // Warriors benefit from balanced stats, penalty for extremes
            float statBalance = CalculateStatBalance(health, damage, speed, utility);
            float balanceBonus = Mathf.Max(0f, (80f - statBalance) * 0.3f);
            
            // Warriors excel when health and damage are both high
            float combatSynergy = Mathf.Min(health, damage) * 0.8f;
            
            return (health * 0.4f) + (damage * 0.4f) + (speed * 0.1f) + (utility * 0.1f) + 
                   balanceBonus + (combatSynergy * 0.2f);
        }
        
        private float CalculateMagePower(float health, float damage, float speed, float utility)
        {
            // Mages are high-risk high-reward - damage is crucial, but low health is risky
            float damageEffectiveness = damage * 1.2f;
            
            // Health penalty - mages are more vulnerable when health is too low
            float healthPenalty = health < 40f ? (40f - health) * 0.5f : 0f;
            
            // Speed bonus for kiting and positioning
            float mobilityBonus = speed > 50f ? (speed - 50f) * 0.3f : 0f;
            
            // Utility helps with control and survivability
            float controlBonus = utility * 0.4f;
            
            return damageEffectiveness + controlBonus + mobilityBonus - healthPenalty;
        }
        
        private float CalculateSupportPower(float health, float damage, float speed, float utility)
        {
            // Supports are all about utility and enabling team play
            float utilityEffectiveness = utility * 1.5f;
            
            // Speed is important for positioning and escaping
            float mobilityBonus = speed * 0.6f;
            
            // Health provides survivability to keep supporting
            float survivabilityBonus = health * 0.4f;
            
            // Damage is least important but still matters
            float damageContribution = damage * 0.2f;
            
            // Synergy bonus when utility and speed are both high
            float supportSynergy = Mathf.Min(utility, speed) * 0.3f;
            
            return (utilityEffectiveness * 0.4f) + (mobilityBonus * 0.25f) + 
                   (survivabilityBonus * 0.2f) + (damageContribution * 0.1f) + (supportSynergy * 0.05f);
        }
        
        private float CalculateTankPower(float health, float damage, float speed, float utility)
        {
            // Tanks prioritize survivability and utility
            float survivabilityCore = health * 1.3f;
            
            // Utility for crowd control and team protection
            float controlEffectiveness = utility * 0.8f;
            
            // Speed penalty for being too fast (not tanky enough)
            float speedPenalty = speed > 60f ? (speed - 60f) * 0.2f : 0f;
            
            // Damage still matters but less
            float damageContribution = damage * 0.4f;
            
            // Tanking synergy - health and utility working together
            float tankingSynergy = (health + utility) * 0.15f;
            
            return survivabilityCore + controlEffectiveness + damageContribution + tankingSynergy - speedPenalty;
        }
        
        private float ApplySynergyModifiers(CharacterType type, float health, float damage, float speed, float utility)
        {
            // Calculate stat synergies that apply to all characters
            float totalStats = health + damage + speed + utility;
            float avgStat = totalStats / 4f;
            
            // Bonus for well-rounded builds
            float balanceBonus = CalculateBalanceBonus(health, damage, speed, utility, avgStat);
            
            // Bonus for specialized builds
            float specializationBonus = CalculateSpecializationBonus(health, damage, speed, utility, avgStat);
            
            // Meta-dependent bonuses
            float metaBonus = CalculateMetaBonus(type, health, damage, speed, utility);
            
            return balanceBonus + specializationBonus + metaBonus;
        }
        
        private float CalculateStatBalance(float health, float damage, float speed, float utility)
        {
            float[] stats = { health, damage, speed, utility };
            float avg = stats.Average();
            float variance = stats.Sum(stat => Mathf.Pow(stat - avg, 2)) / stats.Length;
            return Mathf.Sqrt(variance); // Standard deviation
        }
        
        private float CalculateBalanceBonus(float health, float damage, float speed, float utility, float avgStat)
        {
            // Bonus for having stats close to each other
            float maxDeviation = Mathf.Max(
                Mathf.Abs(health - avgStat),
                Mathf.Abs(damage - avgStat),
                Mathf.Abs(speed - avgStat),
                Mathf.Abs(utility - avgStat)
            );
            
            // Smaller deviation = bigger bonus
            return Mathf.Max(0f, (20f - maxDeviation) * 0.1f);
        }
        
        private float CalculateSpecializationBonus(float health, float damage, float speed, float utility, float avgStat)
        {
            // Bonus for having at least one very high stat
            float maxStat = Mathf.Max(health, damage, speed, utility);
            
            if (maxStat > avgStat + 20f)
            {
                return (maxStat - avgStat - 20f) * 0.05f;
            }
            
            return 0f;
        }
        
        private float CalculateMetaBonus(CharacterType type, float health, float damage, float speed, float utility)
        {
            // Simulate meta shifts that favor certain stat distributions
            float metaCycle = Mathf.Sin(calculationCycle * 0.1f) * 2f;
            
            return type switch
            {
                CharacterType.Warrior => damage * metaCycle * 0.02f,
                CharacterType.Mage => utility * metaCycle * 0.02f,
                CharacterType.Support => speed * metaCycle * 0.02f,
                CharacterType.Tank => health * metaCycle * 0.02f,
                _ => 0f
            };
        }
        
        private Dictionary<CharacterType, float> ConvertPowerToWinRates(Dictionary<CharacterType, float> powerLevels)
        {
            var winRates = new Dictionary<CharacterType, float>();
            
            // Calculate average power
            float avgPower = powerLevels.Values.Average();
            
            foreach (var kvp in powerLevels)
            {
                CharacterType type = kvp.Key;
                float power = kvp.Value;
                
                // Convert power difference to win rate
                float powerRatio = power / avgPower;
                float baseWinRate = 50f * powerRatio;
                
                // Apply archetype modifiers
                if (settings.archetypeModifiers != null)
                {
                    baseWinRate += settings.archetypeModifiers.GetWinRateModifier(type);
                }
                
                // Ensure reasonable bounds
                baseWinRate = Mathf.Clamp(baseWinRate, 25f, 75f);
                
                winRates[type] = baseWinRate;
            }
            
            return winRates;
        }
        
        /// <summary>
        /// Calculate random variance to simulate meta uncertainty
        /// </summary>
        public float CalculateMetaVariance(CharacterType type)
        {
            // Base random variance
            float baseVariance = Random.Range(-settings.maxRandomVariance, settings.maxRandomVariance);
            
            // Add periodic meta shifts
            float metaShift = Mathf.Sin((calculationCycle + (int)type) * 0.15f) * 2f;
            
            // Add some character-specific meta tendencies
            float characterTendency = type switch
            {
                CharacterType.Warrior => Mathf.Sin(calculationCycle * 0.08f) * 1.5f,
                CharacterType.Mage => Mathf.Cos(calculationCycle * 0.12f) * 2f,
                CharacterType.Support => Mathf.Sin(calculationCycle * 0.06f + 1f) * 1f,
                CharacterType.Tank => Mathf.Cos(calculationCycle * 0.1f + 2f) * 1.5f,
                _ => 0f
            };
            
            return baseVariance + metaShift + characterTendency;
        }
        
        /// <summary>
        /// Calculate overall meta health score
        /// </summary>
        public float CalculateMetaHealth(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            float diversityScore = CalculateDiversityScore(characterStats);
            float balanceScore = CalculateBalanceScore(characterStats);
            float engagementScore = CalculateEngagementScore(characterStats);
            
            return (diversityScore * 0.4f) + (balanceScore * 0.4f) + (engagementScore * 0.2f);
        }
        
        private float CalculateDiversityScore(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            // Check how different characters are from each other
            float totalDifference = 0f;
            int comparisons = 0;
            
            var characterTypes = System.Enum.GetValues(typeof(CharacterType)).Cast<CharacterType>().ToArray();
            
            for (int i = 0; i < characterTypes.Length; i++)
            {
                for (int j = i + 1; j < characterTypes.Length; j++)
                {
                    float difference = CalculateCharacterDifference(
                        characterStats[characterTypes[i]], 
                        characterStats[characterTypes[j]]
                    );
                    totalDifference += difference;
                    comparisons++;
                }
            }
            
            float avgDifference = comparisons > 0 ? totalDifference / comparisons : 0f;
            
            // Convert to 0-100 score (higher difference = better diversity)
            return Mathf.Clamp(avgDifference * 2f, 0f, 100f);
        }
        
        private float CalculateCharacterDifference(Dictionary<CharacterStat, float> statsA, Dictionary<CharacterStat, float> statsB)
        {
            float totalDiff = 0f;
            int statCount = 0;
            
            foreach (CharacterStat stat in System.Enum.GetValues(typeof(CharacterStat)))
            {
                if (stat == CharacterStat.WinRate || stat == CharacterStat.Popularity) continue;
                
                float valueA = statsA.GetValueOrDefault(stat, 50f);
                float valueB = statsB.GetValueOrDefault(stat, 50f);
                totalDiff += Mathf.Abs(valueA - valueB);
                statCount++;
            }
            
            return statCount > 0 ? totalDiff / statCount : 0f;
        }
        
        private float CalculateBalanceScore(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            // Check how close win rates are to ideal range
            float totalDeviation = 0f;
            int characterCount = 0;
            
            foreach (var kvp in characterStats)
            {
                float winRate = kvp.Value.GetValueOrDefault(CharacterStat.WinRate, 50f);
                float deviation = Mathf.Abs(winRate - 50f);
                
                if (deviation > settings.idealWinRateRange)
                {
                    totalDeviation += deviation - settings.idealWinRateRange;
                }
                
                characterCount++;
            }
            
            float avgExcessDeviation = characterCount > 0 ? totalDeviation / characterCount : 0f;
            return Mathf.Max(0f, 100f - (avgExcessDeviation * 3f));
        }
        
        private float CalculateEngagementScore(Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats)
        {
            // Check popularity distribution for player engagement
            float minPop = float.MaxValue;
            float maxPop = float.MinValue;
            
            foreach (var kvp in characterStats)
            {
                float popularity = kvp.Value.GetValueOrDefault(CharacterStat.Popularity, 50f);
                minPop = Mathf.Min(minPop, popularity);
                maxPop = Mathf.Max(maxPop, popularity);
            }
            
            float popularityRange = maxPop - minPop;
            
            // Ideal range allows for some preference but not total dominance
            float idealRange = settings.idealPopularityRange;
            
            if (popularityRange <= idealRange)
            {
                return 100f;
            }
            else
            {
                float excess = popularityRange - idealRange;
                return Mathf.Max(0f, 100f - (excess * 2f));
            }
        }
        
        /// <summary>
        /// Update calculation cycle for meta evolution
        /// </summary>
        public void AdvanceCalculationCycle()
        {
            calculationCycle++;
            
            // Update meta shift intensity based on recent changes
            UpdateMetaShiftIntensity();
        }
        
        private void UpdateMetaShiftIntensity()
        {
            // Calculate how much win rates have changed recently
            float totalChange = 0f;
            int characterCount = 0;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (characterManager != null)
                {
                    float currentWinRate = characterManager.GetStat(type, CharacterStat.WinRate);
                    float previousWinRate = previousWinRates.GetValueOrDefault(type, 50f);
                    
                    totalChange += Mathf.Abs(currentWinRate - previousWinRate);
                    previousWinRates[type] = currentWinRate;
                    characterCount++;
                }
            }
            
            metaShiftIntensity = characterCount > 0 ? totalChange / characterCount : 0f;
        }
        
        public float GetMetaShiftIntensity() => metaShiftIntensity;
        public int GetCalculationCycle() => calculationCycle;
        
        [ContextMenu("🔄 Debug: Advance Meta Cycle")]
        public void DebugAdvanceMetaCycle()
        {
            AdvanceCalculationCycle();
            Debug.Log($"Meta cycle advanced to {calculationCycle}, shift intensity: {metaShiftIntensity:F2}");
        }
    }
}