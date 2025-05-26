using UnityEngine;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Manages game resources (Research Points and Community Points)
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        // Singleton instance
        public static ResourceManager Instance { get; private set; }
        
        [Header("Resource Generation")]
        [SerializeField] private int baseRPGeneration = 10;
        [SerializeField] private int baseCPGeneration = 5;
        
        [Header("Events")]
        public UnityEvent<ResourceChangeEvent> onResourceChanged;
        
        // Resource values
        private int _researchPoints;
        private int _communityPoints;
        
        // Resource multipliers
        private float _researchPointMultiplier = 1.0f;
        private float _communityPointMultiplier = 1.0f;
        
        // Properties
        public int ResearchPoints => _researchPoints;
        public int CommunityPoints => _communityPoints;
        public int RPGenerationRate => Mathf.RoundToInt(baseRPGeneration * _researchPointMultiplier);
        public int CPGenerationRate => Mathf.RoundToInt(baseCPGeneration * _communityPointMultiplier);
        
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
        
        /// <summary>
        /// Initialize with starting resources
        /// </summary>
        public void Initialize(int startingRP, int startingCP)
        {
            _researchPoints = startingRP;
            _communityPoints = startingCP;
            
            // Subscribe to game phase changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.onPhaseChanged.AddListener(OnGamePhaseChanged);
            }
            
            // Notify listeners of initial values
            NotifyResourceChange(ResourceType.ResearchPoints, _researchPoints, 0);
            NotifyResourceChange(ResourceType.CommunityPoints, _communityPoints, 0);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from game phase changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.onPhaseChanged.RemoveListener(OnGamePhaseChanged);
            }
        }
        
        private void OnGamePhaseChanged(GamePhase newPhase)
        {
            // Generate resources at the start of Planning phase
            if (newPhase == GamePhase.Planning)
            {
                GenerateResources();
            }
        }
        
        /// <summary>
        /// Generate resources for the new week
        /// </summary>
        private void GenerateResources()
        {
            // Add resources based on generation rates
            AddResources(RPGenerationRate, CPGenerationRate);
        }
        
        /// <summary>
        /// Check if can spend the specified resources
        /// </summary>
        public bool CanSpend(int rp, int cp)
        {
            return _researchPoints >= rp && _communityPoints >= cp;
        }
        
        /// <summary>
        /// Spend resources
        /// </summary>
        public bool SpendResources(int rp, int cp)
        {
            if (!CanSpend(rp, cp))
                return false;
                
            int oldRP = _researchPoints;
            int oldCP = _communityPoints;
            
            _researchPoints -= rp;
            _communityPoints -= cp;
            
            // Notify listeners
            if (rp > 0)
                NotifyResourceChange(ResourceType.ResearchPoints, _researchPoints, oldRP);
                
            if (cp > 0)
                NotifyResourceChange(ResourceType.CommunityPoints, _communityPoints, oldCP);
                
            return true;
        }
        
        /// <summary>
        /// Add resources
        /// </summary>
        public void AddResources(int rp, int cp)
        {
            int oldRP = _researchPoints;
            int oldCP = _communityPoints;
            
            _researchPoints += rp;
            _communityPoints += cp;
            
            // Notify listeners
            if (rp > 0)
                NotifyResourceChange(ResourceType.ResearchPoints, _researchPoints, oldRP);
                
            if (cp > 0)
                NotifyResourceChange(ResourceType.CommunityPoints, _communityPoints, oldCP);
        }
        
        /// <summary>
        /// Set resources directly (for loading saved games)
        /// </summary>
        public void SetResources(int rp, int cp)
        {
            int oldRP = _researchPoints;
            int oldCP = _communityPoints;
            
            _researchPoints = rp;
            _communityPoints = cp;
            
            // Notify listeners
            NotifyResourceChange(ResourceType.ResearchPoints, _researchPoints, oldRP);
            NotifyResourceChange(ResourceType.CommunityPoints, _communityPoints, oldCP);
        }
        
        /// <summary>
        /// Modify resource generation rates
        /// </summary>
        public void ModifyResourceGeneration(ResourceType type, float multiplier)
        {
            if (type == ResourceType.ResearchPoints)
            {
                _researchPointMultiplier = multiplier;
                NotifyResourceChange(ResourceType.ResearchPoints, _researchPoints, _researchPoints);
            }
            else
            {
                _communityPointMultiplier = multiplier;
                NotifyResourceChange(ResourceType.CommunityPoints, _communityPoints, _communityPoints);
            }
        }
        
        /// <summary>
        /// Notify listeners of resource changes
        /// </summary>
        private void NotifyResourceChange(ResourceType type, int newValue, int oldValue)
        {
            ResourceChangeEvent changeEvent = new ResourceChangeEvent
            {
                ResourceType = type,
                NewValue = newValue,
                OldValue = oldValue,
                GenerationRate = type == ResourceType.ResearchPoints ? RPGenerationRate : CPGenerationRate
            };
            
            onResourceChanged.Invoke(changeEvent);
        }
    }
}