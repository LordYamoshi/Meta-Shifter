using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace MetaBalance.Events
{
    [System.Serializable]
    public class GameEvent
    {
        public string id;
        public string title;
        public string description;
        public EventType eventType;
        public DateTime timestamp;
        public bool isResolved;
        public ResponseType? usedResponse;
        public Characters.CharacterType? affectedCharacter;
        public GameEventData eventData;
        
        public GameEvent(GameEventData data)
        {
            eventData = data;
            id = data.id;
            title = data.title;
            description = ProcessDescription(data.description);
            eventType = data.eventType;
            timestamp = DateTime.Now;
            isResolved = false;
            
            // Randomly assign affected character if description has placeholder
            if (data.description.Contains("{character}"))
            {
                var characters = System.Enum.GetValues(typeof(Characters.CharacterType));
                affectedCharacter = (Characters.CharacterType)characters.GetValue(Random.Range(0, characters.Length));
            }
        }
        
        private string ProcessDescription(string template)
        {
            if (affectedCharacter.HasValue)
            {
                template = template.Replace("{character}", affectedCharacter.Value.ToString());
            }
            return template;
        }
        
        public void Resolve(ResponseType response)
        {
            isResolved = true;
            usedResponse = response;
        }
        
        public string GetTimeRemaining()
        {
            var timeElapsed = DateTime.Now - timestamp;
            var timeRemaining = TimeSpan.FromMinutes(5) - timeElapsed; // 5 minute window
            
            if (timeRemaining.TotalSeconds <= 0)
                return "Expired";
                
            return $"{timeRemaining.Minutes}:{timeRemaining.Seconds:D2}";
        }
        
        public bool IsExpired()
        {
            var timeElapsed = DateTime.Now - timestamp;
            return timeElapsed.TotalMinutes > 5; // 5 minute response window
        }
        
        public Color GetEventColor()
        {
            return eventType switch
            {
                EventType.Crisis => new Color(0.8f, 0.2f, 0.2f), // Red
                EventType.Opportunity => new Color(0.2f, 0.6f, 1f), // Blue
                EventType.Community => new Color(0.8f, 0.2f, 0.8f), // Purple
                _ => Color.gray
            };
        }
        
        public string GetEventIcon()
        {
            return eventType switch
            {
                EventType.Crisis => "⚠️",
                EventType.Opportunity => "🎯",
                EventType.Community => "💬",
                _ => "?"
            };
        }
    }
    
    [System.Serializable]
    public class GameEventData
    {
        public string id;
        public string title;
        public string description;
        public EventType eventType;
        public float triggerChance;
        public float communityImpact;
        public List<ResponseType> requiredResponses;
    }
    
    public enum EventType
    {
        Crisis,
        Opportunity,
        Community
    }
    
    public enum ResponseType
    {
        EmergencyFix,
        DevUpdate,
        CommunityManagement,
        MetaShift,
        Ignore
    }
}