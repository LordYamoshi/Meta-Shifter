using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    /// <summary>
    /// Turn-Based Event Manager - Replaces your existing EventManager
    /// Automatically generates events during Event Phase based on turns/weeks
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Event Spawning")]
        [SerializeField] private UI.EventUIManager eventUIManager;
        [SerializeField] private int maxSimultaneousEvents = 3;

        [Header("Turn-Based Generation")]
        [SerializeField] private int minEventsPerEventPhase = 1;
        [SerializeField] private int maxEventsPerEventPhase = 2;
        [SerializeField] private bool guaranteeEventEachEventPhase = true;
        
        [Header("Event Type Probabilities")]
        [Range(0f, 1f)] [SerializeField] private float crisisEventChance = 0.25f;
        [Range(0f, 1f)] [SerializeField] private float opportunityEventChance = 0.25f;
        [Range(0f, 1f)] [SerializeField] private float communityEventChance = 0.25f;
        [Range(0f, 1f)] [SerializeField] private float technicalEventChance = 0.15f;
        [Range(0f, 1f)] [SerializeField] private float competitiveEventChance = 0.10f;

        [Header("Event Triggers")]
        [SerializeField] private bool triggerOnLowSentiment = true;
        [SerializeField] private bool triggerOnHighSentiment = true;
        [SerializeField] private bool triggerOnMajorChanges = true;

        [Header("Trigger Thresholds")]
        [Range(0f, 50f)] [SerializeField] private float lowSentimentThreshold = 30f;
        [Range(50f, 100f)] [SerializeField] private float highSentimentThreshold = 75f;
        [Range(5f, 30f)] [SerializeField] private float majorChangeThreshold = 15f;

        [Header("Events")]
        public UnityEvent<EventData> OnEventTriggered;
        public UnityEvent<EventData, EventResponseType> OnEventResolved;
        public UnityEvent<EventData> OnEventExpired;
        public UnityEvent<string> OnEventSystemMessage;

        // Turn/Phase tracking
        private int currentWeek = 1;
        private int eventsGeneratedThisEventPhase = 0;
        private bool isEventPhase = false;
        private bool hasGeneratedEventsThisPhase = false;

        // Active event tracking
        private List<EventData> activeEvents = new List<EventData>();
        private Queue<EventData> eventQueue = new Queue<EventData>();

        // Event generation state
        private float lastCommunitySentiment = 50f;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitializeEventSystem();
            SubscribeToPhaseManager();
            Debug.Log("🎭 Turn-Based Event Manager initialized and ready!");
        }

        private void OnDestroy()
        {
            // Unsubscribe from phase manager
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.RemoveListener(OnWeekChanged);
            }
        }

        private void Update()
        {
            ProcessEventQueue();
            UpdateActiveEvents();
        }

        #endregion

        #region Initialization

        private void InitializeEventSystem()
        {
            // Auto-find EventUIManager if not set
            if (eventUIManager == null)
            {
                eventUIManager = FindObjectOfType<UI.EventUIManager>();
                if (eventUIManager == null)
                {
                    Debug.LogWarning("⚠️ EventUIManager not found. Events will not display properly.");
                }
                else
                {
                    Debug.Log("✅ EventUIManager auto-found and connected.");
                }
            }
        }

        private void SubscribeToPhaseManager()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
                Debug.Log("📅 EventManager subscribed to PhaseManager events");
                
                // Check current state
                CheckCurrentPhase();
            }
            else
            {
                Debug.LogWarning("⚠️ PhaseManager.Instance not found - event generation will not be phase-aware");
            }
        }

        private void CheckCurrentPhase()
        {
            if (Core.PhaseManager.Instance != null)
            {
                var currentPhase = Core.PhaseManager.Instance.GetCurrentPhase();
                currentWeek = Core.PhaseManager.Instance.GetCurrentWeek();
                OnPhaseChanged(currentPhase);
            }
        }

        #endregion

        #region Phase Management

        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            bool wasEventPhase = isEventPhase;
            isEventPhase = (newPhase == Core.GamePhase.Event);

            Debug.Log($"🎭 EventManager phase changed: {newPhase} - Event phase: {isEventPhase}");

            if (isEventPhase && !wasEventPhase)
            {
                // Entering Event Phase - Generate events automatically
                OnEnterEventPhase();
            }
            else if (!isEventPhase && wasEventPhase)
            {
                // Leaving Event Phase
                OnLeaveEventPhase();
            }
        }

        private void OnWeekChanged(int newWeek)
        {
            currentWeek = newWeek;
            Debug.Log($"📅 Week changed to: {currentWeek}");
            
            // Reset weekly event generation tracking
            eventsGeneratedThisEventPhase = 0;
            hasGeneratedEventsThisPhase = false;
        }

        private void OnEnterEventPhase()
        {
            Debug.Log($"🎬 EventManager entering Event Phase - Week {currentWeek}");
            
            // Reset event generation tracking
            eventsGeneratedThisEventPhase = 0;
            hasGeneratedEventsThisPhase = false;
            
            // Automatically generate events for this Event Phase
            GenerateEventsForEventPhase();
        }

        private void OnLeaveEventPhase()
        {
            Debug.Log($"📚 EventManager leaving Event Phase - Generated {eventsGeneratedThisEventPhase} events this phase");
        }

        #endregion

        #region Turn-Based Event Generation

        private void GenerateEventsForEventPhase()
        {
            Debug.Log($"🎲 Auto-generating events for Event Phase - Week {currentWeek}");

            // Determine how many events to generate
            int eventsToGenerate = DetermineEventCount();
            
            Debug.Log($"📊 Will generate {eventsToGenerate} events this Event Phase");

            // Generate the events
            for (int i = 0; i < eventsToGenerate; i++)
            {
                var eventData = GenerateRandomEvent();
                if (eventData != null)
                {
                    TriggerEvent(eventData);
                    eventsGeneratedThisEventPhase++;
                    Debug.Log($"✅ Generated event {i + 1}/{eventsToGenerate}: {eventData.title}");
                }
            }

            hasGeneratedEventsThisPhase = true;
            
            // Check for special triggered events based on game state
            CheckForTriggeredEvents();
        }

        private int DetermineEventCount()
        {
            if (guaranteeEventEachEventPhase)
            {
                // Always generate at least the minimum
                return Random.Range(minEventsPerEventPhase, maxEventsPerEventPhase + 1);
            }
            else
            {
                // Could generate zero events
                return Random.Range(0, maxEventsPerEventPhase + 1);
            }
        }

        private EventData GenerateRandomEvent()
        {
            // Determine event type based on weights
            EventType eventType = DetermineEventType();
            
            Debug.Log($"🎯 Generating {eventType} event");

            // Create event based on type using EventFactory
            EventData eventData = eventType switch
            {
                EventType.Crisis => CreateCrisisEvent(),
                EventType.Opportunity => CreateOpportunityEvent(),
                EventType.Community => CreateCommunityEvent(),
                EventType.Technical => CreateTechnicalEvent(),
                EventType.Competitive => CreateCompetitiveEvent(),
                _ => CreateCommunityEvent()
            };

            return eventData;
        }

        private EventType DetermineEventType()
        {
            float roll = Random.Range(0f, 1f);
            float cumulative = 0f;

            cumulative += crisisEventChance;
            if (roll <= cumulative) return EventType.Crisis;

            cumulative += opportunityEventChance;
            if (roll <= cumulative) return EventType.Opportunity;

            cumulative += communityEventChance;
            if (roll <= cumulative) return EventType.Community;

            cumulative += technicalEventChance;
            if (roll <= cumulative) return EventType.Technical;

            cumulative += competitiveEventChance;
            if (roll <= cumulative) return EventType.Competitive;

            // Default fallback
            return EventType.Community;
        }

        private void CheckForTriggeredEvents()
        {
            // Check for sentiment-based events
            if (triggerOnLowSentiment && lastCommunitySentiment < lowSentimentThreshold)
            {
                Debug.Log($"🚨 Low sentiment detected ({lastCommunitySentiment:F1}) - triggering additional crisis event");
                var crisisEvent = CreateCrisisEvent();
                TriggerEvent(crisisEvent);
            }
            else if (triggerOnHighSentiment && lastCommunitySentiment > highSentimentThreshold)
            {
                Debug.Log($"⭐ High sentiment detected ({lastCommunitySentiment:F1}) - triggering additional opportunity event");
                var opportunityEvent = CreateOpportunityEvent();
                TriggerEvent(opportunityEvent);
            }
        }

        #endregion

        #region Event Creation Methods

        private EventData CreateCrisisEvent()
        {
            var crisisEvents = new[]
            {
                ("Character Exploit Discovered", "Players found a way to infinitely stack damage. Community demands immediate fix.", EventSeverity.Critical),
                ("Server Stability Issues", "Frequent disconnections during ranked matches. Competitive integrity at risk.", EventSeverity.High),
                ("Balance Complaint Surge", "Multiple characters reported as overpowered. Community sentiment dropping.", EventSeverity.Medium),
                ("Tournament Bug", "Critical bug discovered just before major tournament. Quick decision needed.", EventSeverity.Critical)
            };

            var (title, description, severity) = crisisEvents[Random.Range(0, crisisEvents.Length)];
            
            var eventData = new EventData(title, description, EventType.Crisis, severity)
            {
                timeRemaining = severity == EventSeverity.Critical ? 60f : 90f, // 2-3 turns
                expectedImpact = severity == EventSeverity.Critical ? 9f : 6f,
                expectedImpacts = new List<string> { "Community backlash risk", "Balance disruption", "Player satisfaction impact" }
            };

            // Add response options
            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Emergency Fix",
                    description = "Deploy immediate hotfix to address the issue",
                    responseType = EventResponseType.EmergencyFix,
                    rpCost = 3,
                    cpCost = 0,
                    sentimentChange = 12f,
                    successMessage = "Crisis resolved with quick action!",
                    buttonColor = new Color(0.7f, 0.3f, 0.9f)
                },
                new EventResponseOption
                {
                    buttonText = "Investigate Further",
                    description = "Gather more data before taking drastic action",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 1,
                    cpCost = 1,
                    sentimentChange = -2f,
                    successMessage = "Investigation reveals key insights.",
                    buttonColor = new Color(0.6f, 0.6f, 0.3f)
                }
            };

            return eventData;
        }

        private EventData CreateOpportunityEvent()
        {
            var opportunityEvents = new[]
            {
                ("Viral Gameplay Moment", "A streamer showcased an amazing play. Community excitement is high!", EventSeverity.Medium),
                ("Tournament Success", "Recent tournament had record viewership. Perfect time to capitalize.", EventSeverity.Medium),
                ("Community Creation", "Players created amazing fan content. Great opportunity for engagement.", EventSeverity.Low),
                ("Pro Player Endorsement", "Top players praise recent balance changes. Momentum is building.", EventSeverity.Medium)
            };

            var (title, description, severity) = opportunityEvents[Random.Range(0, opportunityEvents.Length)];
            
            var eventData = new EventData(title, description, EventType.Opportunity, severity)
            {
                timeRemaining = 120f, // 4 turns
                expectedImpact = 5f,
                expectedImpacts = new List<string> { "Positive community impact", "Engagement boost", "PR opportunity" }
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Seize Opportunity",
                    description = "Take advantage of this positive momentum",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 1,
                    cpCost = 2,
                    sentimentChange = 10f,
                    successMessage = "Opportunity successfully leveraged!",
                    buttonColor = new Color(0.9f, 0.6f, 0.2f)
                },
                new EventResponseOption
                {
                    buttonText = "Monitor Situation",
                    description = "Observe the situation before committing resources",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = 2f,
                    successMessage = "Situation monitored for optimal timing.",
                    buttonColor = new Color(0.4f, 0.4f, 0.4f)
                }
            };

            return eventData;
        }

        private EventData CreateCommunityEvent()
        {
            var communityEvents = new[]
            {
                ("Feedback Surge", "Community providing lots of feedback about recent changes.", EventSeverity.Medium),
                ("Reddit Discussion", "Major discussion thread gaining traction about game balance.", EventSeverity.Medium),
                ("Content Creator Review", "Popular content creator reviewing recent updates.", EventSeverity.Medium),
                ("Community Poll Results", "Recent poll results show mixed reactions to changes.", EventSeverity.Low)
            };

            var (title, description, severity) = communityEvents[Random.Range(0, communityEvents.Length)];
            
            var eventData = new EventData(title, description, EventType.Community, severity)
            {
                timeRemaining = 90f, // 3 turns
                expectedImpact = 4f,
                expectedImpacts = new List<string> { "Community engagement", "Player sentiment shift", "Feedback quality" }
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Engage Community",
                    description = "Actively participate in community discussions",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 0,
                    cpCost = 2,
                    sentimentChange = 8f,
                    successMessage = "Community engagement successful!",
                    buttonColor = new Color(0.2f, 0.6f, 0.9f)
                },
                new EventResponseOption
                {
                    buttonText = "Observe & Learn",
                    description = "Monitor community feedback without direct intervention",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = 2f,
                    successMessage = "Valuable community insights gathered.",
                    buttonColor = new Color(0.4f, 0.4f, 0.4f)
                }
            };

            return eventData;
        }

        private EventData CreateTechnicalEvent()
        {
            var technicalEvents = new[]
            {
                ("Server Performance Issues", "Some players experiencing lag during peak hours.", EventSeverity.High),
                ("Matchmaking Irregularities", "Unusual patterns detected in matchmaking algorithm.", EventSeverity.Medium),
                ("Data Collection Error", "Inconsistencies found in player statistics tracking.", EventSeverity.Medium),
                ("Client Crash Reports", "Increased reports of game client crashes.", EventSeverity.High)
            };

            var (title, description, severity) = technicalEvents[Random.Range(0, technicalEvents.Length)];
            
            var eventData = new EventData(title, description, EventType.Technical, severity)
            {
                timeRemaining = 120f, // 4 turns
                expectedImpact = 6f,
                expectedImpacts = new List<string> { "Player experience", "Data integrity", "System stability" }
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Technical Fix",
                    description = "Implement immediate technical solution",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 2,
                    cpCost = 0,
                    sentimentChange = 6f,
                    successMessage = "Technical issue resolved successfully!",
                    buttonColor = new Color(0.3f, 0.8f, 0.3f)
                },
                new EventResponseOption
                {
                    buttonText = "Schedule Maintenance",
                    description = "Plan proper maintenance window for thorough fix",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 1,
                    sentimentChange = 3f,
                    successMessage = "Maintenance scheduled for optimal resolution.",
                    buttonColor = new Color(0.5f, 0.5f, 0.8f)
                }
            };

            return eventData;
        }

        private EventData CreateCompetitiveEvent()
        {
            var competitiveEvents = new[]
            {
                ("Meta Shift Analysis", "Pro players adapting to recent changes in unexpected ways.", EventSeverity.Medium),
                ("Championship Preparation", "Major tournament approaching - teams adjusting strategies.", EventSeverity.Medium),
                ("Tier List Controversy", "Community debating character tier rankings after recent updates.", EventSeverity.Low),
                ("Pro Player Feedback", "Professional players sharing insights about competitive balance.", EventSeverity.Medium)
            };

            var (title, description, severity) = competitiveEvents[Random.Range(0, competitiveEvents.Length)];
            
            var eventData = new EventData(title, description, EventType.Competitive, severity)
            {
                timeRemaining = 150f, // 5 turns
                expectedImpact = 5f,
                expectedImpacts = new List<string> { "Competitive balance", "Pro scene health", "Tournament impact" }
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Analyze Meta",
                    description = "Deep dive into competitive data and trends",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 2,
                    cpCost = 1,
                    sentimentChange = 5f,
                    successMessage = "Meta analysis reveals valuable insights!",
                    buttonColor = new Color(0.8f, 0.4f, 0.2f)
                },
                new EventResponseOption
                {
                    buttonText = "Consult Pros",
                    description = "Reach out to professional players for direct feedback",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 1,
                    cpCost = 2,
                    sentimentChange = 7f,
                    successMessage = "Pro consultation provides expert perspective.",
                    buttonColor = new Color(0.6f, 0.3f, 0.8f)
                }
            };

            return eventData;
        }

        #endregion

        #region Event Management

        /// <summary>
        /// Trigger a specific event immediately
        /// </summary>
        public void TriggerEvent(EventData eventData)
        {
            if (eventData == null) 
            {
                Debug.LogError("❌ Cannot trigger null event");
                return;
            }

            Debug.Log($"🎬 Triggering event: {eventData.title}");

            // Check if we have room for more events
            if (activeEvents.Count >= maxSimultaneousEvents)
            {
                eventQueue.Enqueue(eventData);
                Debug.Log($"📦 Event queued: {eventData.title} (Queue size: {eventQueue.Count})");
                return;
            }

            // Display the event
            DisplayEvent(eventData);
        }

        private void DisplayEvent(EventData eventData)
        {
            activeEvents.Add(eventData);
            
            // Send to UI Manager (your existing EventUIManager)
            if (eventUIManager != null)
            {
                eventUIManager.CreateEvent(eventData);
                Debug.Log($"📺 Event sent to EventUIManager: {eventData.title}");
            }
            else
            {
                Debug.LogError("❌ EventUIManager is null - cannot display event");
            }

            // Trigger event
            OnEventTriggered?.Invoke(eventData);
        }

        private void ProcessEventQueue()
        {
            // Process queued events if we have space
            while (eventQueue.Count > 0 && activeEvents.Count < maxSimultaneousEvents)
            {
                var queuedEvent = eventQueue.Dequeue();
                DisplayEvent(queuedEvent);
                Debug.Log($"📤 Processed queued event: {queuedEvent.title}");
            }
        }

        private void UpdateActiveEvents()
        {
            // Update timers and remove expired events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                var eventData = activeEvents[i];
                
                if (eventData.timeRemaining > 0)
                {
                    eventData.timeRemaining -= Time.deltaTime;
                    
                    if (eventData.timeRemaining <= 0)
                    {
                        Debug.Log($"⏰ Event expired: {eventData.title}");
                        ExpireEvent(eventData);
                    }
                }
            }
        }

        private void ExpireEvent(EventData eventData)
        {
            activeEvents.Remove(eventData);
            OnEventExpired?.Invoke(eventData);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger event generation (for testing)
        /// </summary>
        [ContextMenu("🎲 Force Generate Events")]
        public void ForceGenerateEvents()
        {
            Debug.Log("🎲 Manually forcing event generation...");
            GenerateEventsForEventPhase();
        }

        /// <summary>
        /// Generate a crisis event (for testing)
        /// </summary>
        public void GenerateCrisisEvent(string title, string description, EventSeverity severity = EventSeverity.High)
        {
            var eventData = CreateCrisisEvent();
            eventData.title = title;
            eventData.description = description;
            eventData.severity = severity;
            TriggerEvent(eventData);
        }

        /// <summary>
        /// Update community sentiment (called by other systems)
        /// </summary>
        public void UpdateCommunitySentiment(float newSentiment)
        {
            float oldSentiment = lastCommunitySentiment;
            lastCommunitySentiment = newSentiment;
            
            Debug.Log($"📊 Community sentiment updated: {oldSentiment:F1} → {newSentiment:F1}");
            
            // Check for major sentiment changes during Event Phase
            if (isEventPhase && Mathf.Abs(newSentiment - oldSentiment) > majorChangeThreshold)
            {
                Debug.Log($"📈 Major sentiment change detected during Event Phase!");
                CheckForTriggeredEvents();
            }
        }

        /// <summary>
        /// Get all active events
        /// </summary>
        public List<EventData> GetActiveEvents()
        {
            return new List<EventData>(activeEvents);
        }

        /// <summary>
        /// Clear all events (called by EventUIManager)
        /// </summary>
        public void ClearAllEvents()
        {
            activeEvents.Clear();
            eventQueue.Clear();
            if (eventUIManager != null)
                eventUIManager.ClearAllEvents();
        }

        #endregion

        #region Debug Methods

        [ContextMenu("🚨 Generate Crisis Event")]
        public void DebugGenerateCrisisEvent()
        {
            var crisisEvent = CreateCrisisEvent();
            TriggerEvent(crisisEvent);
        }

        [ContextMenu("⭐ Generate Opportunity Event")]
        public void DebugGenerateOpportunityEvent()
        {
            var opportunityEvent = CreateOpportunityEvent();
            TriggerEvent(opportunityEvent);
        }

        [ContextMenu("💬 Generate Community Event")]
        public void DebugGenerateCommunityEvent()
        {
            var communityEvent = CreateCommunityEvent();
            TriggerEvent(communityEvent);
        }

        [ContextMenu("📊 Debug Event System State")]
        public void DebugEventSystemState()
        {
            Debug.Log("=== 📊 EVENT SYSTEM DEBUG ===");
            Debug.Log($"Current Week: {currentWeek}");
            Debug.Log($"Is Event Phase: {isEventPhase}");
            Debug.Log($"Events Generated This Phase: {eventsGeneratedThisEventPhase}");
            Debug.Log($"Has Generated Events This Phase: {hasGeneratedEventsThisPhase}");
            Debug.Log($"Active Events: {activeEvents.Count}");
            Debug.Log($"Queued Events: {eventQueue.Count}");
            Debug.Log($"Last Community Sentiment: {lastCommunitySentiment:F1}");
            Debug.Log($"EventUIManager Connected: {eventUIManager != null}");

            if (Core.PhaseManager.Instance != null)
            {
                Debug.Log($"PhaseManager Current Phase: {Core.PhaseManager.Instance.GetCurrentPhase()}");
                Debug.Log($"PhaseManager Current Week: {Core.PhaseManager.Instance.GetCurrentWeek()}");
            }
            else
            {
                Debug.Log("❌ PhaseManager.Instance is NULL!");
            }
        }

        #endregion
    }
}