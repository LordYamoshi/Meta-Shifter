using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }
        
        [Header("Events")]
        public UnityEvent<GameEvent> OnEventTriggered;
        public UnityEvent<GameEvent> OnEventResolved;
        public UnityEvent<List<GameEvent>> OnActiveEventsChanged;
        
        private List<GameEvent> activeEvents = new List<GameEvent>();
        private List<GameEventData> availableEvents = new List<GameEventData>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeEvents();
        }
        
        private void Start()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        private void InitializeEvents()
        {
            availableEvents = new List<GameEventData>
            {
                new GameEventData
                {
                    id = "exploit_discovered",
                    title = "Exploit Discovered",
                    description = "Players found a game-breaking exploit with {character}",
                    eventType = EventType.Crisis,
                    triggerChance = 0.3f,
                    communityImpact = -15f,
                    requiredResponses = new List<ResponseType> { ResponseType.EmergencyFix, ResponseType.DevUpdate }
                },
                new GameEventData
                {
                    id = "tournament_announced",
                    title = "Major Tournament Announced",
                    description = "Big esports tournament coming up - showcase current balance",
                    eventType = EventType.Opportunity,
                    triggerChance = 0.4f,
                    communityImpact = 10f,
                    requiredResponses = new List<ResponseType> { ResponseType.DevUpdate, ResponseType.MetaShift }
                },
                new GameEventData
                {
                    id = "streamer_highlight",
                    title = "Viral Gameplay Moment",
                    description = "Popular streamer had amazing {character} play - boost popularity",
                    eventType = EventType.Opportunity,
                    triggerChance = 0.25f,
                    communityImpact = 8f,
                    requiredResponses = new List<ResponseType> { ResponseType.DevUpdate }
                },
                new GameEventData
                {
                    id = "pro_controversy",
                    title = "Pro Player Controversy",
                    description = "Professional player criticized balance changes publicly",
                    eventType = EventType.Crisis,
                    triggerChance = 0.2f,
                    communityImpact = -12f,
                    requiredResponses = new List<ResponseType> { ResponseType.DevUpdate, ResponseType.CommunityManagement }
                }
            };
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Event)
            {
                GenerateRandomEvents();
            }
        }
        
        private void GenerateRandomEvents()
        {
            foreach (var eventData in availableEvents)
            {
                if (Random.Range(0f, 1f) < eventData.triggerChance)
                {
                    TriggerEvent(eventData);
                }
            }
            
            // Ensure at least one event
            if (activeEvents.Count == 0)
            {
                var randomEvent = availableEvents[Random.Range(0, availableEvents.Count)];
                TriggerEvent(randomEvent);
            }
            
            OnActiveEventsChanged.Invoke(new List<GameEvent>(activeEvents));
        }
        
        private void TriggerEvent(GameEventData eventData)
        {
            var gameEvent = new GameEvent(eventData);
            activeEvents.Add(gameEvent);
            OnEventTriggered.Invoke(gameEvent);
            
            Debug.Log($"Event triggered: {gameEvent.title}");
        }
        
        public void ResolveEvent(GameEvent gameEvent, ResponseType responseType)
        {
            if (!activeEvents.Contains(gameEvent)) return;
            
            gameEvent.Resolve(responseType);
            
            // Apply effects
            ApplyEventEffects(gameEvent, responseType);
            
            activeEvents.Remove(gameEvent);
            OnEventResolved.Invoke(gameEvent);
            OnActiveEventsChanged.Invoke(new List<GameEvent>(activeEvents));
            
            Debug.Log($"Event resolved: {gameEvent.title} with {responseType}");
        }
        
        private void ApplyEventEffects(GameEvent gameEvent, ResponseType responseType)
        {
            // Apply community sentiment change
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                float sentimentChange = CalculateSentimentChange(gameEvent, responseType);
                // Apply sentiment change logic here
            }
            
            // Apply character stat changes if needed
            if (Characters.CharacterManager.Instance != null && gameEvent.affectedCharacter != null)
            {
                ApplyCharacterEffects(gameEvent, responseType);
            }
        }
        
        private float CalculateSentimentChange(GameEvent gameEvent, ResponseType responseType)
        {
            float baseImpact = gameEvent.eventData.communityImpact;
            
            // Response effectiveness
            bool isGoodResponse = gameEvent.eventData.requiredResponses.Contains(responseType);
            return isGoodResponse ? baseImpact * 0.5f : baseImpact * 1.2f;
        }
        
        private void ApplyCharacterEffects(GameEvent gameEvent, ResponseType responseType)
        {
            if (gameEvent.affectedCharacter == null) return;
            
            var characterManager = Characters.CharacterManager.Instance;
            
            switch (gameEvent.eventData.eventType)
            {
                case EventType.Crisis:
                    // Crisis typically hurts the character
                    characterManager.ModifyStat(gameEvent.affectedCharacter.Value, Characters.CharacterStat.Popularity, -5f);
                    break;
                    
                case EventType.Opportunity:
                    // Opportunity typically helps the character
                    characterManager.ModifyStat(gameEvent.affectedCharacter.Value, Characters.CharacterStat.Popularity, 8f);
                    break;
            }
        }
        
        public List<GameEvent> GetActiveEvents() => new List<GameEvent>(activeEvents);
        
        public bool HasActiveEvents() => activeEvents.Count > 0;
        
        [ContextMenu("Debug: Trigger Random Event")]
        public void DebugTriggerRandomEvent()
        {
            var randomEvent = availableEvents[Random.Range(0, availableEvents.Count)];
            TriggerEvent(randomEvent);
        }
    }
}