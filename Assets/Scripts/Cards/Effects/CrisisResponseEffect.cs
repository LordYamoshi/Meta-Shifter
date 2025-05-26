using UnityEngine;
using MetaBalance.Characters;
using System.Collections.Generic;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Effect that reacts to emergent problems (Command Pattern)
    /// </summary>
    public class CrisisResponseEffect : CardEffect
    {
        private CrisisType _crisisType;
        private int _responseEffectiveness;
        private bool _hasExecuted = false;
        
        // Track changes for undo
        private Dictionary<CharacterType, Dictionary<CharacterStat, float>> _changes = 
            new Dictionary<CharacterType, Dictionary<CharacterStat, float>>();
        
        public CrisisResponseEffect(CardData source, CrisisType crisisType, int responseEffectiveness) 
            : base(source)
        {
            _crisisType = crisisType;
            _responseEffectiveness = responseEffectiveness;
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
            
            // Apply effect based on crisis type
            switch (_crisisType)
            {
                case CrisisType.GameplayExploit:
                    ApplyGameplayExploitResponse(characterManager);
                    break;
                    
                case CrisisType.ServerIssue:
                    ApplyServerIssueResponse(characterManager);
                    break;
                    
                case CrisisType.ProPlayerControversy:
                    ApplyProPlayerControversyResponse(characterManager);
                    break;
                    
                case CrisisType.DataBreach:
                    ApplyDataBreachResponse(characterManager);
                    break;
                    
                case CrisisType.MajorBug:
                    ApplyMajorBugResponse(characterManager);
                    break;
            }
            
            // Mark as executed
            _hasExecuted = true;
            
            // Log for debugging
            Debug.Log($"Crisis Response: {_crisisType} with effectiveness {_responseEffectiveness}");
            
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
            Debug.Log($"Crisis Response Undone: {_crisisType}");
            
            _hasExecuted = false;
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Handle crisis: {_crisisType} (Effectiveness: {_responseEffectiveness})";
        }
        
        // Different crisis response implementations
        
        private void ApplyGameplayExploitResponse(CharacterManager characterManager)
        {
            // For gameplay exploit, identify most overpowered character and fix
            CharacterType mostOverpowered = FindMostOverpoweredCharacter(characterManager);
            
            // Fix the exploit (reduce win rate based on effectiveness)
            float winRateReduction = -10f * (_responseEffectiveness / 5f);
            RecordAndApplyChange(characterManager, mostOverpowered, CharacterStat.WinRate, winRateReduction);
            
            // But increase popularity due to quick response
            float popularityBoost = 5f * (_responseEffectiveness / 5f);
            RecordAndApplyChange(characterManager, mostOverpowered, CharacterStat.Popularity, popularityBoost);
            
            // Small boost to all characters' popularity for addressing the issue
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                if (type != mostOverpowered)
                {
                    RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityBoost * 0.5f);
                }
            }
        }
        
        private void ApplyServerIssueResponse(CharacterManager characterManager)
        {
            // Server issue affects all characters equally
            float popularityChange = 3f * (_responseEffectiveness / 5f);
            
            // Apply to all characters
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, popularityChange);
            }
        }
        
        private void ApplyProPlayerControversyResponse(CharacterManager characterManager)
        {
            // Pro player controversy affects one random character (typically most popular)
            CharacterType affectedCharacter = FindMostPopularCharacter(characterManager);
            
            // Apply popularity change based on response effectiveness
            float popularityChange = -5f + (_responseEffectiveness * 2); // Can be negative or positive
            RecordAndApplyChange(characterManager, affectedCharacter, CharacterStat.Popularity, popularityChange);
        }
        
        private void ApplyDataBreachResponse(CharacterManager characterManager)
        {
            // Data breach reduces all popularity but good response can mitigate
            float basePopularityDrop = -8f;
            float mitigationFactor = _responseEffectiveness / 10f; // 0 to 1
            float actualPopularityChange = basePopularityDrop * (1f - mitigationFactor);
            
            // Apply to all characters
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                RecordAndApplyChange(characterManager, type, CharacterStat.Popularity, actualPopularityChange);
            }
        }
        
        private void ApplyMajorBugResponse(CharacterManager characterManager)
        {
            // Major bug affects one character
            // For simplicity, we'll pick one at random - in a real implementation, 
            // this would be specified in the card data
            CharacterType[] types = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            CharacterType buggedCharacter = types[Random.Range(0, types.Length)];
            
            // Apply win rate fix
            float winRateFix = -7f * (1 - (_responseEffectiveness / 10f)); // More effective = less negative
            RecordAndApplyChange(characterManager, buggedCharacter, CharacterStat.WinRate, winRateFix);
            
            // Apply popularity change based on response effectiveness
            float popularityChange = -3f + (_responseEffectiveness / 2f); // Can be negative or positive
            RecordAndApplyChange(characterManager, buggedCharacter, CharacterStat.Popularity, popularityChange);
        }
        
        // Helper methods
        
        private CharacterType FindMostOverpoweredCharacter(CharacterManager characterManager)
        {
            CharacterType mostOverpowered = CharacterType.Warrior; // Default
            float highestWinRate = float.MinValue;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameCharacter character = characterManager.GetCharacter(type);
                if (character != null)
                {
                    float winRate = character.GetStat(CharacterStat.WinRate);
                    if (winRate > highestWinRate)
                    {
                        highestWinRate = winRate;
                        mostOverpowered = type;
                    }
                }
            }
            
            return mostOverpowered;
        }
        
        private CharacterType FindMostPopularCharacter(CharacterManager characterManager)
        {
            CharacterType mostPopular = CharacterType.Warrior; // Default
            float highestPopularity = float.MinValue;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameCharacter character = characterManager.GetCharacter(type);
                if (character != null)
                {
                    float popularity = character.GetStat(CharacterStat.Popularity);
                    if (popularity > highestPopularity)
                    {
                        highestPopularity = popularity;
                        mostPopular = type;
                    }
                }
            }
            
            return mostPopular;
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