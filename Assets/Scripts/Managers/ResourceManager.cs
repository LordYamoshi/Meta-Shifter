using UnityEngine;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Updated ResourceManager with configurable starting resources
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        [Header("Starting Resources")]
        [SerializeField] private int startingResearchPoints = 24;
        [SerializeField] private int startingCommunityPoints = 0;
        
        [Header("Current Resources")]
        [SerializeField] private int researchPoints = 24;
        [SerializeField] private int communityPoints = 0;
        
        [Header("Generation Settings")]
        [SerializeField] private int rpPerWeek = 10;
        [SerializeField] private int cpPerWeek = 5;
        
        [Header("Events")]
        public UnityEvent<int, int> OnResourcesChanged; // RP, CP
        public UnityEvent<int, int> OnResourcesGenerated; // RP, CP gained
        
        public int ResearchPoints => researchPoints;
        public int CommunityPoints => communityPoints;
        
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
            // Initialize with starting resources
            researchPoints = startingResearchPoints;
            communityPoints = startingCommunityPoints;
            
            // Subscribe to phase changes
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
            }
            
            // Initial event
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
        }
        
        private void OnPhaseChanged(GamePhase newPhase)
        {
            // You can add phase-specific resource effects here if needed
        }
        
        private void OnWeekChanged(int newWeek)
        {
            // Generate resources at start of new week
            GenerateWeeklyResources();
        }
        
        public void SetStartingResources(int startingRP, int startingCP)
        {
            startingResearchPoints = startingRP;
            startingCommunityPoints = startingCP;
            researchPoints = startingRP;
            communityPoints = startingCP;
            
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
        }
        
        private void GenerateWeeklyResources()
        {
            researchPoints += rpPerWeek;
            communityPoints += cpPerWeek;
            
            OnResourcesGenerated.Invoke(rpPerWeek, cpPerWeek);
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
            
            Debug.Log($"Generated resources: +{rpPerWeek} RP, +{cpPerWeek} CP");
        }
        
        public bool CanSpend(int rp, int cp)
        {
            return researchPoints >= rp && communityPoints >= cp;
        }
        
        public bool SpendResources(int rp, int cp)
        {
            if (!CanSpend(rp, cp))
            {
                Debug.Log($"Not enough resources! Need {rp} RP, {cp} CP. Have {researchPoints} RP, {communityPoints} CP");
                return false;
            }
            
            researchPoints -= rp;
            communityPoints -= cp;
            
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
            Debug.Log($"Spent {rp} RP, {cp} CP");
            
            return true;
        }
        
        public void AddResources(int rp, int cp)
        {
            researchPoints += rp;
            communityPoints += cp;
            
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
            Debug.Log($"Added {rp} RP, {cp} CP");
        }
        
        // Debug methods
        [ContextMenu("Add 10 RP")]
        public void DebugAdd10RP()
        {
            AddResources(10, 0);
        }
        
        [ContextMenu("Add 10 CP")]
        public void DebugAdd10CP()
        {
            AddResources(0, 10);
        }
        
        [ContextMenu("Reset to Starting Resources")]
        public void DebugResetResources()
        {
            researchPoints = startingResearchPoints;
            communityPoints = startingCommunityPoints;
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
        }
    }
}