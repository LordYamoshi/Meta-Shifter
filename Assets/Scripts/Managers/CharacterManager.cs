using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Manages all characters in the game
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        // Singleton instance
        public static CharacterManager Instance { get; private set; }
        
        [Header("Character Database")]
        [SerializeField] private List<CharacterData> characterDatabase = new List<CharacterData>();
        
        [Header("Spawning")]
        [SerializeField] private Transform characterParent;
        [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
        
        [Header("Win Rate Calculation")]
        [SerializeField] private float balanceTargetWinRate = 50f;
        [SerializeField] private float winRateRandomVariation = 2f;
        [SerializeField] private float popularityImpact = 0.25f;
        
        [Header("Events")]
        public UnityEvent<GameCharacter> onCharacterSelected;
        public UnityEvent<GameCharacter> onCharacterStatsChanged;
        public UnityEvent onAllCharactersUpdated;
        
        // Active characters
        private Dictionary<CharacterType, GameCharacter> _characters = new Dictionary<CharacterType, GameCharacter>();
        
        // Currently selected character
        private GameCharacter _selectedCharacter;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void Start()
        {
            // Initialize characters
            InitializeCharacters();
        }
        
        private void Update()
        {
            // Update all characters
            foreach (var character in _characters.Values)
            {
                character.Update();
            }
        }
        
        private void InitializeCharacters()
        {
            // Create characters from database
            int spawnIndex = 0;
            foreach (CharacterData data in characterDatabase)
            {
                // Create character
                GameCharacter character = new GameCharacter(data);
                
                // Store reference
                _characters[data.characterType] = character;
                
                // Spawn visual
                if (spawnPositions.Count > spawnIndex)
                {
                    character.SpawnVisual(characterParent, spawnPositions[spawnIndex].position);
                    spawnIndex++;
                }
                else
                {
                    Debug.LogWarning($"Not enough spawn positions for character {data.characterName}");
                }
            }
            
            // Initial win rate calculation
            RecalculateWinRates();
            
            // Select first character by default
            if (_characters.Count > 0)
            {
                SelectCharacter(characterDatabase[0].characterType);
            }
        }
        
        public void SelectCharacter(CharacterType type)
        {
            if (_characters.TryGetValue(type, out GameCharacter character))
            {
                _selectedCharacter = character;
                onCharacterSelected.Invoke(character);
                
                // Play selection sound if available
                if (character.Visual != null && character.Data.selectionSound != null)
                {
                    AudioSource audio = character.Visual.GetComponent<AudioSource>();
                    if (audio != null)
                    {
                        audio.PlayOneShot(character.Data.selectionSound);
                    }
                }
            }
        }
        
        public GameCharacter GetCharacter(CharacterType type)
        {
            if (_characters.TryGetValue(type, out GameCharacter character))
            {
                return character;
            }
            
            return null;
        }
        
        public GameCharacter GetSelectedCharacter()
        {
            return _selectedCharacter;
        }
        
        public void ModifyCharacterStat(CharacterType type, CharacterStat stat, float percentageChange)
        {
            if (_characters.TryGetValue(type, out GameCharacter character))
            {
                // Apply change
                character.ModifyStat(stat, percentageChange);
                
                // Notify listeners
                onCharacterStatsChanged.Invoke(character);
                
                // Recalculate win rates if needed
                if (stat != CharacterStat.WinRate)
                {
                    RecalculateWinRates();
                }
            }
        }
        
        public void RecalculateWinRates()
        {
            // Calculate relative strength values
            Dictionary<CharacterType, float> strengthValues = new Dictionary<CharacterType, float>();
            
            foreach (var character in _characters.Values)
            {
                // Calculate strength based on stats
                float healthValue = character.GetStat(CharacterStat.Health) * 0.3f;
                float damageValue = character.GetStat(CharacterStat.Damage) * 0.4f;
                float speedValue = character.GetStat(CharacterStat.Speed) * 0.15f;
                float utilityValue = character.GetStat(CharacterStat.Utility) * 0.15f;
                
                float strength = healthValue + damageValue + speedValue + utilityValue;
                strengthValues[character.GetCharacterType()] = strength;
            }
            
            // Calculate total strength
            float totalStrength = 0f;
            foreach (var strength in strengthValues.Values)
            {
                totalStrength += strength;
            }
            
            // Update win rates
            foreach (var character in _characters.Values)
            {
                CharacterType type = character.GetCharacterType();
                
                // Calculate expected win rate
                float relativeStrength = strengthValues[type] / totalStrength;
                float expectedWinRate = relativeStrength * (balanceTargetWinRate * 2f);
                
                // Blend with current win rate to prevent wild swings
                float currentWinRate = character.GetStat(CharacterStat.WinRate);
                float newWinRate = (currentWinRate * 0.7f) + (expectedWinRate * 0.3f);
                
                // Apply random variation
                newWinRate += Random.Range(-winRateRandomVariation, winRateRandomVariation);
                
                // Apply popularity factor - less popular characters often perform better in the hands of specialists
                float popularity = character.GetStat(CharacterStat.Popularity);
                float popularityFactor = (balanceTargetWinRate - popularity) * popularityImpact;
                newWinRate += popularityFactor;
                
                // Set new win rate
                character.SetStat(CharacterStat.WinRate, newWinRate);
                
                // Notify listeners
                onCharacterStatsChanged.Invoke(character);
            }
            
            // Notify that all characters were updated
            onAllCharactersUpdated.Invoke();
        }
        
        public void ResetAllCharacters()
        {
            foreach (var character in _characters.Values)
            {
                character.ResetStats();
                
                // Notify listeners
                onCharacterStatsChanged.Invoke(character);
            }
            
            // Recalculate win rates
            RecalculateWinRates();
        }
        
        /// <summary>
        /// Calculate average player satisfaction based on character balance and popularity
        /// </summary>
        public float CalculatePlayerSatisfaction()
        {
            if (_characters.Count == 0)
                return 0.5f;
                
            float totalSatisfaction = 0f;
            
            foreach (var character in _characters.Values)
            {
                // Balance factor (how close to 50% win rate)
                float winRate = character.GetStat(CharacterStat.WinRate);
                float balance = 1f - (Mathf.Abs(winRate - balanceTargetWinRate) / 50f);
                
                // Popularity factor
                float popularity = character.GetStat(CharacterStat.Popularity) / 100f;
                
                // Higher popularity characters have more impact on satisfaction
                float characterSatisfaction = (balance * 0.7f) + (popularity * 0.3f);
                totalSatisfaction += characterSatisfaction * popularity;
            }
            
            // Normalize by sum of popularities
            float totalPopularity = 0f;
            foreach (var character in _characters.Values)
            {
                totalPopularity += character.GetStat(CharacterStat.Popularity) / 100f;
            }
            
            if (totalPopularity > 0f)
            {
                return totalSatisfaction / totalPopularity;
            }
            
            return 0.5f;
        }
    }
}