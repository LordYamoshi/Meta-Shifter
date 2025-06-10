using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using MetaBalance.Events;

namespace MetaBalance.UI
{
    /// <summary>
    /// Clean Event UI Manager that works perfectly with your existing setup
    /// No references to missing classes - just pure event management
    /// Completely rewritten to fix all method call issues
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
        [SerializeField] private bool advanceEventsOnWeekChange = true;
        [SerializeField] private bool advanceEventsOnPhaseChange = false;
        
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
        }
        
        private void Update()
        {
            // Generate test events if enabled
            if (enableTestEvents && Time.time - lastTestEventTime >= testEventInterval)
            {
                GenerateRandomTestEvent();
                lastTestEventTime = Time.time;
            }
            
            // Update event timers and remove expired events
            UpdateEventTimers();
        }
        
        #region Initialization
        
        private void InitializeEventSystem()
        {
            // Clear any existing events
            ClearAllEvents();
            
            // Initialize UI
            UpdateNoEventsDisplay();
            
            Debug.Log("🎮 Event UI Manager initialized");
        }
        
        #endregion
        
        #region Event Management
        
        /// <summary>
        /// Create and display a new event
        /// </summary>
        public void CreateEvent(EventData eventData)
        {
            if (eventData == null)
            {
                Debug.LogError("❌ Cannot create event: EventData is null");
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
            
            Debug.Log($"✅ Created event: {eventData.title}");
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
            
            Debug.Log($"🗑️ Removed oldest event: {oldestEvent?.title ?? "Unknown"}");
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
            
            Debug.Log($"⏰ Event removed: {eventData?.title ?? "Unknown"}");
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
        
        private void UpdateEventTimers()
        {
            // Remove expired events
            for (int i = activeEvents.Count - 1; i >= 0; i--)
            {
                if (activeEvents[i] != null && activeEvents[i].isResolved)
                {
                    RemoveEvent(i);
                }
            }
        }
        
        #endregion
        
        #region Event Callbacks
        
        private void OnEventResponded(EventData eventData)
        {
            Debug.Log($"🎯 Player responded to event: {eventData.title}");
            
            // Mark as resolved
            eventData.isResolved = true;
            
            // Apply event effects based on type
            ApplyEventEffects(eventData, true);
            
            // Remove from active events
            RemoveEventByData(eventData);
        }
        
        private void OnEventDismissed(EventData eventData)
        {
            Debug.Log($"🗑️ Player dismissed event: {eventData.title}");
            
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
            switch (eventData.eventType)
            {
                case MetaBalance.Events.EventType.Crisis:
                    ApplyCrisisEffects(eventData, responded);
                    break;
                case MetaBalance.Events.EventType.Opportunity:
                    ApplyOpportunityEffects(eventData, responded);
                    break;
                case MetaBalance.Events.EventType.Community:
                    ApplyCommunityEffects(eventData, responded);
                    break;
                case MetaBalance.Events.EventType.Technical:
                    ApplyTechnicalEffects(eventData, responded);
                    break;
                case MetaBalance.Events.EventType.Competitive:
                    ApplyCompetitiveEffects(eventData, responded);
                    break;
                default:
                    Debug.Log($"📝 Generic event effect: {eventData.title}");
                    break;
            }
        }
        
        private void ApplyCrisisEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"🚨 Crisis resolved: {eventData.title}");
                // Apply positive effects to community sentiment
            }
            else
            {
                Debug.Log($"💥 Crisis ignored: {eventData.title}");
                // Apply negative effects to community sentiment
            }
        }
        
        private void ApplyOpportunityEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"⭐ Opportunity seized: {eventData.title}");
                // Apply benefits, boost engagement
            }
            else
            {
                Debug.Log($"😔 Opportunity missed: {eventData.title}");
                // Small negative impact or neutral
            }
        }
        
        private void ApplyCommunityEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"💬 Community addressed: {eventData.title}");
            }
            else
            {
                Debug.Log($"😠 Community ignored: {eventData.title}");
            }
        }
        
        private void ApplyTechnicalEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"🔧 Technical issue fixed: {eventData.title}");
            }
            else
            {
                Debug.Log($"⚠️ Technical issue persists: {eventData.title}");
            }
        }
        
        private void ApplyCompetitiveEffects(EventData eventData, bool responded)
        {
            if (responded)
            {
                Debug.Log($"⚔️ Competitive action taken: {eventData.title}");
            }
            else
            {
                Debug.Log($"📉 Competitive opportunity lost: {eventData.title}");
            }
        }
        
        #endregion
        
        #region Test Event Generation
        
        private void GenerateRandomTestEvent()
        {
            // Use the correct method names from the rewritten EventFactory
            var eventChoice = Random.Range(0, 7);
            
            EventData selectedEvent;
            
            switch (eventChoice)
            {
                case 0:
                    selectedEvent = EventFactory.CreateGameBreakingExploitEvent();
                    break;
                case 1:
                    selectedEvent = EventFactory.CreateTournamentOpportunityEvent();
                    break;
                case 2:
                    selectedEvent = EventFactory.CreateCreatorCollaborationEvent();
                    break;
                case 3:
                    selectedEvent = EventFactory.CreateChampionshipMetaEvent();
                    break;
                case 4:
                    selectedEvent = EventFactory.CreateViralMomentEvent();
                    break;
                case 5:
                    selectedEvent = EventFactory.CreateServerCrisisEvent();
                    break;
                case 6:
                    selectedEvent = EventFactory.CreateFeedbackSurgeEvent();
                    break;
                default:
                    selectedEvent = EventFactory.GetAnyRandomEvent();
                    break;
            }
            
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
            return activeEvents.Count(e => e.severity == severity);
        }
        
        /// <summary>
        /// Check if any critical events are active
        /// </summary>
        public bool HasCriticalEvents()
        {
            return activeEvents.Any(e => e.severity == EventSeverity.Critical);
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
        
        #endregion
        
        #region Debug Context Menu Methods
        
        [ContextMenu("🧪 Generate Game-Breaking Exploit")]
        public void DebugGenerateGameBreakingExploit()
        {
            var testEvent = EventFactory.CreateGameBreakingExploitEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🏆 Generate Tournament Opportunity")]
        public void DebugGenerateTournamentOpportunity()
        {
            var testEvent = EventFactory.CreateTournamentOpportunityEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🎬 Generate Creator Collaboration")]
        public void DebugGenerateCreatorCollaboration()
        {
            var testEvent = EventFactory.CreateCreatorCollaborationEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("📊 Generate Championship Meta Analysis")]
        public void DebugGenerateChampionshipMeta()
        {
            var testEvent = EventFactory.CreateChampionshipMetaEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("⭐ Generate Viral Moment")]
        public void DebugGenerateViralMoment()
        {
            var testEvent = EventFactory.CreateViralMomentEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🔧 Generate Server Crisis")]
        public void DebugGenerateServerCrisis()
        {
            var testEvent = EventFactory.CreateServerCrisisEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("💬 Generate Feedback Surge")]
        public void DebugGenerateFeedbackSurge()
        {
            var testEvent = EventFactory.CreateFeedbackSurgeEvent();
            CreateEvent(testEvent);
        }
        
        [ContextMenu("🧹 Clear All Events")]
        public void DebugClearAllEvents()
        {
            ClearAllEvents();
        }
        
        [ContextMenu("🎲 Generate Any Random Event")]
        public void DebugGenerateAnyRandomEvent()
        {
            var testEvent = EventFactory.GetAnyRandomEvent();
            CreateEvent(testEvent);
        }
        
        #endregion
    }
}