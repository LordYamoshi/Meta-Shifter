using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    /// <summary>
    /// Core Event Manager System - Clean and Simple
    /// Generates events, manages timing, integrates with your game systems
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Event Spawning")]
        [SerializeField] private UI.EventUIManager eventUIManager;
        [SerializeField] private int maxSimultaneousEvents = 3;

        [Header("Event Timing")]
        [Range(0f, 1f)] [SerializeField] private float randomEventChance = 0.3f;
        [Range(10f, 120f)] [SerializeField] private float minTimeBetweenEvents = 30f;
        [Range(60f, 300f)] [SerializeField] private float maxTimeBetweenEvents = 180f;

        [Header("Event Triggers")]
        [SerializeField] private bool triggerOnLowSentiment = true;
        [SerializeField] private bool triggerOnHighSentiment = true;
        [SerializeField] private bool triggerOnMajorChanges = true;
        [SerializeField] private bool triggerOnSeasonalEvents = true;

        [Header("Trigger Thresholds")]
        [Range(0f, 50f)] [SerializeField] private float lowSentimentThreshold = 30f;
        [Range(50f, 100f)] [SerializeField] private float highSentimentThreshold = 75f;
        [Range(5f, 30f)] [SerializeField] private float majorChangeThreshold = 15f;

        [Header("Events")]
        public UnityEvent<EventData> OnEventTriggered;
        public UnityEvent<EventData, EventResponseType> OnEventResolved;
        public UnityEvent<EventData> OnEventExpired;
        public UnityEvent<string> OnEventSystemMessage;

        // Active event tracking
        private List<EventData> activeEvents = new List<EventData>();
        private Queue<EventData> eventQueue = new Queue<EventData>();
        private float lastEventTime = 0f;
        private float nextRandomEventTime = 0f;

        // Event generation state
        private float lastCommunitySentiment = 50f;
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
            ScheduleNextRandomEvent();

            Debug.Log("🎭 Event Manager initialized and ready!");
        }

        private void Update()
        {
            CheckForRandomEvents();
            ProcessEventQueue();
            UpdateActiveEvents();
        }

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
            }

            lastEventTime = Time.time;
        }

        #endregion

        #region Event Generation

        /// <summary>
        /// Trigger a specific event immediately
        /// </summary>
        public void TriggerEvent(EventData eventData)
        {
            if (eventData == null) return;

            // Check if we have room for more events
            if (activeEvents.Count >= maxSimultaneousEvents)
            {
                eventQueue.Enqueue(eventData);
                Debug.Log($"📦 Event queued: {eventData.title}");
                return;
            }

            // Display the event
            DisplayEvent(eventData);
        }

        /// <summary>
        /// Generate a crisis event (for testing and triggered events)
        /// </summary>
        public void GenerateCrisisEvent(string title, string description, EventSeverity severity = EventSeverity.High)
        {
            var eventData = new EventData(title, description, EventType.Crisis, severity)
            {
                timeRemaining = severity == EventSeverity.Critical ? 30f : 60f,
                expectedImpact = severity == EventSeverity.Critical ? 9f : 6f,
                expectedImpacts = new List<string> { "Community backlash risk", "Balance disruption", "Player satisfaction impact" }
            };

            TriggerEvent(eventData);
        }

        /// <summary>
        /// Generate an opportunity event
        /// </summary>
        public void GenerateOpportunityEvent(string title, string description)
        {
            var eventData = new EventData(title, description, EventType.Opportunity, EventSeverity.Medium)
            {
                timeRemaining = 75f,
                expectedImpact = 5f,
                expectedImpacts = new List<string> { "Positive community impact", "Engagement boost", "PR opportunity" }
            };

            TriggerEvent(eventData);
        }

        private void CheckForRandomEvents()
        {
            if (Time.time < nextRandomEventTime) return;
            if (activeEvents.Count >= maxSimultaneousEvents) return;

            if (Random.Range(0f, 1f) < randomEventChance)
            {
                GenerateRandomEvent();
                ScheduleNextRandomEvent();
            }
        }

        private void ScheduleNextRandomEvent()
        {
            float nextEventDelay = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
            nextRandomEventTime = Time.time + nextEventDelay;
        }

        private void GenerateRandomEvent()
        {
            var eventTypes = new EventType[] { EventType.Crisis, EventType.Opportunity, EventType.Community, EventType.Technical, EventType.Competitive };
            var selectedType = eventTypes[Random.Range(0, eventTypes.Length)];

            EventData eventData = selectedType switch
            {
                EventType.Crisis => CreateCrisisEvent(),
                EventType.Opportunity => CreateOpportunityEvent(),
                EventType.Community => CreateCommunityEvent(),
                EventType.Technical => CreateTechnicalEvent(),
                EventType.Competitive => CreateCompetitiveEvent(),
                _ => CreateCommunityEvent()
            };

            TriggerEvent(eventData);
        }

        #endregion

        #region Event Templates

        private EventData CreateCrisisEvent()
        {
            var crisisEvents = new[]
            {
                ("Character Exploit Discovered", "Players found a way to infinitely stack damage. Community demands immediate fix.", EventSeverity.Critical),
                ("Server Stability Issues", "Frequent disconnections during ranked matches. Competitive integrity at risk.", EventSeverity.High),
                ("Balance Complaint Surge", "Multiple characters reported as overpowered. Community sentiment dropping.", EventSeverity.Medium),
                ("Tournament Bug", "Critical bug discovered just before major tournament. Quick decision needed.", EventSeverity.Critical)
            };

            var selected = crisisEvents[Random.Range(0, crisisEvents.Length)];
            return new EventData(selected.Item1, selected.Item2, EventType.Crisis, selected.Item3)
            {
                timeRemaining = selected.Item3 == EventSeverity.Critical ? 30f : 60f,
                expectedImpact = selected.Item3 == EventSeverity.Critical ? 9f : 6f,
                expectedImpacts = new List<string> { "Community backlash", "Player retention risk", "Competitive impact" }
            };
        }

        private EventData CreateOpportunityEvent()
        {
            var opportunityEvents = new[]
            {
                ("Viral Gameplay Clip", "Amazing play went viral. Perfect time to highlight current meta."),
                ("Streamer Showcase", "Popular streamer wants to feature your game. Great PR opportunity."),
                ("Tournament Success", "Recent tournament had amazing viewership. Community excitement high."),
                ("Community Creation", "Fan-made content gaining traction. Opportunity to engage.")
            };

            var selected = opportunityEvents[Random.Range(0, opportunityEvents.Length)];
            return new EventData(selected.Item1, selected.Item2, EventType.Opportunity, EventSeverity.Medium)
            {
                timeRemaining = 75f,
                expectedImpact = 5f,
                expectedImpacts = new List<string> { "Positive PR", "Community engagement", "Player growth" }
            };
        }

        private EventData CreateCommunityEvent()
        {
            var communityEvents = new[]
            {
                ("Weekly Feedback Summary", "Community sentiment analysis available. Mixed reactions to recent changes."),
                ("Player Survey Results", "Monthly player satisfaction survey completed. Some interesting insights."),
                ("Forum Discussion Trending", "Hot debate about character balance trending on community forums."),
                ("Content Creator Feedback", "Several content creators shared thoughts on current meta state.")
            };

            var selected = communityEvents[Random.Range(0, communityEvents.Length)];
            return new EventData(selected.Item1, selected.Item2, EventType.Community, EventSeverity.Low)
            {
                timeRemaining = 90f,
                expectedImpact = 3f,
                expectedImpacts = new List<string> { "Community insights", "Sentiment data", "Feedback trends" }
            };
        }

        private EventData CreateTechnicalEvent()
        {
            var technicalEvents = new[]
            {
                ("Performance Optimization", "New optimization patch ready. Could affect character timings."),
                ("Server Maintenance", "Scheduled maintenance window. Opportunity for hotfixes."),
                ("Platform Update", "Game platform pushing new features. Integration opportunity."),
                ("Analytics Update", "New player behavior data available. Insights into balance impact.")
            };

            var selected = technicalEvents[Random.Range(0, technicalEvents.Length)];
            return new EventData(selected.Item1, selected.Item2, EventType.Technical, EventSeverity.Low)
            {
                timeRemaining = 120f,
                expectedImpact = 2f,
                expectedImpacts = new List<string> { "Technical improvement", "Performance impact", "Data insights" }
            };
        }

        private EventData CreateCompetitiveEvent()
        {
            var competitiveEvents = new[]
            {
                ("Pro Player Complaint", "Professional player raised concerns about character balance."),
                ("Tournament Meta Shift", "Unexpected strategy dominated recent tournament matches."),
                ("Ranking System Feedback", "Competitive players requesting ranking system adjustments."),
                ("Esports Partnership", "Potential esports organization partnership opportunity.")
            };

            var selected = competitiveEvents[Random.Range(0, competitiveEvents.Length)];
            return new EventData(selected.Item1, selected.Item2, EventType.Competitive, EventSeverity.Medium)
            {
                timeRemaining = 60f,
                expectedImpact = 4f,
                expectedImpacts = new List<string> { "Competitive balance", "Pro player satisfaction", "Esports impact" }
            };
        }

        #endregion

        #region Event Management

        private void DisplayEvent(EventData eventData)
        {
            if (eventUIManager == null)
            {
                Debug.LogWarning("⚠️ Cannot display event: EventUIManager not found");
                return;
            }

            activeEvents.Add(eventData);
            eventUIManager.CreateEvent(eventData);
            lastEventTime = Time.time;

            OnEventTriggered.Invoke(eventData);
            OnEventSystemMessage.Invoke($"Event triggered: {eventData.title}");

            Debug.Log($"🎭 Event displayed: {eventData.title}");
        }

        private void ProcessEventQueue()
        {
            if (eventQueue.Count == 0) return;
            if (activeEvents.Count >= maxSimultaneousEvents) return;

            var nextEvent = eventQueue.Dequeue();
            DisplayEvent(nextEvent);
        }

        private void UpdateActiveEvents()
        {
            // Remove expired events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                if (activeEvents[i].IsExpired())
                {
                    var expiredEvent = activeEvents[i];
                    activeEvents.RemoveAt(i);
                    OnEventExpired.Invoke(expiredEvent);
                    Debug.Log($"⏰ Event expired: {expiredEvent.title}");
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get all currently active events
        /// </summary>
        public List<EventData> GetActiveEvents()
        {
            return new List<EventData>(activeEvents);
        }

        /// <summary>
        /// Check if any critical events are active
        /// </summary>
        public bool HasCriticalEvents()
        {
            return activeEvents.Any(e => e.severity == EventSeverity.Critical);
        }

        /// <summary>
        /// Force clear all events (for testing)
        /// </summary>
        public void ClearAllEvents()
        {
            activeEvents.Clear();
            eventQueue.Clear();
            if (eventUIManager != null)
                eventUIManager.ClearAllEvents();
        }

        /// <summary>
        /// React to game state changes
        /// </summary>
        public void OnGameStateChanged(float communitySentiment, float balanceChange)
        {
            // Check for sentiment-triggered events
            if (triggerOnLowSentiment && communitySentiment < lowSentimentThreshold)
            {
                GenerateCrisisEvent("Community Unrest", "Player satisfaction has dropped significantly. Immediate action may be needed.", EventSeverity.High);
            }
            else if (triggerOnHighSentiment && communitySentiment > highSentimentThreshold)
            {
                GenerateOpportunityEvent("Community Excitement", "Players are very happy with recent changes. Great time to build on this momentum.");
            }

            // Check for major balance change events
            if (triggerOnMajorChanges && Mathf.Abs(balanceChange) > majorChangeThreshold)
            {
                GenerateCrisisEvent("Major Balance Impact", "Recent changes have significantly shifted the meta. Community reactions incoming.", EventSeverity.Medium);
            }

            lastCommunitySentiment = communitySentiment;
        }

        #endregion

        #region Debug Methods

        [ContextMenu("🎭 Generate Random Event")]
        public void DebugGenerateRandomEvent()
        {
            GenerateRandomEvent();
        }

        [ContextMenu("🚨 Generate Crisis Event")]
        public void DebugGenerateCrisisEvent()
        {
            GenerateCrisisEvent("DEBUG Crisis", "This is a test crisis event for debugging purposes.", EventSeverity.Critical);
        }

        [ContextMenu("⭐ Generate Opportunity Event")]
        public void DebugGenerateOpportunityEvent()
        {
            GenerateOpportunityEvent("DEBUG Opportunity", "This is a test opportunity event for debugging purposes.");
        }

        [ContextMenu("🧹 Clear All Events")]
        public void DebugClearAllEvents()
        {
            ClearAllEvents();
        }

        #endregion
    }
}