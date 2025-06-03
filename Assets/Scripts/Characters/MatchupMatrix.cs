using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Handles character vs character matchup calculations
    /// </summary>
    public class MatchupMatrix : MonoBehaviour
    {
        private CharacterManager characterManager;
        private BalanceCalculationSettings settings;
        
        // Matchup matrix: [attacker][defender] = modifier
        private Dictionary<CharacterType, Dictionary<CharacterType, float>> matchupMatrix;
        
        public void Initialize(CharacterManager manager, BalanceCalculationSettings calculationSettings)
        {
            characterManager = manager;
            settings = calculationSettings;
            
            BuildMatchupMatrix();
        }
        
        /// <summary>
        /// Apply matchup influences to base win rates
        /// </summary>
        public Dictionary<CharacterType, float> ApplyMatchupInfluence(Dictionary<CharacterType, float> baseWinRates)
        {
            var adjustedWinRates = new Dictionary<CharacterType, float>();
            
            foreach (CharacterType attacker in System.Enum.GetValues(typeof(CharacterType)))
            {
                float baseWinRate = baseWinRates.GetValueOrDefault(attacker, 50f);
                float matchupAdjustment = CalculateMatchupAdjustment(attacker, baseWinRates);
                
                float adjustedWinRate = baseWinRate + (matchupAdjustment * settings.matchupInfluence);
                adjustedWinRates[attacker] = adjustedWinRate;
                
                Debug.Log($"🎯 {attacker}: Base {baseWinRate:F1}% + Matchup {matchupAdjustment:F1}% = {adjustedWinRate:F1}%");
            }
            
            return adjustedWinRates;
        }
        
        private void BuildMatchupMatrix()
        {
            matchupMatrix = new Dictionary<CharacterType, Dictionary<CharacterType, float>>();
            
            // Initialize matrix
            foreach (CharacterType attacker in System.Enum.GetValues(typeof(CharacterType)))
            {
                matchupMatrix[attacker] = new Dictionary<CharacterType, float>();
                
                foreach (CharacterType defender in System.Enum.GetValues(typeof(CharacterType)))
                {
                    matchupMatrix[attacker][defender] = CalculateBaseMatchup(attacker, defender);
                }
            }
            
            Debug.Log("Matchup matrix built with realistic character interactions");
        }
        
        private float CalculateBaseMatchup(CharacterType attacker, CharacterType defender)
        {
            // Self-matchup is neutral
            if (attacker == defender) return 0f;
            
            // Define matchup relationships based on character archetypes
            return (attacker, defender) switch
            {
                // Warrior matchups
                (CharacterType.Warrior, CharacterType.Mage) => 3f,      // Warriors counter mages (gap close)
                (CharacterType.Warrior, CharacterType.Support) => 4f,   // Warriors excel vs supports
                (CharacterType.Warrior, CharacterType.Tank) => -2f,     // Tanks counter warriors
                
                // Mage matchups  
                (CharacterType.Mage, CharacterType.Warrior) => -3f,     // Mages struggle vs warriors
                (CharacterType.Mage, CharacterType.Support) => 2f,      // Mages outrange supports
                (CharacterType.Mage, CharacterType.Tank) => 1f,         // Mages can kite tanks
                
                // Support matchups
                (CharacterType.Support, CharacterType.Warrior) => -4f,  // Supports vulnerable to warriors
                (CharacterType.Support, CharacterType.Mage) => -2f,     // Supports outranged by mages
                (CharacterType.Support, CharacterType.Tank) => 3f,      // Supports excel vs tanks (utility)
                
                // Tank matchups
                (CharacterType.Tank, CharacterType.Warrior) => 2f,      // Tanks counter warriors
                (CharacterType.Tank, CharacterType.Mage) => -1f,        // Tanks struggle vs mages
                (CharacterType.Tank, CharacterType.Support) => -3f,     // Tanks countered by supports
                
                _ => 0f // Default neutral matchup
            };
        }
        
        private float CalculateMatchupAdjustment(CharacterType attacker, Dictionary<CharacterType, float> opponentPopularities)
        {
            float totalAdjustment = 0f;
            float totalWeight = 0f;
            
            foreach (CharacterType defender in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (attacker == defender) continue;
                
                // Get base matchup modifier
                float baseMatchup = matchupMatrix[attacker][defender];
                
                // Weight by opponent popularity (more popular = more encounters)
                float defenderPopularity = GetCharacterPopularity(defender);
                float weight = Mathf.Max(0.1f, defenderPopularity / 100f); // Normalize popularity to weight
                
                // Apply stat-based matchup modifiers
                float statModifier = CalculateStatBasedMatchup(attacker, defender);
                
                float finalMatchup = baseMatchup + statModifier;
                totalAdjustment += finalMatchup * weight;
                totalWeight += weight;
            }
            
            float averageMatchup = totalWeight > 0f ? totalAdjustment / totalWeight : 0f;
            
            // Clamp to maximum modifier
            return Mathf.Clamp(averageMatchup, -settings.maxMatchupModifier, settings.maxMatchupModifier);
        }
        
        private float CalculateStatBasedMatchup(CharacterType attacker, CharacterType defender)
        {
            if (characterManager == null) return 0f;
            
            // Get current stats for both characters
            float attackerHealth = characterManager.GetStat(attacker, CharacterStat.Health);
            float attackerDamage = characterManager.GetStat(attacker, CharacterStat.Damage);
            float attackerSpeed = characterManager.GetStat(attacker, CharacterStat.Speed);
            float attackerUtility = characterManager.GetStat(attacker, CharacterStat.Utility);
            
            float defenderHealth = characterManager.GetStat(defender, CharacterStat.Health);
            float defenderDamage = characterManager.GetStat(defender, CharacterStat.Damage);
            float defenderSpeed = characterManager.GetStat(defender, CharacterStat.Speed);
            float defenderUtility = characterManager.GetStat(defender, CharacterStat.Utility);
            
            // Calculate stat-based advantages
            float healthAdvantage = (attackerHealth - defenderHealth) * 0.02f;
            float damageAdvantage = (attackerDamage - defenderDamage) * 0.03f;
            float speedAdvantage = (attackerSpeed - defenderSpeed) * 0.025f;
            float utilityAdvantage = (attackerUtility - defenderUtility) * 0.02f;
            
            // Apply archetype-specific stat priorities
            float totalAdvantage = CalculateArchetypeSpecificAdvantage(
                attacker, defender,
                healthAdvantage, damageAdvantage, speedAdvantage, utilityAdvantage
            );
            
            // Clamp to reasonable range
            return Mathf.Clamp(totalAdvantage, -3f, 3f);
        }
        
        private float CalculateArchetypeSpecificAdvantage(CharacterType attacker, CharacterType defender,
            float healthAdv, float damageAdv, float speedAdv, float utilityAdv)
        {
            // Different character types prioritize different stat advantages
            return attacker switch
            {
                CharacterType.Warrior => (healthAdv * 0.4f) + (damageAdv * 0.4f) + (speedAdv * 0.2f),
                CharacterType.Mage => (damageAdv * 0.5f) + (speedAdv * 0.3f) + (utilityAdv * 0.2f),
                CharacterType.Support => (utilityAdv * 0.5f) + (speedAdv * 0.3f) + (healthAdv * 0.2f),
                CharacterType.Tank => (healthAdv * 0.5f) + (utilityAdv * 0.3f) + (damageAdv * 0.2f),
                _ => (healthAdv + damageAdv + speedAdv + utilityAdv) * 0.25f
            };
        }
        
        private float GetCharacterPopularity(CharacterType character)
        {
            if (characterManager == null) return 50f;
            return characterManager.GetStat(character, CharacterStat.Popularity);
        }
        
        /// <summary>
        /// Get specific matchup rating between two characters
        /// </summary>
        public float GetMatchupRating(CharacterType attacker, CharacterType defender)
        {
            if (matchupMatrix == null || !matchupMatrix.ContainsKey(attacker))
                return 0f;
                
            return matchupMatrix[attacker].GetValueOrDefault(defender, 0f);
        }
        
        /// <summary>
        /// Update matchup matrix based on current character stats
        /// </summary>
        public void UpdateMatchupMatrix()
        {
            if (matchupMatrix == null) return;
            
            foreach (CharacterType attacker in System.Enum.GetValues(typeof(CharacterType)))
            {
                foreach (CharacterType defender in System.Enum.GetValues(typeof(CharacterType)))
                {
                    if (attacker == defender) continue;
                    
                    float baseMatchup = CalculateBaseMatchup(attacker, defender);
                    float statModifier = CalculateStatBasedMatchup(attacker, defender);
                    
                    matchupMatrix[attacker][defender] = baseMatchup + (statModifier * 0.5f);
                }
            }
        }
        
        /// <summary>
        /// Get all matchups for a specific character
        /// </summary>
        public Dictionary<CharacterType, float> GetCharacterMatchups(CharacterType character)
        {
            if (matchupMatrix == null || !matchupMatrix.ContainsKey(character))
                return new Dictionary<CharacterType, float>();
                
            return new Dictionary<CharacterType, float>(matchupMatrix[character]);
        }
        
        /// <summary>
        /// Calculate overall matchup advantage/disadvantage for a character
        /// </summary>
        public float CalculateOverallMatchupAdvantage(CharacterType character)
        {
            if (matchupMatrix == null || !matchupMatrix.ContainsKey(character))
                return 0f;
            
            float totalAdvantage = 0f;
            int matchupCount = 0;
            
            foreach (var matchup in matchupMatrix[character])
            {
                if (matchup.Key != character)
                {
                    totalAdvantage += matchup.Value;
                    matchupCount++;
                }
            }
            
            return matchupCount > 0 ? totalAdvantage / matchupCount : 0f;
        }
        
        // Debug methods
        [ContextMenu("📊 Debug: Show All Matchups")]
        public void DebugShowAllMatchups()
        {
            if (matchupMatrix == null)
            {
                Debug.Log("Matchup matrix not initialized");
                return;
            }
            
            Debug.Log("=== 📊 MATCHUP MATRIX ===");
            
            foreach (CharacterType attacker in System.Enum.GetValues(typeof(CharacterType)))
            {
                Debug.Log($"\n{attacker} vs:");
                foreach (CharacterType defender in System.Enum.GetValues(typeof(CharacterType)))
                {
                    if (attacker != defender)
                    {
                        float matchup = matchupMatrix[attacker][defender];
                        string advantage = matchup > 0 ? "ADVANTAGE" : matchup < 0 ? "DISADVANTAGE" : "NEUTRAL";
                        Debug.Log($"  {defender}: {matchup:+0.0;-0.0} ({advantage})");
                    }
                }
            }
        }
        
        [ContextMenu("🔄 Debug: Update Matchup Matrix")]
        public void DebugUpdateMatchupMatrix()
        {
            UpdateMatchupMatrix();
            Debug.Log("Matchup matrix updated based on current character stats");
        }
        
        [ContextMenu("⚖️ Debug: Show Matchup Advantages")]
        public void DebugShowMatchupAdvantages()
        {
            Debug.Log("=== ⚖️ OVERALL MATCHUP ADVANTAGES ===");
            
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float advantage = CalculateOverallMatchupAdvantage(character);
                string rating = advantage switch
                {
                    > 2f => "STRONG",
                    > 0.5f => "GOOD",
                    > -0.5f => "BALANCED",
                    > -2f => "WEAK",
                    _ => "VERY WEAK"
                };
                
                Debug.Log($"{character}: {advantage:+0.0;-0.0} ({rating})");
            }
        }
    }
}