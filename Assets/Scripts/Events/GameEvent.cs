using UnityEngine;
using System.Collections.Generic;
using System;

namespace MetaBalance.Events
{
    /// <summary>
    /// Legacy GameEvent class - rewritten to work with new EventData system
    /// Maintains backward compatibility while avoiding conflicts
    /// Uses composition instead of duplicate enums
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        [Header("Event Identity")]
        public string eventId;
        public string eventTitle;
        public string eventDescription;
        public EventType eventType; // Uses the enum from EventData
        public EventSeverity severity; // Uses the enum from EventData
        
        [Header("Event Context")]
        public List<string> affectedCharacters = new List<string>();
        public List<string> requiredTags = new List<string>();
        public int triggerWeek;
        public float duration = 1f; // How many turns the event lasts
        
        [Header("Event Responses")]
        public List<GameEventResponse> availableResponses = new List<GameEventResponse>();
        public GameEventResponse defaultResponse;
        
        [Header("Event Effects")]
        public GameEventEffects effects;
        
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
        
        /// <summary>
        /// Convert this GameEvent to the new EventData format
        /// </summary>
        public EventData ToEventData()
        {
            var eventData = new EventData(eventTitle, eventDescription, eventType, severity)
            {
                timeRemaining = turnsRemaining * 30f, // Convert turns to seconds
                expectedImpact = CalculateExpectedImpact(),
                expectedImpacts = GenerateExpectedImpacts(),
                eventColor = eventColor,
                iconName = eventIcon?.name ?? ""
            };
            
            // Convert responses
            foreach (var response in availableResponses)
            {
                eventData.responseOptions.Add(response.ToEventResponseOption());
            }
            
            return eventData;
        }
        
        private float CalculateExpectedImpact()
        {
            return severity switch
            {
                EventSeverity.Low => 2f,
                EventSeverity.Medium => 5f,
                EventSeverity.High => 8f,
                EventSeverity.Critical => 10f,
                _ => 1f
            };
        }
        
