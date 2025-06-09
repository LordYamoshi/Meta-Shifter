// Assets/Scripts/Managers/EventManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }
        
        [Header("Event Configuration")]
        [SerializeField] private List<GameEvent> availableEvents = new List<GameEvent>();
        [SerializeField] private int maxActiveEvents = 3;
        [SerializeField] private float eventTriggerChance = 0.3f;
        
        [Header("Events")]
        public UnityEvent<GameEvent> OnEventTriggered;
        public UnityEvent<GameEvent> OnEventResolved;
        public UnityEvent<GameEvent, EventResponse> OnEventResponseChosen;
        public UnityEvent<List<GameEvent>> OnActiveEventsChanged;
        
        private List<GameEvent> activeEvents = new List<GameEvent>();
        private Dictionary<string, GameEvent> eventDatabase = new Dictionary<string, GameEvent>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeEventDatabase();
        }
        
        private void Start()
        {
            SubscribeToGameEvents();
            CreateDefaultEvents();
        }
        
        private void InitializeEventDatabase()
        {
            foreach (var gameEvent in availableEvents)
            {
                if (!string.IsNullOrEmpty(gameEvent.eventId))
                {
                    eventDatabase[gameEvent.eventId] = gameEvent;
                }
            }
        }
        
        private void SubscribeToGameEvents()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnBalanceChanged);
            }
            
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(OnSentimentChanged);
            }
        }
        
        private void CreateDefaultEvents()
        {
            var defaultEvents = new List<GameEvent>
            {
                CreateCrisisEvent(),
                CreateOpportunityEvent(),
                CreateCommunityEvent(),
                CreateCompetitiveEvent(),
                CreateTechnicalEvent()
            };
            
            foreach (var evt in defaultEvents)
            {
                eventDatabase[evt.eventId] = evt;
                availableEvents.Add(evt);
            }
        }
        
        private GameEvent CreateCrisisEvent()
        {
            var crisisEvent = new GameEvent
            {
                eventId = "crisis_exploit_discovered",
                eventTitle = "Game-Breaking Exploit Discovered",
                eventDescription = "Players have found a way to stack Support abilities that makes them nearly invincible. The community is in uproar and demanding immediate action.",
                eventType = EventType.Crisis,
                severity = EventSeverity.Critical,
                isUrgent = true,
                duration = 2f,
                turnsRemaining = 2,
                eventColor = new Color(0.8f, 0.1f, 0.1f),
                affectedCharacters = { "Support" }
            };
            
            crisisEvent.availableResponses = new List<EventResponse>
            {
                new EventResponse
                {
                    responseId = "emergency_fix",
                    buttonText = "Emergency Hotfix",
                    responseDescription = "Deploy immediate fix to prevent exploit abuse",
                    responseType = ResponseType.Emergency,
                    rpCost = 8,
                    cpCost = 2,
                    requiredCardTypes = { Cards.CardType.CrisisResponse },
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 15f,
                        successChance = 0.9f,
                        successMessage = "Exploit fixed! Community appreciates quick response.",
                        failureMessage = "Fix didn't work completely. Some issues remain."
                    }
                },
                new EventResponse
                {
                    responseId = "comprehensive_fix", 
                    buttonText = "Comprehensive Solution",
                    responseDescription = "Take time to properly fix the root cause",
                    responseType = ResponseType.Strategic,
                    rpCost = 15,
                    cpCost = 5,
                    requiredCardTypes = { Cards.CardType.BalanceChange, Cards.CardType.CrisisResponse },
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 25f,
                        successChance = 0.95f,
                        successMessage = "Perfect fix! No more exploits and community is thrilled.",
                        failureMessage = "Fix mostly works but took too long."
                    }
                },
                new EventResponse
                {
                    responseId = "ignore_crisis",
                    buttonText = "Do Nothing",
                    responseDescription = "Hope the problem resolves itself",
                    responseType = ResponseType.Ignore,
                    rpCost = 0,
                    cpCost = 0,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = -30f,
                        successChance = 0.1f,
                        successMessage = "Somehow the community forgot about it.",
                        failureMessage = "Community outrage continues to grow."
                    }
                }
            };
            
            return crisisEvent;
        }
        
        private GameEvent CreateOpportunityEvent()
        {
            var opportunityEvent = new GameEvent
            {
                eventId = "tournament_announced",
                eventTitle = "Major Tournament Announced",
                eventDescription = "A large esports organization just announced a major tournament for your game. This is a chance to boost competitive engagement.",
                eventType = EventType.Opportunity,
                severity = EventSeverity.Medium,
                duration = 3f,
                turnsRemaining = 3,
                eventColor = new Color(0.2f, 0.8f, 0.2f)
            };
            
            opportunityEvent.availableResponses = new List<EventResponse>
            {
                new EventResponse
                {
                    responseId = "sponsor_tournament",
                    buttonText = "Official Sponsorship",
                    responseDescription = "Provide official support and prize pool",
                    responseType = ResponseType.Strategic,
                    rpCost = 5,
                    cpCost = 10,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 20f,
                        rpReward = 2,
                        successMessage = "Tournament is a huge success! Great publicity.",
                        successChance = 0.8f
                    }
                },
                new EventResponse
                {
                    responseId = "balance_for_tournament",
                    buttonText = "Tournament Balance Patch",
                    responseDescription = "Create special balance for competitive play",
                    responseType = ResponseType.Technical,
                    rpCost = 12,
                    cpCost = 3,
                    requiredCardTypes = { Cards.CardType.MetaShift },
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 15f,
                        successMessage = "Perfect competitive balance achieved!",
                        successChance = 0.7f
                    }
                }
            };
            
            return opportunityEvent;
        }
        
        private GameEvent CreateCommunityEvent()
        {
            var communityEvent = new GameEvent
            {
                eventId = "viral_meme_trend",
                eventTitle = "Viral Meme About Balance",
                eventDescription = "A meme about game balance has gone viral on social media. Community engagement is through the roof!",
                eventType = EventType.Community,
                severity = EventSeverity.Low,
                duration = 2f,
                turnsRemaining = 2,
                eventColor = new Color(0.8f, 0.2f, 0.8f)
            };
            
            communityEvent.availableResponses = new List<EventResponse>
            {
                new EventResponse
                {
                    responseId = "embrace_meme",
                    buttonText = "Embrace the Meme",
                    responseDescription = "Post official response embracing the community humor",
                    responseType = ResponseType.Community,
                    cpCost = 3,
                    requiredCardTypes = { Cards.CardType.Community },
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 10f,
                        cpReward = 5,
                        successMessage = "Community loves your sense of humor!",
                        successChance = 0.85f
                    }
                },
                new EventResponse
                {
                    responseId = "professional_response",
                    buttonText = "Professional Statement", 
                    responseDescription = "Issue formal statement about balance philosophy",
                    responseType = ResponseType.Community,
                    cpCost = 5,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 5f,
                        successMessage = "Maintains professional image.",
                        successChance = 0.9f
                    }
                }
            };
            
            return communityEvent;
        }
        
        private GameEvent CreateCompetitiveEvent()
        {
            var competitiveEvent = new GameEvent
            {
                eventId = "pro_player_complaint",
                eventTitle = "Pro Player Criticism",
                eventDescription = "A popular professional player has publicly criticized recent balance changes on stream.",
                eventType = EventType.Competitive,
                severity = EventSeverity.High,
                duration = 2f,
                turnsRemaining = 2,
                eventColor = new Color(1f, 0.8f, 0.2f)
            };
            
            competitiveEvent.availableResponses = new List<EventResponse>
            {
                new EventResponse
                {
                    responseId = "direct_communication",
                    buttonText = "Direct Discussion",
                    responseDescription = "Reach out directly to address concerns",
                    responseType = ResponseType.Community,
                    cpCost = 8,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 12f,
                        successMessage = "Pro player appreciates direct communication.",
                        successChance = 0.75f
                    }
                },
                new EventResponse
                {
                    responseId = "balance_adjustment",
                    buttonText = "Address Concerns",
                    responseDescription = "Make targeted changes based on feedback",
                    responseType = ResponseType.Strategic,
                    rpCost = 10,
                    cpCost = 2,
                    requiredCardTypes = { Cards.CardType.BalanceChange },
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 18f,
                        successMessage = "Changes address pro concerns perfectly!",
                        successChance = 0.8f
                    }
                }
            };
            
            return competitiveEvent;
        }
        
        private GameEvent CreateTechnicalEvent()
        {
            var technicalEvent = new GameEvent
            {
                eventId = "server_maintenance",
                eventTitle = "Scheduled Maintenance",
                eventDescription = "Servers need maintenance to improve performance and stability.",
                eventType = EventType.Technical,
                severity = EventSeverity.Low,
                duration = 1f,
                turnsRemaining = 1,
                eventColor = new Color(0.4f, 0.6f, 0.8f)
            };
            
            technicalEvent.availableResponses = new List<EventResponse>
            {
                new EventResponse
                {
                    responseId = "announce_maintenance",
                    buttonText = "Announce Maintenance",
                    responseDescription = "Properly communicate maintenance schedule",
                    responseType = ResponseType.Community,
                    cpCost = 2,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 5f,
                        successMessage = "Community appreciates clear communication.",
                        successChance = 0.95f
                    }
                },
                new EventResponse
                {
                    responseId = "extend_maintenance", 
                    buttonText = "Extended Maintenance",
                    responseDescription = "Use extra time to implement improvements",
                    responseType = ResponseType.Technical,
                    rpCost = 3,
                    cpCost = 1,
                    effects = new EventResponseEffects
                    {
                        satisfactionChange = 8f,
                        successMessage = "Major improvements implemented!",
                        successChance = 0.7f
                    }
                }
            };
            
            return technicalEvent;
        }
        
        public void TriggerRandomEvent()
        {
            if (activeEvents.Count >= maxActiveEvents) return;
            if (Random.Range(0f, 1f) > eventTriggerChance) return;
            
            var availableEventPool = availableEvents.Where(e => !activeEvents.Contains(e) && 
                CanTriggerEvent(e)).ToList();
            
            if (availableEventPool.Count == 0) return;
            
            var selectedEvent = availableEventPool[Random.Range(0, availableEventPool.Count)];
            TriggerEvent(selectedEvent);
        }
        
        public void TriggerEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;
            
            // Create a copy to avoid modifying the original
            var eventCopy = JsonUtility.FromJson<GameEvent>(JsonUtility.ToJson(gameEvent));
            eventCopy.timestamp = System.DateTime.Now;
            eventCopy.isActive = true;
            eventCopy.turnsRemaining = Mathf.RoundToInt(eventCopy.duration);
            
            activeEvents.Add(eventCopy);
            OnEventTriggered.Invoke(eventCopy);
            OnActiveEventsChanged.Invoke(new List<GameEvent>(activeEvents));
            
            Debug.Log($"🎯 Event triggered: {eventCopy.eventTitle}");
        }
        
        public void RespondToEvent(GameEvent gameEvent, EventResponse response)
        {
            if (!activeEvents.Contains(gameEvent)) return;
            if (!response.IsCurrentlyAvailable()) return;
            
            // Spend resources
            if (response.rpCost > 0 || response.cpCost > 0)
            {
                var resourceManager = Core.ResourceManager.Instance;
                if (resourceManager != null)
                {
                    resourceManager.SpendResources(response.rpCost, response.cpCost);
                }
            }
            
            // Apply effects
            ApplyResponseEffects(gameEvent, response);
            
            // Remove event
            activeEvents.Remove(gameEvent);
            gameEvent.isActive = false;
            
            OnEventResponseChosen.Invoke(gameEvent, response);
            OnEventResolved.Invoke(gameEvent);
            OnActiveEventsChanged.Invoke(new List<GameEvent>(activeEvents));
            
            Debug.Log($"✅ Event resolved: {gameEvent.eventTitle} with response: {response.buttonText}");
        }
        
        private void ApplyResponseEffects(GameEvent gameEvent, EventResponse response)
        {
            var effects = response.effects;
            bool success = Random.Range(0f, 1f) < effects.successChance;
            
            // Apply satisfaction change
            if (effects.satisfactionChange != 0f)
            {
                var characterManager = Characters.CharacterManager.Instance;
                if (characterManager != null)
                {
                    // This would need to be implemented in CharacterManager
                    Debug.Log($"Satisfaction change: {effects.satisfactionChange}");
                }
            }
            
            // Apply resource rewards
            if (effects.rpReward > 0 || effects.cpReward > 0)
            {
                var resourceManager = Core.ResourceManager.Instance;
                if (resourceManager != null)
                {
                    resourceManager.AddResources(effects.rpReward, effects.cpReward);
                }
            }
            
            // Apply character effects
            foreach (var charEffect in effects.characterEffects)
            {
                var characterManager = Characters.CharacterManager.Instance;
                if (characterManager != null)
                {
                    characterManager.ModifyStat(charEffect.character, charEffect.stat, charEffect.change);
                }
            }
            
            // Show result message
            string resultMessage = success ? effects.successMessage : effects.failureMessage;
            Debug.Log($"📋 Event result: {resultMessage}");
        }
        
        private bool CanTriggerEvent(GameEvent gameEvent)
        {
            // Check if event requirements are met
            var phaseManager = Core.PhaseManager.Instance;
            if (phaseManager != null && gameEvent.triggerWeek > 0)
            {
                if (phaseManager.GetCurrentWeek() < gameEvent.triggerWeek) return false;
            }
            
            // Add more conditions as needed
            return true;
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Event)
            {
                TriggerRandomEvent();
                UpdateEventTimers();
            }
        }
        
        private void OnWeekChanged(int newWeek)
        {
            UpdateEventTimers();
            TriggerRandomEvent();
        }
        
        private void UpdateEventTimers()
        {
            var expiredEvents = new List<GameEvent>();
            
            foreach (var evt in activeEvents)
            {
                evt.turnsRemaining--;
                if (evt.IsExpired())
                {
                    expiredEvents.Add(evt);
                }
            }
            
            foreach (var expired in expiredEvents)
            {
                activeEvents.Remove(expired);
                OnEventResolved.Invoke(expired);
            }
            
            if (expiredEvents.Count > 0)
            {
                OnActiveEventsChanged.Invoke(new List<GameEvent>(activeEvents));
            }
        }
        
        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            // Trigger events based on stat changes
            if (stat == Characters.CharacterStat.WinRate)
            {
                if (newValue > 60f || newValue < 40f)
                {
                    // Trigger balance concern event
                    var balanceEvent = CreateBalanceConcernEvent(character, newValue);
                    if (Random.Range(0f, 1f) < 0.4f) // 40% chance
                    {
                        TriggerEvent(balanceEvent);
                    }
                }
            }
        }
        
        private void OnBalanceChanged(float balance)
        {
            if (balance < 30f)
            {
                // Trigger emergency balance meeting
                if (Random.Range(0f, 1f) < 0.6f)
                {
                    TriggerEvent(CreateEmergencyMeetingEvent());
                }
            }
        }
        
        private void OnSentimentChanged(float sentiment)
        {
            if (sentiment < 25f)
            {
                // Trigger community outrage event
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    TriggerEvent(CreateCommunityOutrageEvent());
                }
            }
        }
        
        private GameEvent CreateBalanceConcernEvent(Characters.CharacterType character, float winRate)
        {
            string issue = winRate > 60f ? "overpowered" : "underpowered";
            
            return new GameEvent
            {
                eventId = $"balance_concern_{character}_{System.DateTime.Now.Ticks}",
                eventTitle = $"{character} Balance Concerns",
                eventDescription = $"Community is concerned that {character} is {issue} with a {winRate:F1}% win rate.",
                eventType = EventType.Community,
                severity = winRate > 65f || winRate < 35f ? EventSeverity.High : EventSeverity.Medium,
                duration = 2f,
                turnsRemaining = 2,
                affectedCharacters = { character.ToString() },
                eventColor = winRate > 60f ? new Color(1f, 0.4f, 0.1f) : new Color(0.2f, 0.4f, 1f)
            };
        }
        
        private GameEvent CreateEmergencyMeetingEvent()
        {
            return new GameEvent
            {
                eventId = $"emergency_meeting_{System.DateTime.Now.Ticks}",
                eventTitle = "Emergency Balance Meeting",
                eventDescription = "The balance team has called an emergency meeting due to concerning game state.",
                eventType = EventType.Crisis,
                severity = EventSeverity.Critical,
                duration = 1f,
                turnsRemaining = 1,
                isUrgent = true,
                eventColor = new Color(0.8f, 0.1f, 0.1f)
            };
        }
        
        private GameEvent CreateCommunityOutrageEvent()
        {
            return new GameEvent
            {
                eventId = $"community_outrage_{System.DateTime.Now.Ticks}",
                eventTitle = "Community Outrage",
                eventDescription = "The community is extremely upset with the current state of the game.",
                eventType = EventType.Community,
                severity = EventSeverity.High,
                duration = 3f,
                turnsRemaining = 3,
                eventColor = new Color(0.8f, 0.1f, 0.1f)
            };
        }
        
        // Public API
        public List<GameEvent> GetActiveEvents() => new List<GameEvent>(activeEvents);
        public GameEvent GetEventById(string eventId) => eventDatabase.GetValueOrDefault(eventId);
        public void RegisterEvent(GameEvent gameEvent) 
        {
            if (!string.IsNullOrEmpty(gameEvent.eventId))
            {
                eventDatabase[gameEvent.eventId] = gameEvent;
                if (!availableEvents.Contains(gameEvent))
                {
                    availableEvents.Add(gameEvent);
                }
            }
        }
        
        // Debug methods
        [ContextMenu("🎯 Trigger Random Event")]
        public void DebugTriggerRandomEvent()
        {
            TriggerRandomEvent();
        }
        
        [ContextMenu("🚨 Trigger Crisis Event")]
        public void DebugTriggerCrisisEvent()
        {
            TriggerEvent(CreateCrisisEvent());
        }
        
        [ContextMenu("📊 Show Active Events")]
        public void DebugShowActiveEvents()
        {
            Debug.Log($"=== Active Events ({activeEvents.Count}) ===");
            foreach (var evt in activeEvents)
            {
                Debug.Log($"{evt.eventTitle} - {evt.severity} - {evt.GetTimeRemaining()}");
            }
        }
    }
}