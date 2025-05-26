using UnityEngine;
using MetaBalance.Characters;
using System.Collections.Generic;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Effect that creates major, game-defining moments (Command Pattern)
    /// </summary>
    public class SpecialEffect : CardEffect
    {
        private SpecialActionType _actionType;
        private int _impactMagnitude;
        private bool _hasExecuted = false;
        
        // Track original state for undo (only works for some effects)
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> _originalStats = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
        
        // Track changes for undo
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> _changes = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
            
        public SpecialEffect(CardData source, SpecialActionType actionType, int impactMagnitude) 
            : base(source)
        {
            _actionType = actionType;
            _impactMagnitude = impactMagnitude;
        }
        
        public override bool Execute()
        {
            // Find character manager
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("Character Manager not found");
                return false;
            }
            
            // Store original state for effects that need it
            if (_actionType == SpecialActionType.CompleteOverhaul)
            {
                StoreOriginalStats(characterManager);
            }
            
            // Clear change tracking
            _changes.Clear();
            
            // Apply effect based on action type
            switch (_actionType)
            {
                case SpecialActionType.CompleteOverhaul:
                    ApplyCompleteOverhaul(characterManager);
                    break;
                    
                case SpecialActionType.NewGameMode:
                    ApplyNewGameMode(characterManager);
                    break;
                    
                case SpecialActionType.ProCircuitAnnouncement:
                    ApplyProCircuitAnnouncement(characterManager);
                    break;
                    
                case SpecialActionType.MajorContentUpdate:
                    ApplyMajorContentUpdate(characterManager);
                    break;
                    
                case SpecialActionType.CrossoverEvent:
                    ApplyCrossoverEvent(characterManager);
                    break;
            }
            
            // Mark as executed
            _hasExecuted = true;
            
            // Log for debugging
            Debug.Log($"Special Action: {_actionType} with magnitude {_impactMagnitude}");
            
            return true;
        }
        
        public override bool Undo()
        {
            // Special cards often can't be fully undone in a real game
            if (!_hasExecuted)
                return false;
                
            // Find character manager
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("Character Manager not found");
                return false;
            }
            
            // Apply undo logic based on action type
            switch (_actionType)
            {
                case SpecialActionType.CompleteOverhaul:
                    // Restore from original stats
                    RestoreOriginalStats(characterManager);
                    break;
                    
                default:
                    // For other types, just apply reverse changes
                    foreach (var characterEntry in _changes)
                    {
                        CharacterType characterType = characterEntry.Key;
                        
                        foreach (var statEntry in characterEntry.Value)
                        {
                            CharacterStat stat = statEntry.Key;
                            float change = statEntry.Value;
                            
                            // Apply reverse change
                            characterManager.ModifyCharacterStat(characterType, stat, -change);
                        }
                    }
                    break;
            }
            
            // Log for debugging
            Debug.Log($"Special Action Undone (partially): {_actionType}");
            
            _hasExecuted = false;
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Implement special action: {_actionType} (Magnitude: {_impactMagnitude})";
        }
        
        // Different special action implementations
        
        private void ApplyCompleteOverhaul(CharacterManager characterManager)
        {
            // Complete overhaul resets all characters and recalculates win rates
            characterManager.ResetAllCharacters();
            
            // Boost popularity for all characters
            float popularityBoost = 15f * (_impactMagnitude / 10f);
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityBoost);
            }
        }
        
        private void ApplyNewGameMode(CharacterManager characterManager)
        {
            // New game mode favors utility characters
            float supportWinRateBoost = 8f * (_impactMagnitude / 10f);
            float supportPopularityBoost = 20f * (_impactMagnitude / 10f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, supportWinRateBoost);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.Popularity, supportPopularityBoost);
            
            // Boost all characters' popularity
            float generalPopularityBoost = 10f * (_impactMagnitude / 10f);
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (type != CharacterType.Support) // Already boosted Support
                {
                    RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, generalPopularityBoost);
                }
            }
        }
        
        private void ApplyProCircuitAnnouncement(CharacterManager characterManager)
        {
            // Pro circuit announcement boosts competitive interest
            float popularityBoost = 12f * (_impactMagnitude / 10f);
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityBoost);
            }
            
            // Slightly favor characters that are better in organized play
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, 3f * (_impactMagnitude / 10f));
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.WinRate, 2f * (_impactMagnitude / 10f));
        }
        
        private void ApplyMajorContentUpdate(CharacterManager characterManager)
        {
            // Major content update affects everything
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                // Random balanced changes to win rates
                float randomChange = Random.Range(-5f, 5f) * (_impactMagnitude / 10f);
                RecordAndApplyChange(characterManager, type, CharacterStat.WinRate, randomChange);
                
                // Boost popularity
                float popularityBoost = 15f * (_impactMagnitude / 10f);
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityBoost);
            }
        }
        
        private void ApplyCrossoverEvent(CharacterManager characterManager)
        {
            // Crossover event dramatically boosts popularity
            float popularityBoost = 25f * (_impactMagnitude / 10f);
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityBoost);
            }
        }
        
        // Helper methods
        
        private void StoreOriginalStats(CharacterManager characterManager)
        {
            _originalStats.Clear();
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameCharacter character = characterManager.GetCharacter(type);
                if (character != null)
                {
                    _originalStats[type] = new Dictionary<CharacterStat, float>();
                    
                    foreach (CharacterStat stat in System.Enum.GetValues(typeof(CharacterStat)))
                    {
                        _originalStats[type][stat] = character.GetStat(stat);
                    }
                }
            }
        }
        
        private void RestoreOriginalStats(CharacterManager characterManager)
        {
            foreach (var characterEntry in _originalStats)
            {
                CharacterType type = characterEntry.Key;
                GameCharacter character = characterManager.GetCharacter(type);
                
                if (character != null)
                {
                    foreach (var statEntry in characterEntry.Value)
                    {
                        CharacterStat stat = statEntry.Key;
                        float originalValue = statEntry.Value;
                        
                        // Get current value
                        float currentValue = character.GetStat(stat);
                        
                        // Calculate change needed to restore
                        float change = ((originalValue - currentValue) / currentValue) * 100f;
                        
                        // Apply change
                        characterManager.ModifyCharacterStat(type, stat, change);
                    }
                }
            }
        }
        
        // Helper to record changes for undo and apply them
        private void RecordAndApplyChange(CharacterManager characterManager, CharacterType type, CharacterStat stat, float change)
        {
            // Record change for undo
            if (!_changes.ContainsKey(type))
            {
                _changes[type] = new Dictionary<CharacterStat, float>();
            }
            
            if (!_changes[type].ContainsKey(stat))
            {
                _changes[type][stat] = change;
            }
            else
            {
                _changes[type][stat] += change;
            }
            
            // Apply change
            characterManager.ModifyCharacterStat(type, stat, change);
        }
    }
}