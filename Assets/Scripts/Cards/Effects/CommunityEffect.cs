using UnityEngine;
using MetaBalance.Characters;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Effect that influences player perception (Command Pattern)
    /// </summary>
    public class CommunityEffect : CardEffect
    {
        private CommunityActionType _actionType;
        private int _communityImpact;
        private bool _hasExecuted = false;
        
        public CommunityEffect(CardData source, CommunityActionType actionType, int communityImpact) 
            : base(source)
        {
            _actionType = actionType;
            _communityImpact = communityImpact;
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
            
            // Apply effect based on action type
            switch (_actionType)
            {
                case CommunityActionType.DeveloperUpdate:
                    ApplyDeveloperUpdateEffect(characterManager);
                    break;
                    
                case CommunityActionType.Survey:
                    ApplySurveyEffect(characterManager);
                    break;
                    
                case CommunityActionType.EngagementCampaign:
                    ApplyEngagementCampaignEffect(characterManager);
                    break;
                    
                case CommunityActionType.ContentCreatorSpotlight:
                    ApplyContentCreatorSpotlightEffect(characterManager);
                    break;
                    
                case CommunityActionType.CommunityEvent:
                    ApplyCommunityEventEffect(characterManager);
                    break;
            }
            
            // Mark as executed
            _hasExecuted = true;
            
            // Log for debugging
            Debug.Log($"Community Action: {_actionType} with impact {_communityImpact}");
            
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
            
            // Apply reverse effect based on action type
            switch (_actionType)
            {
                case CommunityActionType.DeveloperUpdate:
                    // Reverse developer update effect
                    foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
                    {
                        characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, -_communityImpact * 0.5f);
                    }
                    break;
                    
                case CommunityActionType.Survey:
                    // Reverse survey effect
                    foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
                    {
                        characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, -_communityImpact * 0.3f);
                    }
                    break;
                    
                case CommunityActionType.EngagementCampaign:
                    // Reverse engagement campaign effect
                    foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
                    {
                        characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, -_communityImpact * 0.8f);
                    }
                    break;
                    
                case CommunityActionType.ContentCreatorSpotlight:
                    // Reverse content creator spotlight effect
                    characterManager.ModifyCharacterStat(GetSpotlightCharacter(), CharacterStat.Popularity, -_communityImpact * 2.0f);
                    break;
                    
                case CommunityActionType.CommunityEvent:
                    // Reverse community event effect
                    foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
                    {
                        characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, -_communityImpact * 1.0f);
                    }
                    break;
            }
            
            // Log for debugging
            Debug.Log($"Community Action Undone: {_actionType}");
            
            _hasExecuted = false;
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Implement community action: {_actionType} (Impact: {_communityImpact})";
        }
        
        // Different community action effects
        
        private void ApplyDeveloperUpdateEffect(CharacterManager characterManager)
        {
            // Developer updates improve community perception
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, _communityImpact * 0.5f);
            }
        }
        
        private void ApplySurveyEffect(CharacterManager characterManager)
        {
            // Surveys improve community feeling of being heard
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, _communityImpact * 0.3f);
            }
        }
        
        private void ApplyEngagementCampaignEffect(CharacterManager characterManager)
        {
            // Engagement campaigns significantly boost popularity
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, _communityImpact * 0.8f);
            }
        }
        
        private void ApplyContentCreatorSpotlightEffect(CharacterManager characterManager)
        {
            // Content creator spotlight dramatically boosts one character's popularity
            CharacterType spotlightCharacter = GetSpotlightCharacter();
            characterManager.ModifyCharacterStat(spotlightCharacter, CharacterStat.Popularity, _communityImpact * 2.0f);
        }
        
        private void ApplyCommunityEventEffect(CharacterManager characterManager)
        {
            // Community events boost all characters' popularity
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                characterManager.ModifyCharacterStat(type, CharacterStat.Popularity, _communityImpact * 1.0f);
            }
        }
        
        // Helper to determine spotlight character (least popular or specified in card)
        private CharacterType GetSpotlightCharacter()
        {
            // For this implementation, find the least popular character to spotlight
            CharacterManager characterManager = CharacterManager.Instance;
            CharacterType leastPopular = CharacterType.Warrior; // Default
            float lowestPopularity = float.MaxValue;
            
            foreach (CharacterType type in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameCharacter character = characterManager.GetCharacter(type);
                if (character != null)
                {
                    float popularity = character.GetStat(CharacterStat.Popularity);
                    if (popularity < lowestPopularity)
                    {
                        lowestPopularity = popularity;
                        leastPopular = type;
                    }
                }
            }
            
            return leastPopular;
        }
    }
}