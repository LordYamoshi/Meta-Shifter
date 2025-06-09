using UnityEngine;
using System.Collections.Generic;
using System;

namespace MetaBalance.Events
{
    /// <summary>
    /// Complete Event Data Structure matching your UI design
    /// </summary>
    [System.Serializable]
    public class EventData
    {
        [Header("Basic Event Info")]
        public string eventTitle;
        public string description;
        public EventType eventType;
        public EventUrgency urgencyLevel;
        
        [Header("Timing")]
        public float responseTimeLimit = 30f; // 30 seconds to respond
        public DateTime eventStartTime;
        
        [Header("Impact Information")]
        public List<string> expectedImpacts = new List<string>();
        public float estimatedSentimentImpact = 0f;
        
        [Header("Response Options")]
        public Dictionary<EventResponseType, EventResponse> responses = new Dictionary<EventResponseType, EventResponse>();
        
        [Header("Expiration")]
        public EventResponse expirationPenalty; // What happens if no response is given
        
        public EventData()
        {
            eventStartTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Individual response option for events
    /// </summary>
    [System.Serializable]
    public class EventResponse
    {
        [Header("Response Details")]
        public string responseText;
        public int rpCost = 0;
        public int cpCost = 0;
        
        [Header("Effects")]
        public List<CharacterEffect> characterEffects = new List<CharacterEffect>();
        public float communitySentimentChange = 0f;
        public string specialEffect = ""; // For unique effects
        
        [Header("Success Information")]
        public float successChance = 1f; // 0-1, for responses that can fail
        public string successMessage = "";
        public string failureMessage = "";
    }
    
    /// <summary>
    /// Effect on character stats from event responses
    /// </summary>
    [System.Serializable]
    public class CharacterEffect
    {
        public Characters.CharacterType character;
        public Characters.CharacterStat stat;
        public float changeAmount; // Percentage change
        
        public CharacterEffect(Characters.CharacterType character, Characters.CharacterStat stat, float changeAmount)
        {
            this.character = character;
            this.stat = stat;
            this.changeAmount = changeAmount;
        }
    }
    
    /// <summary>
    /// Event types matching your game design
    /// </summary>
    public enum EventType
    {
        Crisis,           // Problems that need solving
        Opportunity,      // Chances for positive outcomes
        CommunityEvent,   // Community-driven events
        MetaShift,        // Events that change game meta
        TournamentEvent,  // Competition-related events
        SeasonalEvent     // Time-based events
    }
    
    /// <summary>
    /// Urgency levels affecting response time and consequences
    /// </summary>
    public enum EventUrgency
    {
        Low,     // Long response time, minor consequences
        Medium,  // Moderate response time, moderate consequences
        High,    // Short response time, significant consequences
        Critical // Very short response time, major consequences
    }
    
    /// <summary>
    /// Response types matching your UI buttons
    /// </summary>
    public enum EventResponseType
    {
        EmergencyFix,        // Quick technical solution (costs RP)
        CommunityManagement, // PR/Communication solution (costs CP)
        ObserveAndLearn,     // Do nothing but gain insight (usually free)
        IgnoreEvent,         // Completely ignore (may have consequences)
        DelayResponse,       // Buy more time (costs resources)
        SeekAdvice,          // Consult experts (costs CP, improves success chance)
        CustomAction         // Flexible for unique event responses
    }
    
    /// <summary>
    /// Factory for creating pre-defined events
    /// Uses Factory Pattern for consistent event creation
    /// </summary>
    public static class EventDataFactory
    {
        /// <summary>
        /// Create the Support Exploit Crisis from your UI mockup
        /// </summary>
        public static EventData CreateSupportExploitCrisis()
        {
            var eventData = new EventData
            {
                eventTitle = "Game-Breaking Exploit Discovered",
                description = "Players have found a way to stack support abilities that makes them nearly invincible. The community is in uproar and demanding immediate action. Pro players are threatening to boycott upcoming tournaments.",
                eventType = EventType.Crisis,
                urgencyLevel = EventUrgency.Critical,
                responseTimeLimit = 45f,
                estimatedSentimentImpact = -15f,
                expectedImpacts = new List<string>
                {
                    "Support win rate may spike to 65%+",
                    "Community satisfaction dropping fast",
                    "Pro players threatening boycott",
                    "Tournament integrity at risk"
                }
            };
            
            // Emergency Fix Response (3 RP)
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Emergency Fix",
                rpCost = 3,
                cpCost = 0,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.Damage, -15f),
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.Utility, -10f)
                },
                communitySentimentChange = 8f,
                successChance = 0.9f,
                successMessage = "Exploit patched successfully! Community appreciates quick response.",
                failureMessage = "Patch introduced new bugs. Community frustration increased."
            };
            
