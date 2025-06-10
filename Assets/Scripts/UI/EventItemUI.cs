using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using System.Collections;
using EventType = MetaBalance.Events.EventType;
using EventSeverity = MetaBalance.Events.EventSeverity;

namespace MetaBalance.Events
{
    /// <summary>
    /// Enhanced EventUIItem that respects your existing structure and adds phase awareness
    /// Only fully interactive during Event Phase, shows as restricted in other phases
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
        
        // State tracking
        private EventData currentEvent;
        private System.Action<EventData> onEventResponded;
        private System.Action<EventData> onEventDismissed;
        private List<Button> spawnedButtons = new List<Button>();
        private List<GameObject> spawnedImpactItems = new List<GameObject>();
        private bool isActive = true;
        private bool isHistorical = false;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Events only exist during Event Phase, so no phase monitoring needed
        }
        
        #endregion
        
        #region Setup and Display
        
        /// <summary>
        /// Setup event with data and callbacks - Your existing method signature
        /// </summary>
        public void SetupEvent(EventData eventData, System.Action<EventData> respondedCallback, System.Action<EventData> dismissedCallback)
        {
            currentEvent = eventData;
            onEventResponded = respondedCallback;
            onEventDismissed = dismissedCallback;
            isActive = true;
            
            if (currentEvent == null)
            {
                Debug.LogError("❌ EventData is null in SetupEvent!");
                return;
            }
            
            Debug.Log($"🎯 Setting up EventUIItem: {currentEvent.title}");
            Debug.Log($"📋 Event has {currentEvent.responseOptions?.Count ?? 0} response options");
            
            DisplayEvent();
            CreateDynamicButtons();
            CreateImpactList();
            UpdateVisualTheme();
            
            OnEventDisplayed?.Invoke(eventData);
            
            Debug.Log($"✅ EventUIItem setup complete: {eventData.title}");
        }
        
        /// <summary>
        /// Alternative setup method for compatibility - Your existing method
        /// </summary>
        public void DisplayEvent(EventData eventData)
        {
            SetupEvent(eventData, null, null);
        }
        
        /// <summary>
        /// Your existing DisplayEvent method
        /// </summary>
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
        
        #endregion
        

        
        #region Your Existing Helper Methods (Enhanced)
        
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
                EventType.Community => "COMMUNITY IMPACT",
                EventType.Technical => "TECHNICAL IMPACT",
                EventType.Competitive => "TOURNAMENT IMPACT",
                _ => "EXPECTED IMPACT"
            };
        }
        
        private void UpdateText(TextMeshProUGUI textComponent, string content)
        {
            if (textComponent != null)
                textComponent.text = content;
        }
        
        private void UpdateTimeDisplay()
        {
            if (timerText != null && currentEvent != null)
            {
                // Convert seconds to turns (assuming 30 seconds = 1 turn)
                int turnsRemaining = Mathf.CeilToInt(currentEvent.timeRemaining / 30f);
                
                if (turnsRemaining <= 0)
                {
                    timerText.text = "EXPIRED";
                    timerText.color = Color.red;
                }
                else if (turnsRemaining == 1)
                {
                    timerText.text = "1 turn remaining";
                    timerText.color = Color.red;
                }
                else
                {
                    timerText.text = $"{turnsRemaining} turns remaining";
                    
                    // Color code based on urgency
                    if (turnsRemaining <= 2)
                        timerText.color = Color.yellow;
                    else
                        timerText.color = Color.white;
                }
            }
        }
        
        #endregion
        
        #region Button Management
        
        /// <summary>
        /// Your existing CreateDynamicButtons method - Enhanced to ensure buttons are created
        /// </summary>
        private void CreateDynamicButtons()
        {
            Debug.Log($"🔘 CreateDynamicButtons called");
            
            if (currentEvent == null)
            {
                Debug.LogError("❌ Cannot create buttons: currentEvent is null");
                return;
            }
            
            if (buttonContainer == null)
            {
                Debug.LogError("❌ Cannot create buttons: buttonContainer is null. Assign it in the inspector!");
                return;
            }
            
            if (responseButtonPrefab == null)
            {
                Debug.LogError("❌ Cannot create buttons: responseButtonPrefab is null. Assign it in the inspector!");
                return;
            }
            
            // Clear existing buttons first
            ClearSpawnedButtons();
            
            // Check if event has response options
            if (currentEvent.responseOptions == null || currentEvent.responseOptions.Count == 0)
            {
                Debug.LogWarning($"⚠️ Event '{currentEvent.title}' has no response options. Creating default button.");
                CreateDefaultObserveButton();
                return;
            }
            
            Debug.Log($"📋 Creating {currentEvent.responseOptions.Count} response buttons");
            
            // Create buttons for each response option
            for (int i = 0; i < currentEvent.responseOptions.Count; i++)
            {
                var responseOption = currentEvent.responseOptions[i];
                Debug.Log($"  🔘 Creating button {i + 1}: '{responseOption.buttonText}'");
                CreateResponseButton(responseOption);
            }
            
            Debug.Log($"✅ Created {spawnedButtons.Count} buttons total");
        }
        
        private void CreateResponseButton(EventResponseOption response)
        {
            if (response == null)
            {
                Debug.LogError("❌ Cannot create button: response is null");
                return;
            }
            
            Debug.Log($"🔘 Creating button: '{response.buttonText}'");
            
            // Instantiate the button
            GameObject buttonObj = Instantiate(responseButtonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            if (button == null)
            {
                Debug.LogError("❌ Response button prefab doesn't have Button component!");
                Destroy(buttonObj);
                return;
            }
            
            // Setup button text and appearance
            SetupButtonVisuals(button, response);
            
            // Setup button functionality
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => HandleResponseSelected(response));
            
            // Track the button
            spawnedButtons.Add(button);
            
            Debug.Log($"✅ Button created successfully: '{response.buttonText}'");
        }
        
        private void SetupButtonVisuals(Button button, EventResponseOption response)
        {
            // Find text components in the button
            var buttonTexts = button.GetComponentsInChildren<TextMeshProUGUI>();
            var buttonTextsLegacy = button.GetComponentsInChildren<Text>();
            
            // Try TextMeshPro first, then fall back to legacy Text
            if (buttonTexts.Length > 0)
            {
                // Main button text (TextMeshPro)
                buttonTexts[0].text = response.buttonText;
                
                // Cost text if there's a second text component
                if (buttonTexts.Length > 1)
                {
                    string costText = "";
                    if (response.rpCost > 0) costText += $"RP: {response.rpCost} ";
                    if (response.cpCost > 0) costText += $"CP: {response.cpCost}";
                    buttonTexts[1].text = costText.Trim();
                }
            }
            else if (buttonTextsLegacy.Length > 0)
            {
                // Fallback to legacy Text component
                buttonTextsLegacy[0].text = response.buttonText;
                
                if (buttonTextsLegacy.Length > 1)
                {
                    string costText = "";
                    if (response.rpCost > 0) costText += $"RP: {response.rpCost} ";
                    if (response.cpCost > 0) costText += $"CP: {response.cpCost}";
                    buttonTextsLegacy[1].text = costText.Trim();
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Button prefab has no Text or TextMeshPro components! Button text won't display.");
            }
            
            // Set button color if specified
            if (response.buttonColor != Color.white && response.buttonColor != default(Color))
            {
                var buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = response.buttonColor;
                }
            }
        }
        
        private void CreateDefaultObserveButton()
        {
            Debug.Log("🔘 Creating default 'Observe' button");
            
            var defaultResponse = new EventResponseOption
            {
                buttonText = "Observe & Learn",
                description = "Monitor the situation without taking immediate action",
                responseType = EventResponseType.ObserveAndLearn,
                rpCost = 0,
                cpCost = 0,
                sentimentChange = 0f,
                successMessage = "Situation observed.",
                buttonColor = new Color(0.4f, 0.4f, 0.4f)
            };
            
            CreateResponseButton(defaultResponse);
        }
        
        private void ClearSpawnedButtons()
        {
            Debug.Log($"🧹 Clearing {spawnedButtons.Count} existing buttons");
            
            foreach (var button in spawnedButtons)
            {
                if (button != null && button.gameObject != null)
                    Destroy(button.gameObject);
            }
            spawnedButtons.Clear();
        }
        
        #endregion
        
        #region Impact List Management
        
        /// <summary>
        /// Your existing CreateImpactList method
        /// </summary>
        private void CreateImpactList()
        {
            ClearSpawnedImpactItems();
            
            if (currentEvent?.expectedImpacts == null || impactListContainer == null || impactItemPrefab == null)
                return;
            
            foreach (var impact in currentEvent.expectedImpacts)
            {
                CreateImpactItem(impact);
            }
        }
        
        private void CreateImpactItem(string impactText)
        {
            GameObject impactObj = Instantiate(impactItemPrefab, impactListContainer);
            TextMeshProUGUI impactTextComponent = impactObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (impactTextComponent != null)
            {
                impactTextComponent.text = $"• {impactText}";
            }
            
            spawnedImpactItems.Add(impactObj);
        }
        
        private void ClearSpawnedImpactItems()
        {
            foreach (var item in spawnedImpactItems)
            {
                if (item != null)
                    Destroy(item);
            }
            spawnedImpactItems.Clear();
        }
        
        #endregion
        
        #region Visual Theme Management
        
        /// <summary>
        /// Your existing UpdateVisualTheme method
        /// </summary>
        private void UpdateVisualTheme()
        {
            if (currentEvent == null) return;
            
            Color eventColor = GetEventTypeColor(currentEvent.eventType);
            Color priorityColor = GetPriorityColor(currentEvent.severity);
            
            if (eventTypeBackground != null)
                eventTypeBackground.color = eventColor;
                
            if (priorityIndicator != null)
                priorityIndicator.color = priorityColor;
        }
        
        private Color GetEventTypeColor(EventType eventType)
        {
            return eventType switch
            {
                EventType.Crisis => crisisColor,
                EventType.Opportunity => opportunityColor,
                EventType.Community => communityColor,
                EventType.Competitive => tournamentColor,
                _ => normalColor
            };
        }
        
        private Color GetPriorityColor(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Critical => urgentColor,
                EventSeverity.High => normalColor,
                EventSeverity.Medium => normalColor,
                EventSeverity.Low => lowPriorityColor,
                _ => normalColor
            };
        }
        
        #endregion
        
        #region Event Response Handling
        
        private void HandleResponseSelected(EventResponseOption response)
        {
            Debug.Log($"🎯 Response selected: {response.buttonText} for event: {currentEvent.title}");
            
            OnEventResponseSelected?.Invoke(currentEvent, response.responseType);
            onEventResponded?.Invoke(currentEvent);
            
            MarkAsHandled();
        }
        
        private void MarkAsHandled()
        {
            isActive = false;
            
            // Disable all buttons
            foreach (var button in spawnedButtons)
            {
                if (button != null)
                    button.interactable = false;
            }
            
            // Visual feedback for handled state
            if (eventTypeBackground != null)
            {
                var currentColor = eventTypeBackground.color;
                eventTypeBackground.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0.5f);
            }
        }
        
        #endregion
        
        #region State Management
        
        public void MarkAsHistorical()
        {
            isHistorical = true;
            
            // Apply historical styling
            ApplyHistoricalStyling();
            
            // Disable all interactions
            foreach (var button in spawnedButtons)
            {
                if (button != null)
                    button.interactable = false;
            }
            
            Debug.Log($"📚 Event marked as historical: {currentEvent?.title}");
        }
        
        private void ApplyHistoricalStyling()
        {
            Color historicalColor = new Color(0.6f, 0.6f, 0.6f);
            
            if (eventTitleText != null) eventTitleText.color = historicalColor;
            if (eventDescriptionText != null) eventDescriptionText.color = historicalColor;
            if (eventTypeLabel != null) eventTypeLabel.color = historicalColor;
            if (priorityLabel != null) priorityLabel.color = historicalColor;
        }
        
        #endregion
        

        
        #region Public Interface
        
        public bool IsHistorical()
        {
            return isHistorical;
        }
        
        public EventData GetCurrentEvent()
        {
            return currentEvent;
        }
        
        #endregion
        
        #region Testing Methods
        
        [ContextMenu("🧪 Mark as Historical")]
        public void TestMarkAsHistorical()
        {
            MarkAsHistorical();
        }
        
        [ContextMenu("🔘 Debug Button Setup")]
        public void DebugButtonSetup()
        {
            Debug.Log("=== 🔘 BUTTON DEBUG INFO ===");
            Debug.Log($"Current Event: {currentEvent?.title ?? "NULL"}");
            Debug.Log($"Button Container: {buttonContainer?.name ?? "NULL"}");
            Debug.Log($"Response Button Prefab: {responseButtonPrefab?.name ?? "NULL"}");
            Debug.Log($"Spawned Buttons Count: {spawnedButtons.Count}");
            
            if (currentEvent != null)
            {
                Debug.Log($"Event Response Options: {currentEvent.responseOptions?.Count ?? 0}");
                if (currentEvent.responseOptions != null)
                {
                    for (int i = 0; i < currentEvent.responseOptions.Count; i++)
                    {
                        var option = currentEvent.responseOptions[i];
                        Debug.Log($"  Option {i}: '{option.buttonText}' (RP: {option.rpCost}, CP: {option.cpCost})");
                    }
                }
            }
            
            // Check button container children
            if (buttonContainer != null)
            {
                Debug.Log($"Button Container Children: {buttonContainer.childCount}");
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    var child = buttonContainer.GetChild(i);
                    var button = child.GetComponent<Button>();
                    Debug.Log($"  Child {i}: {child.name} - Has Button: {button != null}");
                }
            }
        }
        
        [ContextMenu("🔘 Force Recreate Buttons")]
        public void TestRecreateButtons()
        {
            Debug.Log("🔘 Force recreating buttons...");
            CreateDynamicButtons();
        }
        
        [ContextMenu("🔘 Test Button Creation")]
        public void TestButtonCreation()
        {
            if (currentEvent == null)
            {
                Debug.LogError("❌ No current event to test buttons with");
                return;
            }
            
            Debug.Log("🔘 Testing button creation...");
            
            // Clear existing
            ClearSpawnedButtons();
            
            // Test with current event
            CreateDynamicButtons();
            
            Debug.Log($"✅ Button creation test complete. Created {spawnedButtons.Count} buttons.");
        }
        
        #endregion
    }
}