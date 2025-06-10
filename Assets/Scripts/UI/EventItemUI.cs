using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace MetaBalance.Events
{
    /// <summary>
    /// Beautiful Event UI Item that matches your stunning multi-card design!
    /// Supports dynamic button creation based on event response options
    /// </summary>
    public class EventUIItem : MonoBehaviour
    {
        [Header("Main Event Display")]
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI expectedImpactText;
        [SerializeField] private TextMeshProUGUI responseWindowText;
        
        [Header("Event Type & Priority")]
        [SerializeField] private TextMeshProUGUI eventTypeLabel; // "CRISIS EVENT", "OPPORTUNITY", etc.
        [SerializeField] private TextMeshProUGUI priorityLabel;   // "URGENT", "NORMAL", "LOW PRIORITY"
        [SerializeField] private Image eventTypeBackground;     // Background color for event type
        [SerializeField] private Image priorityIndicator;       // Priority indicator color
        
        [Header("Expected Impact/Benefits Section")]
        [SerializeField] private TextMeshProUGUI impactSectionTitle; // "EXPECTED IMPACT", "POTENTIAL BENEFITS", etc.
        [SerializeField] private Transform impactListContainer; // Container for impact bullet points
        [SerializeField] private GameObject impactItemPrefab;  // Prefab for each impact item
        
        [Header("Response Buttons")]
        [SerializeField] private Transform buttonContainer; // Container where buttons will be spawned
        [SerializeField] private GameObject responseButtonPrefab; // Prefab for response buttons
        
        [Header("Timer Display")]
        [SerializeField] private TextMeshProUGUI timerText; // "2 turns remaining", "4 turns remaining"
        [SerializeField] private Image timerIcon; // Clock icon
        
        [Header("Visual Styling")]
        [SerializeField] private Color crisisColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color opportunityColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private Color communityColor = new Color(0.3f, 0.8f, 0.6f);
        [SerializeField] private Color tournamentColor = new Color(0.9f, 0.7f, 0.2f);
        [SerializeField] private Color urgentColor = new Color(0.9f, 0.1f, 0.1f);
        [SerializeField] private Color normalColor = new Color(0.4f, 0.6f, 0.8f);
        [SerializeField] private Color lowPriorityColor = new Color(0.3f, 0.7f, 0.3f);
        
        [Header("Events")]
        public UnityEvent<EventData, EventResponseType> OnEventResponseSelected;
        public UnityEvent<EventData> OnEventDisplayed;
        public UnityEvent<EventData> OnEventExpired;
        
        private EventData currentEvent;
        private System.Action<EventData> onEventResponded;
        private System.Action<EventData> onEventDismissed;
        private List<Button> spawnedButtons = new List<Button>();
        private List<GameObject> spawnedImpactItems = new List<GameObject>();
        private bool isActive = true;
        
        #region Setup and Display
        
        /// <summary>
        /// Setup event with data and callbacks
        /// </summary>
        public void SetupEvent(EventData eventData, System.Action<EventData> respondedCallback, System.Action<EventData> dismissedCallback)
        {
            currentEvent = eventData;
            onEventResponded = respondedCallback;
            onEventDismissed = dismissedCallback;
            isActive = true;
            
            DisplayEvent();
            CreateDynamicButtons();
            CreateImpactList();
            UpdateVisualTheme();
            
            OnEventDisplayed?.Invoke(eventData);
        }
        
        /// <summary>
        /// Alternative setup method for compatibility
        /// </summary>
        public void DisplayEvent(EventData eventData)
        {
            SetupEvent(eventData, null, null);
        }
        
        private void DisplayEvent()
        {
            if (currentEvent == null) return;
            
            // Main text content
            UpdateText(eventTitleText, currentEvent.title);
            UpdateText(eventDescriptionText, currentEvent.description);
            
            // Event type and priority labels
            UpdateText(eventTypeLabel, GetEventTypeText(currentEvent.eventType));
            UpdateText(priorityLabel, GetPriorityText(currentEvent.severity));
            
            // Impact section title
            UpdateText(impactSectionTitle, GetImpactSectionTitle(currentEvent.eventType));
            
            UpdateTimeDisplay();
        }
        
        private string GetEventTypeText(EventType eventType)
        {
            return eventType switch
            {
                EventType.Crisis => "CRISIS EVENT",
                EventType.Opportunity => "OPPORTUNITY",
                EventType.Community => "COMMUNITY EVENT",
                EventType.Technical => "TECHNICAL EVENT",
                EventType.Competitive => "TOURNAMENT EVENT",
                EventType.Special => "SPECIAL EVENT",
                _ => "EVENT"
            };
        }
        
        private string GetPriorityText(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Critical => "URGENT",
                EventSeverity.High => "NORMAL",
                EventSeverity.Medium => "NORMAL", 
                EventSeverity.Low => "LOW PRIORITY",
                _ => "NORMAL"
            };
        }
        
        private string GetImpactSectionTitle(EventType eventType)
        {
            return eventType switch
            {
                EventType.Crisis => "EXPECTED IMPACT",
                EventType.Opportunity => "POTENTIAL BENEFITS",
                EventType.Community => "POTENTIAL OUTCOMES",
                EventType.Competitive => "META ANALYSIS",
                _ => "EXPECTED IMPACT"
            };
        }
        
        private void UpdateText(TextMeshProUGUI textComponent, string text)
        {
            if (textComponent != null)
                textComponent.text = text;
        }
        
        private void UpdateTimeDisplay()
        {
            if (timerText != null && currentEvent != null)
            {
                int turns = Mathf.CeilToInt(currentEvent.timeRemaining / 30f); // Convert seconds to turns
                timerText.text = $"{turns} turns remaining";
                
                // Color code based on urgency
                if (turns <= 1)
                    timerText.color = Color.red;
                else if (turns <= 2)
                    timerText.color = Color.yellow;
                else
                    timerText.color = Color.white;
            }
        }
        
        #endregion
        
        #region Dynamic Button Creation
        
        private void CreateDynamicButtons()
        {
            if (currentEvent == null || buttonContainer == null || responseButtonPrefab == null) return;
            
            // Clear existing buttons
            ClearSpawnedButtons();
            
            // Create buttons based on event's response options
            foreach (var responseOption in currentEvent.responseOptions)
            {
                CreateResponseButton(responseOption);
            }
            
            // Add default "Observe & Learn" option if no responses exist
            if (currentEvent.responseOptions.Count == 0)
            {
                CreateDefaultObserveButton();
            }
        }
        
        private void CreateResponseButton(EventResponseOption responseOption)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null) return;
            
            // Setup button appearance
            SetupButtonVisuals(button, responseOption);
            
            // Setup button functionality
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleResponse(responseOption.responseType));
            
            // Check affordability
            UpdateButtonAffordability(button, responseOption);
            
            spawnedButtons.Add(button);
        }
        
        private void SetupButtonVisuals(Button button, EventResponseOption responseOption)
        {
            // Find text components in button
            var buttonTexts = button.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (buttonTexts.Length >= 1)
            {
                // Main button text
                buttonTexts[0].text = responseOption.buttonText;
            }
            
            if (buttonTexts.Length >= 2)
            {
                // Cost text
                buttonTexts[1].text = responseOption.GetCostText();
            }
            
            // Set button color
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = GetButtonColor(responseOption.responseType);
            }
        }
        
        private Color GetButtonColor(EventResponseType responseType)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => new Color(0.7f, 0.3f, 0.9f), // Purple for hotfix
                EventResponseType.CommunityManagement => new Color(0.2f, 0.6f, 0.9f), // Blue for promotion
                EventResponseType.CustomResponse => new Color(0.9f, 0.6f, 0.2f), // Orange for balance
                EventResponseType.ObserveAndLearn => new Color(0.4f, 0.4f, 0.4f), // Gray for observe
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
        
        private void CreateDefaultObserveButton()
        {
            var defaultResponse = new EventResponseOption
            {
                buttonText = "Observe & Learn",
                description = "Monitor the situation without taking action",
                responseType = EventResponseType.ObserveAndLearn,
                rpCost = 0,
                cpCost = 0,
                sentimentChange = 0f
            };
            
            CreateResponseButton(defaultResponse);
        }
        
        private void ClearSpawnedButtons()
        {
            foreach (var button in spawnedButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    Destroy(button.gameObject);
                }
            }
            spawnedButtons.Clear();
        }
        
        #endregion
        
        #region Impact List Creation
        
        private void CreateImpactList()
        {
            if (currentEvent == null || impactListContainer == null) return;
            
            // Clear existing impact items
            ClearSpawnedImpactItems();
            
            // Create impact items
            foreach (var impact in currentEvent.expectedImpacts)
            {
                CreateImpactItem(impact);
            }
        }
        
        private void CreateImpactItem(string impactText)
        {
            GameObject impactObj;
            
            if (impactItemPrefab != null)
            {
                impactObj = Instantiate(impactItemPrefab, impactListContainer);
            }
            else
            {
                // Create simple text if no prefab
                impactObj = new GameObject("ImpactItem");
                impactObj.transform.SetParent(impactListContainer);
                var text = impactObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 14;
                text.color = Color.white;
            }
            
            var textComponent = impactObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = $"• {impactText}";
            }
            
            spawnedImpactItems.Add(impactObj);
        }
        
        private void ClearSpawnedImpactItems()
        {
            foreach (var item in spawnedImpactItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            spawnedImpactItems.Clear();
        }
        
        #endregion
        
        #region Response Handling
        
        private void HandleResponse(EventResponseType responseType)
        {
            if (currentEvent == null || !isActive) return;
            
            // Find the specific response option
            var responseOption = currentEvent.responseOptions.Find(r => r.responseType == responseType);
            if (responseOption == null)
            {
                // Default response for observe
                responseOption = new EventResponseOption
                {
                    responseType = responseType,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = 0f
                };
            }
            
            // Check if player can afford the response
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null && !resourceManager.CanSpend(responseOption.rpCost, responseOption.cpCost))
            {
                Debug.Log($"Cannot afford response: {responseOption.rpCost} RP, {responseOption.cpCost} CP");
                ShowAffordabilityFeedback();
                return;
            }
            
            // Spend resources
            if (resourceManager != null && (responseOption.rpCost > 0 || responseOption.cpCost > 0))
            {
                resourceManager.SpendResources(responseOption.rpCost, responseOption.cpCost);
            }
            
            // Mark as resolved
            isActive = false;
            currentEvent.isResolved = true;
            
            // Invoke events
            OnEventResponseSelected?.Invoke(currentEvent, responseType);
            onEventResponded?.Invoke(currentEvent);
            
            // Show feedback
            ShowResponseFeedback(responseOption.successMessage);
            
            // Auto-destroy after feedback
            Invoke(nameof(DestroySelf), 2f);
        }
        
        private void ShowResponseFeedback(string responseText)
        {
            if (eventDescriptionText != null)
            {
                eventDescriptionText.text = $"✅ {responseText}";
                eventDescriptionText.color = Color.green;
            }
            
            SetButtonsInteractable(false);
        }
        
        private void ShowAffordabilityFeedback()
        {
            if (eventDescriptionText != null)
            {
                eventDescriptionText.text = "❌ Insufficient resources for this response!";
                eventDescriptionText.color = Color.red;
            }
        }
        
        private void UpdateButtonAffordability(Button button, EventResponseOption responseOption)
        {
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null) return;
            
            bool canAfford = resourceManager.CanSpend(responseOption.rpCost, responseOption.cpCost);
            button.interactable = canAfford;
            
            // Update cost text color
            var costTexts = button.GetComponentsInChildren<TextMeshProUGUI>();
            if (costTexts.Length >= 2)
            {
                costTexts[1].color = canAfford ? Color.white : Color.red;
            }
        }
        
        private void SetButtonsInteractable(bool interactable)
        {
            foreach (var button in spawnedButtons)
            {
                if (button != null)
                    button.interactable = interactable;
            }
        }
        
        #endregion
        
        #region Visual Updates
        
        private void UpdateVisualTheme()
        {
            if (currentEvent == null) return;
            
            Color themeColor = GetThemeColor();
            Color priorityColor = GetPriorityColor();
            
            // Update background color to match event type
            if (eventTypeBackground != null)
                eventTypeBackground.color = themeColor;
            
            // Update priority indicator
            if (priorityIndicator != null)
                priorityIndicator.color = priorityColor;
            
            // Update event type label color
            if (eventTypeLabel != null)
                eventTypeLabel.color = Color.white;
                
            // Update priority label color
            if (priorityLabel != null)
                priorityLabel.color = Color.white;
        }
        
        private Color GetThemeColor()
        {
            if (currentEvent == null) return Color.white;
            
            return currentEvent.eventType switch
            {
                EventType.Crisis => crisisColor,
                EventType.Opportunity => opportunityColor,
                EventType.Community => communityColor,
                EventType.Competitive => tournamentColor,
                _ => normalColor
            };
        }
        
        private Color GetPriorityColor()
        {
            if (currentEvent == null) return normalColor;
            
            return currentEvent.severity switch
            {
                EventSeverity.Critical => urgentColor,
                EventSeverity.High => normalColor,
                EventSeverity.Medium => normalColor,
                EventSeverity.Low => lowPriorityColor,
                _ => normalColor
            };
        }
        
        #endregion
        
        #region Update and Lifecycle
        
        private void Update()
        {
            if (!isActive || currentEvent == null) return;
            
            // Update timer
            currentEvent.timeRemaining -= Time.deltaTime;
            UpdateTimeDisplay();
            UpdateAllButtonAffordability();
            
            // Check for expiration
            if (currentEvent.timeRemaining <= 0f)
            {
                HandleExpiration();
            }
        }
        
        private void UpdateAllButtonAffordability()
        {
            if (currentEvent == null) return;
            
            for (int i = 0; i < spawnedButtons.Count && i < currentEvent.responseOptions.Count; i++)
            {
                UpdateButtonAffordability(spawnedButtons[i], currentEvent.responseOptions[i]);
            }
        }
        
        private void HandleExpiration()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Invoke expiration events
            OnEventExpired?.Invoke(currentEvent);
            onEventDismissed?.Invoke(currentEvent);
            
            // Show expiration feedback
            if (eventDescriptionText != null)
            {
                eventDescriptionText.text = "⏰ Event expired! Response window closed.";
                eventDescriptionText.color = Color.red;
            }
            
            SetButtonsInteractable(false);
            
            // Auto-destroy
            Invoke(nameof(DestroySelf), 3f);
        }
        
        private void DestroySelf()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Refresh display for external updates
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentEvent != null)
            {
                DisplayEvent();
                UpdateVisualTheme();
                UpdateAllButtonAffordability();
            }
        }
        
        /// <summary>
        /// Check if this event item is still active
        /// </summary>
        public bool IsActive()
        {
            return isActive && currentEvent != null;
        }
        
        /// <summary>
        /// Get the current event data
        /// </summary>
        public EventData GetEventData()
        {
            return currentEvent;
        }
        
        /// <summary>
        /// Force dismiss this event
        /// </summary>
        public void DismissEvent()
        {
            if (isActive)
            {
                HandleExpiration();
            }
        }
        
        #endregion
    }
}