        private List<string> GenerateExpectedImpacts()
        {
            var impacts = new List<string>();
            
            switch (eventType)
            {
                case EventType.Crisis:
                    impacts.Add("Community backlash risk");
                    impacts.Add("Player satisfaction impact");
                    if (severity == EventSeverity.Critical)
                        impacts.Add("Emergency response required");
                    break;
                    
                case EventType.Opportunity:
                    impacts.Add("Positive community impact");
                    impacts.Add("Engagement boost potential");
                    impacts.Add("PR opportunity");
                    break;
                    
                case EventType.Community:
                    impacts.Add("Community sentiment shift");
                    impacts.Add("Player feedback influence");
                    break;
                    
                case EventType.Technical:
                    impacts.Add("System performance impact");
                    impacts.Add("Player experience effects");
                    break;
                    
                case EventType.Competitive:
                    impacts.Add("Competitive balance impact");
                    impacts.Add("Pro player satisfaction");
                    break;
            }
            
            return impacts;
        }
    }
    
    /// <summary>
    /// GameEvent response - converts to new EventResponseOption
    /// </summary>
    [System.Serializable]
    public class GameEventResponse
    {
        public string responseId;
        public string buttonText;
        public string responseDescription;
        public LegacyResponseType responseType; // Renamed to avoid conflicts
        
        [Header("Resource Costs")]
        public int rpCost;
        public int cpCost;
        
        [Header("Required Cards")]
        public List<Cards.CardType> requiredCardTypes = new List<Cards.CardType>();
        public int requiredCardCount = 1;
        
        [Header("Response Effects")]
        public GameEventResponseEffects effects;
        
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
            
            // You'd implement this based on your card system
            // var availableCards = cardManager.GetCurrentHandData();
            
            return true; // Placeholder
        }
        
        /// <summary>
        /// Convert to new EventResponseOption format
        /// </summary>
        public EventResponseOption ToEventResponseOption()
        {
            return new EventResponseOption
            {
                buttonText = buttonText,
                description = responseDescription,
                responseType = ConvertResponseType(),
                rpCost = rpCost,
                cpCost = cpCost,
                sentimentChange = effects?.satisfactionChange ?? 0f,
                successMessage = effects?.successMessage ?? "Response completed",
                buttonColor = GetResponseColor()
            };
        }
        
        private EventResponseType ConvertResponseType()
        {
            return responseType switch
            {
                LegacyResponseType.Emergency => EventResponseType.EmergencyFix,
                LegacyResponseType.Strategic => EventResponseType.CustomResponse,
                LegacyResponseType.Community => EventResponseType.CommunityManagement,
                LegacyResponseType.Technical => EventResponseType.EmergencyFix,
                LegacyResponseType.Ignore => EventResponseType.Ignore,
                LegacyResponseType.Escalate => EventResponseType.CustomResponse,
                _ => EventResponseType.ObserveAndLearn
            };
        }
        
        private Color GetResponseColor()
        {
            return responseType switch
            {
                LegacyResponseType.Emergency => new Color(0.8f, 0.3f, 0.4f), // Red
                LegacyResponseType.Community => new Color(0.6f, 0.4f, 0.8f), // Purple
                LegacyResponseType.Strategic => new Color(0.4f, 0.6f, 0.8f), // Blue
                LegacyResponseType.Technical => new Color(0.5f, 0.5f, 0.5f), // Gray
                _ => new Color(0.3f, 0.4f, 0.8f) // Default blue
            };
        }
    }
    
    /// <summary>
    /// GameEvent response effects
    /// </summary>
    [System.Serializable]
    public class GameEventResponseEffects
    {
        [Header("Immediate Effects")]
        public float satisfactionChange;
        public int rpReward;
        public int cpReward;
        
        [Header("Character Effects")]
        public List<GameEventCharacterEffect> characterEffects = new List<GameEventCharacterEffect>();
        
        [Header("Success Chance")]
        [Range(0f, 1f)]
        public float successChance = 1f;
        
        [Header("Follow-up Events")]
        public List<string> triggeredEventIds = new List<string>();
        
        public string successMessage;
        public string failureMessage;
    }
    
    /// <summary>
    /// Character effect for GameEvents - renamed to avoid conflicts
    /// </summary>
    [System.Serializable]
    public class GameEventCharacterEffect
    {
        public Characters.CharacterType character;
        public Characters.CharacterStat stat;
        public float change;
        public bool isPercentage = true;
        
        /// <summary>
        /// Convert to new CharacterStatChange format
        /// </summary>
        public CharacterStatChange ToCharacterStatChange()
        {
            return new CharacterStatChange(character, stat, change);
        }
    }
    
    /// <summary>
    /// Overall GameEvent effects
    /// </summary>
    [System.Serializable]
    public class GameEventEffects
    {
        [Header("Community Impact")]
        public float sentimentChange = 0f;
        public float engagementChange = 0f;
        
        [Header("Resource Impact")]
        public int rpChange = 0;
        public int cpChange = 0;
        
        [Header("Character Impact")]
        public List<GameEventCharacterEffect> characterEffects = new List<GameEventCharacterEffect>();
        
        [Header("Meta Impact")]
        public float balanceShift = 0f;
        public List<string> affectedSystems = new List<string>();
    }
    
    /// <summary>
    /// Legacy response types - renamed to avoid conflicts with new system
    /// </summary>
    public enum LegacyResponseType
    {
        Emergency,    // Quick fix, high cost
        Strategic,    // Planned response, balanced cost
        Community,    // PR/communication response
        Technical,    // Technical solution
        Ignore,       // Do nothing
        Escalate      // Pass to higher authority
    }
    
    /// <summary>
    /// Factory for creating GameEvents that work with the new system
    /// </summary>
    public static class GameEventFactory
    {
        /// <summary>
        /// Create a GameEvent from an EventData (for backward compatibility)
        /// </summary>
        public static GameEvent CreateFromEventData(EventData eventData)
        {
            var gameEvent = new GameEvent
            {
                eventTitle = eventData.title,
                eventDescription = eventData.description,
                eventType = eventData.eventType,
                severity = eventData.severity,
                turnsRemaining = Mathf.CeilToInt(eventData.timeRemaining / 30f),
                eventColor = eventData.eventColor,
                isActive = !eventData.isResolved
            };
            
            // Convert response options
            foreach (var responseOption in eventData.responseOptions)
            {
                gameEvent.availableResponses.Add(CreateGameEventResponse(responseOption));
            }
            
            return gameEvent;
        }
        
        private static GameEventResponse CreateGameEventResponse(EventResponseOption responseOption)
        {
            return new GameEventResponse
            {
                buttonText = responseOption.buttonText,
                responseDescription = responseOption.description,
                responseType = ConvertToLegacyResponseType(responseOption.responseType),
                rpCost = responseOption.rpCost,
                cpCost = responseOption.cpCost,
                effects = new GameEventResponseEffects
                {
                    satisfactionChange = responseOption.sentimentChange,
                    successMessage = responseOption.successMessage,
                    successChance = responseOption.successChance
                }
            };
        }
        
        private static LegacyResponseType ConvertToLegacyResponseType(EventResponseType responseType)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => LegacyResponseType.Emergency,
                EventResponseType.CommunityManagement => LegacyResponseType.Community,
                EventResponseType.CustomResponse => LegacyResponseType.Strategic,
                EventResponseType.Ignore => LegacyResponseType.Ignore,
                _ => LegacyResponseType.Strategic
            };
        }
        
        /// <summary>
        /// Create a legacy-style crisis event
        /// </summary>
        public static GameEvent CreateLegacyCrisisEvent(string title, string description)
        {
            var gameEvent = new GameEvent
            {
                eventTitle = title,
                eventDescription = description,
                eventType = EventType.Crisis,
                severity = EventSeverity.High,
                turnsRemaining = 2,
                isUrgent = true
            };
            
            // Add default responses
            gameEvent.availableResponses.Add(new GameEventResponse
            {
                buttonText = "Emergency Response",
                responseDescription = "Quick action to address the crisis",
                responseType = LegacyResponseType.Emergency,
                rpCost = 3,
                cpCost = 1
            });
            
            gameEvent.availableResponses.Add(new GameEventResponse
            {
                buttonText = "Communicate",
                responseDescription = "Address community concerns",
                responseType = LegacyResponseType.Community,
                rpCost = 1,
                cpCost = 3
            });
            
            return gameEvent;
        }
    }
}