using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using MetaBalance.Events;

namespace MetaBalance.UI
{
    /// <summary>
    /// Enhanced EventUIManager that only spawns events during Event Phase
    /// Queues events that come in during other phases for later display
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
        
        [Header("Phase Management")]
        [SerializeField] private bool onlyShowDuringEventPhase = true;
        [SerializeField] private string waitingForEventPhaseMessage = "📅 Events will appear during the Event Phase";
        
        [Header("Event Generation (For Testing)")]
        [SerializeField] private bool enableTestEvents = false;
        [SerializeField] private float testEventInterval = 5f;
        
        // Event tracking
        private List<EventUIItem> activeEventItems = new List<EventUIItem>();
        private List<EventData> activeEvents = new List<EventData>();
        private Queue<EventData> queuedEvents = new Queue<EventData>(); // Events waiting for Event Phase
        private float lastTestEventTime = 0f;
        
        // Phase tracking
        private bool isEventPhase = false;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeEventSystem();
            SubscribeToPhaseManager();
            CheckCurrentPhase();
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
        
        private void OnDestroy()
        {
            // Unsubscribe from phase manager
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeEventSystem()
        {
            // Clear any existing events
            ClearAllEvents();
            
            // Initialize UI
            UpdateNoEventsDisplay();
            
            Debug.Log("🎮 Event UI Manager initialized with phase management");
        }
        
        private void SubscribeToPhaseManager()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Debug.Log("📅 EventUIManager subscribed to phase changes");
            }
            else
            {
                Debug.LogWarning("⚠️ PhaseManager.Instance not found - events will always be visible");
            }
        }
        
        private void CheckCurrentPhase()
        {
            if (Core.PhaseManager.Instance != null)
            {
                var currentPhase = Core.PhaseManager.Instance.GetCurrentPhase();
                Debug.Log($"🔍 CheckCurrentPhase: {currentPhase}");
                OnPhaseChanged(currentPhase);
            }
            else
            {
                Debug.LogWarning("⚠️ PhaseManager.Instance is NULL during CheckCurrentPhase!");
            }
        }
        
        #endregion
        
        #region Phase Management
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            bool wasEventPhase = isEventPhase;
            isEventPhase = (newPhase == Core.GamePhase.Event);
            
            Debug.Log($"🎭 EventUIManager phase changed: {newPhase} - Event phase: {isEventPhase} - Was event phase: {wasEventPhase}");
            
            if (isEventPhase && !wasEventPhase)
            {
                // Entering Event Phase - spawn queued events
                Debug.Log($"🎬 ENTERING Event Phase!");
                OnEnterEventPhase();
            }
            else if (!isEventPhase && wasEventPhase)
            {
                // Leaving Event Phase - archive events
                Debug.Log($"📚 LEAVING Event Phase!");
                OnLeaveEventPhase();
            }
            else
            {
                Debug.Log($"🔄 Phase changed but no Event Phase transition");
            }
            
            UpdateNoEventsDisplay();
        }
        
        private void OnEnterEventPhase()
        {
            Debug.Log($"🎬 OnEnterEventPhase called - Processing {queuedEvents.Count} queued events");
            
            int processedCount = 0;
            // Spawn all queued events
            while (queuedEvents.Count > 0)
            {
                var queuedEvent = queuedEvents.Dequeue();
                Debug.Log($"  📤 Processing queued event {processedCount + 1}: {queuedEvent.title}");
                CreateEventImmediately(queuedEvent);
                processedCount++;
            }
            
            Debug.Log($"✅ Processed {processedCount} queued events. Active events now: {activeEventItems.Count}");
        }
        
        private void OnLeaveEventPhase()
        {
            Debug.Log($"📚 Leaving Event Phase - Archiving {activeEventItems.Count} active events");
            
            // Move all active events to history (or just clear them)
            ArchiveAllActiveEvents();
        }
        
        private void ArchiveAllActiveEvents()
        {
            // Mark all current events as historical
            foreach (var eventItem in activeEventItems)
            {
                if (eventItem != null)
                {
                    eventItem.MarkAsHistorical();
                }
            }
            
            // Clear the active lists
            activeEventItems.Clear();
            activeEvents.Clear();
            
            Debug.Log("📚 All events archived");
        }
        
        #endregion
        
        #region Event Management
        
        /// <summary>
        /// Create and display a new event (respects phase restrictions)
        /// </summary>
        public void CreateEvent(EventData eventData)
        {
            if (eventData == null)
            {
                Debug.LogError("❌ Cannot create event: EventData is null");
                return;
            }
            
            Debug.Log($"🎬 CreateEvent called: '{eventData.title}' - Current phase: {(isEventPhase ? "Event" : "Not Event")} - Only show during event phase: {onlyShowDuringEventPhase}");
            
            if (onlyShowDuringEventPhase && !isEventPhase)
            {
                // Queue the event for later
                queuedEvents.Enqueue(eventData);
                Debug.Log($"📅 Event queued for Event Phase: {eventData.title} (Queue size: {queuedEvents.Count})");
                UpdateNoEventsDisplay();
                return;
            }
            
            // Create immediately
            Debug.Log($"⚡ Creating event immediately: {eventData.title}");
            CreateEventImmediately(eventData);
        }
        
        /// <summary>
        /// Create event immediately without phase checks
        /// </summary>
        private void CreateEventImmediately(EventData eventData)
        {
            Debug.Log($"⚡ CreateEventImmediately called for: {eventData.title}");
            
            // Check if we have too many events
            if (activeEventItems.Count >= maxVisibleEvents)
            {
                Debug.Log($"🗑️ Removing oldest event - too many active ({activeEventItems.Count}/{maxVisibleEvents})");
                RemoveOldestEvent();
            }
            
            // Check required components
            if (eventItemPrefab == null)
            {
                Debug.LogError("❌ eventItemPrefab is NULL! Assign the prefab in inspector.");
                return;
            }
            
            if (eventContainer == null)
            {
                Debug.LogError("❌ eventContainer is NULL! Assign the container in inspector.");
                return;
            }
            
            Debug.Log($"📦 Instantiating event prefab...");
            
            // Instantiate new event item
            GameObject eventObj = Instantiate(eventItemPrefab, eventContainer);
            EventUIItem eventItem = eventObj.GetComponent<EventUIItem>();
            
            if (eventItem == null)
            {
                Debug.LogError("❌ Event prefab missing EventUIItem component!");
                Destroy(eventObj);
                return;
            }
            
            Debug.Log($"🎯 Setting up event item...");
            
            // Setup the event item
            eventItem.SetupEvent(eventData, OnEventResponded, OnEventDismissed);
            
            // Add to tracking lists
            activeEventItems.Add(eventItem);
            activeEvents.Add(eventData);
            
            // Update UI state
            UpdateNoEventsDisplay();
            
            // Move to top
            eventObj.transform.SetAsFirstSibling();
            
            Debug.Log($"✅ Successfully created event: {eventData.title} - Total active events: {activeEventItems.Count}");
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
        
        /// <summary>
        /// RefreshDisplay method for EventUIItem compatibility
        /// </summary>
        public void RefreshDisplay()
        {
            RefreshAllEventItems();
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
            if (noEventsText == null) return;
            
            bool hasActiveEvents = activeEvents.Count > 0;
            bool hasQueuedEvents = queuedEvents.Count > 0;
            
            if (hasActiveEvents)
            {
                // Has active events - hide the message
                noEventsText.gameObject.SetActive(false);
            }
            else if (!isEventPhase && hasQueuedEvents)
            {
                // Not event phase but has queued events
                noEventsText.gameObject.SetActive(true);
                noEventsText.text = $"{waitingForEventPhaseMessage}\n({queuedEvents.Count} events queued)";
            }
            else if (!isEventPhase && !hasQueuedEvents)
            {
                // Not event phase and no queued events
                noEventsText.gameObject.SetActive(true);
                noEventsText.text = waitingForEventPhaseMessage;
            }
            else
            {
                // Event phase but no events
                noEventsText.gameObject.SetActive(true);
                noEventsText.text = "📋 No active events\nEverything is running smoothly!";
            }
        }
        
        #endregion
        
        #region Event Callbacks
        
        private void OnEventResponded(EventData eventData)
        {
            Debug.Log($"📝 Event responded to: {eventData.title}");
            
            // Find and mark the event as handled
            for (int i = 0; i < activeEvents.Count; i++)
            {
                if (activeEvents[i] == eventData)
                {
                    // Event is already marked as handled by the EventUIItem
                    break;
                }
            }
        }
        
        private void OnEventDismissed(EventData eventData)
        {
            Debug.Log($"❌ Event dismissed: {eventData.title}");
            
            // Find and remove the event
            for (int i = 0; i < activeEvents.Count; i++)
            {
                if (activeEvents[i] == eventData)
                {
                    RemoveEvent(i);
                    break;
                }
            }
        }
        
        #endregion
        
        #region Timer Management
        
        private void UpdateEventTimers()
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
                        ExpireEvent(i);
                    }
                }
            }
        }
        
        private void ExpireEvent(int index)
        {
            if (index >= 0 && index < activeEventItems.Count)
            {
                var eventItem = activeEventItems[index];
                var eventData = activeEvents[index];
                
                // Trigger expiration event
                if (eventItem != null)
                {
                    eventItem.OnEventExpired?.Invoke(eventData);
                }
                
                RemoveEvent(index);
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Clear all events - compatible with your existing EventManager.ClearAllEvents() call
        /// </summary>
        public void ClearAllEvents()
        {
            // Clear active events
            foreach (var item in activeEventItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            
            activeEventItems.Clear();
            activeEvents.Clear();
            
            // Clear queued events
            queuedEvents.Clear();
            
            UpdateNoEventsDisplay();
            
            Debug.Log("🧹 All events cleared");
        }
        
        public void ClearQueuedEvents()
        {
            queuedEvents.Clear();
            UpdateNoEventsDisplay();
            Debug.Log("🧹 Queued events cleared");
        }
        
        #endregion
        
        #region Testing and Debug
        
        private void GenerateRandomTestEvent()
        {
            if (!enableTestEvents) return;
            
            var testEvents = new[]
            {
                new EventData("Test Community Event", "This is a test community event.", MetaBalance.Events.EventType.Community, MetaBalance.Events.EventSeverity.Medium),
                new EventData("Test Crisis Event", "This is a test crisis requiring immediate attention.", MetaBalance.Events.EventType.Crisis, MetaBalance.Events.EventSeverity.Critical),
                new EventData("Test Technical Event", "A technical issue has been detected.", MetaBalance.Events.EventType.Technical, MetaBalance.Events.EventSeverity.High),
                new EventData("Test Opportunity", "A new opportunity has emerged.", MetaBalance.Events.EventType.Opportunity, MetaBalance.Events.EventSeverity.Low)
            };
            
            var randomEvent = testEvents[Random.Range(0, testEvents.Length)];
            randomEvent.timeRemaining = Random.Range(30f, 120f);
            
            CreateEvent(randomEvent);
        }
        
        [ContextMenu("🧪 Generate Test Event")]
        public void TestGenerateEvent()
        {
            GenerateRandomTestEvent();
        }
        
        [ContextMenu("🧪 Force Generate Crisis Event")]
        public void TestForceCrisisEvent()
        {
            var crisisEvent = new EventData(
                "FORCED Crisis Event", 
                "This is a manually forced crisis event for testing.", 
                MetaBalance.Events.EventType.Crisis, 
                MetaBalance.Events.EventSeverity.Critical
            );
            crisisEvent.timeRemaining = 60f;
            CreateEvent(crisisEvent);
            Debug.Log("🚨 FORCED Crisis Event Created!");
        }
        
        [ContextMenu("🧪 Clear All Events")]
        public void TestClearAllEvents()
        {
            ClearAllEvents();
        }
        
        [ContextMenu("🧪 Clear Queued Events")]
        public void TestClearQueuedEvents()
        {
            ClearQueuedEvents();
        }
        
        [ContextMenu("📊 Debug Event State")]
        public void DebugEventState()
        {
            Debug.Log("=== 📊 EVENT MANAGER DEBUG ===");
            Debug.Log($"Is Event Phase: {isEventPhase}");
            Debug.Log($"Active Events: {activeEvents.Count}");
            Debug.Log($"Queued Events: {queuedEvents.Count}");
            Debug.Log($"Only Show During Event Phase: {onlyShowDuringEventPhase}");
            
            if (Core.PhaseManager.Instance != null)
            {
                Debug.Log($"Current Phase: {Core.PhaseManager.Instance.GetCurrentPhase()}");
            }
            else
            {
                Debug.Log("❌ PhaseManager.Instance is NULL!");
            }
            
            Debug.Log("Active Events:");
            for (int i = 0; i < activeEvents.Count; i++)
            {
                Debug.Log($"  {i}: {activeEvents[i].title}");
            }
            
            Debug.Log("Queued Events:");
            int queueIndex = 0;
            foreach (var queuedEvent in queuedEvents)
            {
                Debug.Log($"  {queueIndex}: {queuedEvent.title}");
                queueIndex++;
            }
            
            // Check EventManager connection
            var eventManager = FindObjectOfType<Events.EventManager>();
            if (eventManager != null)
            {
                Debug.Log($"✅ EventManager found: {eventManager.name}");
            }
            else
            {
                Debug.Log("❌ EventManager NOT FOUND! This could be why no events are spawning.");
            }
        }
        
        [ContextMenu("🧪 Force Event Phase")]
        public void TestForceEventPhase()
        {
            Debug.Log("🎭 FORCING Event Phase...");
            OnPhaseChanged(Core.GamePhase.Event);
        }
        
        [ContextMenu("🧪 Force Planning Phase")]
        public void TestForcePlanningPhase()
        {
            Debug.Log("🎭 FORCING Planning Phase...");
            OnPhaseChanged(Core.GamePhase.Planning);
        }
        
        [ContextMenu("🔌 Test EventManager Integration")]
        public void TestEventManagerIntegration()
        {
            Debug.Log("🔌 Testing EventManager Integration...");
            
            var eventManager = FindObjectOfType<Events.EventManager>();
            if (eventManager == null)
            {
                Debug.LogError("❌ EventManager not found! Events won't generate automatically.");
                return;
            }
            
            Debug.Log("✅ EventManager found! Triggering test event through EventManager...");
            
            // Try to trigger an event through the EventManager
            eventManager.GenerateCrisisEvent("EventManager Test", "This event was triggered through EventManager to test integration.");
        }
        
        [ContextMenu("🎯 Test Direct Event Creation")]
        public void TestDirectEventCreation()
        {
            Debug.Log("🎯 Testing Direct Event Creation...");
            
            var testEvent = new EventData(
                "Direct Creation Test", 
                "This event was created directly in EventUIManager.", 
                MetaBalance.Events.EventType.Community, 
                MetaBalance.Events.EventSeverity.Medium
            );
            testEvent.timeRemaining = 30f;
            
            Debug.Log($"Creating event directly... Current phase: {(isEventPhase ? "Event" : "Other")}");
            CreateEvent(testEvent);
            Debug.Log("✅ Direct event creation completed!");
        }
        
        [ContextMenu("🎯 Force Create Event (Bypass Phase)")]
        public void TestForceCreateEventBypassPhase()
        {
            Debug.Log("🎯 FORCING event creation (bypassing phase restrictions)...");
            
            var testEvent = new EventData(
                "FORCED Event (No Phase Check)", 
                "This event was forced to spawn regardless of phase.", 
                MetaBalance.Events.EventType.Crisis, 
                MetaBalance.Events.EventSeverity.High
            );
            testEvent.timeRemaining = 45f;
            
            // Bypass phase restrictions by calling CreateEventImmediately directly
            CreateEventImmediately(testEvent);
            Debug.Log("✅ Force event creation completed!");
        }
        
        #endregion
        
        #region Public Interface
        
        public int GetActiveEventCount()
        {
            return activeEvents.Count;
        }
        
        public int GetQueuedEventCount()
        {
            return queuedEvents.Count;
        }
        
        public bool IsEventPhase()
        {
            return isEventPhase;
        }
        
        public void SetOnlyShowDuringEventPhase(bool value)
        {
            onlyShowDuringEventPhase = value;
            UpdateNoEventsDisplay();
        }
        
        #endregion
    }
}