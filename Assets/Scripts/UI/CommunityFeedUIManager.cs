using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    /// <summary>
    /// FIXED Community Feed UI Manager - Compatible with Enhanced CommunityFeedbackManager
    /// Removes all broken method calls and uses only the new enhanced API
    /// </summary>
    public class CommunityFeedUIManager : MonoBehaviour
    {
        [Header("Basic UI References")]
        [SerializeField] private Transform communityFeedContainer;  // Your existing container
        [SerializeField] private GameObject feedItemPrefab;         // Your existing prefab
        [SerializeField] private int maxVisibleItems = 15;
        
        [Header("Tab Controls (Optional)")]
        [SerializeField] private Button communityTabButton;
        [SerializeField] private Button eventsTabButton;
        [SerializeField] private GameObject communityFeedPanel;
        [SerializeField] private GameObject eventsPanel;
        
        [Header("Enhanced Sentiment Display")]
        [SerializeField] private TextMeshProUGUI sentimentText;
        [SerializeField] private TextMeshProUGUI sentimentTrendText;  // NEW: Show trend
        [SerializeField] private Slider sentimentSlider;
        [SerializeField] private TextMeshProUGUI metaStabilityText;   // NEW: Meta stability
        [SerializeField] private TextMeshProUGUI seasonInfoText;      // NEW: Season info
        
        [Header("Enhanced Stats Display (Optional)")]
        [SerializeField] private TextMeshProUGUI totalFeedbackText;   // NEW: Total feedback count
        [SerializeField] private TextMeshProUGUI viralFeedbackText;   // NEW: Viral content count
        [SerializeField] private Transform viralFeedbackContainer;    // NEW: Special viral section
        
        // Simple lists to track active items
        private List<CommunityFeedItem> activeFeedItems = new List<CommunityFeedItem>();
        private List<CommunityFeedItem> viralFeedItems = new List<CommunityFeedItem>();
        private bool showingCommunityFeed = true;
        
        private void Start()
        {
            SetupBasicUI();
            SubscribeToEnhancedEvents();
        }
        
        private void SetupBasicUI()
        {
            // Auto-find container if not assigned
            if (communityFeedContainer == null)
            {
                var found = GameObject.Find("CommunityContent");
                if (found != null)
                    communityFeedContainer = found.transform;
            }
            
            // Setup tab buttons if you have them
            if (communityTabButton != null)
                communityTabButton.onClick.AddListener(() => SwitchToTab(true));
            
            if (eventsTabButton != null)
                eventsTabButton.onClick.AddListener(() => SwitchToTab(false));
                
            // Start with community tab
            SwitchToTab(true);
            
            // Set initial sentiment
            UpdateSentimentDisplay(65f);
            UpdateEnhancedDisplays();
            
            Debug.Log("✅ Enhanced Community Feed UI Manager initialized");
        }
        
        private void SubscribeToEnhancedEvents()
        {
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                // FIXED: Use only methods that exist in the enhanced manager
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.AddListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(OnSentimentChanged);
                
                // NEW: Subscribe to enhanced events
                Community.CommunityFeedbackManager.Instance.OnStrategyActivated.AddListener(OnStrategyActivated);
                Community.CommunityFeedbackManager.Instance.OnViralFeedbackGenerated.AddListener(OnViralFeedbackGenerated);
                
                Debug.Log("📡 Subscribed to enhanced community feedback events");
            }
            else
            {
                Debug.LogWarning("⚠️ CommunityFeedbackManager.Instance not found - will retry in 1 second");
                Invoke(nameof(SubscribeToEnhancedEvents), 1f);
            }
        }
        
        #region Enhanced Event Handlers
        
        private void OnNewFeedbackReceived(Community.CommunityFeedback feedback)
        {
            if (showingCommunityFeed)
            {
                AddFeedItem(feedback);
            }
            
            // Update enhanced displays
            UpdateEnhancedDisplays();
        }
        
        private void OnSentimentChanged(float newSentiment)
        {
            UpdateSentimentDisplay(newSentiment);
            UpdateSentimentTrend();
        }
        
        private void OnStrategyActivated(string strategyInfo)
        {
            Debug.Log($"🎯 Strategy activated: {strategyInfo}");
            // You can add visual feedback here if desired
        }
        
        private void OnViralFeedbackGenerated(Community.FeedbackEventData eventData)
        {
            Debug.Log($"🌟 Viral feedback generated: {eventData.feedback.author} - {eventData.feedback.content}");
            
            // Add to viral section if you have one
            if (viralFeedbackContainer != null)
            {
                AddViralFeedItem(eventData.feedback);
            }
            
            UpdateEnhancedDisplays();
        }
        
        #endregion
        
        #region Feed Item Management
        
        private void AddFeedItem(Community.CommunityFeedback feedback)
        {
            if (feedItemPrefab == null || communityFeedContainer == null)
            {
                Debug.LogError("Feed item prefab or container not assigned!");
                return;
            }
            
            // Create new feed item using your prefab
            GameObject newItemObj = Instantiate(feedItemPrefab, communityFeedContainer);
            
            // Get the CommunityFeedItem component
            var feedItem = newItemObj.GetComponent<CommunityFeedItem>();
            if (feedItem == null)
            {
                Debug.LogWarning("Feed item prefab doesn't have CommunityFeedItem component - adding one");
                feedItem = newItemObj.AddComponent<CommunityFeedItem>();
            }
            
            // FIXED: Use the correct method name
            feedItem.SetupWithProPlayerSupport(feedback);
            
            // Add to our tracking list
            activeFeedItems.Insert(0, feedItem);
            
            // Move to top (your layout group should handle positioning)
            newItemObj.transform.SetAsFirstSibling();
            
            // Highlight viral content
            if (feedback.isViralCandidate)
            {
                HighlightViralContent(feedItem);
            }
            
            // Remove old items if we have too many
            while (activeFeedItems.Count > maxVisibleItems)
            {
                RemoveOldestFeedItem();
            }
            
            Debug.Log($"✅ Added feed item: {feedback.author} ({feedback.feedbackType}) - Total items: {activeFeedItems.Count}");
        }
        
        private void AddViralFeedItem(Community.CommunityFeedback feedback)
        {
            if (viralFeedbackContainer == null) return;
            
            GameObject viralItemObj = Instantiate(feedItemPrefab, viralFeedbackContainer);
            var feedItem = viralItemObj.GetComponent<CommunityFeedItem>();
            
            if (feedItem == null)
            {
                feedItem = viralItemObj.AddComponent<CommunityFeedItem>();
            }
            
            feedItem.SetupWithProPlayerSupport(feedback);
            HighlightViralContent(feedItem);
            
            viralFeedItems.Insert(0, feedItem);
            viralItemObj.transform.SetAsFirstSibling();
            
            // Limit viral items
            while (viralFeedItems.Count > 5)
            {
                RemoveOldestViralItem();
            }
            
            Debug.Log($"🌟 Added viral feed item: {feedback.author}");
        }
        
        private void HighlightViralContent(CommunityFeedItem feedItem)
        {
            // Add visual highlight for viral content
            var background = feedItem.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(1f, 0.8f, 0.2f, 0.3f); // Golden highlight
            }
            
            // You could add a "🔥" icon or other visual indicator here
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
        
        private void RemoveOldestViralItem()
        {
            if (viralFeedItems.Count == 0) return;
            
            var oldestItem = viralFeedItems[viralFeedItems.Count - 1];
            viralFeedItems.RemoveAt(viralFeedItems.Count - 1);
            
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
            
            // Update tab button colors if you have them
            if (communityTabButton != null)
            {
                var colors = communityTabButton.colors;
                colors.normalColor = showCommunity ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                communityTabButton.colors = colors;
            }
            
            if (eventsTabButton != null)
            {
                var colors = eventsTabButton.colors;
                colors.normalColor = !showCommunity ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                eventsTabButton.colors = colors;
            }
            
            // Show/hide panels if you have them
            if (communityFeedPanel != null)
                communityFeedPanel.SetActive(showCommunity);
                
            if (eventsPanel != null)
                eventsPanel.SetActive(!showCommunity);
            
            // Load existing feedback when switching to community tab
            if (showCommunity)
            {
                LoadExistingFeedback();
            }
        }
        
        private void LoadExistingFeedback()
        {
            // Clear current items
            ClearFeedItems();
            
            // FIXED: Use the enhanced API method
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                var existingFeedback = feedbackManager.GetActiveFeedback();
                
                foreach (var feedback in existingFeedback.Take(maxVisibleItems))
                {
                    AddFeedItem(feedback);
                }
                
                Debug.Log($"📋 Loaded {existingFeedback.Count} existing feedback items");
            }
        }
        
        private void ClearFeedItems()
        {
            foreach (var item in activeFeedItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            activeFeedItems.Clear();
            
            foreach (var item in viralFeedItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            viralFeedItems.Clear();
        }
        
        #endregion
        
        #region Enhanced Display Updates
        
        private void UpdateSentimentDisplay(float sentiment)
        {
            if (sentimentSlider != null)
                sentimentSlider.value = sentiment / 100f;
                
            if (sentimentText != null)
            {
                sentimentText.text = $"{sentiment:F1}%";
                sentimentText.color = GetSentimentColor(sentiment);
            }
        }
        
        private void UpdateSentimentTrend()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager == null || sentimentTrendText == null) return;
            
            float trend = feedbackManager.GetSentimentTrend();
            
            if (Mathf.Abs(trend) < 0.1f)
            {
                sentimentTrendText.text = "→ Stable";
                sentimentTrendText.color = Color.gray;
            }
            else if (trend > 0)
            {
                sentimentTrendText.text = $"↗ +{trend:F1}%";
                sentimentTrendText.color = Color.green;
            }
            else
            {
                sentimentTrendText.text = $"↘ {trend:F1}%";
                sentimentTrendText.color = Color.red;
            }
        }
        
        private void UpdateEnhancedDisplays()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager == null) return;
            
            // Update meta stability
            if (metaStabilityText != null)
            {
                float stability = feedbackManager.GetMetaStabilityScore();
                metaStabilityText.text = $"Meta: {stability:F0}%";
                metaStabilityText.color = GetStabilityColor(stability);
            }
            
            // Update season info
            if (seasonInfoText != null)
            {
                string seasonInfo = "";
                if (feedbackManager.IsRankedSeason()) seasonInfo += "🏆 Ranked ";
                if (feedbackManager.IsTournamentSeason()) seasonInfo += "🎯 Tournament ";
                if (string.IsNullOrEmpty(seasonInfo)) seasonInfo = "📅 Off-Season";
                
                seasonInfoText.text = seasonInfo.Trim();
            }
            
            // Update feedback counts
            if (totalFeedbackText != null)
            {
                int activeCount = feedbackManager.GetActiveFeedback().Count;
                totalFeedbackText.text = $"Total: {activeCount}";
            }
            
            if (viralFeedbackText != null)
            {
                int viralCount = feedbackManager.GetViralFeedback().Count;
                viralFeedbackText.text = $"Viral: {viralCount}";
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private Color GetSentimentColor(float sentiment)
        {
            return sentiment switch
            {
                >= 70f => new Color(0.2f, 0.8f, 0.2f),    // Green
                >= 40f => Color.Lerp(Color.gray, new Color(0.2f, 0.8f, 0.2f), (sentiment - 40f) / 30f),
                >= 30f => Color.gray,                      // Gray
                _ => Color.Lerp(Color.gray, new Color(0.8f, 0.2f, 0.2f), (30f - sentiment) / 30f)   // Red
            };
        }
        
        private Color GetStabilityColor(float stability)
        {
            return stability switch
            {
                >= 75f => Color.green,
                >= 50f => Color.yellow,
                >= 25f => new Color(1f, 0.5f, 0f), // Orange
                _ => Color.red
            };
        }
        
        #endregion
        
        #region Enhanced Test Methods
        
        [ContextMenu("🧪 Test Enhanced Pro Player Feedback")]
        public void TestEnhancedProPlayerFeedback()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "TSM_Legend",
                content = "Finally! Warrior feels balanced now ★ These health changes improve competitive diversity",
                sentiment = 0.8f,
                feedbackType = Community.FeedbackType.ProPlayerOpinion,
                communitySegment = "Pro Players",
                timestamp = System.DateTime.Now,
                upvotes = 95,
                replies = 28,
                isViralCandidate = true,
                impactScore = 2.5f
            };
            
            AddFeedItem(testFeedback);
            Debug.Log("🧪 Added enhanced pro player test feedback");
        }
        
        [ContextMenu("🌟 Test Viral Content Creator Feedback")]
        public void TestViralContentCreatorFeedback()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "GameGuruYT",
                content = "MASSIVE patch analysis coming! ▲ These changes will reshape the entire meta",
                sentiment = 0.9f,
                feedbackType = Community.FeedbackType.ContentCreator,
                communitySegment = "Content Creators",
                timestamp = System.DateTime.Now,
                upvotes = 187,
                replies = 56,
                isViralCandidate = true,
                impactScore = 3.2f
            };
            
            AddFeedItem(testFeedback);
            if (viralFeedbackContainer != null)
            {
                AddViralFeedItem(testFeedback);
            }
            
            Debug.Log("🌟 Added viral content creator test feedback");
        }
        
        [ContextMenu("📊 Test Enhanced UI Updates")]
        public void TestEnhancedUIUpdates()
        {
            UpdateSentimentDisplay(72.5f);
            UpdateSentimentTrend();
            UpdateEnhancedDisplays();
            
            Debug.Log("📊 Tested all enhanced UI updates");
        }
        
        [ContextMenu("🔄 Refresh From Enhanced Manager")]
        public void RefreshFromEnhancedManager()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                // Update sentiment
                float sentiment = feedbackManager.GetCommunitySentiment();
                UpdateSentimentDisplay(sentiment);
                
                // Update all enhanced displays
                UpdateEnhancedDisplays();
                
                // Reload feedback
                LoadExistingFeedback();
                
                Debug.Log($"🔄 Refreshed UI from enhanced manager - Sentiment: {sentiment:F1}%");
            }
            else
            {
                Debug.LogError("❌ Enhanced CommunityFeedbackManager not found!");
            }
        }
        
        [ContextMenu("🧹 Clear All Enhanced Items")]
        public void ClearAllEnhancedItems()
        {
            ClearFeedItems();
            Debug.Log("🧹 Cleared all enhanced feed items");
        }
        
        [ContextMenu("📋 Show Enhanced Debug Info")]
        public void ShowEnhancedDebugInfo()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            
            Debug.Log("=== 📋 ENHANCED UI DEBUG INFO ===");
            Debug.Log($"Community Container: {communityFeedContainer?.name ?? "NULL"}");
            Debug.Log($"Feed Item Prefab: {feedItemPrefab?.name ?? "NULL"}");
            Debug.Log($"Active Feed Items: {activeFeedItems.Count}");
            Debug.Log($"Viral Feed Items: {viralFeedItems.Count}");
            Debug.Log($"Max Visible Items: {maxVisibleItems}");
            Debug.Log($"Showing Community Feed: {showingCommunityFeed}");
            
            if (feedbackManager != null)
            {
                Debug.Log($"Manager Active Feedback: {feedbackManager.GetActiveFeedback().Count}");
                Debug.Log($"Manager Viral Feedback: {feedbackManager.GetViralFeedback().Count}");
                Debug.Log($"Current Sentiment: {feedbackManager.GetCommunitySentiment():F1}%");
                Debug.Log($"Sentiment Trend: {feedbackManager.GetSentimentTrend():F2}");
                Debug.Log($"Meta Stability: {feedbackManager.GetMetaStabilityScore():F1}%");
                Debug.Log($"Is Ranked Season: {feedbackManager.IsRankedSeason()}");
                Debug.Log($"Is Tournament Season: {feedbackManager.IsTournamentSeason()}");
            }
            else
            {
                Debug.Log("❌ Enhanced CommunityFeedbackManager not found!");
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.RemoveListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(OnSentimentChanged);
                Community.CommunityFeedbackManager.Instance.OnStrategyActivated.RemoveListener(OnStrategyActivated);
                Community.CommunityFeedbackManager.Instance.OnViralFeedbackGenerated.RemoveListener(OnViralFeedbackGenerated);
            }
            
            Debug.Log("🎭 Enhanced Community Feed UI Manager destroyed and cleaned up");
        }
    }
}