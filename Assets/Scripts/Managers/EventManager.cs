using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    /// <summary>
    /// Manages game events
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        // Singleton instance
        public static EventManager Instance { get; private set; }
        
        [Header("Event Database")]
        [SerializeField] private List<GameEvent> eventDatabase = new List<GameEvent>();
        
        [Header("Event Generation")]
        [SerializeField, Range(0, 1)] private float eventChancePerWeek = 0.7f;
        [SerializeField, Range(0, 1)] private float crisisChance = 0.3f;
        [SerializeField, Range(0, 1)] private float opportunityChance = 0.4f;
        [SerializeField, Range(0, 1)] private float communityChance = 0.25f;
        [SerializeField, Range(0, 1)] private float specialChance = 0.05f;
        
        [Header("Events")]
        public UnityEvent<GameEvent> onEventTriggered;
        public UnityEvent<GameEvent, EventOption> onEventResolved;
        
        // Current event
        private GameEvent _currentEvent;
        private bool _eventPending = false;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private void Start()
        {
            // Subscribe to game phase changes
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.onPhaseChanged.AddListener(OnGamePhaseChanged);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from game phase changes
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.onPhaseChanged.RemoveListener(OnGamePhaseChanged);
            }
        }
        
        private void OnGamePhaseChanged(Core.GamePhase newPhase)
        {
            // Check for event phase
            if (newPhase == Core.GamePhase.Event)
            {
                TriggerRandomEvent();
            }
        }
        
        /// <summary>
        /// Trigger a random event
        /// </summary>
        private void TriggerRandomEvent()
        {
            // Check if an event should occur
            if (Random.value > eventChancePerWeek)
            {
                // No event this week
                Debug.Log("No event triggered this week");
                Core.GameManager.Instance.AdvanceToNextPhase();
                return;
            }
            
            // Determine event category
            float categoryRoll = Random.value;
            EventCategory category;
            
            if (categoryRoll < crisisChance)
            {
                category = EventCategory.Crisis;
            }
            else if (categoryRoll < crisisChance + opportunityChance)
            {
                category = EventCategory.Opportunity;
            }
            else if (categoryRoll < crisisChance + opportunityChance + communityChance)
            {
                category = EventCategory.Community;
            }
            else
            {
                category = EventCategory.Special;
            }
            
            // Find events of the selected category
            List<GameEvent> eligibleEvents = new List<GameEvent>();
            foreach (GameEvent gameEvent in eventDatabase)
            {
                if (gameEvent.category == category)
                {
                    eligibleEvents.Add(gameEvent);
                }
            }
            
            if (eligibleEvents.Count == 0)
            {
                // No eligible events, advance to next phase
                Debug.LogWarning($"No eligible events found for category {category}");
                Core.GameManager.Instance.AdvanceToNextPhase();
                return;
            }
            
            // Select a random event
            int eventIndex = Random.Range(0, eligibleEvents.Count);
            GameEvent selectedEvent = eligibleEvents[eventIndex];
            
            // Set current event and mark as pending
            _currentEvent = selectedEvent;
            _eventPending = true;
            
            // Play event sound
            if (selectedEvent.eventSound != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(selectedEvent.eventSound);
                }
            }
            
            // Notify listeners
            onEventTriggered.Invoke(_currentEvent);
        }
        
        /// <summary>
        /// Resolve the current event with the selected option
        /// </summary>
        public void ResolveEvent(EventOption selectedOption)
        {
            if (!_eventPending || _currentEvent == null)
            {
                Debug.LogWarning("Attempted to resolve an event when none is pending");
                return;
            }
            
            // Apply event effects
            ApplyEventEffects(_currentEvent, selectedOption);
            
            // Notify listeners
            onEventResolved.Invoke(_currentEvent, selectedOption);
            
            // Clear current event
            _eventPending = false;
            _currentEvent = null;
            
            // Advance to next phase
            Core.GameManager.Instance.AdvanceToNextPhase();
        }
        
        /// <summary>
        /// Apply the effects of an event and option
        /// </summary>
        private void ApplyEventEffects(GameEvent gameEvent, EventOption option)
        {
            // Get necessary managers
            Characters.CharacterManager characterManager = Characters.CharacterManager.Instance;
            Core.ResourceManager resourceManager = Core.ResourceManager.Instance;
            
            if (characterManager == null || resourceManager == null)
            {
                Debug.LogError("Could not find required managers to apply event effects");
                return;
            }
            
            // Check if enough resources
            if (!resourceManager.CanSpend(option.researchPointsCost, option.communityPointsCost))
            {
                Debug.LogWarning("Not enough resources to resolve event with selected option");
                return;
            }
            
            // Apply resource costs
            resourceManager.SpendResources(option.researchPointsCost, option.communityPointsCost);
            
            // Apply resource rewards
            resourceManager.AddResources(option.researchPointsReward, option.communityPointsReward);
            
            // Apply character effects from event
            foreach (CharacterEffect effect in gameEvent.characterEffects)
            {
                characterManager.ModifyCharacterStat(effect.characterType, effect.targetStat, effect.percentageChange);
            }
            
            // Apply character effects from option
            foreach (CharacterEffect effect in option.characterEffects)
            {
                characterManager.ModifyCharacterStat(effect.characterType, effect.targetStat, effect.percentageChange);
            }
            
            // Apply overall satisfaction effect to all characters' popularity
            float satisfactionEffect = gameEvent.playerSatisfactionEffect + option.playerSatisfactionEffect;
            
            if (satisfactionEffect != 0)
            {
                foreach (Characters.CharacterType type in System.Enum.GetValues(typeof(Characters.CharacterType)))
                {
                    characterManager.ModifyCharacterStat(type, Characters.CharacterStat.Popularity, satisfactionEffect * 0.2f);
                }
            }
            
            // Recalculate win rates to account for all changes
            characterManager.RecalculateWinRates();
        }
        
        /// <summary>
        /// Check if an event is currently pending
        /// </summary>
        public bool IsEventPending()
        {
            return _eventPending;
        }
        
        /// <summary>
        /// Get the current event
        /// </summary>
        public GameEvent GetCurrentEvent()
        {
            return _currentEvent;
        }
        
        /// <summary>
        /// Force a specific event to trigger
        /// </summary>
        public void ForceEvent(GameEvent gameEvent)
        {
            // Set current event and mark as pending
            _currentEvent = gameEvent;
            _eventPending = true;
            
            // Play event sound
            if (gameEvent.eventSound != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(gameEvent.eventSound);
                }
            }
            
            // Notify listeners
            onEventTriggered.Invoke(_currentEvent);
        }
    }
}