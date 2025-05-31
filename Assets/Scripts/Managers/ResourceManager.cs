using UnityEngine;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Manages Research Points (RP) and Community Points (CP)
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        [Header("Current Resources")]
        [SerializeField] private int researchPoints = 15;
        [SerializeField] private int communityPoints = 10;
        
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
            // Subscribe to phase changes
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
            
            // Initial event
            OnResourcesChanged.Invoke(researchPoints, communityPoints);
        }
        
        private void OnPhaseChanged(GamePhase newPhase)
        {
            // Generate resources at start of Planning phase (new week)
            if (newPhase == GamePhase.Planning)
            {
                GenerateWeeklyResources();
            }
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
    }
}