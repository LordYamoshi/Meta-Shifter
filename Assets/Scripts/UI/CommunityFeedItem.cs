using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MetaBalance.UI
{
    /// <summary>
    /// Individual community feed item component
    /// </summary>
    public class CommunityFeedItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI authorText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI timestampText;
        [SerializeField] private TextMeshProUGUI segmentText;
        [SerializeField] private TextMeshProUGUI upvotesText;
        [SerializeField] private TextMeshProUGUI repliesText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image sentimentIndicator;
        [SerializeField] private Image typeIcon;
        [SerializeField] private GameObject highlightBorder;
        
        [Header("Visual Settings")]
        [SerializeField] private Color defaultBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Color highlightBackgroundColor = new Color(0.2f, 0.4f, 0.6f, 0.9f);
        
        private Community.CommunityFeedback currentFeedback;
        private bool isHighlighted = false;
        
        private void Awake()
        {
            AutoFindComponents();
        }
        
        private void AutoFindComponents()
        {
            if (authorText == null)
                authorText = transform.Find("Header/Author")?.GetComponent<TextMeshProUGUI>();
            
            if (contentText == null)
                contentText = transform.Find("Content")?.GetComponent<TextMeshProUGUI>();
            
            if (timestampText == null)
                timestampText = transform.Find("Footer/Timestamp")?.GetComponent<TextMeshProUGUI>();
            
            if (segmentText == null)
                segmentText = transform.Find("Header/Segment")?.GetComponent<TextMeshProUGUI>();
            
            if (upvotesText == null)
                upvotesText = transform.Find("Footer/Upvotes")?.GetComponent<TextMeshProUGUI>();
            
            if (repliesText == null)
                repliesText = transform.Find("Footer/Replies")?.GetComponent<TextMeshProUGUI>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (sentimentIndicator == null)
                sentimentIndicator = transform.Find("SentimentIndicator")?.GetComponent<Image>();
            
            if (typeIcon == null)
                typeIcon = transform.Find("Header/TypeIcon")?.GetComponent<Image>();
            
            if (highlightBorder == null)
                highlightBorder = transform.Find("HighlightBorder")?.gameObject;
        }
        
        public void Setup(Community.CommunityFeedback feedback)
        {
            currentFeedback = feedback;
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (currentFeedback == null) return;
            
            UpdateTextContent();
            UpdateVisualStyling();
            UpdateEngagementDisplay();
            RefreshTimestamp();
        }
        
        private void UpdateTextContent()
        {
            if (authorText != null)
            {
                authorText.text = currentFeedback.author;
                authorText.color = GetAuthorColor();
            }
            
            if (contentText != null)
            {
                contentText.text = currentFeedback.content;
            }
            
            if (segmentText != null)
            {
                segmentText.text = currentFeedback.communitySegment;
                segmentText.color = GetSegmentColor(currentFeedback.communitySegment);
            }
        }
        
        private void UpdateVisualStyling()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = isHighlighted ? highlightBackgroundColor : defaultBackgroundColor;
            }
            
            if (sentimentIndicator != null)
            {
                sentimentIndicator.color = currentFeedback.GetSentimentColor();
                
                float alpha = Mathf.Lerp(0.3f, 1f, Mathf.Abs(currentFeedback.sentiment));
                var color = sentimentIndicator.color;
                color.a = alpha;
                sentimentIndicator.color = color;
            }
            
            if (typeIcon != null)
            {
                typeIcon.sprite = GetTypeIcon(currentFeedback.feedbackType);
                typeIcon.color = GetTypeColor(currentFeedback.feedbackType);
            }
            
            if (highlightBorder != null)
            {
                highlightBorder.SetActive(isHighlighted);
            }
        }
        
        private void UpdateEngagementDisplay()
        {
            if (upvotesText != null)
            {
                if (currentFeedback.upvotes > 0)
                {
                    upvotesText.text = $"👍 {FormatNumber(currentFeedback.upvotes)}";
                    upvotesText.gameObject.SetActive(true);
                }
                else
                {
                    upvotesText.gameObject.SetActive(false);
                }
            }
            
            if (repliesText != null)
            {
                if (currentFeedback.replies > 0)
                {
                    repliesText.text = $"💬 {FormatNumber(currentFeedback.replies)}";
                    repliesText.gameObject.SetActive(true);
                }
                else
                {
                    repliesText.gameObject.SetActive(false);
                }
            }
        }
        
        public void RefreshTimestamp()
        {
            if (timestampText != null && currentFeedback != null)
            {
                timestampText.text = currentFeedback.GetTimeAgo();
            }
        }
        
        public void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;
            UpdateVisualStyling();
        }
        
        private Color GetAuthorColor()
        {
            return currentFeedback.communitySegment switch
            {
                "Pro Players" => new Color(1f, 0.8f, 0.2f),
                "Content Creators" => new Color(0.8f, 0.2f, 0.8f),
                "Competitive" => new Color(0.2f, 0.8f, 0.2f),
                "Casual Players" => new Color(0.4f, 0.7f, 1f),
                _ => Color.white
            };
        }
        
        private Color GetSegmentColor(string segment)
        {
            return segment switch
            {
                "Pro Players" => new Color(1f, 0.8f, 0.2f, 0.8f),
                "Content Creators" => new Color(0.8f, 0.2f, 0.8f, 0.8f),
                "Competitive" => new Color(0.2f, 0.8f, 0.2f, 0.8f),
                "Casual Players" => new Color(0.4f, 0.7f, 1f, 0.8f),
                _ => new Color(0.7f, 0.7f, 0.7f, 0.8f)
            };
        }
        
        private Sprite GetTypeIcon(Community.FeedbackType feedbackType)
        {
            // Return appropriate icon sprite based on feedback type
            // For now, return null - icons would be loaded from Resources or assigned in inspector
            return null;
        }
        
        private Color GetTypeColor(Community.FeedbackType feedbackType)
        {
            return feedbackType switch
            {
                Community.FeedbackType.BalanceReaction => new Color(1f, 0.6f, 0.2f),
                Community.FeedbackType.PopularityShift => new Color(0.2f, 0.8f, 0.8f),
                Community.FeedbackType.MetaAnalysis => new Color(0.6f, 0.2f, 1f),
                Community.FeedbackType.ProPlayerOpinion => new Color(1f, 0.8f, 0.2f),
                Community.FeedbackType.CasualPlayerFeedback => new Color(0.4f, 0.7f, 1f),
                Community.FeedbackType.ContentCreator => new Color(0.8f, 0.2f, 0.8f),
                Community.FeedbackType.CompetitiveScene => new Color(0.2f, 0.8f, 0.2f),
                Community.FeedbackType.Meme => new Color(1f, 1f, 0.2f),
                Community.FeedbackType.Bug => new Color(1f, 0.2f, 0.2f),
                Community.FeedbackType.Suggestion => new Color(0.2f, 1f, 0.2f),
                _ => Color.white
            };
        }
        
        private string FormatNumber(int number)
        {
            return number switch
            {
                >= 1000000 => $"{number / 1000000f:F1}M",
                >= 1000 => $"{number / 1000f:F1}K",
                _ => number.ToString()
            };
        }
        
        public void OnItemClicked()
        {
            StartCoroutine(AnimateClick());
        }
        
        private System.Collections.IEnumerator AnimateClick()
        {
            var rectTransform = GetComponent<RectTransform>();
            Vector3 originalScale = rectTransform.localScale;
            
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1f, 0.95f, elapsed / 0.1f);
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }
            
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(0.95f, 1f, elapsed / 0.1f);
                rectTransform.localScale = originalScale * scale;
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }
        
        public void ShowDetailedView()
        {
            Debug.Log($"Showing details for feedback from {currentFeedback.author}: {currentFeedback.content}");
        }
        
        public void SetViral(bool isViral)
        {
            if (isViral)
            {
                StartCoroutine(ViralEffect());
            }
        }
        
        private System.Collections.IEnumerator ViralEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                if (sentimentIndicator != null)
                {
                    var originalColor = sentimentIndicator.color;
                    
                    sentimentIndicator.color = Color.yellow;
                    yield return new WaitForSeconds(0.2f);
                    
                    sentimentIndicator.color = originalColor;
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
        
        public void UpdateSentiment(float newSentiment)
        {
            if (currentFeedback != null)
            {
                currentFeedback.sentiment = newSentiment;
                UpdateVisualStyling();
            }
        }
        
        public void SimulateEngagementGrowth()
        {
            if (currentFeedback == null) return;
            
            if (Random.Range(0f, 1f) < 0.3f)
            {
                currentFeedback.upvotes += Random.Range(1, 5);
            }
            
            if (Random.Range(0f, 1f) < 0.2f)
            {
                currentFeedback.replies += Random.Range(0, 2);
            }
            
            UpdateEngagementDisplay();
        }
    }
}