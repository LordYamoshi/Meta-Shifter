using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Manages character stats and win rates
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }
        
        [Header("Character Stats")]
        [SerializeField] private List<CharacterData> characterDatabase;
        
        [Header("Events")]
        public UnityEvent<CharacterType, CharacterStat, float> OnStatChanged; // Character, Stat, NewValue
        public UnityEvent<float> OnOverallBalanceChanged; // Overall balance score
        
        // Runtime character data
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> characterStats = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            InitializeCharacters();
        }
        
        private void InitializeCharacters()
        {
            // Initialize all characters with base stats
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterStats[type] = new Dictionary<CharacterStat, float>();
                
                // Set base stats
                characterStats[type][CharacterStat.Health] = 50f;
                characterStats[type][CharacterStat.Damage] = 50f;
                characterStats[type][CharacterStat.Speed] = 50f;
                characterStats[type][CharacterStat.Utility] = 50f;
                characterStats[type][CharacterStat.WinRate] = 50f + Random.Range(-5f, 5f);
                characterStats[type][CharacterStat.Popularity] = 50f + Random.Range(-10f, 10f);
            }
            
            Debug.Log("Characters initialized with base stats");
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
            
            // Clamp values to reasonable ranges
            newValue = stat switch
            {
                CharacterStat.WinRate => Mathf.Clamp(newValue, 20f, 80f),
                CharacterStat.Popularity => Mathf.Clamp(newValue, 0f, 100f),
                _ => Mathf.Max(10f, newValue) // Other stats minimum 10
            };
            
            characterStats[character][stat] = newValue;
            
            OnStatChanged.Invoke(character, stat, newValue);
            
            Debug.Log($"{character} {stat}: {currentValue:F1} -> {newValue:F1} ({percentageChange:+0.0;-0.0}%)");
            
            // Recalculate win rates if other stats changed
            if (stat != CharacterStat.WinRate && stat != CharacterStat.Popularity)
            {
                RecalculateWinRates();
            }
        }
        
        public void RecalculateWinRates()
        {
            // Simple win rate calculation based on relative power
            Dictionary<CharacterType, float> powerLevels = new Dictionary<CharacterType, float>();
            
            // Calculate power level for each character
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float health = GetStat(type, CharacterStat.Health);
                float damage = GetStat(type, CharacterStat.Damage);
                float speed = GetStat(type, CharacterStat.Speed);
                float utility = GetStat(type, CharacterStat.Utility);
                
                // Weighted power calculation
                float power = (health * 0.3f) + (damage * 0.4f) + (speed * 0.15f) + (utility * 0.15f);
                powerLevels[type] = power;
            }
            
            // Calculate average power
            float totalPower = 0f;
            foreach (var power in powerLevels.Values)
                totalPower += power;
            float avgPower = totalPower / powerLevels.Count;
            
            // Adjust win rates based on power relative to average
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                float relativePower = powerLevels[type] / avgPower;
                float targetWinRate = 50f * relativePower;
                
                // Smooth transition to new win rate
                float currentWinRate = GetStat(type, CharacterStat.WinRate);
                float newWinRate = Mathf.Lerp(currentWinRate, targetWinRate, 0.3f);
                
                // Add some randomness
                newWinRate += Random.Range(-2f, 2f);
                newWinRate = Mathf.Clamp(newWinRate, 25f, 75f);
                
                characterStats[type][CharacterStat.WinRate] = newWinRate;
                OnStatChanged.Invoke(type, CharacterStat.WinRate, newWinRate);
            }
            
            // Calculate overall balance score
            float balanceScore = CalculateOverallBalance();
            OnOverallBalanceChanged.Invoke(balanceScore);
            
            Debug.Log($"Win rates recalculated. Balance score: {balanceScore:F1}");
        }
        
        public float CalculateOverallBalance()
        {
            // Calculate how balanced all characters are (closer win rates = better balance)
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
            float balanceScore = Mathf.Max(0f, 100f - (avgDeviation * 2f));
            
            return balanceScore;
        }
        
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
    }
    
    /// <summary>
    /// ScriptableObject for character base data
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