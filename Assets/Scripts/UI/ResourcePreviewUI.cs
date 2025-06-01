using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Simple resource preview showing only total cost and remaining resources
    /// </summary>
    public class ResourcePreviewUI : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI currentRPText;
        [SerializeField] private TextMeshProUGUI currentCPText;
        [SerializeField] private TextMeshProUGUI afterRPText;
        [SerializeField] private TextMeshProUGUI afterCPText;
        [SerializeField] private TextMeshProUGUI rpGenerationText; // Add this for "+10/turn"
        [SerializeField] private TextMeshProUGUI cpGenerationText; // Add this for "+5/turn"
        
        [Header("Visual Feedback")]
        [SerializeField] private Image rpPreviewBackground;
        [SerializeField] private Image cpPreviewBackground;
        [SerializeField] private Color canAffordColor = Color.green;
        [SerializeField] private Color cannotAffordColor = Color.red;
        [SerializeField] private Color warningColor = Color.yellow;
        
        public static ResourcePreviewUI Instance { get; private set; }
        
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
            // Subscribe to drop zone changes and resource changes
            SubscribeToEvents();
            
            // Force immediate update after a few frames to ensure all components are ready
            StartCoroutine(InitialUpdateCoroutine());
        }
        
        /// <summary>
        /// Ensure initial update happens after all components are initialized
        /// </summary>
        private System.Collections.IEnumerator InitialUpdateCoroutine()
        {
            // Wait a few frames for all components to initialize
            yield return null;
            yield return null;
            yield return null;
            
            // Force first update
            UpdateResourcePreview();
            
            // Set up continuous monitoring for the first few seconds
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.2f);
                UpdateResourcePreview();
            }
            
            Debug.Log("✅ ResourcePreviewUI fully initialized and monitoring");
        }
        
        /// <summary>
        /// Subscribe to all existing drop zones and resource manager events
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to resource changes
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
                Debug.Log("✅ Subscribed to ResourceManager events");
            }
            else
            {
                Debug.LogWarning("ResourceManager not found during subscription - will retry");
                Invoke(nameof(RetryResourceManagerSubscription), 0.5f);
            }
            
            // Subscribe to drop zone changes
            SubscribeToDropZones();
            
            // Also set up a recurring check for new drop zones (in case they're created later)
            InvokeRepeating(nameof(SubscribeToDropZones), 1f, 2f);
        }
        
        /// <summary>
        /// Subscribe to all existing drop zones and any that might be created later
        /// </summary>
        private void SubscribeToDropZones()
        {
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            int newSubscriptions = 0;
            
            foreach (var dropZone in dropZones)
            {
                // Check if we're already subscribed to avoid duplicates
                if (!IsSubscribedToDropZone(dropZone))
                {
                    dropZone.OnCardsChanged.AddListener(OnDropZoneChanged);
                    newSubscriptions++;
                    Debug.Log($"📡 Subscribed to drop zone: {dropZone.name}");
                }
            }
            
            if (newSubscriptions > 0)
            {
                Debug.Log($"✅ ResourcePreviewUI subscribed to {newSubscriptions} new drop zones (total found: {dropZones.Length})");
                
                // Trigger immediate update when new drop zones are found
                UpdateResourcePreview();
            }
        }
        
        private bool IsSubscribedToDropZone(Cards.CardDropZone dropZone)
        {
            // Simple check - if there are any persistent listeners, we're probably subscribed
            return dropZone.OnCardsChanged.GetPersistentEventCount() > 0;
        }
        
        private void RetryResourceManagerSubscription()
        {
            if (Core.ResourceManager.Instance != null && !IsResourceManagerSubscribed())
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
                Debug.Log("✅ Successfully subscribed to ResourceManager on retry");
            }
        }
        
        private bool IsResourceManagerSubscribed()
        {
            // Check if we're already subscribed (to avoid duplicate subscriptions)
            return Core.ResourceManager.Instance != null && 
                   Core.ResourceManager.Instance.OnResourcesChanged.GetPersistentEventCount() > 0;
        }
        
        private void OnDropZoneChanged(List<Cards.CardData> queuedCards)
        {
            Debug.Log($"🔄 Drop zone changed - {queuedCards.Count} cards queued, updating preview");
            UpdateResourcePreview();
            
            // Also update main UI if it exists
            UpdateMainUI();
        }
        
        private void OnResourcesChanged(int rp, int cp)
        {
            Debug.Log($"💰 Resources changed - RP: {rp}, CP: {cp}, updating preview");
            UpdateResourcePreview();
            
            // Also update main UI if it exists
            UpdateMainUI();
        }
        
        /// <summary>
        /// Update the main UI resource display (if UIManager exists)
        /// </summary>
        private void UpdateMainUI()
        {
            // Try to find and update UIManager
            var uiManager = FindObjectOfType<UI.UIManager>();
            if (uiManager != null)
            {
                // Force UIManager to refresh its resource display
                uiManager.UpdateResourceDisplay();
            }
            
            // Also trigger any other UI updates that might be needed
            // You can add more UI update calls here if needed
        }
        
        private void UpdateResourcePreview()
        {
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null) 
            {
                Debug.LogWarning("ResourceManager not found - retrying in 0.5 seconds");
                Invoke(nameof(UpdateResourcePreview), 0.5f);
                return;
            }
            
            // Get current resources
            int currentRP = resourceManager.ResearchPoints;
            int currentCP = resourceManager.CommunityPoints;
            
            // Get generation rates (dynamic)
            int rpPerWeek = resourceManager.RPPerWeek;
            int cpPerWeek = resourceManager.CPPerWeek;
            
            // Calculate total cost from all drop zones
            int totalRPCost = 0;
            int totalCPCost = 0;
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                if (dropZone != null)
                {
                    dropZone.GetTotalQueuedCost(out int rpCost, out int cpCost);
                    totalRPCost += rpCost;
                    totalCPCost += cpCost;
                }
            }
            
            // Calculate resources after spending
            int afterRP = currentRP - totalRPCost;
            int afterCP = currentCP - totalCPCost;
            
            // Update current resource display
            if (currentRPText != null)
                currentRPText.text = $"RP: {currentRP}";
            if (currentCPText != null)
                currentCPText.text = $"CP: {currentCP}";
            
            // Update generation rate display (this was probably the issue!)
            if (rpGenerationText != null)
                rpGenerationText.text = $"+{rpPerWeek}/turn";
            if (cpGenerationText != null)
                cpGenerationText.text = $"+{cpPerWeek}/turn";
            
            // Update preview display with total cost (remove (-x) from after text)
            if (afterRPText != null)
            {
                if (totalRPCost > 0)
                {
                    afterRPText.text = $"After: {afterRP}";  // Removed (-{totalRPCost})
                    afterRPText.gameObject.SetActive(true);
                }
                else
                {
                    afterRPText.gameObject.SetActive(false);
                }
            }
            
            if (afterCPText != null)
            {
                if (totalCPCost > 0)
                {
                    afterCPText.text = $"After: {afterCP}";  // Removed (-{totalCPCost})
                    afterCPText.gameObject.SetActive(true);
                }
                else
                {
                    afterCPText.gameObject.SetActive(false);
                }
            }
            
            // Update visual feedback colors
            UpdateVisualFeedback(currentRP, currentCP, totalRPCost, totalCPCost, afterRP, afterCP);
            
            Debug.Log($"💰 Resource Preview Updated: RP {currentRP} (+{rpPerWeek}/turn) → {afterRP} (cost: {totalRPCost}), CP {currentCP} (+{cpPerWeek}/turn) → {afterCP} (cost: {totalCPCost})");
        }
        
        private void UpdateVisualFeedback(int currentRP, int currentCP, int costRP, int costCP, int afterRP, int afterCP)
        {
            // RP visual feedback
            if (rpPreviewBackground != null && costRP > 0)
            {
                if (afterRP < 0)
                {
                    rpPreviewBackground.color = cannotAffordColor; // Red - can't afford
                }
                else if (afterRP < 5)
                {
                    rpPreviewBackground.color = warningColor; // Yellow - warning (low resources)
                }
                else
                {
                    rpPreviewBackground.color = canAffordColor; // Green - can afford
                }
                rpPreviewBackground.gameObject.SetActive(true);
            }
            else if (rpPreviewBackground != null)
            {
                rpPreviewBackground.gameObject.SetActive(false);
            }
            
            // CP visual feedback
            if (cpPreviewBackground != null && costCP > 0)
            {
                if (afterCP < 0)
                {
                    cpPreviewBackground.color = cannotAffordColor; // Red - can't afford
                }
                else if (afterCP < 3)
                {
                    cpPreviewBackground.color = warningColor; // Yellow - warning (low resources)
                }
                else
                {
                    cpPreviewBackground.color = canAffordColor; // Green - can afford
                }
                cpPreviewBackground.gameObject.SetActive(true);
            }
            else if (cpPreviewBackground != null)
            {
                cpPreviewBackground.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Check if all queued cards can be afforded
        /// </summary>
        public bool CanAffordAllQueuedCards()
        {
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null) return false;
            
            int totalRPCost = 0;
            int totalCPCost = 0;
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                dropZone.GetTotalQueuedCost(out int rpCost, out int cpCost);
                totalRPCost += rpCost;
                totalCPCost += cpCost;
            }
            
            return resourceManager.CanSpend(totalRPCost, totalCPCost);
        }
        
        /// <summary>
        /// Get the total cost of all queued cards
        /// </summary>
        public void GetTotalQueuedCost(out int totalRP, out int totalCP)
        {
            totalRP = 0;
            totalCP = 0;
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                dropZone.GetTotalQueuedCost(out int rpCost, out int cpCost);
                totalRP += rpCost;
                totalCP += cpCost;
            }
        }
        
        [ContextMenu("Debug: Force Update Preview")]
        public void DebugForceUpdate()
        {
            UpdateResourcePreview();
        }
        
        [ContextMenu("Debug: Check All Subscriptions")]
        public void DebugCheckSubscriptions()
        {
            Debug.Log("=== RESOURCE PREVIEW SUBSCRIPTION STATUS ===");
            
            // Check ResourceManager subscription
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null)
            {
                int rmListeners = resourceManager.OnResourcesChanged.GetPersistentEventCount();
                Debug.Log($"ResourceManager listeners: {rmListeners}");
            }
            else
            {
                Debug.Log("ResourceManager: NOT FOUND");
            }
            
            // Check drop zone subscriptions
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            Debug.Log($"Drop zones found: {dropZones.Length}");
            
            for (int i = 0; i < dropZones.Length; i++)
            {
                int listeners = dropZones[i].OnCardsChanged.GetPersistentEventCount();
                int cardCount = dropZones[i].GetQueuedCardCount();
                Debug.Log($"  Drop zone {i} ({dropZones[i].name}): {listeners} listeners, {cardCount} cards");
            }
        }
        
        [ContextMenu("Debug: Force Resubscribe All")]
        public void DebugForceResubscribe()
        {
            Debug.Log("🔄 Force resubscribing to all events...");
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.RemoveListener(OnResourcesChanged);
            }
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                if (dropZone != null)
                {
                    dropZone.OnCardsChanged.RemoveListener(OnDropZoneChanged);
                }
            }
            
            // Stop recurring checks
            CancelInvoke();
        }
        
        [ContextMenu("Debug: Check Startup State")]
        public void DebugCheckStartupState()
        {
            Debug.Log("=== RESOURCE PREVIEW STARTUP STATE ===");
            
            var resourceManager = Core.ResourceManager.Instance;
            Debug.Log($"ResourceManager found: {resourceManager != null}");
            if (resourceManager != null)
            {
                Debug.Log($"Current RP: {resourceManager.ResearchPoints}");
                Debug.Log($"Current CP: {resourceManager.CommunityPoints}");
            }
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            Debug.Log($"Drop zones found: {dropZones.Length}");
            
            int totalRP = 0, totalCP = 0;
            for (int i = 0; i < dropZones.Length; i++)
            {
                if (dropZones[i] != null)
                {
                    dropZones[i].GetTotalQueuedCost(out int rpCost, out int cpCost);
                    Debug.Log($"  Drop zone {i} ({dropZones[i].name}): {rpCost} RP, {cpCost} CP, {dropZones[i].GetQueuedCardCount()} cards");
                    totalRP += rpCost;
                    totalCP += cpCost;
                }
            }
            
            Debug.Log($"TOTAL QUEUED COST: {totalRP} RP, {totalCP} CP");
            
            Debug.Log($"UI Components:");
            Debug.Log($"  currentRPText: {currentRPText != null}");
            Debug.Log($"  currentCPText: {currentCPText != null}");
            Debug.Log($"  afterRPText: {afterRPText != null}");
            Debug.Log($"  afterCPText: {afterCPText != null}");
        }
    }
}