            // Community Management Response (4 CP)
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Community Management",
                rpCost = 0,
                cpCost = 4,
                communitySentimentChange = 5f,
                specialEffect = "delay_exploit_impact",
                successChance = 0.7f,
                successMessage = "Community calmed through transparent communication.",
                failureMessage = "PR response seen as empty promises. Trust decreased."
            };
            
            // Observe and Learn Response (Free)
            eventData.responses[EventResponseType.ObserveAndLearn] = new EventResponse
            {
                responseText = "Observe and learn",
                rpCost = 0,
                cpCost = 0,
                communitySentimentChange = -5f,
                specialEffect = "research_boost",
                successMessage = "Valuable data collected on community reactions.",
                failureMessage = "Inaction led to further community unrest."
            };
            
            // Expiration Penalty
            eventData.expirationPenalty = new EventResponse
            {
                responseText = "Event Expired",
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.WinRate, 8f)
                },
                communitySentimentChange = -12f,
                specialEffect = "tournament_boycott"
            };
            
            return eventData;
        }
        
        /// <summary>
        /// Create Tournament Opportunity Event
        /// </summary>
        public static EventData CreateTournamentOpportunity()
        {
            var eventData = new EventData
            {
                eventTitle = "Major Tournament Announced",
                description = "A large esports organization just announced a major tournament for your game. This is a chance to boost competitive engagement and show off the current balance state.",
                eventType = EventType.Opportunity,
                urgencyLevel = EventUrgency.Medium,
                responseTimeLimit = 60f,
                estimatedSentimentImpact = 10f,
                expectedImpacts = new List<string>
                {
                    "+15% Community satisfaction",
                    "Highlight current meta balance",
                    "Boost character popularity based on performance",
                    "Increased player engagement"
                }
            };
            
            // Community Management Response
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Promote Tournament",
                rpCost = 0,
                cpCost = 3,
                communitySentimentChange = 12f,
                specialEffect = "tournament_hype",
                successMessage = "Tournament promotion successful! Community excitement high.",
                failureMessage = "Promotion fell flat. Missed opportunity for engagement."
            };
            
            // Emergency Fix Response (balance for tournament)
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Balance for Tournament",
                rpCost = 5,
                cpCost = 2,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, 2f),
                    new CharacterEffect(Characters.CharacterType.Mage, Characters.CharacterStat.WinRate, 1f),
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.WinRate, -1f),
                    new CharacterEffect(Characters.CharacterType.Tank, Characters.CharacterStat.WinRate, 1f)
                },
                communitySentimentChange = 8f,
                specialEffect = "tournament_ready",
                successMessage = "Game balanced for competitive play. Tournament will showcase great matches!",
                failureMessage = "Balance changes backfired. Tournament may be chaotic."
            };
            
            // Observe Response
            eventData.responses[EventResponseType.ObserveAndLearn] = new EventResponse
            {
                responseText = "Monitor Tournament",
                rpCost = 0,
                cpCost = 0,
                communitySentimentChange = 3f,
                specialEffect = "tournament_data",
                successMessage = "Valuable competitive data gathered from tournament play.",
                failureMessage = "Missed opportunity to engage with competitive scene."
            };
            
            return eventData;
        }
        
        /// <summary>
        /// Create Community Feedback Crisis
        /// </summary>
        public static EventData CreateCommunityFeedbackCrisis()
        {
            var eventData = new EventData
            {
                eventTitle = "Community Sentiment Plummeting",
                description = "Recent balance changes have caused widespread community dissatisfaction. Player count is dropping, and negative reviews are flooding in. Immediate action needed to restore faith.",
                eventType = EventType.Crisis,
                urgencyLevel = EventUrgency.High,
                responseTimeLimit = 40f,
                estimatedSentimentImpact = -20f,
                expectedImpacts = new List<string>
                {
                    "Player retention declining",
                    "Negative review bombing",
                    "Content creators making criticism videos",
                    "Risk of lasting reputation damage"
                }
            };
            
            // Community Management (Major PR effort)
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Major PR Campaign",
                rpCost = 1,
                cpCost = 6,
                communitySentimentChange = 15f,
                specialEffect = "community_outreach",
                successChance = 0.8f,
                successMessage = "Community outreach successful! Trust beginning to restore.",
                failureMessage = "PR campaign seen as damage control. Community remains skeptical."
            };
            
            // Emergency Revert
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Emergency Revert",
                rpCost = 4,
                cpCost = 1,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Warrior, Characters.CharacterStat.Health, 5f),
                    new CharacterEffect(Characters.CharacterType.Mage, Characters.CharacterStat.Damage, -3f),
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.Utility, 8f)
                },
                communitySentimentChange = 18f,
                specialEffect = "revert_changes",
                successMessage = "Quick revert appreciated by community. Stability restored.",
                failureMessage = "Revert caused confusion. Community lost faith in design direction."
            };
            
            // Seek Advice
            eventData.responses[EventResponseType.SeekAdvice] = new EventResponse
            {
                responseText = "Consult Community Leaders",
                rpCost = 0,
                cpCost = 4,
                communitySentimentChange = 8f,
                specialEffect = "community_council",
                successMessage = "Community leaders provide valuable feedback. Collaborative approach appreciated.",
                failureMessage = "Consultation seen as too little, too late."
            };
            
            return eventData;
        }
        
        /// <summary>
        /// Create Meta Shift Opportunity
        /// </summary>
        public static EventData CreateMetaShiftOpportunity()
        {
            var eventData = new EventData
            {
                eventTitle = "Fresh Meta Opportunity",
                description = "The current meta has become stale with 70% of players using the same character. This presents an opportunity to shake things up and create exciting new strategies.",
                eventType = EventType.MetaShift,
                urgencyLevel = EventUrgency.Low,
                responseTimeLimit = 90f,
                estimatedSentimentImpact = 5f,
                expectedImpacts = new List<string>
                {
                    "Increased character diversity",
                    "New strategic possibilities",
                    "Content creation opportunities",
                    "Renewed player interest"
                }
            };
            
            // Major Rebalance
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Major Rebalance",
                rpCost = 8,
                cpCost = 2,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Warrior, Characters.CharacterStat.Damage, -8f),
                    new CharacterEffect(Characters.CharacterType.Mage, Characters.CharacterStat.Health, 12f),
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.Speed, 15f),
                    new CharacterEffect(Characters.CharacterType.Tank, Characters.CharacterStat.Utility, 10f)
                },
                communitySentimentChange = 8f,
                specialEffect = "meta_revolution",
                successChance = 0.75f,
                successMessage = "Meta revolution successful! Fresh strategies emerging.",
                failureMessage = "Rebalance created new problems. Community confused."
            };
            
            // Gradual Changes
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Gradual Meta Evolution",
                rpCost = 3,
                cpCost = 3,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Warrior, Characters.CharacterStat.Popularity, -5f),
                    new CharacterEffect(Characters.CharacterType.Mage, Characters.CharacterStat.Popularity, 3f),
                    new CharacterEffect(Characters.CharacterType.Tank, Characters.CharacterStat.Popularity, 4f)
                },
                communitySentimentChange = 5f,
                specialEffect = "gradual_shift",
                successMessage = "Gradual changes well-received. Meta evolving naturally.",
                failureMessage = "Changes too subtle. Meta remains stagnant."
            };
            
            // Wait and See
            eventData.responses[EventResponseType.ObserveAndLearn] = new EventResponse
            {
                responseText = "Monitor Current Meta",
                rpCost = 0,
                cpCost = 0,
                communitySentimentChange = -2f,
                specialEffect = "meta_analysis",
                successMessage = "Deep meta analysis reveals hidden strategies.",
                failureMessage = "Inaction leads to further meta stagnation."
            };
            
            return eventData;
        }
        
        /// <summary>
        /// Create seasonal event based on current game state
        /// </summary>
        public static EventData CreateSeasonalEvent(int currentWeek)
        {
            // Different events based on game week
            return (currentWeek % 10) switch
            {
                0 or 1 => CreateNewSeasonEvent(),
                4 or 5 => CreateMidSeasonEvent(),
                8 or 9 => CreateSeasonFinaleEvent(),
                _ => CreateRandomSeasonalEvent()
            };
        }
        
        private static EventData CreateNewSeasonEvent()
        {
            var eventData = new EventData
            {
                eventTitle = "New Competitive Season Starting",
                description = "A fresh competitive season is about to begin. This is the perfect time to ensure the game is balanced and ready for serious competition.",
                eventType = EventType.SeasonalEvent,
                urgencyLevel = EventUrgency.Medium,
                responseTimeLimit = 75f,
                estimatedSentimentImpact = 8f,
                expectedImpacts = new List<string>
                {
                    "Increased competitive activity",
                    "Meta refinement opportunity",
                    "Player engagement boost",
                    "Ranking system refresh"
                }
            };
            
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Season Preparation Patch",
                rpCost = 6,
                cpCost = 2,
                characterEffects = new List<CharacterEffect>
                {
                    new CharacterEffect(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, 1f),
                    new CharacterEffect(Characters.CharacterType.Mage, Characters.CharacterStat.WinRate, 1f),
                    new CharacterEffect(Characters.CharacterType.Support, Characters.CharacterStat.WinRate, 1f),
                    new CharacterEffect(Characters.CharacterType.Tank, Characters.CharacterStat.WinRate, 1f)
                },
                communitySentimentChange = 12f,
                specialEffect = "season_boost",
                successMessage = "Season ready! Balanced competition ahead.",
                failureMessage = "Last-minute changes caused confusion before season start."
            };
            
            return eventData;
        }
        
        private static EventData CreateMidSeasonEvent()
        {
            var eventData = new EventData
            {
                eventTitle = "Mid-Season Fatigue Setting In",
                description = "Players are reporting that the current meta has become predictable and boring. Engagement is declining as people wait for the next major update.",
                eventType = EventType.CommunityEvent,
                urgencyLevel = EventUrgency.Medium,
                responseTimeLimit = 60f,
                estimatedSentimentImpact = -5f,
                expectedImpacts = new List<string>
                {
                    "Player engagement declining",
                    "Calls for meta shake-up",
                    "Content creators seeking fresh material",
                    "Risk of player migration to competitors"
                }
            };
            
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Community Events",
                rpCost = 1,
                cpCost = 5,
                communitySentimentChange = 10f,
                specialEffect = "community_engagement",
                successMessage = "Community events spark renewed interest!",
                failureMessage = "Events seen as distractions from core issues."
            };
            
            return eventData;
        }
        
        private static EventData CreateSeasonFinaleEvent()
        {
            var eventData = new EventData
            {
                eventTitle = "Season Finale Championship",
                description = "The biggest tournament of the season is approaching. This is your chance to showcase the culmination of a season's worth of balance work to the world.",
                eventType = EventType.TournamentEvent,
                urgencyLevel = EventUrgency.High,
                responseTimeLimit = 30f,
                estimatedSentimentImpact = 15f,
                expectedImpacts = new List<string>
                {
                    "Global audience watching",
                    "Meta showcase opportunity",
                    "Potential for viral moments",
                    "Season-defining matches"
                }
            };
            
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Championship Polish",
                rpCost = 4,
                cpCost = 4,
                communitySentimentChange = 20f,
                specialEffect = "championship_ready",
                successMessage = "Championship showcases perfect balance! Global acclaim achieved.",
                failureMessage = "Last-minute changes disrupted championship preparations."
            };
            
            return eventData;
        }
        
        private static EventData CreateRandomSeasonalEvent()
        {
            var randomEvents = new[]
            {
                CreateCommunityFeedbackCrisis(),
                CreateMetaShiftOpportunity(),
                CreateTournamentOpportunity()
            };
            
            return randomEvents[UnityEngine.Random.Range(0, randomEvents.Length)];
        }
        
        /// <summary>
        /// Create a completely random event for testing
        /// </summary>
        public static EventData CreateRandomEvent()
        {
            var eventTypes = System.Enum.GetValues(typeof(EventType));
            var urgencyLevels = System.Enum.GetValues(typeof(EventUrgency));
            
            var randomType = (EventType)eventTypes.GetValue(UnityEngine.Random.Range(0, eventTypes.Length));
            var randomUrgency = (EventUrgency)urgencyLevels.GetValue(UnityEngine.Random.Range(0, urgencyLevels.Length));
            
            return randomType switch
            {
                EventType.Crisis => CreateSupportExploitCrisis(),
                EventType.Opportunity => CreateTournamentOpportunity(),
                EventType.CommunityEvent => CreateCommunityFeedbackCrisis(),
                EventType.MetaShift => CreateMetaShiftOpportunity(),
                EventType.TournamentEvent => CreateTournamentOpportunity(),
                _ => CreateSupportExploitCrisis()
            };
        }
        
        /// <summary>
        /// Create event based on current game state and community sentiment
        /// </summary>
        public static EventData CreateContextualEvent(float communitySentiment, List<Community.BalanceChange> recentChanges)
        {
            // Crisis events when sentiment is low
            if (communitySentiment < 40f)
            {
                return UnityEngine.Random.Range(0f, 1f) < 0.7f ? 
                       CreateCommunityFeedbackCrisis() : 
                       CreateSupportExploitCrisis();
            }
            
            // Opportunity events when sentiment is high
            if (communitySentiment > 70f)
            {
                return UnityEngine.Random.Range(0f, 1f) < 0.6f ? 
                       CreateTournamentOpportunity() : 
                       CreateMetaShiftOpportunity();
            }
            
            // Balanced mix for moderate sentiment
            return CreateRandomEvent();
        }
    }
    
    /// <summary>
    /// Extension methods for EventData
    /// </summary>
    public static class EventDataExtensions
    {
        public static bool IsExpired(this EventData eventData)
        {
            return (DateTime.Now - eventData.eventStartTime).TotalSeconds > eventData.responseTimeLimit;
        }
        
        public static float GetTimeRemaining(this EventData eventData)
        {
            var elapsed = (DateTime.Now - eventData.eventStartTime).TotalSeconds;
            return Mathf.Max(0f, eventData.responseTimeLimit - (float)elapsed);
        }
        
        public static EventResponse GetCheapestResponse(this EventData eventData)
        {
            EventResponse cheapest = null;
            int lowestCost = int.MaxValue;
            
            foreach (var response in eventData.responses.Values)
            {
                int totalCost = response.rpCost + response.cpCost;
                if (totalCost < lowestCost)
                {
                    lowestCost = totalCost;
                    cheapest = response;
                }
            }
            
            return cheapest;
        }
        
        public static List<EventResponseType> GetAffordableResponses(this EventData eventData, int availableRP, int availableCP)
        {
            var affordable = new List<EventResponseType>();
            
            foreach (var kvp in eventData.responses)
            {
                var response = kvp.Value;
                if (response.rpCost <= availableRP && response.cpCost <= availableCP)
                {
                    affordable.Add(kvp.Key);
                }
            }
            
            return affordable;
        }
    }