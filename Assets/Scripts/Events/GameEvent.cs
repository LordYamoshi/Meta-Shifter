// Assets/Scripts/Events/GameEvent.cs
using UnityEngine;
using System.Collections.Generic;
using System;

namespace MetaBalance.Events
{
    [System.Serializable]
    public class GameEvent
    {
        [Header("Event Identity")]
        public string eventId;
        public string eventTitle;
        public string eventDescription;
        public EventType eventType;
        public EventSeverity severity;
        
        [Header("Event Context")]
        public List<string> affectedCharacters = new List<string>();
        public List<string> requiredTags = new List<string>();
        public int triggerWeek;
        public float duration = 1f; // How many turns the event lasts
        
        [Header("Event Responses")]
        public List<EventResponse> availableResponses = new List<EventResponse>();
        public EventResponse defaultResponse;
        
        [Header("Event Effects")]
        public EventEffects effects;
        
        [Header("Visual & Audio")]
        public Sprite eventIcon;
        public Color eventColor = Color.white;
        public AudioClip eventSound;
        
        public DateTime timestamp;
        public bool isActive = true;
        public bool isUrgent = false;
        public int turnsRemaining;
        
        public GameEvent()
        {
            timestamp = DateTime.Now;
            eventId = Guid.NewGuid().ToString();
        }
        
        public bool IsExpired()
        {
            return turnsRemaining <= 0 || !isActive;
        }
        
        public string GetTimeRemaining()
        {
            if (turnsRemaining <= 0) return "Expired";
            if (turnsRemaining == 1) return "1 turn left";
            return $"{turnsRemaining} turns left";
        }
        
        public Color GetSeverityColor()
        {
            return severity switch
            {
                EventSeverity.Low => new Color(0.2f, 0.8f, 0.2f),      // Green
                EventSeverity.Medium => new Color(1f, 0.8f, 0.2f),    // Yellow
                EventSeverity.High => new Color(1f, 0.4f, 0.1f),      // Orange
                EventSeverity.Critical => new Color(0.8f, 0.1f, 0.1f), // Red
                _ => Color.white
            };
        }
        
        public string GetSeverityEmoji()
        {
            return severity switch
            {
                EventSeverity.Low => "ℹ️",
                EventSeverity.Medium => "⚠️", 
                EventSeverity.High => "🚨",
                EventSeverity.Critical => "💥",
                _ => "📋"
            };
        }
    }
    
    [System.Serializable]
    public class EventResponse
    {
        public string responseId;
        public string buttonText;
        public string responseDescription;
        public ResponseType responseType;
        
        [Header("Resource Costs")]
        public int rpCost;
        public int cpCost;
        
        [Header("Required Cards")]
        public List<Cards.CardType> requiredCardTypes = new List<Cards.CardType>();
        public int requiredCardCount = 1;
        
        [Header("Response Effects")]
        public EventResponseEffects effects;
        
        [Header("Availability")]
        public List<string> requiredConditions = new List<string>();
        public bool isAvailable = true;
        public string unavailableReason = "";
        
        public bool CanAfford()
        {
            var resourceManager = Core.ResourceManager.Instance;
            return resourceManager != null && resourceManager.CanSpend(rpCost, cpCost);
        }
        
        public bool HasRequiredCards()
        {
            if (requiredCardTypes.Count == 0) return true;
            
            var cardManager = Cards.CardManager.Instance;
            if (cardManager == null) return false;
            
            var availableCards = cardManager.GetCurrentHandData();
            
            foreach (var requiredType in requiredCardTypes)
            {
                int countOfType = 0;
                foreach (var card in availableCards)
                {
                    if (card.cardType == requiredType) countOfType++;
                }
                
                if (countOfType < requiredCardCount) return false;
            }
            
            return true;
        }
        
        public bool IsCurrentlyAvailable()
        {
            if (!isAvailable) return false;
            if (!CanAfford()) return false;
            if (!HasRequiredCards()) return false;
            
            // Check custom conditions
            foreach (var condition in requiredConditions)
            {
                if (!CheckCondition(condition)) return false;
            }
            
            return true;
        }
        
        private bool CheckCondition(string condition)
        {
            return condition switch
            {
                "planning_phase" => Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning,
                "implementation_phase" => Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Implementation,
                "high_satisfaction" => Characters.CharacterManager.Instance?.CalculateOverallBalance() > 70f,
                "low_satisfaction" => Characters.CharacterManager.Instance?.CalculateOverallBalance() < 40f,
                _ => true // Unknown conditions default to true
            };
        }
    }
    
    [System.Serializable]
    public class EventEffects
    {
        [Header("Character Effects")]
        public List<CharacterEffect> characterEffects = new List<CharacterEffect>();
        
        [Header("Resource Effects")]
        public int rpChange;
        public int cpChange;
        
        [Header("Community Effects")]
        public float satisfactionChange;
        public string communityMessage;
        
        [Header("Meta Effects")]
        public bool triggersMetaShift;
        public float metaStabilityChange;
    }
    
    [System.Serializable]
    public class EventResponseEffects
    {
        [Header("Immediate Effects")]
        public float satisfactionChange;
        public int rpReward;
        public int cpReward;
        
        [Header("Character Effects")]
        public List<CharacterEffect> characterEffects = new List<CharacterEffect>();
        
        [Header("Success Chance")]
        [Range(0f, 1f)]
        public float successChance = 1f;
        
        [Header("Follow-up Events")]
        public List<string> triggeredEventIds = new List<string>();
        
        public string successMessage;
        public string failureMessage;
    }
    
    [System.Serializable]
    public class CharacterEffect
    {
        public Characters.CharacterType character;
        public Characters.CharacterStat stat;
        public float change;
        public bool isPercentage = true;
    }
    
    public enum EventType
    {
        Crisis,           // Exploits, bugs, server issues
        Community,        // Social media trends, feedback
        Competitive,      // Tournament results, pro player actions
        Opportunity,      // Positive events, partnerships
        Seasonal,         // Holiday events, special periods
        Meta,            // Natural meta shifts, discoveries
        Technical,       // Server maintenance, updates
        Special          // Unique one-time events
    }
    
    public enum EventSeverity
    {
        Low,      // Minor impact, optional response
        Medium,   // Moderate impact, response recommended  
        High,     // Major impact, response strongly advised
        Critical  // Severe impact, immediate response required
    }
    
    public enum ResponseType
    {
        Emergency,    // Quick fix, high cost
        Strategic,    // Planned response, balanced cost
        Community,    // PR/communication response
        Technical,    // Technical solution
        Ignore,       // Do nothing
        Escalate      // Pass to higher authority
    }
}