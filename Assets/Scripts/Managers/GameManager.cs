using UnityEngine;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Main game manager
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        [Header("Game Settings")]
        [SerializeField] private int startingWeek = 1;
        [SerializeField] private int startingResearchPoints = 10;
        [SerializeField] private int startingCommunityPoints = 5;
        
        [Header("Events")]
        public UnityEvent<GamePhase> onPhaseChanged;
        public UnityEvent<int> onWeekChanged;
        public UnityEvent<GameState> onGameStateChanged;
        
        // Current game state
        private GameState _currentState;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            // Initialize game state
            _currentState = new GameState
            {
                CurrentWeek = startingWeek,
                CurrentPhase = GamePhase.Planning
            };
        }
        
        private void Start()
        {
            // Initialize resource manager
            ResourceManager.Instance.Initialize(startingResearchPoints, startingCommunityPoints);
            
            // Notify listeners of initial state
            onGameStateChanged.Invoke(_currentState);
            onPhaseChanged.Invoke(_currentState.CurrentPhase);
            onWeekChanged.Invoke(_currentState.CurrentWeek);
        }
        
        /// <summary>
        /// Advances to the next phase of the game loop
        /// </summary>
        public void AdvanceToNextPhase()
        {
            GamePhase nextPhase = _currentState.CurrentPhase switch
            {
                GamePhase.Planning => GamePhase.Implementation,
                GamePhase.Implementation => GamePhase.Feedback,
                GamePhase.Feedback => GamePhase.Event,
                GamePhase.Event => GamePhase.Planning,
                _ => GamePhase.Planning
            };
            
            // If we're starting a new week
            int nextWeek = _currentState.CurrentWeek;
            if (nextPhase == GamePhase.Planning && _currentState.CurrentPhase == GamePhase.Event)
            {
                nextWeek++;
                onWeekChanged.Invoke(nextWeek);
            }
            
            // Update state
            _currentState.CurrentPhase = nextPhase;
            _currentState.CurrentWeek = nextWeek;
            
            // Notify listeners
            onPhaseChanged.Invoke(nextPhase);
            onGameStateChanged.Invoke(_currentState);
        }
        
        /// <summary>
        /// Gets the current game state
        /// </summary>
        public GameState GetGameState()
        {
            return _currentState;
        }
        
        /// <summary>
        /// Save the current game
        /// </summary>
        public void SaveGame(string slotName)
        {
            // Create save data
            GameSaveData saveData = new GameSaveData
            {
                Week = _currentState.CurrentWeek,
                Phase = (int)_currentState.CurrentPhase,
                ResearchPoints = ResourceManager.Instance.ResearchPoints,
                CommunityPoints = ResourceManager.Instance.CommunityPoints
                // Add additional save data here
            };
            
            // Save to player prefs or file system
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString($"save_{slotName}", json);
            PlayerPrefs.Save();
            
            Debug.Log($"Game saved to slot {slotName}");
        }
        
        /// <summary>
        /// Load a saved game
        /// </summary>
        public bool LoadGame(string slotName)
        {
            // Check if save exists
            if (!PlayerPrefs.HasKey($"save_{slotName}"))
            {
                Debug.LogWarning($"No save found in slot {slotName}");
                return false;
            }
            
            // Load save data
            string json = PlayerPrefs.GetString($"save_{slotName}");
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            // Update game state
            _currentState.CurrentWeek = saveData.Week;
            _currentState.CurrentPhase = (GamePhase)saveData.Phase;
            
            // Update resources
            ResourceManager.Instance.SetResources(saveData.ResearchPoints, saveData.CommunityPoints);
            
            // Notify listeners
            onGameStateChanged.Invoke(_currentState);
            onPhaseChanged.Invoke(_currentState.CurrentPhase);
            onWeekChanged.Invoke(_currentState.CurrentWeek);
            
            Debug.Log($"Game loaded from slot {slotName}");
            
            return true;
        }
    }
}