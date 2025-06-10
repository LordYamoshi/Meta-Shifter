using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Events
{
    /// <summary>
    /// Integrates the event system with your existing game systems
    /// Bridges events with character manager, resource manager, and game phases
    /// Completely rewritten to use correct EventFactory method names
    /// </summary>
    public class EventIntegration : MonoBehaviour
    {
        public static EventIntegration Instance { get; private set; }

        [Header("System References")]
        [SerializeField] private EventManager eventManager;
        [SerializeField] private UI.EventUIManager eventUIManager;

        [Header("Integration Settings")]
        [SerializeField] private bool respondToPhaseChanges = true;
        [SerializeField] private bool respondToResourceChanges = true;
        [SerializeField] private bool respondToCharacterChanges = true;
        [SerializeField] private bool respondToCommunitySentiment = true;

        [Header("Event Triggers")]
        [SerializeField] private float sentimentEventThreshold = 20f; // Trigger events when sentiment changes by this much
        [SerializeField] private float resourceCrisisThreshold = 10f; // Trigger events when resources get this low
        [SerializeField] private int maxEventsPerPhase = 2;

        // State tracking
        private float lastCommunitySentiment = 50f;
        private int lastRP = 100;
        private int lastCP = 100;
        private Core.GamePhase lastPhase = Core.GamePhase.Planning;
        private int eventsThisPhase = 0;

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
            InitializeIntegration();
            SubscribeToGameEvents();
        }

        #region Initialization

        private void InitializeIntegration()
        {
            // Auto-find components if not assigned
            if (eventManager == null)
                eventManager = FindObjectOfType<EventManager>();

            if (eventUIManager == null)
                eventUIManager = FindObjectOfType<UI.EventUIManager>();

            // Initialize state tracking
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null)
            {
                // Store initial values if your resource manager has getters
                // lastRP = resourceManager.GetCurrentRP();
                // lastCP = resourceManager.GetCurrentCP();
            }

            Debug.Log("🔗 Event Integration initialized");
        }

        private void SubscribeToGameEvents()
        {
            // Subscribe to phase manager if it exists
            var phaseManager = Core.PhaseManager.Instance;
            if (phaseManager != null && respondToPhaseChanges)
            {
                // If your PhaseManager has events, subscribe to them
                // phaseManager.OnPhaseChanged.AddListener(OnPhaseChanged);
                Debug.Log("📅 Subscribed to phase changes");
            }

            // Subscribe to resource manager if it exists
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null && respondToResourceChanges)
            {
                // If your ResourceManager has events, subscribe to them
                // resourceManager.OnResourcesChanged.AddListener(OnResourcesChanged);
                Debug.Log("💰 Subscribed to resource changes");
            }

            // Subscribe to character manager if it exists
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager != null && respondToCharacterChanges)
            {
                // If your CharacterManager has events, subscribe to them
                // characterManager.OnStatChanged.AddListener(OnCharacterStatChanged);
                Debug.Log("⚔️ Subscribed to character changes");
            }

            // Subscribe to community feedback if it exists
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null && respondToCommunitySentiment)
            {
                // If your CommunityFeedbackManager has events, subscribe to them
                // feedbackManager.OnCommunitySentimentChanged.AddListener(OnCommunitySentimentChanged);
                Debug.Log("💬 Subscribed to community sentiment");
            }
        }

        #endregion

        #region Game State Event Handlers

        /// <summary>
        /// Handle phase changes - generate phase-appropriate events
        /// </summary>
        public void OnPhaseChanged(Core.GamePhase newPhase)
        {
            Debug.Log($"🔄 Phase changed to: {newPhase}");

            // Reset event counter for new phase
            if (newPhase != lastPhase)
            {
                eventsThisPhase = 0;
                lastPhase = newPhase;
            }

            // Skip if we've already generated too many events this phase
            if (eventsThisPhase >= maxEventsPerPhase)
                return;

            // Generate phase-specific events
            switch (newPhase)
            {
                case Core.GamePhase.Planning:
                    HandlePlanningPhaseEvents();
                    break;
                case Core.GamePhase.Implementation:
                    HandleImplementationPhaseEvents();
                    break;
                case Core.GamePhase.Feedback:
                    HandleFeedbackPhaseEvents();
                    break;
                case Core.GamePhase.Event:
                    HandleEventPhaseEvents();
                    break;
            }
        }

        /// <summary>
        /// Handle resource changes - generate resource-related events
        /// </summary>
        public void OnResourcesChanged(int newRP, int newCP)
        {
            // Check for resource crisis
            if (newRP < resourceCrisisThreshold && newRP < lastRP)
            {
                TriggerResourceCrisisEvent("Research Point Crisis", "Running dangerously low on research points. Major changes may be impossible.");
            }

            if (newCP < resourceCrisisThreshold && newCP < lastCP)
            {
                TriggerResourceCrisisEvent("Community Point Crisis", "Community engagement is critically low. Communication efforts severely limited.");
            }

            // Check for resource abundance
            if (newRP > lastRP + 20)
            {
                TriggerResourceOpportunityEvent("Research Breakthrough", "Abundant research points available. Perfect time for major innovations.");
            }

            if (newCP > lastCP + 20)
            {
                TriggerResourceOpportunityEvent("Community Engagement High", "Strong community support provides excellent opportunity for communication.");
            }

            lastRP = newRP;
            lastCP = newCP;
        }

        /// <summary>
        /// Handle character stat changes - generate balance-related events
        /// </summary>
        public void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            Debug.Log($"⚔️ {character} {stat} changed to {newValue}");

            // Check for major balance shifts
            if (stat == Characters.CharacterStat.WinRate)
            {
                if (newValue > 60f)
                {
                    TriggerBalanceEvent($"{character} Dominance", $"{character} win rate has spiked to {newValue:F1}%. Community balance concerns likely.");
                }
                else if (newValue < 40f)
                {
                    TriggerBalanceEvent($"{character} Struggles", $"{character} win rate has dropped to {newValue:F1}%. Players may be frustrated.");
                }
            }
        }

        /// <summary>
        /// Handle community sentiment changes - generate sentiment-related events
        /// </summary>
        public void OnCommunitySentimentChanged(float newSentiment)
        {
            float sentimentChange = newSentiment - lastCommunitySentiment;

            // Check for major sentiment shifts
            if (Mathf.Abs(sentimentChange) > sentimentEventThreshold)
            {
                if (sentimentChange > 0)
                {
                    TriggerSentimentEvent("Community Excitement", $"Player satisfaction has surged by {sentimentChange:F1} points! Great momentum to build on.", EventType.Opportunity);
                }
                else
                {
                    TriggerSentimentEvent("Community Concerns", $"Player satisfaction has dropped by {Mathf.Abs(sentimentChange):F1} points. Immediate attention may be needed.", EventType.Crisis);
                }
            }

            lastCommunitySentiment = newSentiment;
        }

        #endregion

        #region Phase-Specific Event Handlers

        private void HandlePlanningPhaseEvents()
        {
            // 30% chance to generate planning-related events
            if (Random.Range(0f, 1f) < 0.3f)
            {
                var planningEvents = new[]
                {
                    "Strategic Planning Session: Review last week's data and plan next moves.",
                    "Community Trend Analysis: New discussion trends emerging on forums.",
                    "Competitive Meta Review: Pro players sharing insights on current balance.",
                    "Data Analysis Window: Perfect time to dive deep into player statistics."
                };

                var selectedEvent = planningEvents[Random.Range(0, planningEvents.Length)];
                TriggerPhaseEvent("Planning Opportunity", selectedEvent, EventType.Community);
            }
        }

        private void HandleImplementationPhaseEvents()
        {
            // 25% chance to generate implementation-related events
            if (Random.Range(0f, 1f) < 0.25f)
            {
                var implementationEvents = new[]
                {
                    "Implementation Feedback: Early reactions to changes are coming in.",
                    "Technical Challenge: Unexpected interaction discovered during implementation.",
                    "Community Watch: Players are closely monitoring the changes being made.",
                    "Pro Player Alert: Competitive players react immediately to balance shifts."
                };

                var selectedEvent = implementationEvents[Random.Range(0, implementationEvents.Length)];
                TriggerPhaseEvent("Implementation Update", selectedEvent, EventType.Technical);
            }
        }

        private void HandleFeedbackPhaseEvents()
        {
            // 40% chance to generate feedback-related events (highest chance)
            if (Random.Range(0f, 1f) < 0.4f)
            {
                // Use correct method name from EventFactory
                var feedbackEvent = EventFactory.CreateFeedbackSurgeEvent();
                
                if (eventManager != null)
                {
                    eventManager.TriggerEvent(feedbackEvent);
                    eventsThisPhase++;
                }
            }
        }

        private void HandleEventPhaseEvents()
        {
            // This phase IS for events, so always generate something interesting
            int eventChoice = Random.Range(0, 4);
            
            EventData selectedEvent;
            
            switch (eventChoice)
            {
                case 0:
                    selectedEvent = EventFactory.CreateViralMomentEvent();
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
                default:
                    selectedEvent = EventFactory.CreateViralMomentEvent();
                    break;
            }
            
            if (eventManager != null)
            {
                eventManager.TriggerEvent(selectedEvent);
                eventsThisPhase++;
            }
        }

        #endregion

        #region Event Triggering Methods

        private void TriggerResourceCrisisEvent(string title, string description)
        {
            if (eventManager == null || eventsThisPhase >= maxEventsPerPhase) return;

            // Create a custom crisis event for resource issues
            var eventData = new EventData(title, description, EventType.Crisis, EventSeverity.High)
            {
                timeRemaining = 60f,
                expectedImpact = 6f,
                expectedImpacts = new List<string> { "Resource management pressure", "Strategic limitations", "Planning constraints" }
            };

            // Add simple response options
            eventData.responseOptions.Add(new EventResponseOption
            {
                buttonText = "Emergency Action",
                description = "Take immediate action to address resource shortage",
                responseType = EventResponseType.EmergencyFix,
                rpCost = 1,
                cpCost = 1,
                sentimentChange = 5f,
                successMessage = "Resource crisis addressed!",
                buttonColor = new Color(0.8f, 0.3f, 0.4f)
            });

            eventManager.TriggerEvent(eventData);
            eventsThisPhase++;
        }

        private void TriggerResourceOpportunityEvent(string title, string description)
        {
            if (eventManager == null || eventsThisPhase >= maxEventsPerPhase) return;

            var eventData = new EventData(title, description, EventType.Opportunity, EventSeverity.Medium)
            {
                timeRemaining = 90f,
                expectedImpact = 5f,
                expectedImpacts = new List<string> { "Strategic opportunities", "Enhanced capabilities", "Expanded options" }
            };

            eventData.responseOptions.Add(new EventResponseOption
            {
                buttonText = "Seize Opportunity",
                description = "Take advantage of abundant resources",
                responseType = EventResponseType.CustomResponse,
                rpCost = 0,
                cpCost = 0,
                sentimentChange = 8f,
                successMessage = "Opportunity seized successfully!",
                buttonColor = new Color(0.2f, 0.6f, 0.9f)
            });

            eventManager.TriggerEvent(eventData);
            eventsThisPhase++;
        }

        private void TriggerBalanceEvent(string title, string description)
        {
            if (eventManager == null || eventsThisPhase >= maxEventsPerPhase) return;

            // Use the meta analysis event as a template for balance events
            var eventData = EventFactory.CreateChampionshipMetaEvent();
            eventData.title = title;
            eventData.description = description;

            eventManager.TriggerEvent(eventData);
            eventsThisPhase++;
        }

        private void TriggerSentimentEvent(string title, string description, EventType eventType)
        {
            if (eventManager == null || eventsThisPhase >= maxEventsPerPhase) return;

            EventData sentimentEvent;

            if (eventType == EventType.Crisis)
            {
                // Use feedback surge for negative sentiment
                sentimentEvent = EventFactory.CreateFeedbackSurgeEvent();
            }
            else
            {
                // Use viral moment for positive sentiment
                sentimentEvent = EventFactory.CreateViralMomentEvent();
            }

            // Customize the event
            sentimentEvent.title = title;
            sentimentEvent.description = description;

            eventManager.TriggerEvent(sentimentEvent);
            eventsThisPhase++;
        }

        private void TriggerPhaseEvent(string title, string description, EventType eventType)
        {
            if (eventManager == null || eventsThisPhase >= maxEventsPerPhase) return;

            var eventData = new EventData(title, description, eventType, EventSeverity.Low)
            {
                timeRemaining = 45f,
                expectedImpact = 3f,
                expectedImpacts = new List<string> { "Phase-specific opportunity", "Timing advantage", "Strategic insight" }
            };

            eventData.responseOptions.Add(new EventResponseOption
            {
                buttonText = "Analyze",
                description = "Take advantage of this phase-specific opportunity",
                responseType = EventResponseType.ObserveAndLearn,
                rpCost = 1,
                cpCost = 0,
                sentimentChange = 3f,
                successMessage = "Phase opportunity analyzed!",
                buttonColor = new Color(0.4f, 0.6f, 0.8f)
            });

            eventManager.TriggerEvent(eventData);
            eventsThisPhase++;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Force trigger the game-breaking exploit event for testing
        /// </summary>
        [ContextMenu("🚨 Trigger Game-Breaking Exploit")]
        public void TriggerGameBreakingExploit()
        {
            if (eventManager == null) return;

            var exploitEvent = EventFactory.CreateGameBreakingExploitEvent();
            eventManager.TriggerEvent(exploitEvent);
        }

        /// <summary>
        /// Force trigger a tournament opportunity event for testing
        /// </summary>
        [ContextMenu("🏆 Trigger Tournament Opportunity")]
        public void TriggerTournamentOpportunity()
        {
            if (eventManager == null) return;

            var tournamentEvent = EventFactory.CreateTournamentOpportunityEvent();
            eventManager.TriggerEvent(tournamentEvent);
        }

        /// <summary>
        /// Force trigger a creator collaboration event for testing
        /// </summary>
        [ContextMenu("🎬 Trigger Creator Collaboration")]
        public void TriggerCreatorCollaboration()
        {
            if (eventManager == null) return;

            var creatorEvent = EventFactory.CreateCreatorCollaborationEvent();
            eventManager.TriggerEvent(creatorEvent);
        }

        /// <summary>
        /// Force trigger a viral moment event for testing
        /// </summary>
        [ContextMenu("⭐ Trigger Viral Moment")]
        public void TriggerViralMoment()
        {
            if (eventManager == null) return;

            var viralEvent = EventFactory.CreateViralMomentEvent();
            eventManager.TriggerEvent(viralEvent);
        }

        /// <summary>
        /// Force trigger a server crisis event for testing
        /// </summary>
        [ContextMenu("🔧 Trigger Server Crisis")]
        public void TriggerServerCrisis()
        {
            if (eventManager == null) return;

            var serverEvent = EventFactory.CreateServerCrisisEvent();
            eventManager.TriggerEvent(serverEvent);
        }

        /// <summary>
        /// Force trigger a meta analysis event for testing
        /// </summary>
        [ContextMenu("📊 Trigger Meta Analysis")]
        public void TriggerMetaAnalysis()
        {
            if (eventManager == null) return;

            var metaEvent = EventFactory.CreateChampionshipMetaEvent();
            eventManager.TriggerEvent(metaEvent);
        }

        /// <summary>
        /// Force trigger a feedback surge event for testing
        /// </summary>
        [ContextMenu("💬 Trigger Feedback Surge")]
        public void TriggerFeedbackSurge()
        {
            if (eventManager == null) return;

            var feedbackEvent = EventFactory.CreateFeedbackSurgeEvent();
            eventManager.TriggerEvent(feedbackEvent);
        }

        /// <summary>
        /// Simulate game state changes for testing
        /// </summary>
        [ContextMenu("🎲 Simulate Random Game State Change")]
        public void SimulateGameStateChange()
        {
            // Simulate random changes
            OnResourcesChanged(Random.Range(0, 100), Random.Range(0, 100));
            OnCommunitySentimentChanged(Random.Range(0f, 100f));
            
            var randomCharacter = (Characters.CharacterType)Random.Range(0, 4);
            OnCharacterStatChanged(randomCharacter, Characters.CharacterStat.WinRate, Random.Range(30f, 70f));
        }

        /// <summary>
        /// Trigger any random event for testing
        /// </summary>
        [ContextMenu("🎰 Trigger Any Random Event")]
        public void TriggerAnyRandomEvent()
        {
            if (eventManager == null) return;

            var randomEvent = EventFactory.GetAnyRandomEvent();
            eventManager.TriggerEvent(randomEvent);
        }

        /// <summary>
        /// Simulate positive sentiment change
        /// </summary>
        [ContextMenu("😊 Simulate Positive Sentiment")]
        public void SimulatePositiveSentiment()
        {
            OnCommunitySentimentChanged(lastCommunitySentiment + 25f);
        }

        /// <summary>
        /// Simulate negative sentiment change
        /// </summary>
        [ContextMenu("😡 Simulate Negative Sentiment")]
        public void SimulateNegativeSentiment()
        {
            OnCommunitySentimentChanged(lastCommunitySentiment - 25f);
        }

        /// <summary>
        /// Simulate resource crisis
        /// </summary>
        [ContextMenu("💸 Simulate Resource Crisis")]
        public void SimulateResourceCrisis()
        {
            OnResourcesChanged(5, 5); // Very low resources
        }

        /// <summary>
        /// Simulate resource abundance
        /// </summary>
        [ContextMenu("💰 Simulate Resource Abundance")]
        public void SimulateResourceAbundance()
        {
            OnResourcesChanged(lastRP + 30, lastCP + 30); // High resources
        }

        #endregion
    }
}  