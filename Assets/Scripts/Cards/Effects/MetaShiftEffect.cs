using UnityEngine;
using MetaBalance.Characters;
using System.Collections.Generic;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Effect that changes the game environment 
    /// </summary>
    public class MetaShiftEffect : CardEffect
    {
        private MetaShiftType _shiftType;
        private int _shiftPower;
        private bool _hasExecuted = false;
        
        // Track changes for undo
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> _changes = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
        
        public MetaShiftEffect(CardData source, MetaShiftType shiftType, int shiftPower) 
            : base(source)
        {
            _shiftType = shiftType;
            _shiftPower = shiftPower;
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
            
            // Clear previous changes
            _changes.Clear();
            
            // Apply meta shift based on type
            switch (_shiftType)
            {
                case MetaShiftType.MapPool:
                    ApplyMapPoolShift(characterManager);
                    break;
                    
                case MetaShiftType.GameMode:
                    ApplyGameModeShift(characterManager);
                    break;
                    
                case MetaShiftType.ItemBalance:
                    ApplyItemBalanceShift(characterManager);
                    break;
                    
                case MetaShiftType.SeasonsChange:
                    ApplySeasonsChangeShift(characterManager);
                    break;
                    
                case MetaShiftType.TournamentFormat:
                    ApplyTournamentFormatShift(characterManager);
                    break;
            }
            
            // Mark as executed
            _hasExecuted = true;
            
            // Log for debugging
            Debug.Log($"Meta Shift: {_shiftType} applied with power {_shiftPower}");
            
            return true;
        }
        
        public override bool Undo()
        {
            if (!_hasExecuted)
                return false;
                
            // Find character manager
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("Character Manager not found");
                return false;
            }
            
            // Revert all changes
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
            
            // Log for debugging
            Debug.Log($"Meta Shift Undone: {_shiftType}");
            
            _hasExecuted = false;
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Apply meta shift: {_shiftType} (Power: {_shiftPower})";
        }
        
        // Different meta shift implementations
        
        private void ApplyMapPoolShift(CharacterManager characterManager)
        {
            // Map pool favors tanks and utility characters
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.WinRate, _shiftPower * 1.0f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, _shiftPower * 0.8f);
            RecordAndApplyChange(characterManager, CharacterType.Warrior, CharacterStat.WinRate, _shiftPower * -0.5f);
            RecordAndApplyChange(characterManager, CharacterType.Mage, CharacterStat.WinRate, _shiftPower * -0.3f);
        }
        
        private void ApplyGameModeShift(CharacterManager characterManager)
        {
            // Game mode favors damage dealers
            RecordAndApplyChange(characterManager, CharacterType.Mage, CharacterStat.WinRate, _shiftPower * 1.0f);
            RecordAndApplyChange(characterManager, CharacterType.Warrior, CharacterStat.WinRate, _shiftPower * 0.8f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, _shiftPower * -0.6f);
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.WinRate, _shiftPower * -0.4f);
        }
        
        private void ApplyItemBalanceShift(CharacterManager characterManager)
        {
            // Item balance affects all characters
            RecordAndApplyChange(characterManager, CharacterType.Warrior, CharacterStat.WinRate, _shiftPower * 0.5f);
            RecordAndApplyChange(characterManager, CharacterType.Mage, CharacterStat.WinRate, _shiftPower * -0.3f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, _shiftPower * 0.4f);
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.WinRate, _shiftPower * -0.2f);
            
            // Also affects popularity
            RecordAndApplyChange(characterManager, CharacterType.Warrior, CharacterStat.Popularity, _shiftPower * 0.3f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.Popularity, _shiftPower * 0.2f);
        }
        
        private void ApplySeasonsChangeShift(CharacterManager characterManager)
        {
            // Seasons change is major - resets meta and increases diversity
            // First, flatten win rates a bit
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameCharacter character = characterManager.GetCharacter(type);
                if (character != null)
                {
                    float winRate = character.GetStat(CharacterStat.WinRate);
                    float targetWinRate = 50f;
                    
                    // Calculate how much to move towards 50%
                    float moveTowardCenter = (targetWinRate - winRate) * 0.3f * _shiftPower / 5f;
                    RecordAndApplyChange(characterManager, type, CharacterStat.WinRate, moveTowardCenter);
                }
            }
            
            // Boost popularity for all (new season excitement)
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, _shiftPower * 1.5f);
            }
        }
        
        private void ApplyTournamentFormatShift(CharacterManager characterManager)
        {
            // Tournament format favors coordinated comps
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.WinRate, _shiftPower * 0.7f);
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.WinRate, _shiftPower * 0.6f);
            
            // Increases popularity of characters seen in tournaments
            RecordAndApplyChange(characterManager, CharacterType.Warrior, CharacterStat.Popularity, _shiftPower * 0.8f);
            RecordAndApplyChange(characterManager, CharacterType.Mage, CharacterStat.Popularity, _shiftPower * 0.9f);
            RecordAndApplyChange(characterManager, CharacterType.Support, CharacterStat.Popularity, _shiftPower * 1.1f);
            RecordAndApplyChange(characterManager, CharacterType.Tank, CharacterStat.Popularity, _shiftPower * 0.7f);
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