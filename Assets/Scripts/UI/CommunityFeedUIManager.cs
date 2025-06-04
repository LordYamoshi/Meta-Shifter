using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    /// <summary>
    /// Enhanced Community Feed UI Manager - Phase-aware with sequential feedback display
    /// Only shows feedback during Feedback Phase, displays them one by one with animations
    /// </summary>
    public class CommunityFeedUIManager : MonoBehaviour
    {
        [Header("Basic UI References")]
        [SerializeField] private Transform communityFeedContainer;
        [SerializeField] private GameObject feedItemPrefab;
        [SerializeField] private int maxVisibleItems = 15;
        
        [Header("Phase Control")]
        [SerializeField] private GameObject feedbackPanelContainer; // Hide this during non-feedback phases
        [SerializeField] private TextMeshProUGUI phaseStatusText;
        [SerializeField] private GameObject sequenceProgressIndicator; // Optional loading indicator
        
        [Header("Tab Controls (Optional)")]
        [SerializeField] private Button communityTabButton;
        [SerializeField] private Button eventsTabButton;
        [SerializeField] private GameObject communityFeedPanel;
        [SerializeField] private GameObject eventsPanel;
        
        [Header("Sentiment Display (Optional)")]
        [SerializeField] private TextMeshProUGUI sentimentText;
        [SerializeField] private Slider sentimentSlider;
        
        [Header("Animation Settings")]
        [SerializeField] private float itemSlideInDuration = 0.5f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Simple lists to track active items
        private List<CommunityFeedItem> activeFeedItems = new List<CommunityFeedItem>();
        private bool showingCommunityFeed = true;
        private bool isFeedbackPhase = false;
        
        private void Start()
        {
            SetupBasicUI();
            SubscribeToEvents();
            UpdatePhaseDisplay();
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
            
            // Hide progress indicator initially
            if (sequenceProgressIndicator != null)
                sequenceProgressIndicator.SetActive(false);
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to feedback manager events
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.AddListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(UpdateSentimentDisplay);
                Community.CommunityFeedbackManager.Instance.OnFeedbackSequenceStarted.AddListener(OnSequenceStarted);
                Community.CommunityFeedbackManager.Instance.OnFeedbackSequenceCompleted.AddListener(OnSequenceCompleted);
            }
            
            // Subscribe to phase manager events
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            isFeedbackPhase = (newPhase == Core.GamePhase.Feedback);
            UpdatePhaseDisplay();
            
            // DON'T clear feedback when leaving feedback phase
            // Feedback should accumulate throughout the entire game
            Debug.Log($"🎭 Phase changed to {newPhase} - feedback remains visible");
        }
        
        private void UpdatePhaseDisplay()
        {
            // Update phase status text
            if (phaseStatusText != null)
            {
                if (isFeedbackPhase)
                {
                    phaseStatusText.text = "Community Feedback - Live Reactions";
                    phaseStatusText.color = new Color(0.2f, 0.8f, 0.2f); // Green
                }
                else
                {
                    phaseStatusText.text = "Community Feed - Previous Reactions";
                    phaseStatusText.color = new Color(0.6f, 0.8f, 1f); // Light Blue
                }
            }
            
            // Keep feedback panel always visible (remove phase-based hiding)
            // if (feedbackPanelContainer != null)
            // {
            //     feedbackPanelContainer.SetActive(true); // Always show
            // }
        }
        
        private void OnSequenceStarted()
        {
            Debug.Log("🎬 Feedback sequence started - showing progress indicator");
            
            if (sequenceProgressIndicator != null)
                sequenceProgressIndicator.SetActive(true);
                
            if (phaseStatusText != null)
            {
                phaseStatusText.text = "Community Reacting...";
                phaseStatusText.color = new Color(1f, 0.8f, 0.2f); // Orange
            }
        }
        
        private void OnSequenceCompleted()
        {
            Debug.Log("✅ Feedback sequence completed - hiding progress indicator");
            
            if (sequenceProgressIndicator != null)
                sequenceProgressIndicator.SetActive(false);
                
            if (phaseStatusText != null && isFeedbackPhase)
            {
                phaseStatusText.text = "Community Feedback - All Reactions Received";
                phaseStatusText.color = new Color(0.2f, 0.8f, 0.2f); // Green
            }
        }
        
        private void OnNewFeedbackReceived(Community.CommunityFeedback feedback)
        {
            // Only add NEW feedback during feedback phase, but don't remove existing feedback
            if (!isFeedbackPhase)
            {
                Debug.Log($"📝 Feedback received outside feedback phase - storing for later: {feedback.author}");
                return; // Don't display, but don't remove existing ones either
            }
            
            if (!showingCommunityFeed)
            {
                Debug.Log($"❌ Ignoring feedback - not showing community tab: {feedback.author}");
                return;
            }
            
            Debug.Log($"📝 Displaying NEW feedback during feedback phase: {feedback.author}");
            AddFeedItemWithAnimation(feedback);
        }
        
        private void AddFeedItemWithAnimation(Community.CommunityFeedback feedback)
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
            
            // Start slide-in animation
            StartCoroutine(AnimateItemSlideIn(feedItem));
            
            // Remove old items if we have too many
            while (activeFeedItems.Count > maxVisibleItems)
            {
                RemoveOldestFeedItem();
            }
            
            Debug.Log($"✅ Added animated feed item: {feedback.author} - Total items: {activeFeedItems.Count}");
        }
        
        private IEnumerator AnimateItemSlideIn(CommunityFeedItem feedItem)
        {
            if (feedItem == null) yield break;
            
            var rectTransform = feedItem.GetComponent<RectTransform>();
            var canvasGroup = feedItem.GetComponent<CanvasGroup>();
            
            // Add CanvasGroup if it doesn't exist
            if (canvasGroup == null)
                canvasGroup = feedItem.gameObject.AddComponent<CanvasGroup>();
            
            // Store original values
            Vector3 originalPosition = rectTransform.localPosition;
            Vector3 originalScale = rectTransform.localScale;
            
            // Start from off-screen (slide from right)
            Vector3 startPosition = originalPosition + Vector3.right * 300f;
            Vector3 startScale = originalScale * 0.8f;
            
            // Set initial state
            rectTransform.localPosition = startPosition;
            rectTransform.localScale = startScale;
            canvasGroup.alpha = 0f;
            
            // Animate to final position
            float elapsed = 0f;
            
            while (elapsed < itemSlideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / itemSlideInDuration;
                float curveT = slideInCurve.Evaluate(t);
                
                // Slide in from right
                rectTransform.localPosition = Vector3.Lerp(startPosition, originalPosition, curveT);
                
                // Scale up slightly
                rectTransform.localScale = Vector3.Lerp(startScale, originalScale, curveT);
                
                // Fade in
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, curveT);
                
                yield return null;
            }
            
            // Ensure final state
            rectTransform.localPosition = originalPosition;
            rectTransform.localScale = originalScale;
            canvasGroup.alpha = 1f;
        }
        
        private void RemoveOldestFeedItem()
        {
            if (activeFeedItems.Count == 0) return;
            
            var oldestItem = activeFeedItems[activeFeedItems.Count - 1];
            activeFeedItems.RemoveAt(activeFeedItems.Count - 1);
            
            if (oldestItem != null && oldestItem.gameObject != null)
            {
                StartCoroutine(AnimateItemSlideOut(oldestItem));
            }
        }
        
        private IEnumerator AnimateItemSlideOut(CommunityFeedItem feedItem)
        {
            if (feedItem == null) yield break;
            
            var rectTransform = feedItem.GetComponent<RectTransform>();
            var canvasGroup = feedItem.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
                canvasGroup = feedItem.gameObject.AddComponent<CanvasGroup>();
            
            Vector3 startPosition = rectTransform.localPosition;
            Vector3 endPosition = startPosition + Vector3.left * 300f; // Slide out to left
            
            float elapsed = 0f;
            float duration = itemSlideInDuration * 0.5f; // Faster slide out
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                rectTransform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                
                yield return null;
            }
            
            Destroy(feedItem.gameObject);
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
            
            // Load existing feedback when switching to community tab (always load accumulated feedback)
            if (showCommunity)
            {
                LoadExistingFeedback();
            }
        }
        
        private void LoadExistingFeedback()
        {
            // Clear current items first
            ClearFeedItems();
            
            // Load all accumulated feedback from feedback manager
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                var existingFeedback = feedbackManager.GetActiveFeedback();
                
                Debug.Log($"📚 Loading {existingFeedback.Count} accumulated feedback items");
                
                // Add existing feedback without animation (instant display)
                foreach (var feedback in existingFeedback.Take(maxVisibleItems))
                {
                    AddFeedItemInstant(feedback);
                }
            }
        }
        
        /// <summary>
        /// Add feedback item without animation (for loading existing feedback)
        /// </summary>
        private void AddFeedItemInstant(Community.CommunityFeedback feedback)
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
            
            // Add to our tracking list (at the end for existing items)
            activeFeedItems.Add(feedItem);
            
            // Move to bottom (existing items appear in chronological order)
            newItemObj.transform.SetAsLastSibling();
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
            
            Debug.Log("🧹 Cleared all feed items");
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
        
        // Manual controls for testing
        public void ForceStartSequentialDisplay()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                feedbackManager.ForceStartSequentialDisplay();
            }
        }
        
        public void ForceStopSequentialDisplay()
        {
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                feedbackManager.ForceStopSequentialDisplay();
            }
        }
        
        // Test methods for debugging
        [ContextMenu("🧪 Test Add Pro Player Feedback")]
        public void TestAddProPlayerFeedback()
        {
            if (!isFeedbackPhase)
            {
                Debug.LogWarning("⚠️ Cannot test feedback - not in feedback phase!");
                return;
            }
            
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
            
            AddFeedItemWithAnimation(testFeedback);
        }
        
        [ContextMenu("🧪 Test Add Content Creator Feedback")]
        public void TestAddContentCreatorFeedback()
        {
            if (!isFeedbackPhase)
            {
                Debug.LogWarning("⚠️ Cannot test feedback - not in feedback phase!");
                return;
            }
            
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
            
            AddFeedItemWithAnimation(testFeedback);
        }
        
        [ContextMenu("🧪 Test Sequential Feedback (If in Feedback Phase)")]
        public void TestSequentialFeedback()
        {
            if (!isFeedbackPhase)
            {
                Debug.LogWarning("⚠️ Cannot test sequential feedback - not in feedback phase!");
                return;
            }
            
            StartCoroutine(TestSequentialFeedbackCoroutine());
        }
        
        private IEnumerator TestSequentialFeedbackCoroutine()
        {
            var testFeedbacks = new[]
            {
                new Community.CommunityFeedback
                {
                    author = "TSM_Legend",
                    content = "Finally! Warrior feels balanced now 💪",
                    sentiment = 0.8f,
                    communitySegment = "Pro Players",
                    timestamp = System.DateTime.Now
                },
                new Community.CommunityFeedback
                {
                    author = "CasualGamer42",
                    content = "I like these changes! More fun to play 😊",
                    sentiment = 0.6f,
                    communitySegment = "Casual Players",
                    timestamp = System.DateTime.Now
                },
                new Community.CommunityFeedback
                {
                    author = "GameGuruYT",
                    content = "Making a tier list video about these updates!",
                    sentiment = 0.3f,
                    communitySegment = "Content Creators",
                    timestamp = System.DateTime.Now
                }
            };
            
            foreach (var feedback in testFeedbacks)
            {
                AddFeedItemWithAnimation(feedback);
                yield return new WaitForSeconds(1.5f);
            }
        }
        
        [ContextMenu("🧹 Clear All Feed Items (Debug Only)")]
        public void TestClearAllItems()
        {
            ClearFeedItems();
            Debug.Log("🧹 DEBUG: Manually cleared all feed items (this should not happen during normal gameplay)");
        }
        
        [ContextMenu("🔄 Simulate Phase Change to Feedback")]
        public void TestSimulateFeedbackPhase()
        {
            isFeedbackPhase = true;
            UpdatePhaseDisplay();
            Debug.Log("🎭 Simulated phase change to Feedback phase");
        }
        
        [ContextMenu("🔄 Simulate Phase Change Away from Feedback")]
        public void TestSimulateNonFeedbackPhase()
        {
            isFeedbackPhase = false;
            UpdatePhaseDisplay();
            // DON'T clear feed items - they should persist
            Debug.Log("🎭 Simulated phase change away from Feedback phase - feedback remains visible");
        }
        
        [ContextMenu("📊 Debug: Show UI State")]
        public void DebugShowUIState()
        {
            Debug.Log("=== 📊 COMMUNITY UI DEBUG INFO ===");
            Debug.Log($"Community Container: {communityFeedContainer?.name ?? "NULL"}");
            Debug.Log($"Feed Item Prefab: {feedItemPrefab?.name ?? "NULL"}");
            Debug.Log($"Active Feed Items: {activeFeedItems.Count}");
            Debug.Log($"Max Visible Items: {maxVisibleItems}");
            Debug.Log($"Showing Community Feed: {showingCommunityFeed}");
            Debug.Log($"Is Feedback Phase: {isFeedbackPhase}");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            
            if (communityFeedContainer != null)
            {
                Debug.Log($"Container Child Count: {communityFeedContainer.childCount}");
            }
            
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                Debug.Log($"Is Displaying Sequence: {feedbackManager.IsDisplayingSequence()}");
                Debug.Log($"Pending Feedback Count: {feedbackManager.GetPendingFeedbackCount()}");
            }
        }
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.RemoveListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(UpdateSentimentDisplay);
                Community.CommunityFeedbackManager.Instance.OnFeedbackSequenceStarted.RemoveListener(OnSequenceStarted);
                Community.CommunityFeedbackManager.Instance.OnFeedbackSequenceCompleted.RemoveListener(OnSequenceCompleted);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
        }
    }
}