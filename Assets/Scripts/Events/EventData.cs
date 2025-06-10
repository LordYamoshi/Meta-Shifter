using UnityEngine;
using System.Collections.Generic;
using System;

namespace MetaBalance.Events
{
    /// <summary>
    /// Core Event Data Structure - Clean and Simple
    /// Matches your beautiful UI design shown in the image
    /// </summary>
    [System.Serializable]
    public class EventData
    {
        [Header("Basic Event Info")]
        public string title;
        public string description;
        public EventType eventType;
        public EventSeverity severity;
        
        [Header("Timing")]
        public float timeRemaining = 30f;
        public DateTime eventStartTime;
        public bool isResolved = false;
        
        [Header("Impact Information")]
        public float expectedImpact = 0f;
        public List<string> expectedImpacts = new List<string>();
        public float estimatedSentimentImpact = 0f;
        
        [Header("Response Options")]
        public List<EventResponseOption> responseOptions = new List<EventResponseOption>();
        
        [Header("Visual")]
        public Color eventColor = Color.white;
        public string iconName = "";
        
        public EventData()
        {
            eventStartTime = DateTime.Now;
        }
        
        public EventData(string title, string description, EventType type, EventSeverity severity)
        {
            this.title = title;
            this.description = description;
            this.eventType = type;
            this.severity = severity;
            this.eventStartTime = DateTime.Now;
        }
        
        public bool IsExpired()
        {
            return timeRemaining <= 0f || isResolved;
        }
        
        public string GetTimeRemainingText()
        {
            if (timeRemaining <= 0f) return "EXPIRED";
            
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            return $"{minutes:00}:{seconds:00}";
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
        
        public string GetSeverityText()
        {
            return severity switch
            {
                EventSeverity.Low => "Low Priority",
                EventSeverity.Medium => "Medium Priority", 
                EventSeverity.High => "High Priority",
                EventSeverity.Critical => "Urgent",
                _ => "Unknown"
            };
        }
    }
    
    /// <summary>
    /// Response option for events - matches your button design
    /// </summary>
    [System.Serializable]
    public class EventResponseOption
    {
        [Header("Response Details")]
        public string buttonText;
        public string description;
        public EventResponseType responseType;
        
        [Header("Resource Costs")]
        public int rpCost = 0;
        public int cpCost = 0;
        
        [Header("Effects")]
        public float sentimentChange = 0f;
        public List<CharacterStatChange> characterEffects = new List<CharacterStatChange>();
        public string successMessage = "";
        public string failureMessage = "";
        public float successChance = 1f;
        
        [Header("Visual")]
        public Color buttonColor = Color.white;
        
        public bool CanAfford()
        {
            var resourceManager = Core.ResourceManager.Instance;
            return resourceManager != null && resourceManager.CanSpend(rpCost, cpCost);
        }
        
        public string GetCostText()
        {
            if (rpCost > 0 && cpCost > 0)
                return $"{rpCost} RP, {cpCost} CP";
            else if (rpCost > 0)
                return $"{rpCost} RP";
            else if (cpCost > 0)
                return $"{cpCost} CP";
            else
                return "Free";
        }
    }
    
    /// <summary>
    /// Character stat change from event responses
    /// </summary>
    [System.Serializable]
    public class CharacterStatChange
    {
        public Characters.CharacterType character;
        public Characters.CharacterStat stat;
        public float changeAmount;
        public bool isPercentage = true;
        
        public CharacterStatChange(Characters.CharacterType character, Characters.CharacterStat stat, float change)
        {
            this.character = character;
            this.stat = stat;
            this.changeAmount = change;
        }
    }
    
    /// <summary>
    /// Event types for categorization
    /// </summary>
    public enum EventType
    {
        Crisis,
        Opportunity,
        Community,
        Technical,
        Competitive,
        Special
    }
    
    /// <summary>
    /// Event severity levels
    /// </summary>
    public enum EventSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Response types for events
    /// </summary>
    public enum EventResponseType
    {
        EmergencyFix,
        CommunityManagement,
        ObserveAndLearn,
        CustomResponse,
        Ignore
    }
}