using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Events
{
    public class EventResponseManager : MonoBehaviour
    {
        public static EventResponseManager Instance { get; private set; }
        
        [System.Serializable]
        public class ResponseData
        {
            public ResponseType type;
            public string name;
            public string description;
            public int rpCost;
            public int cpCost;
            public Color buttonColor;
        }
        
        [Header("Response Types")]
        [SerializeField] private List<ResponseData> availableResponses;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeResponses();
        }
        
        private void InitializeResponses()
        {
            availableResponses = new List<ResponseData>
            {
                new ResponseData
                {
                    type = ResponseType.EmergencyFix,
                    name = "Emergency Fix",
                    description = "Quick hotfix to address the issue",
                    rpCost = 3,
                    cpCost = 0,
                    buttonColor = new Color(0.8f, 0.2f, 0.2f) // Red
                },
                new ResponseData
                {
                    type = ResponseType.DevUpdate,
                    name = "Developer Update",
                    description = "Communicate with the community",
                    rpCost = 0,
                    cpCost = 2,
                    buttonColor = new Color(0.2f, 0.6f, 1f) // Blue
                },
                new ResponseData
                {
                    type = ResponseType.CommunityManagement,
                    name = "Community Management",
                    description = "Engage with community concerns",
                    rpCost = 0,
                    cpCost = 4,
                    buttonColor = new Color(0.8f, 0.2f, 0.8f) // Purple
                },
                new ResponseData
                {
                    type = ResponseType.MetaShift,
                    name = "Meta Adjustment",
                    description = "Make strategic balance changes",
                    rpCost = 5,
                    cpCost = 1,
                    buttonColor = new Color(0.2f, 0.8f, 0.2f) // Green
                },
                new ResponseData
                {
                    type = ResponseType.Ignore,
                    name = "Do Nothing",
                    description = "Let the situation resolve itself",
                    rpCost = 0,
                    cpCost = 0,
                    buttonColor = Color.gray
                }
            };
        }
        
        public List<ResponseData> GetAvailableResponses() => new List<ResponseData>(availableResponses);
        
        public ResponseData GetResponseData(ResponseType type)
        {
            return availableResponses.Find(r => r.type == type);
        }
        
        public bool CanAffordResponse(ResponseType type)
        {
            var responseData = GetResponseData(type);
            if (responseData == null) return false;
            
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null) return true;
            
            return resourceManager.CanSpend(responseData.rpCost, responseData.cpCost);
        }
        
        public bool ExecuteResponse(GameEvent gameEvent, ResponseType responseType)
        {
            var responseData = GetResponseData(responseType);
            if (responseData == null) return false;
            
            if (!CanAffordResponse(responseType)) return false;
            
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null && responseType != ResponseType.Ignore)
            {
                resourceManager.SpendResources(responseData.rpCost, responseData.cpCost);
            }
            
            // Let EventManager handle the actual resolution
            if (EventManager.Instance != null)
            {
                EventManager.Instance.ResolveEvent(gameEvent, responseType);
            }
            
            Debug.Log($"Executed response: {responseData.name} for event: {gameEvent.title}");
            return true;
        }
        
        public float CalculateResponseEffectiveness(GameEvent gameEvent, ResponseType responseType)
        {
            bool isOptimalResponse = gameEvent.eventData.requiredResponses.Contains(responseType);
            
            return isOptimalResponse ? 1f : 0.5f; // Good response vs poor response
        }
    }
}