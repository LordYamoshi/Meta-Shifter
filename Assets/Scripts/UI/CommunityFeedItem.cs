using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MetaBalance.UI
{
    /// <summary>
    /// Fixed Community Feed Item - Properly shows VIP badges for all non-casual segments
    /// </summary>
    public class CommunityFeedItem : MonoBehaviour
    {
        [Header("Text References - Just Assign These")]
        [SerializeField] private TextMeshProUGUI authorText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI timestampText;
        [SerializeField] private TextMeshProUGUI upvotesText;
        [SerializeField] private TextMeshProUGUI repliesText;
        [SerializeField] private TextMeshProUGUI segmentBadgeText; // Only if you have a badge
        
        [Header("Optional Image References")]
        [SerializeField] private Image leftBorderImage;          // Only if you want border color to change
        [SerializeField] private Image segmentBadgeBackground;   // Only if you want badge color to change
        [SerializeField] private GameObject segmentBadge;        // Only if you want to show/hide badge
        
        [Header("Colors for Different Segments")]
        [SerializeField] private Color proPlayerColor = new Color(1f, 0.8f, 0.2f, 1f);      // Gold
        [SerializeField] private Color contentCreatorColor = new Color(0.8f, 0.2f, 0.8f, 1f); // Purple
        [SerializeField] private Color competitiveColor = new Color(1f, 0.4f, 0.1f, 1f);     // Orange
        [SerializeField] private Color casualColor = new Color(0.6f, 0.8f, 1f, 1f);          // Light Blue
        [SerializeField] private Color vipColor = new Color(0.9f, 0.1f, 0.9f, 1f);            // Bright Purple for VIP
        
        [Header("Colors for Sentiment")]
        [SerializeField] private Color positiveColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // Green
        [SerializeField] private Color negativeColor = new Color(0.8f, 0.2f, 0.2f, 1f);  // Red
        [SerializeField] private Color neutralColor = new Color(0.6f, 0.6f, 0.6f, 1f);   // Gray
        
        private Community.CommunityFeedback currentFeedback;
        
        public void SetupWithProPlayerSupport(Community.CommunityFeedback feedback)
        {
            currentFeedback = feedback;
            
            // ONLY update text content and colors - nothing else
            UpdateTextContent();
            UpdateTextColors();
            UpdateBadgeIfExists();
            UpdateBorderIfExists();
            
            Debug.Log($"✅ Updated text for: {feedback.author} ({feedback.communitySegment}) - Badge: {ShouldShowBadge()}");
        }
        
        private void UpdateTextContent()
        {
            // Update author name
            if (authorText != null)
            {
                authorText.text = currentFeedback.author;
            }
            
            // Update content text
            if (contentText != null)
            {
                contentText.text = currentFeedback.content;
            }
            
            // Update timestamp
            if (timestampText != null)
            {
                timestampText.text = currentFeedback.GetTimeAgo();
            }
            
            // Update upvotes
            if (upvotesText != null)
            {
                if (currentFeedback.upvotes > 0)
                {
                    upvotesText.text = $"▲ {currentFeedback.upvotes}"; // Use triangle instead of emoji
                }
                else
                {
                    upvotesText.text = "";
                }
            }
            
            // Update replies
            if (repliesText != null)
            {
                if (currentFeedback.replies > 0)
                {
                    repliesText.text = $"● {currentFeedback.replies}"; // Use bullet instead of emoji
                }
                else
                {
                    repliesText.text = "";
                }
            }
            
            // Update badge text
            if (segmentBadgeText != null)
            {
                segmentBadgeText.text = GetBadgeText();
            }
        }
        
        private void UpdateTextColors()
        {
            // Change author text color based on segment
            if (authorText != null)
            {
                authorText.color = GetSegmentColor();
            }
            
            // Keep other text colors as they are - don't change them
            // Content text, timestamp, upvotes, replies stay their existing colors
        }
        
        private void UpdateBadgeIfExists()
        {
            // Show badge for everyone who should have one (using the improved logic)
            if (segmentBadge != null)
            {
                bool shouldShow = ShouldShowBadge();
                segmentBadge.SetActive(shouldShow);
                
                Debug.Log($"🏷️ Badge for {currentFeedback.author} ({currentFeedback.communitySegment}): {(shouldShow ? "SHOW" : "HIDE")} - Badge Text: '{GetBadgeText()}'");
            }
            
            // Change badge background color if you have one
            if (segmentBadgeBackground != null)
            {
                segmentBadgeBackground.color = GetSegmentColor();
            }
            
            // Badge text color stays white
            if (segmentBadgeText != null)
            {
                segmentBadgeText.color = Color.white;
            }
        }
        
        private void UpdateBorderIfExists()
        {
            // Only if you have a left border assigned
            if (leftBorderImage != null)
            {
                leftBorderImage.color = GetSentimentColor();
            }
        }
        
        /// <summary>
        /// Determines if this user should show a badge
        /// </summary>
        private bool ShouldShowBadge()
        {
            return currentFeedback.communitySegment switch
            {
                "Pro Players" => true,
                "Content Creators" => true,
                "Competitive" => true,
                "Casual Players" => true,   // Casual players DO get badges
                null => false,              // No segment = no badge
                "" => false,                // Empty segment = no badge
                _ => true                   // Any other segment gets VIP badge
            };
        }
        
        private string GetBadgeText()
        {
            return currentFeedback.communitySegment switch
            {
                "Pro Players" => "Pro Player",
                "Content Creators" => "Content Creator", 
                "Competitive" => "Competitive",
                "Casual Players" => "Casual", // Casual players get "Casual" badge
                null => "",
                "" => "",
                _ => "VIP" // Any unrecognized segment becomes VIP
            };
        }
        
        private Color GetSegmentColor()
        {
            return currentFeedback.communitySegment switch
            {
                "Pro Players" => proPlayerColor,
                "Content Creators" => contentCreatorColor,
                "Competitive" => competitiveColor,
                "Casual Players" => casualColor,
                _ => vipColor // VIP gets bright purple
            };
        }
        
        private Color GetSentimentColor()
        {
            if (currentFeedback.sentiment > 0.2f)
                return positiveColor;
            else if (currentFeedback.sentiment < -0.2f)
                return negativeColor;
            else
                return neutralColor;
        }
        
        public void RefreshTimestamp()
        {
            if (timestampText != null && currentFeedback != null)
            {
                timestampText.text = currentFeedback.GetTimeAgo();
            }
        }
        
        // Legacy compatibility
        public void Setup(Community.CommunityFeedback feedback)
        {
            SetupWithProPlayerSupport(feedback);
        }
        
        // Test methods
        [ContextMenu("🧪 Test Pro Player")]
        public void TestProPlayer()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "TSM_Legend",
                content = "Finally! Warrior feels balanced now 💪 These health changes improve competitive diversity",
                sentiment = 0.8f,
                communitySegment = "Pro Players",
                upvotes = 45,
                replies = 12,
                timestamp = System.DateTime.Now
            };
            SetupWithProPlayerSupport(testFeedback);
        }
        
        [ContextMenu("🧪 Test Content Creator")]
        public void TestContentCreator()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "GameGuruYT",
                content = "Support utility nerf feels too harsh 😔 Making a reaction video tonight!",
                sentiment = -0.6f,
                communitySegment = "Content Creators",
                upvotes = 87,
                replies = 34,
                timestamp = System.DateTime.Now.AddMinutes(-5)
            };
            SetupWithProPlayerSupport(testFeedback);
        }
        
        [ContextMenu("🧪 Test Casual (With Badge)")]
        public void TestCasual()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "CasualGamer42",
                content = "Love the Warrior changes! Feels more fun to play now 😊",
                sentiment = 0.7f,
                communitySegment = "Casual Players",
                upvotes = 19,
                replies = 3,
                timestamp = System.DateTime.Now.AddMinutes(-9)
            };
            SetupWithProPlayerSupport(testFeedback);
        }
        
        [ContextMenu("🧪 Test VIP (Custom Segment)")]
        public void TestVIP()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "MysteriousBalanceGuru",
                content = "These changes align perfectly with the meta predictions 🔮✨",
                sentiment = 0.9f,
                communitySegment = "Beta Tester", // Custom segment -> becomes VIP
                upvotes = 156,
                replies = 42,
                timestamp = System.DateTime.Now.AddMinutes(-2)
            };
            SetupWithProPlayerSupport(testFeedback);
        }
        
        [ContextMenu("🧪 Test Unknown Segment (Should show VIP)")]
        public void TestUnknownSegment()
        {
            var testFeedback = new Community.CommunityFeedback
            {
                author = "SpecialUser123",
                content = "Interesting balance philosophy behind these changes 🤔",
                sentiment = 0.4f,
                communitySegment = "Game Designer", // Unknown segment -> becomes VIP
                upvotes = 78,
                replies = 23,
                timestamp = System.DateTime.Now.AddMinutes(-7)
            };
            SetupWithProPlayerSupport(testFeedback);
        }
        
        [ContextMenu("🔍 Debug: Check Current Badge State")]
        public void DebugCheckBadgeState()
        {
            if (currentFeedback == null)
            {
                Debug.Log("❌ No current feedback set");
                return;
            }
            
            Debug.Log("=== 🔍 BADGE DEBUG INFO ===");
            Debug.Log($"Author: {currentFeedback.author}");
            Debug.Log($"Segment: '{currentFeedback.communitySegment}'");
            Debug.Log($"Should Show Badge: {ShouldShowBadge()}");
            Debug.Log($"Badge Text: '{GetBadgeText()}'");
            Debug.Log($"Badge Color: {GetSegmentColor()}");
            Debug.Log($"Badge GameObject Active: {segmentBadge?.activeSelf ?? false}");
            
            if (segmentBadgeText != null)
                Debug.Log($"Badge Text Component Text: '{segmentBadgeText.text}'");
            else
                Debug.Log("❌ Badge Text Component: NULL");
        }
    }
}