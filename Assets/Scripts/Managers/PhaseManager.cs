using UnityEngine;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Updated PhaseManager that properly handles button text and phase transitions
    /// </summary>
    public class PhaseManager : MonoBehaviour
    {
        public static PhaseManager Instance { get; private set; }
        
        [Header("Current State")]
        [SerializeField] private int currentWeek = 1;
        [SerializeField] private GamePhase currentPhase = GamePhase.Planning;
        
        [Header("Events")]
        public UnityEvent<GamePhase> OnPhaseChanged;
        public UnityEvent<int> OnWeekChanged;
        public UnityEvent<string> OnPhaseButtonTextChanged;
        
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
            // Start the first phase
            OnPhaseChanged.Invoke(currentPhase);
            UpdateButtonText();
        }
        
        /// <summary>
        /// Called by the UI button to advance phases
        /// </summary>
        public void AdvancePhase()
        {
            GamePhase nextPhase = GetNextPhase(currentPhase);
            bool isNewWeek = false;
            
            // Check if we're starting a new week
            if (nextPhase == GamePhase.Planning && currentPhase == GamePhase.Event)
            {
                currentWeek++;
                isNewWeek = true;
            }
            
            // Update current phase
            currentPhase = nextPhase;
            
            // Notify listeners
            OnPhaseChanged.Invoke(currentPhase);
            
            if (isNewWeek)
            {
                OnWeekChanged.Invoke(currentWeek);
                Debug.Log($"Started Week {currentWeek}");
            }
            
            UpdateButtonText();
            Debug.Log($"Advanced to {currentPhase} - Week {currentWeek}");
        }
        
        private GamePhase GetNextPhase(GamePhase current)
        {
            return current switch
            {
                GamePhase.Planning => GamePhase.Implementation,
                GamePhase.Implementation => GamePhase.Feedback,
                GamePhase.Feedback => GamePhase.Event,
                GamePhase.Event => GamePhase.Planning,
                _ => GamePhase.Planning
            };
        }
        
        private void UpdateButtonText()
        {
            string buttonText = currentPhase switch
            {
                GamePhase.Planning => "Implement Changes",
                GamePhase.Implementation => "View Feedback", 
                GamePhase.Feedback => "Handle Events",
                GamePhase.Event => "End Week",
                _ => "Continue"
            };
            
            OnPhaseButtonTextChanged.Invoke(buttonText);
        }
        
        /// <summary>
        /// Get the current phase description for UI display
        /// </summary>
        public string GetPhaseDescription()
        {
            return currentPhase switch
            {
                GamePhase.Planning => "Planning Phase - Select cards to queue for implementation",
                GamePhase.Implementation => "Implementation Phase - Changes are being applied",
                GamePhase.Feedback => "Feedback Phase - Review the results of your changes",
                GamePhase.Event => "Event Phase - Handle community events and prepare for next week",
                _ => "Unknown Phase"
            };
        }
        
        /// <summary>
        /// Get the current phase display name for UI
        /// </summary>
        public string GetPhaseDisplayName()
        {
            return currentPhase switch
            {
                GamePhase.Planning => "Planning Phase",
                GamePhase.Implementation => "Implementation Phase",
                GamePhase.Feedback => "Feedback Phase",
                GamePhase.Event => "Event Phase",
                _ => "Unknown Phase"
            };
        }
        
        /// <summary>
        /// Check if player can perform certain actions based on current phase
        /// </summary>
        public bool CanDragCards() => currentPhase == GamePhase.Planning;
        public bool CanImplementCards() => currentPhase == GamePhase.Implementation;
        public bool CanViewFeedback() => currentPhase == GamePhase.Feedback;
        public bool CanHandleEvents() => currentPhase == GamePhase.Event;
        
        public GamePhase GetCurrentPhase() => currentPhase;
        public int GetCurrentWeek() => currentWeek;
        
        /// <summary>
        /// Reset to beginning for testing
        /// </summary>
        [ContextMenu("Reset to Week 1")]
        public void ResetToWeekOne()
        {
            currentWeek = 1;
            currentPhase = GamePhase.Planning;
            OnPhaseChanged.Invoke(currentPhase);
            OnWeekChanged.Invoke(currentWeek);
            UpdateButtonText();
        }
        
        /// <summary>
        /// Skip to specific phase for testing
        /// </summary>
        public void SetPhase(GamePhase phase)
        {
            currentPhase = phase;
            OnPhaseChanged.Invoke(currentPhase);
            UpdateButtonText();
        }
        
        /// <summary>
        /// Skip to specific week for testing
        /// </summary>
        public void SetWeek(int week)
        {
            currentWeek = week;
            OnWeekChanged.Invoke(currentWeek);
        }
    }
}