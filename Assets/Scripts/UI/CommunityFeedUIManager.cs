using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MetaBalance.UI
{
    /// <summary>
    /// Enhanced CommunityFeedUIManager that restricts community feed visibility to Feedback phase only
    /// </summary>
    public class CommunityFeedUIManager : MonoBehaviour
    {
        [Header("Feed Components")]
        [SerializeField] private GameObject feedItemPrefab;
        [SerializeField] private Transform communityFeedContainer;
        [SerializeField] private Transform viralFeedbackContainer;
        [SerializeField] private ScrollRect feedScrollRect;
        
        [Header("Tab System")]
        [SerializeField] private Button communityTabButton;
        [SerializeField] private Button eventsTabButton;
        [SerializeField] private GameObject communityFeedPanel;
        [SerializeField] private GameObject eventsPanel;
        
        [Header("Phase Restriction UI")]
        [SerializeField] private GameObject feedbackPhaseOnlyMessage;
        [SerializeField] private Text phaseRestrictionText;
        
        [Header("Settings")]
        [SerializeField] private int maxVisibleItems = 15;
        [SerializeField] private bool showTimestamps = true;
        
        // State tracking
        private List<CommunityFeedItem> activeFeedItems = new List<CommunityFeedItem>();
        private bool showingCommunityFeed = true;
        private bool isFeedbackPhase = false;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            CheckCurrentPhase();
        }
        
        private void InitializeUI()
        {
            // Setup tab buttons
            if (communityTabButton != null)
                communityTabButton.onClick.AddListener(() => SwitchToTab(true));
                
            if (eventsTabButton != null)
                eventsTabButton.onClick.AddListener(() => SwitchToTab(false));
            
            // Initialize with community tab
            SwitchToTab(true);
            
            // Setup phase restriction message
            SetupPhaseRestrictionUI();
        }
        
        private void SetupPhaseRestrictionUI()
        {
            if (feedbackPhaseOnlyMessage != null)
            {
                feedbackPhaseOnlyMessage.SetActive(false);
            }
            
            if (phaseRestrictionText != null)
            {
                phaseRestrictionText.text = "Community feedback is only available during the Feedback Phase.\nCurrently in: " + GetCurrentPhaseDisplayName();
            }
        }
        
        #endregion
        
        #region Event Subscription
        
        private void SubscribeToEvents()
        {
            // Subscribe to phase changes
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Debug.Log("📅 CommunityFeedUI subscribed to phase changes");
            }
            
            // Subscribe to community feedback (but only process during feedback phase)
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.AddListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(UpdateSentimentDisplay);
                Debug.Log("💬 CommunityFeedUI subscribed to feedback events");
            }
        }
        
        #endregion
        
        #region Phase Management
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            bool wasFeedbackPhase = isFeedbackPhase;
            isFeedbackPhase = (newPhase == Core.GamePhase.Feedback);
            
            Debug.Log($"🎭 CommunityFeedUI phase changed: {newPhase} - Feedback phase: {isFeedbackPhase}");
            
            // Update UI visibility based on phase
            UpdateFeedVisibilityForPhase();
            
            // If we just entered feedback phase, refresh the feed
            if (isFeedbackPhase && !wasFeedbackPhase)
            {
                RefreshFeedForFeedbackPhase();
            }
            
            // If we left feedback phase, clear the feed
            if (!isFeedbackPhase && wasFeedbackPhase)
            {
                ClearFeedItems();
            }
            
            // Update phase restriction message
            UpdatePhaseRestrictionMessage();
        }
        
        private void CheckCurrentPhase()
        {
            if (Core.PhaseManager.Instance != null)
            {
                var currentPhase = Core.PhaseManager.Instance.GetCurrentPhase();
                OnPhaseChanged(currentPhase);
            }
        }
        
        private void UpdateFeedVisibilityForPhase()
        {
            // Show/hide community feed based on phase
            if (communityFeedPanel != null)
            {
                bool shouldShowFeed = isFeedbackPhase && showingCommunityFeed;
                communityFeedContainer?.gameObject.SetActive(shouldShowFeed);
            }
            
            // Show/hide phase restriction message
            if (feedbackPhaseOnlyMessage != null)
            {
                bool shouldShowRestriction = !isFeedbackPhase && showingCommunityFeed;
                feedbackPhaseOnlyMessage.SetActive(shouldShowRestriction);
            }
        }
        
        private void UpdatePhaseRestrictionMessage()
        {
            if (phaseRestrictionText != null)
            {
                string currentPhase = GetCurrentPhaseDisplayName();
                if (isFeedbackPhase)
                {
                    phaseRestrictionText.text = "Community feedback is now available!";
                }
                else
                {
                    phaseRestrictionText.text = $"Community feedback is only available during the Feedback Phase.\nCurrently in: {currentPhase}";
                }
            }
        }
        
        private string GetCurrentPhaseDisplayName()
        {
            if (Core.PhaseManager.Instance != null)
            {
                return Core.PhaseManager.Instance.GetPhaseDisplayName();
            }
            return "Unknown Phase";
        }
        
        #endregion
        
        #region Feed Management
        
        private void OnNewFeedbackReceived(Community.CommunityFeedback feedback)
        {
            // Only add feedback items during feedback phase
            if (isFeedbackPhase && showingCommunityFeed)
            {
                AddFeedItem(feedback);
            }
            else
            {
                Debug.Log($"📝 Feedback received but not in feedback phase - storing for later: {feedback.author}");
                // Could store for later if needed
            }
        }
        
        private void AddFeedItem(Community.CommunityFeedback feedback)
        {
            if (feedItemPrefab == null || communityFeedContainer == null)
            {
                Debug.LogError("❌ Feed item prefab or container not assigned!");
                return;
            }
            
            if (!isFeedbackPhase)
            {
                Debug.LogWarning("⚠️ Attempted to add feed item outside of feedback phase");
                return;
            }
            
            // Create new feed item
            GameObject newItemObj = Instantiate(feedItemPrefab, communityFeedContainer);
            
            // Get or add CommunityFeedItem component
            var feedItem = newItemObj.GetComponent<CommunityFeedItem>();
            if (feedItem == null)
            {
                Debug.LogWarning("⚠️ Feed item prefab missing CommunityFeedItem component - adding one");
                feedItem = newItemObj.AddComponent<CommunityFeedItem>();
            }
            
            // Setup the feed item
            feedItem.SetupWithProPlayerSupport(feedback);
            
            // Track and position
            activeFeedItems.Insert(0, feedItem);
            newItemObj.transform.SetAsFirstSibling();
            
            // Remove excess items
            while (activeFeedItems.Count > maxVisibleItems)
            {
                RemoveOldestFeedItem();
            }
            
            Debug.Log($"✅ Added feed item (Feedback Phase): {feedback.author} - Total: {activeFeedItems.Count}");
        }
        
        private void RefreshFeedForFeedbackPhase()
        {
            Debug.Log("🔄 Refreshing community feed for feedback phase");
            
            // Could request recent feedback from the manager
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                // If your feedback manager has a method to get recent feedback, call it here
                // var recentFeedback = Community.CommunityFeedbackManager.Instance.GetRecentFeedback();
                // foreach (var feedback in recentFeedback)
                // {
                //     AddFeedItem(feedback);
                // }
            }
        }
        
        public void ClearFeedItems()
        {
            Debug.Log("🧹 Clearing community feed items (leaving feedback phase)");
            
            foreach (var item in activeFeedItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            activeFeedItems.Clear();
        }
        
        private void RemoveOldestFeedItem()
        {
            if (activeFeedItems.Count == 0) return;
            
            var oldestItem = activeFeedItems[activeFeedItems.Count - 1];
            activeFeedItems.RemoveAt(activeFeedItems.Count - 1);
            
            if (oldestItem != null && oldestItem.gameObject != null)
            {
                Destroy(oldestItem.gameObject);
            }
        }
        
        #endregion
        
        #region Tab Management
        
        private void SwitchToTab(bool showCommunity)
        {
            showingCommunityFeed = showCommunity;
            
            // Update tab button appearance
            UpdateTabButtonAppearance();
            
            // Update panel visibility
            UpdatePanelVisibility();
            
            // Update feed visibility based on phase
            UpdateFeedVisibilityForPhase();
            
            Debug.Log($"🔄 Switched to {(showCommunity ? "Community" : "Events")} tab");
        }
        
        private void UpdateTabButtonAppearance()
        {
            if (communityTabButton != null)
            {
                var colors = communityTabButton.colors;
                colors.normalColor = showingCommunityFeed ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                communityTabButton.colors = colors;
            }
            
            if (eventsTabButton != null)
            {
                var colors = eventsTabButton.colors;
                colors.normalColor = !showingCommunityFeed ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                eventsTabButton.colors = colors;
            }
        }
        
        private void UpdatePanelVisibility()
        {
            if (communityFeedPanel != null)
                communityFeedPanel.SetActive(showingCommunityFeed);
                
            if (eventsPanel != null)
                eventsPanel.SetActive(!showingCommunityFeed);
        }
        
        #endregion
        
        #region Sentiment Display
        
        private void UpdateSentimentDisplay(float sentiment)
        {
            // Only update sentiment display during feedback phase
            if (!isFeedbackPhase)
            {
                Debug.Log($"📊 Sentiment update received but not in feedback phase: {sentiment:F1}");
                return;
            }
            
            Debug.Log($"📊 Updating sentiment display: {sentiment:F1}");
            // Your existing sentiment display logic here
        }
        
        #endregion
        
        #region Testing Methods
        
        [ContextMenu("🧪 Test Phase Restriction")]
        public void TestPhaseRestriction()
        {
            Debug.Log("🧪 Testing phase restriction...");
            
            // Force different phases for testing
            if (Core.PhaseManager.Instance != null)
            {
                var currentPhase = Core.PhaseManager.Instance.GetCurrentPhase();
                Debug.Log($"Current phase: {currentPhase}");
                Debug.Log($"Is feedback phase: {isFeedbackPhase}");
                Debug.Log($"Feed visibility: {communityFeedContainer?.gameObject.activeSelf}");
            }
        }
        
        [ContextMenu("🧪 Force Feedback Phase")]
        public void TestForceFeedbackPhase()
        {
            OnPhaseChanged(Core.GamePhase.Feedback);
        }
        
        [ContextMenu("🧪 Force Planning Phase")]
        public void TestForcePlanningPhase()
        {
            OnPhaseChanged(Core.GamePhase.Planning);
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
            
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.RemoveListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(UpdateSentimentDisplay);
            }
        }
        
        #endregion
    }
}