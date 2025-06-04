using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace MetaBalance.UI
{
    /// <summary>
    /// UI Manager for displaying community feedback in the community feed
    /// </summary>
    public class CommunityFeedUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform feedContainer;
        [SerializeField] private GameObject feedItemPrefab;
        [SerializeField] private ScrollRect feedScrollRect;
        [SerializeField] private Button communityTabButton;
        [SerializeField] private Button eventsTabButton;
        [SerializeField] private TextMeshProUGUI sentimentText;
        [SerializeField] private Slider sentimentSlider;
        
        [Header("Feed Settings")]
        [SerializeField] private int maxVisibleItems = 15;
        [SerializeField] private float newItemAnimationDuration = 0.5f;
        [SerializeField] private float feedRefreshInterval = 2f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color positiveColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color neutralColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color negativeColor = new Color(0.8f, 0.2f, 0.2f);
        
        // Object pool for feed items
        private Queue<CommunityFeedItem> feedItemPool = new Queue<CommunityFeedItem>();
        private List<CommunityFeedItem> activeFeedItems = new List<CommunityFeedItem>();
        
        // State management
        private bool showingCommunityFeed = true;
        private Queue<Community.CommunityFeedback> pendingFeedback = new Queue<Community.CommunityFeedback>();
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            InitializeFeedItemPool();
            StartCoroutine(FeedRefreshCoroutine());
        }
        
        private void InitializeUI()
        {
            if (communityTabButton != null)
            {
                communityTabButton.onClick.AddListener(() => SwitchTab(true));
            }
            
            if (eventsTabButton != null)
            {
                eventsTabButton.onClick.AddListener(() => SwitchTab(false));
            }
            
            SwitchTab(true);
            UpdateSentimentDisplay(65f);
        }
        
        private void SubscribeToEvents()
        {
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.AddListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.AddListener(OnSentimentChanged);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        private void InitializeFeedItemPool()
        {
            for (int i = 0; i < maxVisibleItems + 5; i++)
            {
                var item = CreateFeedItem();
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    feedItemPool.Enqueue(item);
                }
            }
        }
        
        private CommunityFeedItem CreateFeedItem()
        {
            if (feedItemPrefab == null || feedContainer == null)
            {
                Debug.LogError("Feed item prefab or container not assigned!");
                return null;
            }
            
            GameObject itemObject = Instantiate(feedItemPrefab, feedContainer);
            var feedItem = itemObject.GetComponent<CommunityFeedItem>();
            
            if (feedItem == null)
            {
                feedItem = itemObject.AddComponent<CommunityFeedItem>();
            }
            
            return feedItem;
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Feedback)
            {
                HighlightFeedForPhase();
            }
        }
        
        private void OnNewFeedbackReceived(Community.CommunityFeedback feedback)
        {
            pendingFeedback.Enqueue(feedback);
            
            if (pendingFeedback.Count <= 3)
            {
                ProcessPendingFeedback();
            }
        }
        
        private void OnSentimentChanged(float newSentiment)
        {
            UpdateSentimentDisplay(newSentiment);
        }
        
        private IEnumerator FeedRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(feedRefreshInterval);
                
                if (pendingFeedback.Count > 0)
                {
                    ProcessPendingFeedback();
                }
                
                RefreshActiveFeedItems();
            }
        }
        
        private void ProcessPendingFeedback()
        {
            int processed = 0;
            const int maxProcessPerFrame = 2;
            
            while (pendingFeedback.Count > 0 && processed < maxProcessPerFrame)
            {
                var feedback = pendingFeedback.Dequeue();
                if (showingCommunityFeed)
                {
                    DisplayFeedbackItem(feedback);
                }
                processed++;
            }
        }
        
        private void DisplayFeedbackItem(Community.CommunityFeedback feedback)
        {
            var feedItem = GetPooledFeedItem();
            if (feedItem == null) return;
            
            feedItem.Setup(feedback);
            feedItem.gameObject.SetActive(true);
            
            activeFeedItems.Insert(0, feedItem);
            feedItem.transform.SetAsFirstSibling();
            
            StartCoroutine(AnimateFeedItemIn(feedItem));
            
            if (activeFeedItems.Count > maxVisibleItems)
            {
                RemoveOldestFeedItem();
            }
            
            if (feedScrollRect != null)
            {
                feedScrollRect.verticalNormalizedPosition = 1f;
            }
        }
        
        private CommunityFeedItem GetPooledFeedItem()
        {
            if (feedItemPool.Count > 0)
            {
                return feedItemPool.Dequeue();
            }
            
            return CreateFeedItem();
        }
        
        private void ReturnFeedItemToPool(CommunityFeedItem item)
        {
            if (item == null) return;
            
            item.gameObject.SetActive(false);
            item.transform.SetParent(feedContainer);
            feedItemPool.Enqueue(item);
        }
        
        private void RemoveOldestFeedItem()
        {
            if (activeFeedItems.Count == 0) return;
            
            var oldestItem = activeFeedItems[activeFeedItems.Count - 1];
            activeFeedItems.RemoveAt(activeFeedItems.Count - 1);
            
            StartCoroutine(AnimateFeedItemOut(oldestItem));
        }
        
        private IEnumerator AnimateFeedItemIn(CommunityFeedItem item)
        {
            if (item == null) yield break;
            
            var canvasGroup = item.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
            
            var rectTransform = item.GetComponent<RectTransform>();
            Vector3 originalPosition = rectTransform.localPosition;
            Vector3 startPosition = originalPosition + Vector3.right * 300f;
            
            rectTransform.localPosition = startPosition;
            canvasGroup.alpha = 0f;
            
            float elapsed = 0f;
            while (elapsed < newItemAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / newItemAnimationDuration;
                float easedT = 1f - Mathf.Pow(1f - t, 3f);
                
                rectTransform.localPosition = Vector3.Lerp(startPosition, originalPosition, easedT);
                canvasGroup.alpha = easedT;
                
                yield return null;
            }
            
            rectTransform.localPosition = originalPosition;
            canvasGroup.alpha = 1f;
        }
        
        private IEnumerator AnimateFeedItemOut(CommunityFeedItem item)
        {
            if (item == null) yield break;
            
            var canvasGroup = item.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
            
            var rectTransform = item.GetComponent<RectTransform>();
            Vector3 originalPosition = rectTransform.localPosition;
            Vector3 endPosition = originalPosition + Vector3.left * 300f;
            
            float elapsed = 0f;
            float duration = newItemAnimationDuration * 0.5f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easedT = t * t;
                
                rectTransform.localPosition = Vector3.Lerp(originalPosition, endPosition, easedT);
                canvasGroup.alpha = 1f - easedT;
                
                yield return null;
            }
            
            ReturnFeedItemToPool(item);
        }
        
        private void RefreshActiveFeedItems()
        {
            foreach (var item in activeFeedItems)
            {
                if (item != null)
                {
                    item.RefreshTimestamp();
                }
            }
        }
        
        private void SwitchTab(bool showCommunityFeed)
        {
            showingCommunityFeed = showCommunityFeed;
            
            if (communityTabButton != null)
            {
                communityTabButton.interactable = !showCommunityFeed;
            }
            
            if (eventsTabButton != null)
            {
                eventsTabButton.interactable = showCommunityFeed;
            }
            
            if (showCommunityFeed)
            {
                ShowCommunityFeed();
            }
            else
            {
                ShowEventsView();
            }
        }
        
        private void ShowCommunityFeed()
        {
            ClearFeedDisplay();
            
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager != null)
            {
                var activeFeedback = feedbackManager.GetActiveFeedback();
                
                foreach (var feedback in activeFeedback.Take(maxVisibleItems))
                {
                    DisplayFeedbackItem(feedback);
                }
            }
        }
        
        private void ShowEventsView()
        {
            ClearFeedDisplay();
            DisplayActiveEvents();
        }
        
        private void DisplayActiveEvents()
        {
            var mockEvents = new List<Community.CommunityFeedback>
            {
                new Community.CommunityFeedback
                {
                    author = "System",
                    content = "🚨 Support exploit discovered - requires immediate response",
                    sentiment = -0.8f,
                    feedbackType = Community.FeedbackType.Bug,
                    communitySegment = "System",
                    timestamp = System.DateTime.Now.AddMinutes(-2)
                }
            };
            
            foreach (var eventItem in mockEvents)
            {
                DisplayFeedbackItem(eventItem);
            }
        }
        
        private void ClearFeedDisplay()
        {
            foreach (var item in activeFeedItems)
            {
                ReturnFeedItemToPool(item);
            }
            activeFeedItems.Clear();
        }
        
        private void UpdateSentimentDisplay(float sentiment)
        {
            if (sentimentSlider != null)
            {
                sentimentSlider.value = sentiment / 100f;
            }
            
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
                >= 70f => positiveColor,
                >= 40f => Color.Lerp(neutralColor, positiveColor, (sentiment - 40f) / 30f),
                >= 30f => neutralColor,
                _ => Color.Lerp(neutralColor, negativeColor, (30f - sentiment) / 30f)
            };
        }
        
        private void HighlightFeedForPhase()
        {
            StartCoroutine(PulseFeedContainer());
        }
        
        private IEnumerator PulseFeedContainer()
        {
            if (feedContainer == null) yield break;
            
            var canvasGroup = feedContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = feedContainer.gameObject.AddComponent<CanvasGroup>();
            
            for (int i = 0; i < 3; i++)
            {
                float elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 1.3f, elapsed / 0.5f);
                    yield return null;
                }
                
                elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1.3f, 1f, elapsed / 0.5f);
                    yield return null;
                }
            }
            
            canvasGroup.alpha = 1f;
        }
        
        public void ForceRefreshFeed()
        {
            if (showingCommunityFeed)
            {
                ShowCommunityFeed();
            }
            else
            {
                ShowEventsView();
            }
        }
        
        private void OnDestroy()
        {
            if (Community.CommunityFeedbackManager.Instance != null)
            {
                Community.CommunityFeedbackManager.Instance.OnNewFeedbackAdded.RemoveListener(OnNewFeedbackReceived);
                Community.CommunityFeedbackManager.Instance.OnCommunitySentimentChanged.RemoveListener(OnSentimentChanged);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
            
            StopAllCoroutines();
        }
    }
}