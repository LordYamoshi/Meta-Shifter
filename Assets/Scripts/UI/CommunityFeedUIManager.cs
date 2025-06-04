using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    /// <summary>
    /// Minimal Community Feed Manager - Just manages text updates, doesn't mess with layouts
    /// Works with your existing UI setup and feed item prefabs
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
        
        [Header("Sentiment Display (Optional)")]
        [SerializeField] private TextMeshProUGUI sentimentText;
        [SerializeField] private Slider sentimentSlider;
        
        // Simple lists to track active items
        private List<CommunityFeedItem> activeFeedItems = new List<CommunityFeedItem>();
        private bool showingCommunityFeed = true;
        
        private void Start()
        {
            SetupBasicUI();
            SubscribeToEvents();
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
        }
        
        private void SubscribeToEvents()
        {
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.AddListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(UpdateSentimentDisplay);
            }
        }
        
        private void OnNewFeedbackReceived(Community.CommunityFeedback feedback)
        {
            if (showingCommunityFeed)
            {
                AddFeedItem(feedback);
            }
        }
        
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
            
            // Setup the feed item with your data
            feedItem.SetupWithProPlayerSupport(feedback);
            
            // Add to our tracking list
            activeFeedItems.Insert(0, feedItem);
            
            // Move to top (your layout group should handle positioning)
            newItemObj.transform.SetAsFirstSibling();
            
            // Remove old items if we have too many
            while (activeFeedItems.Count > maxVisibleItems)
            {
                RemoveOldestFeedItem();
            }
            
            Debug.Log($"✅ Added feed item: {feedback.author} - Total items: {activeFeedItems.Count}");
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
            
            // Load from feedback manager if available
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                var existingFeedback = feedbackManager.GetActiveFeedback();
                
                foreach (var feedback in existingFeedback.Take(maxVisibleItems))
                {
                    AddFeedItem(feedback);
                }
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
        }
        
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
        
        // Test methods for debugging
        [ContextMenu("🧪 Test Add Pro Player Feedback")]
        public void TestAddProPlayerFeedback()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "TSM_Legend",
                content = "Finally! Warrior feels balanced now 💪 These health changes improve competitive diversity",
                sentiment = 0.8f,
                feedbackType = Community.FeedbackType.ProPlayerOpinion,
                communitySegment = "Pro Players",
                timestamp = System.DateTime.Now,
                upvotes = 45,
                replies = 12
            };
            
            AddFeedItem(testFeedback);
        }
        
        [ContextMenu("🧪 Test Add Content Creator Feedback")]
        public void TestAddContentCreatorFeedback()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "GameGuruYT",
                content = "Support utility nerf feels too harsh 😔 Making a reaction video tonight!",
                sentiment = -0.6f,
                feedbackType = Community.FeedbackType.ContentCreator,
                communitySegment = "Content Creators",
                timestamp = System.DateTime.Now,
                upvotes = 87,
                replies = 34
            };
            
            AddFeedItem(testFeedback);
        }
        
        [ContextMenu("🧪 Test Add Multiple Items")]
        public void TestAddMultipleItems()
        {
            TestAddProPlayerFeedback();
            
            var casualFeedback = new Community.CommunityFeedback
            {
                author = "CasualGamer42",
                content = "I like these Warrior changes! More fun to play now 😊",
                sentiment = 0.6f,
                feedbackType = Community.FeedbackType.CasualPlayerFeedback,
                communitySegment = "Casual Players",
                timestamp = System.DateTime.Now,
                upvotes = 8,
                replies = 3
            };
            AddFeedItem(casualFeedback);
            
            TestAddContentCreatorFeedback();
        }
        
        [ContextMenu("🧹 Clear All Feed Items")]
        public void TestClearAllItems()
        {
            ClearFeedItems();
        }
        
        [ContextMenu("📊 Debug: Show Feed Info")]
        public void DebugShowFeedInfo()
        {
            Debug.Log("=== 📊 FEED DEBUG INFO ===");
            Debug.Log($"Community Container: {communityFeedContainer?.name ?? "NULL"}");
            Debug.Log($"Feed Item Prefab: {feedItemPrefab?.name ?? "NULL"}");
            Debug.Log($"Active Feed Items: {activeFeedItems.Count}");
            Debug.Log($"Max Visible Items: {maxVisibleItems}");
            Debug.Log($"Showing Community Feed: {showingCommunityFeed}");
            
            if (communityFeedContainer != null)
            {
                Debug.Log($"Container Child Count: {communityFeedContainer.childCount}");
            }
        }
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.RemoveListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(UpdateSentimentDisplay);
            }
        }
    }
}