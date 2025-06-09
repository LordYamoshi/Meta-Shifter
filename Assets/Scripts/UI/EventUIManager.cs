using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    /// <summary>
    /// Event UI Manager that handles event display and management
    /// Compatible with the enhanced EventUIItem with emoji support
    /// </summary>
    public class EventUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform eventContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private ScrollRect eventScrollRect;
        [SerializeField] private TextMeshProUGUI noEventsText;
        
        [Header("Event Management")]
        [SerializeField] private int maxVisibleEvents = 10;
        [SerializeField] private bool advanceEventsOnWeekChange = true;  // Auto-advance when weeks change
        [SerializeField] private bool advanceEventsOnPhaseChange = false; // Or advance on every phase change
        
        [Header("Event Generation (For Testing)")]
        [SerializeField] private bool enableTestEvents = false;
        [SerializeField] private float testEventInterval = 5f;
        
        // Event tracking
        private List<EventUIItem> activeEventItems = new List<EventUIItem>();
        private List<EventData> activeEvents = new List<EventData>();
        private float lastTestEventTime = 0f;
        
        private void Start()
        {
            InitializeEventSystem();
            SubscribeToGameEvents();
        }
        
        private void Update()
        {
            // Generate test events if enabled
            if (enableTestEvents && Time.time - lastTestEventTime >= testEventInterval)
            {
                GenerateRandomTestEvent();
                lastTestEventTime = Time.time;
            }
        }
        
        private void InitializeEventSystem()
        {
            // Auto-find components if not assigned
            if (eventContainer == null)
            {
                var foundContainer = GameObject.Find("EventContainer");
                if (foundContainer != null)
                    eventContainer = foundContainer.transform;
            }
            
            if (noEventsText == null)
            {
                var foundText = GameObject.Find("NoEventsText");
                if (foundText != null)
                    noEventsText = foundText.GetComponent<TextMeshProUGUI>();
            }
            
            // Setup initial state
            UpdateNoEventsDisplay();
            
            Debug.Log("✅ EventUIManager initialized");
        }
        
        private void SubscribeToGameEvents()
        {
            // Subscribe to relevant game systems
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(OnCommunitySentimentChanged);
                Debug.Log("📡 Subscribed to CommunityFeedbackManager events");
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnOverallBalanceChanged);
                Debug.Log("📡 Subscribed to CharacterManager events");
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
                Debug.Log("📡 Subscribed to PhaseManager events for turn advancement");
            }
        }
        
        #region Event Management Methods
        
        /// <summary>
        /// Setup method for EventUIItem compatibility
        /// </summary>
        public void Setup(EventData eventData)
        {
            CreateEventItem(eventData);
        }
        
        /// <summary>
        /// GetEvent method for EventUIItem compatibility
        /// </summary>
        public EventData GetEvent()
        {
            return activeEvents.FirstOrDefault();
        }
        
        /// <summary>
        /// UpdateTimeDisplay method for EventUIItem compatibility (renamed for turns)
        /// </summary>
        public void UpdateTimeDisplay(float newTurnsRemaining)
        {
            // Update all event items with new turn count
            for (int i = 0; i < activeEventItems.Count; i++)
            {
                if (activeEventItems[i] != null)
                {
                    activeEventItems[i].UpdateTurnDisplay(newTurnsRemaining);
                }
            }
        }
        
        /// <summary>
        /// Advance all events by one turn (call when turn/week changes)
        /// </summary>
        public void AdvanceAllEventsByOneTurn()
        {
            for (int i = activeEventItems.Count - 1; i >= 0; i--)
            {
                if (activeEventItems[i] != null)
                {
                    bool hasExpired = activeEventItems[i].AdvanceTurn();
                    
                    if (hasExpired)
                    {
                        Debug.Log($"⏰ Event expired: {activeEvents[i]?.Title ?? "Unknown"}");
                        RemoveEvent(i);
                    }
                }
            }
            
            UpdateNoEventsDisplay();
        }
        
        /// <summary>
        /// Create and display a new event
        /// </summary>
        public void CreateEvent(EventData eventData)
        {
            CreateEventItem(eventData);
        }
        
        private void CreateEventItem(EventData eventData)
        {
            if (eventItemPrefab == null || eventContainer == null)
            {
                Debug.LogError("❌ EventUIManager: Missing prefab or container!");
                return;
            }
            
            // Check if we have too many events
            if (activeEventItems.Count >= maxVisibleEvents)
            {
                RemoveOldestEvent();
            }
            
            // Instantiate new event item
            GameObject eventObj = Instantiate(eventItemPrefab, eventContainer);
            EventUIItem eventItem = eventObj.GetComponent<EventUIItem>();
            
            if (eventItem == null)
            {
                Debug.LogError("❌ Event prefab missing EventUIItem component!");
                Destroy(eventObj);
                return;
            }
            
            // Setup the event item
            eventItem.SetupEvent(eventData, OnEventResponded, OnEventDismissed);
            
            // Add to tracking lists
            activeEventItems.Add(eventItem);
            activeEvents.Add(eventData);
            
            // Update UI state
            UpdateNoEventsDisplay();
            
            // Move to top
            eventObj.transform.SetAsFirstSibling();
            
            Debug.Log($"✅ Created event: {eventData.Title}");
        }
        
        private void RemoveOldestEvent()
        {
            if (activeEventItems.Count == 0) return;
            
            var oldestItem = activeEventItems[activeEventItems.Count - 1];
            var oldestEvent = activeEvents[activeEvents.Count - 1];
            
            // Remove from lists
            activeEventItems.RemoveAt(activeEventItems.Count - 1);
            activeEvents.RemoveAt(activeEvents.Count - 1);
            
            // Destroy UI object
            if (oldestItem != null && oldestItem.gameObject != null)
            {
                Destroy(oldestItem.gameObject);
            }
            
            Debug.Log($"🗑️ Removed oldest event: {oldestEvent?.Title ?? "Unknown"}");
        }
        
        /// <summary>
        /// RefreshDisplay method for EventUIItem compatibility
        /// </summary>
        public void RefreshDisplay()
        {
            RefreshAllEventItems();
        }
        
        private void RemoveEvent(int index)
        {
            if (index < 0 || index >= activeEvents.Count) return;
            
            var eventData = activeEvents[index];
            var eventItem = activeEventItems[index];
            
            // Remove from lists
            activeEvents.RemoveAt(index);
            activeEventItems.RemoveAt(index);
            
            // Destroy UI object
            if (eventItem != null && eventItem.gameObject != null)
            {
                Destroy(eventItem.gameObject);
            }
            
            UpdateNoEventsDisplay();
            
            Debug.Log($"⏰ Event expired: {eventData?.Title ?? "Unknown"}");
        }
        
        private void RefreshAllEventItems()
        {
            for (int i = 0; i < activeEventItems.Count; i++)
            {
                if (activeEventItems[i] != null && activeEvents[i] != null)
                {
                    activeEventItems[i].SetupEvent(activeEvents[i], OnEventResponded, OnEventDismissed);
                }
            }
            
            UpdateNoEventsDisplay();
        }
        
        private void UpdateNoEventsDisplay()
        {
            bool hasEvents = activeEvents.Count > 0;
            
            if (noEventsText != null)
            {
                noEventsText.gameObject.SetActive(!hasEvents);
                if (!hasEvents)
                {
                    noEventsText.text = "📋 No active events\nEverything is running smoothly!";
                }
            }
        }
        
        #endregion
        
        #region Event Callbacks
        
        private void OnEventResponded(EventData eventData)
        {
            Debug.Log($"🎯 Player responded to event: {eventData.Title}");
            
            // Mark as resolved
            eventData.IsResolved = true;
            
            // Apply event effects based on type
            ApplyEventEffects(eventData, true);
            
            // Remove from active events
            RemoveEventByData(eventData);
        }
        
        private void OnEventDismissed(EventData eventData)
        {
            Debug.Log($"🗑️ Player dismissed event: {eventData.Title}");
            
            // Apply dismissal effects (usually negative)
            ApplyEventEffects(eventData, false);
            
            // Remove from active events
            RemoveEventByData(eventData);
        }
        
        private void RemoveEventByData(EventData eventData)
        {
            int index = activeEvents.IndexOf(eventData);
            if (index >= 0)
            {
                RemoveEvent(index);
            }
        }
        
        private void ApplyEventEffects(EventData eventData, bool responded)
        {
            // Apply effects based on event type and response
            switch (eventData.EventType)
            {
                case EventType.Crisis:
                    ApplyCrisisEffects(eventData, responded);
                    break;
                case EventType.Opportunity:
                    ApplyOpportunityEffects(eventData, responded);
                    break;
                case EventType.Community:
                    ApplyCommunityEffects(eventData, responded);
                    break;
                case EventType.Technical:
                    ApplyTechnicalEffects(eventData, responded);
                    break;
                case EventType.Competitive:
                    ApplyCompetitiveEffects(eventData, responded);
                    break;
            }
        }
        
        private void ApplyCrisisEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                // Positive effects for handling crisis
                Debug.Log($"✅ Crisis resolved: {eventData.Title}");
                // Could improve community sentiment, prevent negative effects
            }
            else
            {
                // Negative effects for ignoring crisis
                Debug.Log($"❌ Crisis ignored: {eventData.Title}");
                // Could damage community sentiment, cause ongoing issues
            }
        }
        
        private void ApplyOpportunityEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                // Positive effects for seizing opportunity
                Debug.Log($"💎 Opportunity seized: {eventData.Title}");
                // Could provide resources, improve reputation
            }
            else
            {
                // Missed opportunity (neutral/slight negative)
                Debug.Log($"😞 Opportunity missed: {eventData.Title}");
            }
        }
        
        private void ApplyCommunityEffects(EventData eventData, bool responded)
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager == null) return;
            
            if (responded)
            {
                // Improve community sentiment
                Debug.Log($"💬 Community addressed: {eventData.Title}");
            }
            else
            {
                // Worsen community sentiment
                Debug.Log($"😠 Community ignored: {eventData.Title}");
            }
        }
        
        private void ApplyTechnicalEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"🔧 Technical issue fixed: {eventData.Title}");
            }
            else
            {
                Debug.Log($"⚠️ Technical issue persists: {eventData.Title}");
            }
        }
        
        private void ApplyCompetitiveEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"⚔️ Competitive action taken: {eventData.Title}");
            }
            else
            {
                Debug.Log($"📉 Competitive opportunity lost: {eventData.Title}");
            }
        }
        
        #endregion
        
        #region Game Event Handlers - Turn Based
        
        private void OnWeekChanged(int newWeek)
        {
            Debug.Log($"📅 Week {newWeek} started - advancing all events by one turn");
            
            if (advanceEventsOnWeekChange)
            {
                AdvanceAllEventsByOneTurn();
            }
            
            // Generate week-based events occasionally
            if (Random.Range(0f, 1f) < 0.3f)
            {
                GenerateWeeklyEvent(newWeek);
            }
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            // Generate phase-specific events
            if (newPhase == Core.GamePhase.Event)
            {
                // Event phase - higher chance of events
                if (Random.Range(0f, 1f) < 0.4f)
                {
                    GeneratePhaseSpecificEvent();
                }
            }
        }
        
        private void OnCommunitySentimentChanged(float newSentiment)
        {
            // Generate events based on sentiment changes
            if (newSentiment < 30f)
            {
                // Low sentiment might trigger crisis events
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    GenerateCommunityCrisisEvent();
                }
            }
            else if (newSentiment > 75f)
            {
                // High sentiment might trigger opportunity events
                if (Random.Range(0f, 1f) < 0.2f)
                {
                    GeneratePositiveCommunityEvent();
                }
            }
        }
        
        private void OnOverallBalanceChanged(float balanceScore)
        {
            // Generate events based on balance changes
            if (balanceScore < 40f)
            {
                if (Random.Range(0f, 1f) < 0.25f)
                {
                    GenerateBalanceIssueEvent();
                }
            }
        }
        
        private void GenerateWeeklyEvent(int weekNumber)
        {
            var weeklyEvents = new[]
            {
                new EventData
                {
                    Title = $"Week {weekNumber} Community Check-in",
                    Description = "Time to assess community sentiment and address any brewing concerns.",
                    Severity = EventSeverity.Medium,
                    EventType = EventType.Community,
                    TimeRemaining = 4f,
                    Impact = 4.0f
                },
                new EventData
                {
                    Title = "Weekly Performance Review",
                    Description = "Server metrics show unusual patterns. Investigation recommended.",
                    Severity = EventSeverity.Low,
                    EventType = EventType.Technical,
                    TimeRemaining = 5f,
                    Impact = 3.2f
                }
            };
            
            var selectedEvent = weeklyEvents[Random.Range(0, weeklyEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        #endregion
        
        #region Event Generation
        
        private void GenerateCommunityCrisisEvent()
        {
            var crisisEvents = new[]
            {
                new EventData
                {
                    Title = "Community Backlash",
                    Description = "Players are organizing a boycott due to recent balance changes. Immediate response needed.",
                    Severity = EventSeverity.High,
                    EventType = EventType.Community,
                    TimeRemaining = 3f,
                    Impact = 7.5f
                },
                new EventData
                {
                    Title = "Negative Reviews Surge",
                    Description = "Steam reviews are turning negative rapidly. Community management required.",
                    Severity = EventSeverity.High,
                    EventType = EventType.Community,
                    TimeRemaining = 3f,
                    Impact = 6.8f
                }
            };
            
            var selectedEvent = crisisEvents[Random.Range(0, crisisEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        private void GeneratePositiveCommunityEvent()
        {
            var positiveEvents = new[]
            {
                new EventData
                {
                    Title = "Viral Social Media Post",
                    Description = "A popular content creator praised the recent balance changes. Great publicity opportunity!",
                    Severity = EventSeverity.Opportunity,
                    EventType = EventType.Opportunity,
                    TimeRemaining = 4f,
                    Impact = 5.2f
                },
                new EventData
                {
                    Title = "Community Tournament Interest",
                    Description = "Players are organizing a community tournament. Consider official support.",
                    Severity = EventSeverity.Opportunity,
                    EventType = EventType.Competitive,
                    TimeRemaining = 5f,
                    Impact = 4.8f
                }
            };
            
            var selectedEvent = positiveEvents[Random.Range(0, positiveEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        private void GenerateBalanceIssueEvent()
        {
            var balanceEvents = new[]
            {
                new EventData
                {
                    Title = "Character Dominance Detected",
                    Description = "Data shows one character has >65% win rate. Balance intervention recommended.",
                    Severity = EventSeverity.High,
                    EventType = EventType.Technical,
                    TimeRemaining = 3f,
                    Impact = 6.5f
                },
                new EventData
                {
                    Title = "Meta Stagnation Warning",
                    Description = "Pick diversity has dropped significantly. Meta refresh may be needed.",
                    Severity = EventSeverity.Medium,
                    EventType = EventType.Technical,
                    TimeRemaining = 4f,
                    Impact = 5.0f
                }
            };
            
            var selectedEvent = balanceEvents[Random.Range(0, balanceEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        private void GeneratePhaseSpecificEvent()
        {
            var phaseEvents = new[]
            {
                new EventData
                {
                    Title = "Press Inquiry",
                    Description = "Gaming journalist wants interview about balance philosophy. PR opportunity.",
                    Severity = EventSeverity.Opportunity,
                    EventType = EventType.Opportunity,
                    TimeRemaining = 2f,
                    Impact = 4.5f
                },
                new EventData
                {
                    Title = "Server Performance Issue",
                    Description = "Increased player activity is causing server strain. Technical response needed.",
                    Severity = EventSeverity.Medium,
                    EventType = EventType.Technical,
                    TimeRemaining = 3f,
                    Impact = 5.5f
                }
            };
            
            var selectedEvent = phaseEvents[Random.Range(0, phaseEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        private void GenerateRandomTestEvent()
        {
            var testEvents = new[]
            {
                new EventData
                {
                    Title = "Test Critical Event",
                    Description = "This is a test critical event to verify the system works properly.",
                    Severity = EventSeverity.Critical,
                    EventType = EventType.Crisis,
                    TimeRemaining = 1f,
                    Impact = 8.0f
                },
                new EventData
                {
                    Title = "Test Opportunity",
                    Description = "This is a test opportunity event with positive implications.",
                    Severity = EventSeverity.Opportunity,
                    EventType = EventType.Opportunity,
                    TimeRemaining = 3f,
                    Impact = 6.0f
                },
                new EventData
                {
                    Title = "Test Community Event",
                    Description = "This is a test community event requiring player response.",
                    Severity = EventSeverity.Medium,
                    EventType = EventType.Community,
                    TimeRemaining = 2f,
                    Impact = 4.5f
                }
            };
            
            var selectedEvent = testEvents[Random.Range(0, testEvents.Length)];
            CreateEvent(selectedEvent);
        }
        
        #endregion
        
        #region Public API and Debug Methods
        
        /// <summary>
        /// Get all active events
        /// </summary>
        public List<EventData> GetActiveEvents()
        {
            return new List<EventData>(activeEvents);
        }
        
        /// <summary>
        /// Get count of active events by severity
        /// </summary>
        public int GetEventCountBySeverity(EventSeverity severity)
        {
            return activeEvents.Count(e => e.Severity == severity);
        }
        
        /// <summary>
        /// Check if any critical events are active
        /// </summary>
        public bool HasCriticalEvents()
        {
            return activeEvents.Any(e => e.Severity == EventSeverity.Critical);
        }
        
        /// <summary>
        /// Clear all events
        /// </summary>
        public void ClearAllEvents()
        {
            // Destroy all UI items
            foreach (var item in activeEventItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            // Clear lists
            activeEventItems.Clear();
            activeEvents.Clear();
            
            UpdateNoEventsDisplay();
            
            Debug.Log("🧹 All events cleared");
        }
        
        // Debug methods
        [ContextMenu("🧪 Generate Test Critical Event")]
        public void DebugGenerateCriticalEvent()
        {
            var testEvent = new EventData
            {
                Title = "DEBUG: Game-Breaking Bug",
                Description = "Critical bug discovered that breaks core gameplay. Immediate hotfix required!",
                Severity = EventSeverity.Critical,
                EventType = EventType.Crisis,
                TimeRemaining = 0.5f,
                Impact = 9.5f
            };
            
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🧪 Generate Test Opportunity")]
        public void DebugGenerateOpportunity()
        {
            var testEvent = new EventData
            {
                Title = "DEBUG: Partnership Offer",
                Description = "Major gaming company interested in collaboration. Time-sensitive opportunity.",
                Severity = EventSeverity.Opportunity,
                EventType = EventType.Opportunity,
                TimeRemaining = 4f,
                Impact = 7.2f
            };
            
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🧪 Generate Multiple Test Events")]
        public void DebugGenerateMultipleEvents()
        {
            for (int i = 0; i < 3; i++)
            {
                GenerateRandomTestEvent();
            }
        }
        
        [ContextMenu("🧹 Clear All Events")]
        public void DebugClearAllEvents()
        {
            ClearAllEvents();
        }
        
        /// <summary>
        /// Manually advance all events by one turn (for testing)
        /// </summary>
        [ContextMenu("⏰ Advance All Events by 1 Turn")]
        public void DebugAdvanceOneTurn()
        {
            AdvanceAllEventsByOneTurn();
            Debug.Log("⏰ Manually advanced all events by 1 turn");
        }
        
        [ContextMenu("📊 Show Event Statistics")]
        public void DebugShowEventStatistics()
        {
            Debug.Log("=== EVENT STATISTICS ===");
            Debug.Log($"Active Events: {activeEvents.Count}");
            Debug.Log($"Critical Events: {GetEventCountBySeverity(EventSeverity.Critical)}");
            Debug.Log($"High Priority Events: {GetEventCountBySeverity(EventSeverity.High)}");
            Debug.Log($"Opportunities: {GetEventCountBySeverity(EventSeverity.Opportunity)}");
            Debug.Log($"Has Critical Events: {HasCriticalEvents()}");
            
            foreach (var eventData in activeEvents)
            {
                Debug.Log($"  - {eventData.Title} ({eventData.Severity}, {eventData.TimeRemaining:F1} turns left)");
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(OnCommunitySentimentChanged);
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.RemoveListener(OnOverallBalanceChanged);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.RemoveListener(OnWeekChanged);
            }
            
            Debug.Log("🎭 EventUIManager destroyed and cleaned up");
        }
    }
}