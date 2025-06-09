using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MetaBalance.UI
{
    /// <summary>
    /// Simplified Event UI Item with only essential components
    /// Remove any components you don't have in your UI prefab
    /// </summary>
    public class EventUIItem : MonoBehaviour
    {
        [Header("Essential UI References (Required)")]
        [SerializeField] private TextMeshProUGUI eventTitleText;        // REQUIRED: Event title
        [SerializeField] private TextMeshProUGUI eventDescriptionText;  // REQUIRED: Event description
        [SerializeField] private TextMeshProUGUI severityEmojiText;      // REQUIRED: Emoji for severity
        [SerializeField] private Button respondButton;                  // REQUIRED: Main action button
        [SerializeField] private TextMeshProUGUI timeRemainingText;     // REQUIRED: Shows turns left
        
        [Header("Optional UI References (Remove if you don't have them)")]
        [SerializeField] private TextMeshProUGUI impactText;            // OPTIONAL: Shows impact level
        [SerializeField] private Button dismissButton;                 // OPTIONAL: Dismiss/ignore button
        [SerializeField] private Image backgroundImage;                // OPTIONAL: Background color
        [SerializeField] private Image leftBorderImage;                // OPTIONAL: Left border color
        
        [Header("Colors")]
        [SerializeField] private Color crisisColor = Color.red;
        [SerializeField] private Color opportunityColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color infoColor = Color.blue;
        
        private EventData currentEvent;
        private System.Action<EventData> onRespondCallback;
        private System.Action<EventData> onDismissCallback;
        
        /// <summary>
        /// Setup the event with minimal required components
        /// </summary>
        public void SetupEvent(EventData eventData, System.Action<EventData> respondCallback = null, System.Action<EventData> dismissCallback = null)
        {
            currentEvent = eventData;
            onRespondCallback = respondCallback;
            onDismissCallback = dismissCallback;
            
            UpdateEventDisplay();
            SetupButtons();
        }
        
        private void UpdateEventDisplay()
        {
            if (currentEvent == null) return;
            
            // ESSENTIAL: Update title (REQUIRED)
            if (eventTitleText != null)
                eventTitleText.text = currentEvent.Title;
            else
                Debug.LogWarning("⚠️ eventTitleText is missing! Assign it in the inspector.");
            
            // ESSENTIAL: Update description (REQUIRED)
            if (eventDescriptionText != null)
                eventDescriptionText.text = currentEvent.Description;
            else
                Debug.LogWarning("⚠️ eventDescriptionText is missing! Assign it in the inspector.");
            
            // ESSENTIAL: Update severity emoji (REQUIRED)
            if (severityEmojiText != null)
            {
                var (emoji, color) = GetSeverityEmojiAndColor(currentEvent.Severity);
                severityEmojiText.text = emoji;
                severityEmojiText.color = color;
                severityEmojiText.fontSize = 24f;
            }
            else
            {
                Debug.LogWarning("⚠️ severityEmojiText is missing! Assign it in the inspector.");
            }
            
            // ESSENTIAL: Update time remaining in turns (REQUIRED)
            if (timeRemainingText != null)
            {
                if (currentEvent.TimeRemaining > 0)
                {
                    timeRemainingText.text = $"⏱️ {FormatTurnsRemaining(currentEvent.TimeRemaining)}";
                    timeRemainingText.color = GetTurnUrgencyColor(currentEvent.TimeRemaining);
                }
                else
                {
                    timeRemainingText.text = "⏰ This Turn";
                    timeRemainingText.color = crisisColor;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ timeRemainingText is missing! Assign it in the inspector for turn tracking.");
            }
            
            // OPTIONAL: Update impact (remove if you don't have this)
            if (impactText != null)
            {
                var (impactEmoji, impactLevel) = GetImpactEmojiAndLevel(currentEvent.Impact);
                impactText.text = $"{impactEmoji} {impactLevel} Impact";
            }
            
            // OPTIONAL: Update background colors (remove if you don't have these)
            UpdateOptionalVisuals();
        }
        
        private void UpdateOptionalVisuals()
        {
            // Only update if components exist
            if (backgroundImage != null)
            {
                Color bgColor = GetEventTypeColor();
                bgColor.a = 0.1f;
                backgroundImage.color = bgColor;
            }
            
            if (leftBorderImage != null)
            {
                leftBorderImage.color = GetEventTypeColor();
            }
        }
        
        private void SetupButtons()
        {
            // ESSENTIAL: Setup respond button (REQUIRED)
            if (respondButton != null)
            {
                respondButton.onClick.RemoveAllListeners();
                respondButton.onClick.AddListener(() => {
                    onRespondCallback?.Invoke(currentEvent);
                    Debug.Log($"✅ Responded to: {currentEvent.Title}");
                });
                
                // Update button text based on event type
                var buttonText = respondButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = GetRespondButtonText();
                }
            }
            else
            {
                Debug.LogError("❌ respondButton is missing! This is required for player interaction.");
            }
            
            // OPTIONAL: Setup dismiss button (remove if you don't have this)
            if (dismissButton != null)
            {
                bool canDismiss = currentEvent.Severity != EventSeverity.Critical;
                dismissButton.gameObject.SetActive(canDismiss);
                
                if (canDismiss)
                {
                    dismissButton.onClick.RemoveAllListeners();
                    dismissButton.onClick.AddListener(() => {
                        onDismissCallback?.Invoke(currentEvent);
                        Debug.Log($"🗑️ Dismissed: {currentEvent.Title}");
                    });
                }
            }
        }
        
        private (string emoji, Color color) GetSeverityEmojiAndColor(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Critical => ("🚨", crisisColor),      // Red alert
                EventSeverity.High => ("⚠️", warningColor),         // Warning
                EventSeverity.Medium => ("📋", infoColor),          // Info
                EventSeverity.Low => ("💡", opportunityColor),      // Idea
                EventSeverity.Opportunity => ("🎯", opportunityColor), // Target
                _ => ("ℹ️", infoColor)                              // Default
            };
        }
        
        private (string emoji, string level) GetImpactEmojiAndLevel(float impact)
        {
            return impact switch
            {
                >= 8f => ("💥", "Massive"),
                >= 6f => ("🔥", "Major"),
                >= 4f => ("⚡", "Significant"),
                >= 2f => ("📈", "Moderate"),
                _ => ("📊", "Minor")
            };
        }
        
        private Color GetTurnUrgencyColor(float turnsRemaining)
        {
            return turnsRemaining switch
            {
                <= 1f => crisisColor,      // Red for this turn only
                <= 2f => warningColor,     // Orange for 2 turns
                <= 3f => Color.yellow,     // Yellow for 3 turns
                _ => Color.white           // White for 4+ turns
            };
        }
        
        private Color GetEventTypeColor()
        {
            return currentEvent.EventType switch
            {
                EventType.Crisis => crisisColor,
                EventType.Opportunity => opportunityColor,
                EventType.Community => infoColor,
                EventType.Technical => warningColor,
                _ => infoColor
            };
        }
        
        private string FormatTurnsRemaining(float turnsRemaining)
        {
            int wholeTurns = Mathf.FloorToInt(turnsRemaining);
            
            return wholeTurns switch
            {
                0 => "This Turn",
                1 => "1 Turn Left",
                _ => $"{wholeTurns} Turns Left"
            };
        }
        
        private string GetRespondButtonText()
        {
            return currentEvent.EventType switch
            {
                EventType.Crisis => "🛠️ Fix Crisis",
                EventType.Opportunity => "💎 Seize Opportunity",
                EventType.Community => "💬 Address Issue",
                EventType.Technical => "🔧 Apply Fix",
                EventType.Competitive => "⚔️ Take Action",
                _ => "📝 Respond"
            };
        }
        
        /// <summary>
        /// Update turn display when a new turn/week begins
        /// Call this from PhaseManager when turns advance
        /// </summary>
        public void UpdateTurnDisplay(float newTurnsRemaining)
        {
            if (currentEvent != null)
            {
                currentEvent.TimeRemaining = newTurnsRemaining;
                
                if (timeRemainingText != null)
                {
                    if (newTurnsRemaining > 0)
                    {
                        timeRemainingText.text = $"⏱️ {FormatTurnsRemaining(newTurnsRemaining)}";
                        timeRemainingText.color = GetTurnUrgencyColor(newTurnsRemaining);
                    }
                    else
                    {
                        timeRemainingText.text = "⏰ Expired";
                        timeRemainingText.color = crisisColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// Decrease turn counter by 1 (call when turn advances)
        /// Returns true if event has expired
        /// </summary>
        public bool AdvanceTurn()
        {
            if (currentEvent != null)
            {
                currentEvent.TimeRemaining -= 1f;
                UpdateTurnDisplay(currentEvent.TimeRemaining);
                
                // Return true if event has expired
                return currentEvent.TimeRemaining <= 0f;
            }
            return false;
        }
        
        // Public getters for EventUIManager compatibility
        public EventData GetEventData() => currentEvent;
        public bool IsUrgent() => currentEvent?.Severity == EventSeverity.Critical || currentEvent?.TimeRemaining <= 1f;
        public bool HasExpired() => currentEvent?.TimeRemaining <= 0f;
        public int GetTurnsRemaining() => Mathf.FloorToInt(currentEvent?.TimeRemaining ?? 0f);
        
        // Test methods
        [ContextMenu("🧪 Test Critical Event")]
        public void TestCriticalEvent()
        {
            var testEvent = new EventData
            {
                Title = "Critical Bug Discovered",
                Description = "Players found game-breaking exploit. Immediate action required!",
                Severity = EventSeverity.Critical,
                EventType = EventType.Crisis,
                TimeRemaining = 2f,  // 2 turns to respond
                Impact = 9f
            };
            
            SetupEvent(testEvent, 
                (evt) => Debug.Log($"✅ Fixed: {evt.Title}"),
                (evt) => Debug.Log($"🗑️ Ignored: {evt.Title}"));
        }
        
        [ContextMenu("🧪 Test Opportunity")]
        public void TestOpportunity()
        {
            var testEvent = new EventData
            {
                Title = "Partnership Opportunity",
                Description = "Major streamer wants to showcase our game!",
                Severity = EventSeverity.Opportunity,
                EventType = EventType.Opportunity,
                TimeRemaining = 4f,  // 4 turns to decide
                Impact = 6f
            };
            
            SetupEvent(testEvent,
                (evt) => Debug.Log($"💎 Seized: {evt.Title}"),
                (evt) => Debug.Log($"😞 Missed: {evt.Title}"));
        }
    }
    
    // Supporting data structures (same as before)
    [System.Serializable]
    public class EventData
    {
        public string Title;
        public string Description;
        public EventSeverity Severity;
        public EventType EventType;
        public float TimeRemaining;
        public float Impact;
        public bool IsResolved;
        public System.DateTime CreatedTime;
        
        public EventData()
        {
            CreatedTime = System.DateTime.Now;
            IsResolved = false;
        }
    }
    
    public enum EventSeverity
    {
        Critical,    // 🚨 Must respond immediately
        High,        // ⚠️ Important but not urgent
        Medium,      // 📋 Standard priority
        Low,         // 💡 Minor issue
        Opportunity  // 🎯 Positive chance
    }
    
    public enum EventType
    {
        Crisis,      // Game-breaking issues
        Opportunity, // Positive chances
        Community,   // Player feedback issues
        Technical,   // Performance problems
        Competitive, // Esports/balance issues
        Seasonal     // Time-limited events
    }
}