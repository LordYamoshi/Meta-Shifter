using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Enhanced CharacterManager with realistic win rate calculation system
    /// Preserves existing CharacterData structure
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }
        
        [Header("Character Stats")]
        [SerializeField] private List<CharacterData> characterDatabase;
        
        [Header("Balance Configuration")]
        [SerializeField] private BalanceCalculationSettings balanceSettings;
        
        [Header("Events")]
        public UnityEvent<CharacterType, CharacterStat, float> OnStatChanged;
        public UnityEvent<float> OnOverallBalanceChanged;
        
        // Runtime character data
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
        
        // Meta calculation components
        private MetaCalculator metaCalculator;
        private MatchupMatrix matchupMatrix;
        private PopularityInfluence popularityInfluence;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Initialize calculation components
            InitializeCalculationSystems();
        }
        
        private void Start()
        {
            InitializeCharacters();
        }
        
        private void InitializeCalculationSystems()
        {
            // Create balance settings if missing
            if (balanceSettings == null)
            {
                balanceSettings = ScriptableObject.CreateInstance<BalanceCalculationSettings>();
                balanceSettings.InitializeDefaults();
            }
            
            // Initialize meta calculation components
            metaCalculator = gameObject.AddComponent<MetaCalculator>();
            matchupMatrix = gameObject.AddComponent<MatchupMatrix>();
            popularityInfluence = gameObject.AddComponent<PopularityInfluence>();
            
            // Setup references
            metaCalculator.Initialize(this, balanceSettings);
            matchupMatrix.Initialize(this, balanceSettings);
            popularityInfluence.Initialize(this, balanceSettings);
        }
        
        private void InitializeCharacters()
        {
            // Initialize all characters with base stats
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterStats[type] = new Dictionary<CharacterStat, float>();
                
                // Get base stats from database or use defaults
                var characterData = GetCharacterDataFromDatabase(type);
                
                characterStats[type][CharacterStat.Health] = characterData?.baseHealth ?? GetDefaultBaseStat(type, CharacterStat.Health);
                characterStats[type][CharacterStat.Damage] = characterData?.baseDamage ?? GetDefaultBaseStat(type, CharacterStat.Damage);
                characterStats[type][CharacterStat.Speed] = characterData?.baseSpeed ?? GetDefaultBaseStat(type, CharacterStat.Speed);
                characterStats[type][CharacterStat.Utility] = characterData?.baseUtility ?? GetDefaultBaseStat(type, CharacterStat.Utility);
                
                // Initialize win rate and popularity with realistic starting values
                characterStats[type][CharacterStat.WinRate] = 50f + Random.Range(-3f, 3f);
                characterStats[type][CharacterStat.Popularity] = GetInitialPopularity(type);
            }
            
            // Calculate initial win rates based on stats
            RecalculateWinRates();
            
            Debug.Log("Characters initialized with enhanced calculation system");
        }
        
        private CharacterData GetCharacterDataFromDatabase(CharacterType type)
        {
            if (characterDatabase == null || characterDatabase.Count == 0) return null;
            
            foreach (var data in characterDatabase)
            {
                if (data != null && data.characterType == type)
                    return data;
            }
            return null;
        }
        
        private float GetDefaultBaseStat(CharacterType type, CharacterStat stat)
        {
            // Character archetypes with distinct stat distributions
            return type switch
            {
                CharacterType.Warrior => stat switch
                {
                    CharacterStat.Health => 60f,
                    CharacterStat.Damage => 55f,
                    CharacterStat.Speed => 45f,
                    CharacterStat.Utility => 40f,
                    _ => 50f
                },
                CharacterType.Mage => stat switch
                {
                    CharacterStat.Health => 35f,
                    CharacterStat.Damage => 70f,
                    CharacterStat.Speed => 50f,
                    CharacterStat.Utility => 45f,
                    _ => 50f
                },
                CharacterType.Support => stat switch
                {
                    CharacterStat.Health => 40f,
                    CharacterStat.Damage => 30f,
                    CharacterStat.Speed => 55f,
                    CharacterStat.Utility => 75f,
                    _ => 50f
                },
                CharacterType.Tank => stat switch
                {
                    CharacterStat.Health => 80f,
                    CharacterStat.Damage => 40f,
                    CharacterStat.Speed => 25f,
                    CharacterStat.Utility => 55f,
                    _ => 50f
                },
                _ => 50f
            };
        }
        
        private float GetInitialPopularity(CharacterType type)
        {
            // Different characters start with different popularity based on archetype appeal
            return type switch
            {
                CharacterType.Warrior => Random.Range(45f, 65f), // Generally popular
                CharacterType.Mage => Random.Range(35f, 55f),    // Skill-dependent popularity
                CharacterType.Support => Random.Range(25f, 45f), // Often less popular
                CharacterType.Tank => Random.Range(30f, 50f),    // Moderate popularity
                _ => Random.Range(40f, 60f)
            };
        }
        
        public float GetStat(CharacterType character, CharacterStat stat)
        {
            if (characterStats.ContainsKey(character) && characterStats[character].ContainsKey(stat))
            {
                return characterStats[character][stat];
            }
            return 50f; // Default value
        }
        
        public void ModifyStat(CharacterType character, CharacterStat stat, float percentageChange)
        {
            if (!characterStats.ContainsKey(character))
                return;
            
            float currentValue = GetStat(character, stat);
            float change = currentValue * (percentageChange / 100f);
            float newValue = currentValue + change;
            
            // Clamp values to appropriate ranges
            newValue = stat switch
            {
                CharacterStat.WinRate => Mathf.Clamp(newValue, 15f, 85f),
                CharacterStat.Popularity => Mathf.Clamp(newValue, 0f, 100f),
                CharacterStat.Health => Mathf.Clamp(newValue, 10f, 150f),
                CharacterStat.Damage => Mathf.Clamp(newValue, 10f, 150f),
                CharacterStat.Speed => Mathf.Clamp(newValue, 10f, 150f),
                CharacterStat.Utility => Mathf.Clamp(newValue, 10f, 150f),
                _ => Mathf.Max(10f, newValue)
            };
            
            characterStats[character][stat] = newValue;
            
            OnStatChanged.Invoke(character, stat, newValue);
            
            Debug.Log($"{character} {stat}: {currentValue:F1} -> {newValue:F1} ({percentageChange:+0.0;-0.0}%)");
            
            // Recalculate win rates if core stats changed
            if (stat != CharacterStat.WinRate && stat != CharacterStat.Popularity)
            {
                RecalculateWinRates();
            }
            else if (stat == CharacterStat.Popularity)
            {
                // Popularity changes affect win rates through meta influence
                RecalculateWinRatesFromPopularity();
            }
        }
        
        public void RecalculateWinRates()
        {
            Debug.Log("🔄 Starting realistic win rate recalculation based on stats...");
            
            // Step 1: Calculate raw power from stats
            var rawPowerLevels = CalculateRawPowerFromStats();
            
            // Step 2: Convert power to win rates with realistic scaling
            var baseWinRates = ConvertPowerToWinRates(rawPowerLevels);
            
            // Step 3: Apply matchup adjustments (small influence)
            var matchupAdjustedRates = ApplyMatchupAdjustments(baseWinRates);
            
            // Step 4: Apply popularity effects (meta adaptation)
            var finalWinRates = ApplyPopularityMetaEffects(matchupAdjustedRates);
            
            // Step 5: Update win rates and recalculate popularity
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float newWinRate = finalWinRates[type];
                
                // Smooth transition to avoid jarring changes
                float currentWinRate = GetStat(type, CharacterStat.WinRate);
                float smoothedWinRate = Mathf.Lerp(currentWinRate, newWinRate, 0.3f);
                
                // Add small random variance for realism
                smoothedWinRate += Random.Range(-1f, 1f);
                smoothedWinRate = Mathf.Clamp(smoothedWinRate, 25f, 75f);
                
                characterStats[type][CharacterStat.WinRate] = smoothedWinRate;
                OnStatChanged.Invoke(type, CharacterStat.WinRate, smoothedWinRate);
                
                Debug.Log($"📊 {type}: Stats→Power→WinRate {smoothedWinRate:F1}%");
            }
            
            // Step 6: Update popularity based on win rates and character appeal
            UpdatePopularityBasedOnWinRates();
            
            // Step 7: Calculate community satisfaction based on balance state
            float satisfaction = CalculateCommunitySatisfaction();
            OnOverallBalanceChanged.Invoke(satisfaction);
            
            Debug.Log($"✅ Win rate recalculation complete. Community Satisfaction: {satisfaction:F1}%");
        }
        
        private Dictionary<CharacterType, float> CalculateRawPowerFromStats()
        {
            var powerLevels = new Dictionary<CharacterType, float>();
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float health = GetStat(type, CharacterStat.Health);
                float damage = GetStat(type, CharacterStat.Damage);
                float speed = GetStat(type, CharacterStat.Speed);
                float utility = GetStat(type, CharacterStat.Utility);
                
                // Calculate power based on character archetype
                float power = CalculateArchetypePower(type, health, damage, speed, utility);
                powerLevels[type] = power;
                
                Debug.Log($"💪 {type} Power: H{health:F0} D{damage:F0} S{speed:F0} U{utility:F0} = {power:F1}");
            }
            
            return powerLevels;
        }
        
        private float CalculateArchetypePower(CharacterType type, float health, float damage, float speed, float utility)
        {
            return type switch
            {
                // Warriors: Balanced fighters, need health + damage
                CharacterType.Warrior => (health * 0.35f) + (damage * 0.35f) + (speed * 0.15f) + (utility * 0.15f),
                
                // Mages: Glass cannons, damage is key but need some survivability
                CharacterType.Mage => (damage * 0.5f) + (utility * 0.25f) + (speed * 0.15f) + (health * 0.1f),
                
                // Supports: Utility focused, need speed to position and some survivability
                CharacterType.Support => (utility * 0.5f) + (speed * 0.3f) + (health * 0.15f) + (damage * 0.05f),
                
                // Tanks: Health is key, utility for crowd control, damage least important
                CharacterType.Tank => (health * 0.5f) + (utility * 0.3f) + (damage * 0.15f) + (speed * 0.05f),
                
                _ => (health + damage + speed + utility) * 0.25f // Fallback: equal weights
            };
        }
        
        private Dictionary<CharacterType, float> ConvertPowerToWinRates(Dictionary<CharacterType, float> powerLevels)
        {
            var winRates = new Dictionary<CharacterType, float>();
            
            // Find average power to normalize around 50% win rate
            float totalPower = 0f;
            foreach (var power in powerLevels.Values)
                totalPower += power;
            float avgPower = totalPower / powerLevels.Count;
            
            foreach (var kvp in powerLevels)
            {
                CharacterType type = kvp.Key;
                float power = kvp.Value;
                
                // Convert power difference to win rate
                // Higher power = higher win rate, but not linearly
                float powerRatio = power / avgPower;
                
                // Base win rate around 50%, with power affecting it
                float winRate = 50f + ((powerRatio - 1f) * 25f); // ±25% max swing
                
                // Add archetype modifiers (some characters are naturally harder/easier)
                winRate += GetArchetypeWinRateModifier(type);
                
                // Ensure reasonable bounds
                winRate = Mathf.Clamp(winRate, 30f, 70f);
                
                winRates[type] = winRate;
            }
            
            return winRates;
        }
        
        private float GetArchetypeWinRateModifier(CharacterType type)
        {
            return type switch
            {
                CharacterType.Warrior => 2f,     // Easier to play effectively
                CharacterType.Mage => -3f,       // High skill ceiling, harder to master
                CharacterType.Support => -4f,    // Team dependent, lower individual win rate
                CharacterType.Tank => 1f,        // Somewhat easier, more forgiving
                _ => 0f
            };
        }
        
        private Dictionary<CharacterType, float> ApplyMatchupAdjustments(Dictionary<CharacterType, float> baseWinRates)
        {
            var adjustedRates = new Dictionary<CharacterType, float>();
            
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float baseRate = baseWinRates[character];
                float matchupBonus = CalculateMatchupBonus(character);
                
                adjustedRates[character] = baseRate + matchupBonus;
            }
            
            return adjustedRates;
        }
        
        private float CalculateMatchupBonus(CharacterType character)
        {
            // Simple matchup system - characters that counter popular ones get bonuses
            float bonus = 0f;
            
            foreach (CharacterType opponent in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (character == opponent) continue;
                
                float opponentPopularity = GetStat(opponent, CharacterStat.Popularity);
                float matchupAdvantage = GetBasicMatchup(character, opponent);
                
                // Weight matchup by opponent's popularity
                if (opponentPopularity > 60f)
                {
                    bonus += matchupAdvantage * (opponentPopularity - 60f) / 40f * 0.02f;
                }
            }
            
            return Mathf.Clamp(bonus, -2f, 2f);
        }
        
        private float GetBasicMatchup(CharacterType attacker, CharacterType defender)
        {
            return (attacker, defender) switch
            {
                (CharacterType.Warrior, CharacterType.Mage) => 3f,      // Warriors beat mages
                (CharacterType.Warrior, CharacterType.Support) => 4f,   // Warriors beat supports
                (CharacterType.Warrior, CharacterType.Tank) => -2f,     // Tanks beat warriors
                (CharacterType.Mage, CharacterType.Tank) => 2f,         // Mages beat tanks
                (CharacterType.Mage, CharacterType.Support) => 1f,      // Mages slightly beat supports
                (CharacterType.Support, CharacterType.Tank) => 3f,      // Supports beat tanks
                _ => -GetBasicMatchup(defender, attacker) // Reverse matchup
            };
        }
        
        private Dictionary<CharacterType, float> ApplyPopularityMetaEffects(Dictionary<CharacterType, float> baseWinRates)
        {
            var adjustedRates = new Dictionary<CharacterType, float>();
            
            foreach (CharacterType character in System.Enum.GetValues(typeof(CharacterType)))
            {
                float baseRate = baseWinRates[character];
                float popularity = GetStat(character, CharacterStat.Popularity);
                
                // Popular characters get slight penalties (people adapt to counter them)
                float popularityPenalty = 0f;
                if (popularity > 70f)
                {
                    popularityPenalty = (popularity - 70f) / 30f * -3f; // Up to -3% win rate
                }
                else if (popularity < 30f)
                {
                    popularityPenalty = (30f - popularity) / 30f * 2f; // Up to +2% win rate
                }
                
                adjustedRates[character] = baseRate + popularityPenalty;
            }
            
            return adjustedRates;
        }
        
        private void UpdatePopularityBasedOnWinRates()
        {
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float currentPopularity = GetStat(type, CharacterStat.Popularity);
                float winRate = GetStat(type, CharacterStat.WinRate);
                
                // Calculate popularity changes
                float winRateEffect = (winRate - 50f) * 0.3f; // Good win rate = more popular
                float archetypeAppeal = GetArchetypeAppeal(type);
                float randomVariance = Random.Range(-1f, 1f);
                
                float popularityChange = (winRateEffect + archetypeAppeal + randomVariance) * 0.2f;
                float newPopularity = Mathf.Clamp(currentPopularity + popularityChange, 5f, 95f);
                
                characterStats[type][CharacterStat.Popularity] = newPopularity;
                OnStatChanged.Invoke(type, CharacterStat.Popularity, newPopularity);
                
                if (Mathf.Abs(popularityChange) > 0.5f)
                {
                    Debug.Log($"📈 {type} popularity: {currentPopularity:F1}% → {newPopularity:F1}% (WR: {winRate:F1}%)");
                }
            }
        }
        
        private float GetArchetypeAppeal(CharacterType type)
        {
            return type switch
            {
                CharacterType.Warrior => 1f,      // Generally popular - straightforward
                CharacterType.Mage => 0.5f,       // Moderate appeal - skill-based
                CharacterType.Support => -1.5f,   // Less popular - team dependent
                CharacterType.Tank => -0.5f,      // Slightly less popular - slower gameplay
                _ => 0f
            };
        }
        
        private float CalculateCommunitySatisfaction()
        {
            // Community satisfaction based on how balanced the game feels
            float satisfaction = 100f;
            
            // Check for overpowered characters (win rate too high)
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float winRate = GetStat(type, CharacterStat.WinRate);
                float popularity = GetStat(type, CharacterStat.Popularity);
                
                // High win rate characters cause frustration
                if (winRate > 55f)
                {
                    float penalty = (winRate - 55f) * 2f; // -2 satisfaction per % over 55%
                    satisfaction -= penalty;
                    Debug.Log($"😤 {type} too strong ({winRate:F1}%): -{penalty:F1} satisfaction");
                }
                
                // Very low win rate characters also cause frustration
                if (winRate < 45f)
                {
                    float penalty = (45f - winRate) * 1.5f; // -1.5 satisfaction per % under 45%
                    satisfaction -= penalty;
                    Debug.Log($"😞 {type} too weak ({winRate:F1}%): -{penalty:F1} satisfaction");
                }
                
                // Overly dominant popularity is bad for meta health
                if (popularity > 80f)
                {
                    float penalty = (popularity - 80f) * 0.5f;
                    satisfaction -= penalty;
                    Debug.Log($"🎯 {type} too popular ({popularity:F1}%): -{penalty:F1} satisfaction");
                }
            }
            
            // Check overall meta diversity
            float diversityScore = CalculateMetaDiversity();
            if (diversityScore < 70f)
            {
                float penalty = (70f - diversityScore) * 0.3f;
                satisfaction -= penalty;
                Debug.Log($"📊 Poor meta diversity ({diversityScore:F1}%): -{penalty:F1} satisfaction");
            }
            
            // Bonus for balanced meta
            if (AllCharactersInRange(45f, 55f))
            {
                satisfaction += 10f;
                Debug.Log($"✨ Perfect balance bonus: +10 satisfaction");
            }
            
            return Mathf.Clamp(satisfaction, 20f, 100f);
        }
        
        private float CalculateMetaDiversity()
        {
            // How evenly distributed is character usage?
            float totalPopularity = 0f;
            float minPop = float.MaxValue;
            float maxPop = float.MinValue;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float popularity = GetStat(type, CharacterStat.Popularity);
                totalPopularity += popularity;
                minPop = Mathf.Min(minPop, popularity);
                maxPop = Mathf.Max(maxPop, popularity);
            }
            
            float popularityRange = maxPop - minPop;
            float diversityScore = Mathf.Max(0f, 100f - (popularityRange - 30f)); // Ideal range is 30%
            
            return diversityScore;
        }
        
        private bool AllCharactersInRange(float minWinRate, float maxWinRate)
        {
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float winRate = GetStat(type, CharacterStat.WinRate);
                if (winRate < minWinRate || winRate > maxWinRate)
                    return false;
            }
            return true;
        }
        
        private void RecalculateWinRatesFromPopularity()
        {
            // Lighter recalculation when only popularity changes
            var currentWinRates = new Dictionary<CharacterType, float>();
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                currentWinRates[type] = GetStat(type, CharacterStat.WinRate);
            }
            
            var adjustedRates = popularityInfluence.ApplyPopularityEffects(currentWinRates, characterStats);
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float newWinRate = adjustedRates[type];
                characterStats[type][CharacterStat.WinRate] = newWinRate;
                OnStatChanged.Invoke(type, CharacterStat.WinRate, newWinRate);
            }
        }
        
        public float CalculateOverallBalance()
        {
            // This is now community satisfaction, not just balance score
            return CalculateCommunitySatisfaction();
        }
        
        private float CalculateWinRateVariance()
        {
            float totalDeviation = 0f;
            int characterCount = 0;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float winRate = GetStat(type, CharacterStat.WinRate);
                float deviation = Mathf.Abs(winRate - 50f);
                totalDeviation += deviation;
                characterCount++;
            }
            
            if (characterCount == 0) return 50f;
            
            float avgDeviation = totalDeviation / characterCount;
            return Mathf.Max(0f, 100f - (avgDeviation * 3f)); // More sensitive to deviations
        }
        
        private float CalculatePopularityDistribution()
        {
            // Check how evenly popularity is distributed
            float totalPopularity = 0f;
            float minPop = float.MaxValue;
            float maxPop = float.MinValue;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float popularity = GetStat(type, CharacterStat.Popularity);
                totalPopularity += popularity;
                minPop = Mathf.Min(minPop, popularity);
                maxPop = Mathf.Max(maxPop, popularity);
            }
            
            float popularityRange = maxPop - minPop;
            return Mathf.Max(0f, 100f - (popularityRange * 1.5f)); // Penalize large popularity gaps
        }
        
        // Public getters for other systems
        public Dictionary<CharacterType, float> GetAllWinRates()
        {
            Dictionary<CharacterType, float> winRates = new Dictionary<CharacterType, float>();
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                winRates[type] = GetStat(type, CharacterStat.WinRate);
            }
            return winRates;
        }
        
        public Dictionary<CharacterType, Dictionary<CharacterStat, float>> GetAllStats()
        {
            return new Dictionary<CharacterType, Dictionary<CharacterStat, float>>(characterStats);
        }
        
        // Debug and testing methods
        [ContextMenu("🔄 Force Recalculate Win Rates")]
        public void DebugRecalculateWinRates()
        {
            RecalculateWinRates();
        }
        
        [ContextMenu("📊 Debug: Show All Character Stats")]
        public void DebugShowAllStats()
        {
            Debug.Log("=== 📊 ALL CHARACTER STATS ===");
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                Debug.Log($"{type}:");
                Debug.Log($"  Health: {GetStat(type, CharacterStat.Health):F1}");
                Debug.Log($"  Damage: {GetStat(type, CharacterStat.Damage):F1}");
                Debug.Log($"  Speed: {GetStat(type, CharacterStat.Speed):F1}");
                Debug.Log($"  Utility: {GetStat(type, CharacterStat.Utility):F1}");
                Debug.Log($"  Win Rate: {GetStat(type, CharacterStat.WinRate):F1}%");
                Debug.Log($"  Popularity: {GetStat(type, CharacterStat.Popularity):F1}%");
            }
        }
        
        [ContextMenu("🎲 Debug: Randomize One Character")]
        public void DebugRandomizeCharacter()
        {
            var types = System.Enum.GetValues(typeof(CharacterType));
            CharacterType randomType = (CharacterType)types.GetValue(Random.Range(0, types.Length));
            
            // Apply random changes
            ModifyStat(randomType, CharacterStat.Health, Random.Range(-20f, 20f));
            ModifyStat(randomType, CharacterStat.Damage, Random.Range(-20f, 20f));
            
            Debug.Log($"🎲 Randomized {randomType} stats");
        }
        
        [ContextMenu("⚖️ Debug: Show Balance Analysis")]
        public void DebugShowBalanceAnalysis()
        {
            float winRateVar = CalculateWinRateVariance();
            float popDist = CalculatePopularityDistribution();
            float metaHealth = metaCalculator.CalculateMetaHealth(characterStats);
            float overall = CalculateOverallBalance();
            
            Debug.Log("=== ⚖️ BALANCE ANALYSIS ===");
            Debug.Log($"Win Rate Variance Score: {winRateVar:F1}/100");
            Debug.Log($"Popularity Distribution: {popDist:F1}/100");
            Debug.Log($"Meta Health: {metaHealth:F1}/100");
            Debug.Log($"Overall Balance: {overall:F1}/100");
        }
    }
    
    /// <summary>
    /// ScriptableObject for character base data - EXACT copy from your existing code
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Meta Balance/Character Data")]
    public class CharacterData : ScriptableObject
    {
        public CharacterType characterType;
        public string characterName;
        public string description;
        
        [Header("Base Stats")]
        public float baseHealth = 50f;
        public float baseDamage = 50f;
        public float baseSpeed = 50f;
        public float baseUtility = 50f;
    }
}