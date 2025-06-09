using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    /// <summary>
    /// Complete Event Manager System - Integrates with your existing game systems
    /// Uses Observer Pattern to respond to game state changes
    /// Uses Strategy Pattern for different event triggering strategies
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Event Spawning")] [SerializeField]
        private Transform eventUIContainer;

        [SerializeField] private GameObject eventUIItemPrefab;
        [SerializeField] private int maxSimultaneousEvents = 3;

        [Header("Event Timing")] [Range(0f, 1f)] [SerializeField]
        private float randomEventChance = 0.3f;

        [Range(10f, 120f)] [SerializeField] private float minTimeBetweenEvents = 30f;
        [Range(60f, 300f)] [SerializeField] private float maxTimeBetweenEvents = 180f;

        [Header("Event Triggers")] [SerializeField]
        private bool triggerOnLowSentiment = true;

        [SerializeField] private bool triggerOnHighSentiment = true;
        [SerializeField] private bool triggerOnMajorChanges = true;
        [SerializeField] private bool triggerOnSeasonalEvents = true;

        [Header("Trigger Thresholds")] [Range(0f, 50f)] [SerializeField]
        private float lowSentimentThreshold = 30f;

        [Range(50f, 100f)] [SerializeField] private float highSentimentThreshold = 75f;
        [Range(5f, 30f)] [SerializeField] private float majorChangeThreshold = 15f;

        [Header("Events")] public UnityEvent<EventData> OnEventTriggered;
        public UnityEvent<EventData, EventResponseType> OnEventResolved;
        public UnityEvent<EventData> OnEventExpired;
        public UnityEvent<string> OnEventSystemMessage; // For debugging/logging

        // Active event tracking
        private List<EventUIItem> activeEvents = new List<EventUIItem>();
        private Queue<EventData> eventQueue = new Queue<EventData>();
        private float lastEventTime = 0f;
        private float nextRandomEventTime = 0f;

        // System integration
        private Community.CommunityFeedbackManager feedbackManager;
        private Characters.CharacterManager characterManager;
        private Core.ResourceManager resourceManager;
        private Core.PhaseManager phaseManager;

        // Event generation state
        private float lastCommunitySentiment = 50f;
        private List<Community.BalanceChange> recentMajorChanges = new List<Community.BalanceChange>();
        private int currentGameWeek = 1;

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
            SubscribeToGameEvents();
            ScheduleNextRandomEvent();

            Debug.Log("🎭 Event Manager initialized and ready!");
        }

        private void Update()
        {
            UpdateActiveEvents();
            CheckForRandomEvents();
            ProcessEventQueue();
        }

        #region Initialization

        private void InitializeEventSystem()
        {
            // Auto-find UI container if not set
            if (eventUIContainer == null)
            {
                var found = GameObject.Find("EventContainer");
                if (found != null)
                    eventUIContainer = found.transform;
                else
                    eventUIContainer = transform; // Use self as fallback
            }

            // Auto-find prefab if not set
            if (eventUIItemPrefab == null)
            {
                eventUIItemPrefab = Resources.Load<GameObject>("EventUIItem");
                if (eventUIItemPrefab == null)
                {
                    Debug.LogWarning("⚠️ EventUIItem prefab not found. Events will not display properly.");
                }
            }

            OnEventSystemMessage.Invoke("Event system initialized");
        }

        private void SubscribeToGameEvents()
        {
            // Get references to other managers
            feedbackManager = Community.CommunityFeedbackManager.Instance;
            characterManager = Characters.CharacterManager.Instance;
            resourceManager = Core.ResourceManager.Instance;
            phaseManager = Core.PhaseManager.Instance;

            // Subscribe to relevant events
            if (feedbackManager != null)
            {
                feedbackManager.OnCommunitySentimentChanged.AddListener(OnCommunitySentimentChanged);
                Debug.Log("📡 Subscribed to community sentiment changes");
            }

            if (characterManager != null)
            {
                characterManager.OnStatChanged.AddListener(OnCharacterStatChanged);
                Debug.Log("📡 Subscribed to character stat changes");
            }

            if (phaseManager != null)
            {
                phaseManager.OnWeekChanged.AddListener(OnWeekChanged);
                phaseManager.OnPhaseChanged.AddListener(OnPhaseChanged);
                Debug.Log("📡 Subscribed to phase and week changes");
            }
        }

        #endregion

        #region Event Triggering Logic

        private void OnCommunitySentimentChanged(float newSentiment)
        {
            float sentimentChange = Mathf.Abs(newSentiment - lastCommunitySentiment);
            lastCommunitySentiment = newSentiment;

            // Trigger crisis events for low sentiment
            if (triggerOnLowSentiment && newSentiment < lowSentimentThreshold)
            {
                float crisisChance = (lowSentimentThreshold - newSentiment) / lowSentimentThreshold;
                if (Random.Range(0f, 1f) < crisisChance * 0.4f) // Max 40% chance
                {
                    TriggerCommunityMoodEvent(newSentiment);
                }
            }

            // Trigger opportunity events for high sentiment
            if (triggerOnHighSentiment && newSentiment > highSentimentThreshold)
            {
                float opportunityChance = (newSentiment - highSentimentThreshold) / (100f - highSentimentThreshold);
                if (Random.Range(0f, 1f) < opportunityChance * 0.3f) // Max 30% chance
                {
                    TriggerOpportunityEvent(newSentiment);
                }
            }

            OnEventSystemMessage.Invoke($"Sentiment changed to {newSentiment:F1}% (change: {sentimentChange:F1})");
        }

        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat,
            float newValue)
        {
            if (!triggerOnMajorChanges) return;

            // Track major changes
            var previousValue = GetPreviousStatValue(character, stat);
            float changeAmount = Mathf.Abs(newValue - previousValue);

            if (changeAmount >= majorChangeThreshold)
            {
                var majorChange = new Community.BalanceChange(character, stat, previousValue, newValue);
                recentMajorChanges.Add(majorChange);

                // Remove old changes (keep only last 5 minutes of game time)
                recentMajorChanges.RemoveAll(c => Time.time - c.timestamp > 300f);

                // Trigger event if multiple major changes occurred recently
                if (recentMajorChanges.Count >= 3)
                {
                    TriggerMajorChangeEvent();
                }

                OnEventSystemMessage.Invoke($"Major change detected: {character} {stat} changed by {changeAmount:F1}");
            }
        }

        private void OnWeekChanged(int newWeek)
        {
            currentGameWeek = newWeek;

            if (triggerOnSeasonalEvents)
            {
                // Check for seasonal events
                if (ShouldTriggerSeasonalEvent(newWeek))
                {
                    TriggerSeasonalEvent(newWeek);
                }
            }

            OnEventSystemMessage.Invoke($"Week {newWeek} started - checking for seasonal events");
        }

        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            // Events more likely to trigger during feedback phase
            if (newPhase == Core.GamePhase.Event)
            {
                CheckForPhaseBasedEvents();
            }
        }

        #endregion

        #region Event Generation Methods

        private void TriggerCommunityMoodEvent(float sentiment)
        {
            EventData eventData;

            if (sentiment < 25f)
            {
                // Very low sentiment - major crisis
                eventData = EventDataFactory.CreateCommunityFeedbackCrisis();
            }
            else
            {
                // Moderate low sentiment - smaller issue
                eventData = EventDataFactory.CreateSupportExploitCrisis();
            }

            QueueEvent(eventData);
            OnEventSystemMessage.Invoke($"Community mood event triggered (sentiment: {sentiment:F1}%)");
        }

        private void TriggerOpportunityEvent(float sentiment)
        {
            var opportunityEvents = new[]
            {
                EventDataFactory.CreateTournamentOpportunity(),
                EventDataFactory.CreateMetaShiftOpportunity()
            };

            var selectedEvent = opportunityEvents[Random.Range(0, opportunityEvents.Length)];
            QueueEvent(selectedEvent);
            OnEventSystemMessage.Invoke($"Opportunity event triggered (sentiment: {sentiment:F1}%)");
        }

        private void TriggerMajorChangeEvent()
        {
            // Create an event based on the recent major changes
            var eventData = CreateMajorChangeReactionEvent();
            QueueEvent(eventData);

            // Clear the recent changes since we've responded to them
            recentMajorChanges.Clear();
            OnEventSystemMessage.Invoke("Major change reaction event triggered");
        }

        private void TriggerSeasonalEvent(int week)
        {
            var seasonalEvent = EventDataFactory.CreateSeasonalEvent(week);
            QueueEvent(seasonalEvent);
            OnEventSystemMessage.Invoke($"Seasonal event triggered for week {week}");
        }

        private void CheckForPhaseBasedEvents()
        {
            // During event phase, check if we should trigger contextual events
            if (Random.Range(0f, 1f) < 0.4f) // 40% chance during event phase
            {
                var contextualEvent =
                    EventDataFactory.CreateContextualEvent(lastCommunitySentiment, recentMajorChanges);
                QueueEvent(contextualEvent);
                OnEventSystemMessage.Invoke("Phase-based contextual event triggered");
            }
        }

        private EventData CreateMajorChangeReactionEvent()
        {
            var eventData = new EventData
            {
                eventTitle = "Community Reacting to Major Changes",
                description =
                    $"Recent major balance changes have stirred up significant community discussion. {recentMajorChanges.Count} major adjustments in quick succession have players debating the game's direction.",
                eventType = EventType.CommunityEvent,
                urgencyLevel = EventUrgency.Medium,
                responseTimeLimit = 50f,
                estimatedSentimentImpact = Random.Range(-8f, 8f),
                expectedImpacts = new List<string>
                {
                    "Community divided on changes",
                    "Meta adaptation period",
                    "Increased forum activity",
                    "Content creator reactions"
                }
            };

            // Community Management Response
            eventData.responses[EventResponseType.CommunityManagement] = new EventResponse
            {
                responseText = "Address Community Concerns",
                rpCost = 0,
                cpCost = 4,
                communitySentimentChange = 8f,
                specialEffect = "community_dialogue",
                successMessage = "Community appreciates transparent communication about changes.",
                failureMessage = "Response seen as corporate PR. Community remains skeptical."
            };

            // Emergency Fix Response (minor adjustments)
            eventData.responses[EventResponseType.EmergencyFix] = new EventResponse
            {
                responseText = "Fine-tune Changes",
                rpCost = 3,
                cpCost = 1,
                characterEffects = recentMajorChanges.Take(2).Select(change =>
                    new CharacterEffect(change.character, change.stat, -change.magnitude * 0.3f)).ToList(),
                communitySentimentChange = 5f,
                successMessage = "Fine-tuning addresses community concerns while preserving change intent.",
                failureMessage = "Adjustments created confusion about design direction."
            };

            // Observe Response
            eventData.responses[EventResponseType.ObserveAndLearn] = new EventResponse
            {
                responseText = "Monitor Community Adaptation",
                rpCost = 0,
                cpCost = 0,
                communitySentimentChange = -2f,
                specialEffect = "adaptation_data",
                successMessage = "Valuable data on community adaptation patterns collected.",
                failureMessage = "Lack of response seen as indifference to community concerns."
            };

            return eventData;
        }

        #endregion

        #region Random Event System

        private void CheckForRandomEvents()
        {
            if (Time.time >= nextRandomEventTime && CanSpawnRandomEvent())
            {
                if (Random.Range(0f, 1f) < randomEventChance)
                {
                    TriggerRandomEvent();
                }

                ScheduleNextRandomEvent();
            }
        }

        private void TriggerRandomEvent()
        {
            var randomEvent = EventDataFactory.CreateRandomEvent();
            QueueEvent(randomEvent);
            OnEventSystemMessage.Invoke("Random event triggered");
        }

        private bool CanSpawnRandomEvent()
        {
            return activeEvents.Count < maxSimultaneousEvents &&
                   Time.time - lastEventTime >= minTimeBetweenEvents;
        }

        private void ScheduleNextRandomEvent()
        {
            float nextEventDelay = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
            nextRandomEventTime = Time.time + nextEventDelay;
        }

        private bool ShouldTriggerSeasonalEvent(int week)
        {
            // Seasonal events more likely at specific intervals
            return (week % 5 == 0) || // Every 5 weeks
                   (week % 10 == 1) || // Start of cycles
                   (week == 1); // First week
        }

        #endregion

        #region Event Queue and Display Management

        private void QueueEvent(EventData eventData)
        {
            eventQueue.Enqueue(eventData);
            OnEventTriggered.Invoke(eventData);
            Debug.Log($"🎭 Event queued: {eventData.eventTitle}");
        }

        private void ProcessEventQueue()
        {
            while (eventQueue.Count > 0 && activeEvents.Count < maxSimultaneousEvents)
            {
                var eventData = eventQueue.Dequeue();
                DisplayEvent(eventData);
            }
        }

        private void DisplayEvent(EventData eventData)
        {
            if (eventUIItemPrefab == null || eventUIContainer == null)
            {
                Debug.LogError("❌ Cannot display event: missing prefab or container");
                return;
            }

            // Create event UI
            GameObject eventUIObject = Instantiate(eventUIItemPrefab, eventUIContainer);
            EventUIItem eventUIItem = eventUIObject.GetComponent<EventUIItem>();

            if (eventUIItem == null)
            {
                Debug.LogError("❌ EventUIItem component not found on prefab!");
                Destroy(eventUIObject);
                return;
            }

            // Setup event UI
            eventUIItem.DisplayEvent(eventData);
            activeEvents.Add(eventUIItem);
            lastEventTime = Time.time;

            // Subscribe to event resolution
            eventUIItem.OnEventResponseSelected.AddListener(OnEventResponseExecuted);
            eventUIItem.OnEventExpired.AddListener(OnEventExpiredHandler);

            OnEventSystemMessage.Invoke($"Event displayed: {eventData.eventTitle}");
            Debug.Log($"✅ Event displayed: {eventData.eventTitle}");
        }

        private void UpdateActiveEvents()
        {
            // Clean up completed events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                if (activeEvents[i] == null || !activeEvents[i].IsActive())
                {
                    activeEvents.RemoveAt(i);
                }
                else
                {
                    // Refresh event display (for resource changes, etc.)
                    activeEvents[i].RefreshDisplay();
                }
            }
        }

        #endregion

        #region Event Response Handling

        private void OnEventResponseExecuted(EventData eventData, EventResponseType responseType)
        {
            OnEventResolved.Invoke(eventData, responseType);

            // Generate additional feedback based on response effectiveness
            GenerateResponseFeedback(eventData, responseType);

            OnEventSystemMessage.Invoke($"Event resolved: {eventData.eventTitle} with {responseType}");
            Debug.Log($"🎯 Event resolved: {eventData.eventTitle} with response: {responseType}");
        }

        private void OnEventExpiredHandler(EventData eventData)
        {
            OnEventExpired.Invoke(eventData);

            // Generate negative feedback for ignored events
            GenerateExpirationFeedback(eventData);

            OnEventSystemMessage.Invoke($"Event expired: {eventData.eventTitle}");
            Debug.Log($"⏰ Event expired: {eventData.eventTitle}");
        }

        private void GenerateResponseFeedback(EventData eventData, EventResponseType responseType)
        {
            if (feedbackManager == null) return;

            var response = eventData.responses[responseType];

            // Create contextual feedback about the event response
            var feedbackContent = GenerateEventResponseFeedbackContent(eventData, responseType, response);
            var feedbackSentiment = CalculateEventResponseSentiment(eventData, responseType, response);

            var responseFeedback = new Community.CommunityFeedback
            {
                author = GetEventResponseAuthor(eventData, responseType),
                content = feedbackContent,
                sentiment = feedbackSentiment,
                feedbackType = Community.FeedbackType.BalanceReaction,
                communitySegment = GetEventResponseSegment(eventData, responseType),
                timestamp = System.DateTime.Now,
                upvotes = Random.Range(15, 60),
                replies = Random.Range(8, 30),
                isOrganic = false
            };

            // Add to feedback system
            feedbackManager.OnNewFeedbackAdded.Invoke(responseFeedback);
        }

        private void GenerateExpirationFeedback(EventData eventData)
        {
            if (feedbackManager == null) return;

            var expirationFeedback = new Community.CommunityFeedback
            {
                author = "CommunityWatcher",
                content = GetExpirationFeedbackContent(eventData),
                sentiment = Random.Range(-0.8f, -0.3f), // Generally negative
                feedbackType = Community.FeedbackType.BalanceReaction,
                communitySegment = "Competitive",
                timestamp = System.DateTime.Now,
                upvotes = Random.Range(25, 80),
                replies = Random.Range(15, 45),
                isOrganic = false
            };

            feedbackManager.OnNewFeedbackAdded.Invoke(expirationFeedback);
        }

        #endregion

        #region Utility Methods

        private float GetPreviousStatValue(Characters.CharacterType character, Characters.CharacterStat stat)
        {
            // Look for recent changes to get previous value
            var recentChange = recentMajorChanges
                .Where(c => c.character == character && c.stat == stat)
                .OrderByDescending(c => c.timestamp)
                .FirstOrDefault();

            return recentChange?.previousValue ?? 50f; // Default to 50 if no previous change
        }

        private string GenerateEventResponseFeedbackContent(EventData eventData, EventResponseType responseType,
            EventResponse response)
        {
            return (eventData.eventType, responseType) switch
            {
                (EventType.Crisis, EventResponseType.EmergencyFix) =>
                    "Quick response to the crisis! ⚡ Hopefully this fixes the immediate issues",
                (EventType.Crisis, EventResponseType.CommunityManagement) =>
                    "Good communication from the devs about this situation 📢",
                (EventType.Opportunity, EventResponseType.CommunityManagement) =>
                    "Devs are capitalizing on this opportunity well ★",
                (EventType.Opportunity, EventResponseType.EmergencyFix) =>
                    "Smart move to take advantage of this situation ►",
                (_, EventResponseType.ObserveAndLearn) => "Interesting approach - let's see how this develops 👀",
                _ => $"Response to {eventData.eventTitle}: {response.responseText}"
            };
        }

        private float CalculateEventResponseSentiment(EventData eventData, EventResponseType responseType,
            EventResponse response)
        {
            float baseSentiment = responseType switch
            {
                EventResponseType.EmergencyFix => 0.5f, // Generally positive
                EventResponseType.CommunityManagement => 0.3f, // Moderately positive
                EventResponseType.ObserveAndLearn => 0.0f, // Neutral
                EventResponseType.IgnoreEvent => -0.6f, // Negative
                _ => 0.0f
            };

            // Adjust based on event urgency
            if (eventData.urgencyLevel == EventUrgency.Critical && responseType == EventResponseType.EmergencyFix)
            {
                baseSentiment += 0.3f; // Extra positive for addressing critical issues quickly
            }

            // Add variance
            return Mathf.Clamp(baseSentiment + Random.Range(-0.2f, 0.2f), -1f, 1f);
        }

        private string GetEventResponseAuthor(EventData eventData, EventResponseType responseType)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => "TechResponse_Team",
                EventResponseType.CommunityManagement => "Community_Manager",
                EventResponseType.ObserveAndLearn => "Data_Analyst",
                EventResponseType.SeekAdvice => "Community_Council",
                _ => "Event_Observer"
            };
        }

        private string GetEventResponseSegment(EventData eventData, EventResponseType responseType)
        {
            return (eventData.eventType, responseType) switch
            {
                (EventType.Crisis, EventResponseType.EmergencyFix) => "Competitive",
                (EventType.Crisis, EventResponseType.CommunityManagement) => "Casual Players",
                (EventType.Opportunity, _) => "Content Creators",
                (EventType.TournamentEvent, _) => "Pro Players",
                _ => "General"
            };
        }

        private string GetExpirationFeedbackContent(EventData eventData)
        {
            return eventData.eventType switch
            {
                EventType.Crisis => "No response to this crisis? Community concerns ignored ↓",
                EventType.Opportunity => "Missed opportunity! Devs should have acted faster ✗",
                EventType.CommunityEvent => "Community event ignored... disappointing 😕",
                _ => $"No response to {eventData.eventTitle} - concerning silence"
            };
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually trigger a specific event (for testing or special occasions)
        /// </summary>
        public void TriggerEvent(EventData eventData)
        {
            QueueEvent(eventData);
            OnEventSystemMessage.Invoke($"Manual event triggered: {eventData.eventTitle}");
        }

        /// <summary>
        /// Clear all active events (for testing or emergency situations)
        /// </summary>
        public void ClearAllEvents()
        {
            foreach (var eventUI in activeEvents)
            {
                if (eventUI != null)
                {
                    eventUI.CloseEvent();
                }
            }

            activeEvents.Clear();
            eventQueue.Clear();
            OnEventSystemMessage.Invoke("All events cleared");
        }

        /// <summary>
        /// Get information about current event system state
        /// </summary>
        public (int active, int queued, float nextRandom) GetEventSystemStatus()
        {
            float timeToNextRandom = Mathf.Max(0f, nextRandomEventTime - Time.time);
            return (activeEvents.Count, eventQueue.Count, timeToNextRandom);
        }

        /// <summary>
        /// Check if a specific event type is currently active
        /// </summary>
        public bool HasActiveEventOfType(EventType eventType)
        {
            return activeEvents.Any(e => e.GetCurrentEvent()?.eventType == eventType);
        }

        /// <summary>
        /// Force trigger seasonal event for current week
        /// </summary>
        public void ForceTriggerSeasonalEvent()
        {
            TriggerSeasonalEvent(currentGameWeek);
        }

        #endregion

        #region Debug Methods

        [ContextMenu("🧪 Test Crisis Event")]
        public void DebugTestCrisisEvent()
        {
            var crisisEvent = EventDataFactory.CreateSupportExploitCrisis();
            TriggerEvent(crisisEvent);
        }

        [ContextMenu("🧪 Test Opportunity Event")]
        public void DebugTestOpportunityEvent()
        {
            var opportunityEvent = EventDataFactory.CreateTournamentOpportunity();
            TriggerEvent(opportunityEvent);
        }

        [ContextMenu("🧪 Test Random Event")]
        public void DebugTestRandomEvent()
        {
            TriggerRandomEvent();
        }

        [ContextMenu("🧪 Test All Event Types")]
        public void DebugTestAllEventTypes()
        {
            var testEvents = new[]
            {
                EventDataFactory.CreateSupportExploitCrisis(),
                EventDataFactory.CreateTournamentOpportunity(),
                EventDataFactory.CreateCommunityFeedbackCrisis(),
                EventDataFactory.CreateMetaShiftOpportunity()
            };

            foreach (var eventData in testEvents)
            {
                QueueEvent(eventData);
            }

            OnEventSystemMessage.Invoke("All event types queued for testing");
        }

        [ContextMenu("🔍 Debug Event System Status")]
        public void DebugShowEventSystemStatus()
        {
            var (active, queued, nextRandom) = GetEventSystemStatus();

            Debug.Log("=== 🔍 EVENT SYSTEM STATUS ===");
            Debug.Log($"Active Events: {active}/{maxSimultaneousEvents}");
            Debug.Log($"Queued Events: {queued}");
            Debug.Log($"Next Random Event: {nextRandom:F1}s");
            Debug.Log($"Last Event Time: {Time.time - lastEventTime:F1}s ago");
            Debug.Log($"Community Sentiment: {lastCommunitySentiment:F1}%");
            Debug.Log($"Recent Major Changes: {recentMajorChanges.Count}");
            Debug.Log($"Current Week: {currentGameWeek}");

            if (activeEvents.Count > 0)
            {
                Debug.Log("\nActive Events:");
                foreach (var eventUI in activeEvents)
                {
                    var eventData = eventUI.GetCurrentEvent();
                    if (eventData != null)
                    {
                        Debug.Log(
                            $"  {eventData.eventTitle} ({eventData.eventType}) - {eventUI.GetTimeRemaining():F1}s remaining");
                    }
                }
            }
        }

        [ContextMenu("🔄 Force Random Event")]
        public void DebugForceRandomEvent()
        {
            nextRandomEventTime = Time.time; // Trigger immediately
            OnEventSystemMessage.Invoke("Random event forced");
        }

        [ContextMenu("🧹 Clear All Events")]
        public void DebugClearAllEvents()
        {
            ClearAllEvents();
        }

        [ContextMenu("📊 Test Event Response Feedback")]
        public void DebugTestEventResponseFeedback()
        {
            var testEvent = EventDataFactory.CreateSupportExploitCrisis();
            GenerateResponseFeedback(testEvent, EventResponseType.EmergencyFix);
            OnEventSystemMessage.Invoke("Test event response feedback generated");
        }

        #endregion

        private void OnDestroy()
        {
            // Clean up subscriptions
            if (feedbackManager != null)
            {
                feedbackManager.OnCommunitySentimentChanged.RemoveListener(OnCommunitySentimentChanged);
            }

            if (characterManager != null)
            {
                characterManager.OnStatChanged.RemoveListener(OnCharacterStatChanged);
            }

            if (phaseManager != null)
            {
                phaseManager.OnWeekChanged.RemoveListener(OnWeekChanged);
                phaseManager.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }

            Debug.Log("🎭 Event Manager destroyed and cleaned up");
        }
    }
}