using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Characters
{
    /// <summary>
    /// Runtime representation of a character
    /// </summary>
    public class GameCharacter
    {
        // Reference to data
        public CharacterData Data { get; private set; }
        
        // Current stats
        private Dictionary<CharacterStat, float> _currentStats = new Dictionary<CharacterStat, float>();
        
        // State
        private ICharacterState _currentState;
        
        // Stat modifiers
        private List<StatModifier> _statModifiers = new List<StatModifier>();
        
        // Visual instance
        public GameObject VisualInstance { get; private set; }
        public CharacterVisual Visual { get; private set; }
        
        public GameCharacter(CharacterData data)
        {
            Data = data;
            InitializeStats();
        }
        
        private void InitializeStats()
        {
            // Set initial stats from base values
            _currentStats[CharacterStat.Health] = Data.baseHealth;
            _currentStats[CharacterStat.Damage] = Data.baseDamage;
            _currentStats[CharacterStat.Speed] = Data.baseSpeed;
            _currentStats[CharacterStat.Utility] = Data.baseUtility;
            _currentStats[CharacterStat.Popularity] = Data.basePopularity;
            _currentStats[CharacterStat.WinRate] = 50f; // Default win rate
        }
        
        public void SpawnVisual(Transform parent, Vector3 position)
        {
            if (Data.characterPrefab != null)
            {
                // Instantiate visual
                VisualInstance = GameObject.Instantiate(Data.characterPrefab, position, Quaternion.identity, parent);
                
                // Get CharacterVisual component
                Visual = VisualInstance.GetComponent<CharacterVisual>();
                
                // Initialize visual
                if (Visual != null)
                {
                    Visual.Initialize(this);
                }
            }
        }
        
        public float GetStat(CharacterStat stat)
        {
            if (_currentStats.TryGetValue(stat, out float value))
            {
                return value;
            }
            
            return 0f;
        }
        
        public void ModifyStat(CharacterStat stat, float percentageChange)
        {
            if (!_currentStats.ContainsKey(stat))
                return;
                
            float currentValue = _currentStats[stat];
            float change = currentValue * (percentageChange / 100f);
            _currentStats[stat] = currentValue + change;
            
            // Apply limits
            ClampStats();
            
            // Add modifier
            _statModifiers.Add(new StatModifier { 
                Stat = stat, 
                PercentageChange = percentageChange,
                TimeApplied = Time.time
            });
            
            // Update visual if available
            if (Visual != null)
            {
                Visual.UpdateVisual();
                
                // Play appropriate sound
                if (percentageChange > 0 && Data.buffSound != null)
                {
                    AudioSource audio = Visual.GetComponent<AudioSource>();
                    if (audio != null)
                    {
                        audio.PlayOneShot(Data.buffSound);
                    }
                }
                else if (percentageChange < 0 && Data.nerfSound != null)
                {
                    AudioSource audio = Visual.GetComponent<AudioSource>();
                    if (audio != null)
                    {
                        audio.PlayOneShot(Data.nerfSound);
                    }
                }
            }
            
            // Update state
            UpdateState();
        }
        
        public void SetStat(CharacterStat stat, float value)
        {
            if (!_currentStats.ContainsKey(stat))
                return;
                
            _currentStats[stat] = value;
            
            // Apply limits
            ClampStats();
            
            // Update visual if available
            if (Visual != null)
            {
                Visual.UpdateVisual();
            }
            
            // Update state
            UpdateState();
        }
        
        private void ClampStats()
        {
            // Ensure stats stay within valid ranges
            _currentStats[CharacterStat.Health] = Mathf.Max(1f, _currentStats[CharacterStat.Health]);
            _currentStats[CharacterStat.Damage] = Mathf.Max(1f, _currentStats[CharacterStat.Damage]);
            _currentStats[CharacterStat.Speed] = Mathf.Max(1f, _currentStats[CharacterStat.Speed]);
            _currentStats[CharacterStat.Utility] = Mathf.Max(1f, _currentStats[CharacterStat.Utility]);
            
            // Win rate and popularity are percentages (0-100)
            _currentStats[CharacterStat.WinRate] = Mathf.Clamp(_currentStats[CharacterStat.WinRate], 0f, 100f);
            _currentStats[CharacterStat.Popularity] = Mathf.Clamp(_currentStats[CharacterStat.Popularity], 0f, 100f);
        }
        
        public void ResetStats()
        {
            // Reset to base values
            InitializeStats();
            
            // Clear modifiers
            _statModifiers.Clear();
            
            // Update visual
            if (Visual != null)
            {
                Visual.UpdateVisual();
            }
            
            // Update state
            UpdateState();
        }
        
        public void Update()
        {
            // Update state
            _currentState?.Update();
        }
        
        private void UpdateState()
        {
            // Determine state based on win rate
            float winRate = GetStat(CharacterStat.WinRate);
            
            ICharacterState newState = null;
            
            if (winRate > 55f)
            {
                newState = new OverpoweredState(this);
            }
            else if (winRate < 45f)
            {
                newState = new UnderpoweredState(this);
            }
            else
            {
                newState = new BalancedState(this);
            }
            
            // Only change state if different
            if (_currentState == null || newState.GetType() != _currentState.GetType())
            {
                // Exit current state
                _currentState?.Exit();
                
                // Set new state
                _currentState = newState;
                
                // Enter new state
                _currentState.Enter();
            }
        }
        
        public CharacterType GetCharacterType()
        {
            return Data.characterType;
        }
        
        public string GetCharacterName()
        {
            return Data.characterName;
        }
        
        public ICharacterState GetCurrentState()
        {
            return _currentState;
        }
    }
}