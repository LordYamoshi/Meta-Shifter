using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Events
{
    /// <summary>
    /// Complete Event UI Item - Handles display and interaction for game events
    /// Integrates with your existing community feedback and resource systems
    /// Uses Strategy Pattern for different event response types
    /// </summary>
    public class EventUIItem : MonoBehaviour
    {
        [Header("Event Display")]
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private TextMeshProUGUI expectedImpactText;
        [SerializeField] private TextMeshProUGUI responseWindowText;
        [SerializeField] private Image eventTypeIcon;
        [SerializeField] private Image urgencyIndicator;
        
        [Header("Response Buttons")]
        [SerializeField] private Button emergencyFixButton;
        [SerializeField] private Button communityManagementButton;
        [SerializeField] private Button observeAndLearnButton;
        [SerializeField] private Button customResponseButton; // For flexible responses
        
        [Header("Button Text & Costs")]
        [SerializeField] private TextMeshProUGUI emergencyFixText;
        [SerializeField] private TextMeshProUGUI emergencyFixCost;
        [SerializeField] private TextMeshProUGUI communityManagementText;
        [SerializeField] private TextMeshProUGUI communityManagementCost;
        [SerializeField] private TextMeshProUGUI observeText;
        [SerializeField] private TextMeshProUGUI customResponseText;
        [SerializeField] private TextMeshProUGUI customResponseCost;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color crisisColor = Color.red;
        [SerializeField] private Color opportunityColor = Color.green;
        [SerializeField] private Color neutralColor = Color.blue;
        [SerializeField] private Color urgentColor = Color.yellow;
        
        [Header("Events")]
        public UnityEvent<EventData, EventResponseType> OnEventResponseSelected;
        public UnityEvent<EventData> OnEventDisplayed;
        public UnityEvent<EventData> OnEventExpired;
        
        private EventData currentEvent;
        private float timeRemaining;
        private bool isActive = false;
        
        private void Start()
        {
            SetupButtonListeners();
        }
        
        private void Update()
        {
            if (isActive && currentEvent != null)
            {
                UpdateTimer();
            }
        }
        
        #region Public Interface
        
        /// <summary>
        /// Display an event with full UI setup
        /// </summary>
        public void DisplayEvent(EventData eventData)
        {
            currentEvent = eventData;
            isActive = true;
            timeRemaining = eventData.responseTimeLimit;
            
            UpdateEventDisplay();
            UpdateResponseOptions();
            UpdateButtonAffordability();
            
            OnEventDisplayed.Invoke(eventData);
            
            Debug.Log($"🎭 Displaying event: {eventData.eventTitle}");
        }
        
        /// <summary>
        /// Refresh the event display (useful for resource changes)
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentEvent != null)
            {
                UpdateButtonAffordability();
                UpdateTimer();
            }
        }
        
        /// <summary>
        /// Close/dismiss the event
        /// </summary>
        public void CloseEvent()
        {
            isActive = false;
            currentEvent = null;
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Display Updates
        
        private void UpdateEventDisplay()
        {
            if (currentEvent == null) return;
            
            // Update main text content
            if (eventTitleText != null)
                eventTitleText.text = currentEvent.eventTitle;
                
            if (eventDescriptionText != null)
                eventDescriptionText.text = currentEvent.description;
                
            if (expectedImpactText != null)
                expectedImpactText.text = FormatExpectedImpact(currentEvent.expectedImpacts);
            
            // Update visual elements
            UpdateEventTypeVisuals();
            UpdateUrgencyVisuals();
        }
        
        private void UpdateResponseOptions()
        {
            if (currentEvent == null) return;
            
            // Emergency Fix Button
            if (emergencyFixButton != null && currentEvent.responses.ContainsKey(EventResponseType.EmergencyFix))
            {
                var response = currentEvent.responses[EventResponseType.EmergencyFix];
                emergencyFixButton.gameObject.SetActive(true);
                
                if (emergencyFixText != null)
                    emergencyFixText.text = response.responseText;
                if (emergencyFixCost != null)
                    emergencyFixCost.text = FormatCost(response.rpCost, response.cpCost);
            }
            else if (emergencyFixButton != null)
            {
                emergencyFixButton.gameObject.SetActive(false);
            }
            
            // Community Management Button
            if (communityManagementButton != null && currentEvent.responses.ContainsKey(EventResponseType.CommunityManagement))
            {
                var response = currentEvent.responses[EventResponseType.CommunityManagement];
                communityManagementButton.gameObject.SetActive(true);
                
                if (communityManagementText != null)
                    communityManagementText.text = response.responseText;
                if (communityManagementCost != null)
                    communityManagementCost.text = FormatCost(response.rpCost, response.cpCost);
            }
            else if (communityManagementButton != null)
            {
                communityManagementButton.gameObject.SetActive(false);
            }
            
            // Observe and Learn Button (usually free)
            if (observeAndLearnButton != null && currentEvent.responses.ContainsKey(EventResponseType.ObserveAndLearn))
            {
                var response = currentEvent.responses[EventResponseType.ObserveAndLearn];
                observeAndLearnButton.gameObject.SetActive(true);
                
                if (observeText != null)
                    observeText.text = response.responseText;
            }
            else if (observeAndLearnButton != null)
            {
                observeAndLearnButton.gameObject.SetActive(false);
            }
            
            // Custom Response Button (flexible for special events)
            UpdateCustomResponseButton();
        }
        
        private void UpdateCustomResponseButton()
        {
            if (customResponseButton == null) return;
            
            // Check for custom response types
            var customResponses = new List<EventResponseType>
            {
                EventResponseType.IgnoreEvent,
                EventResponseType.DelayResponse,
                EventResponseType.SeekAdvice,
                EventResponseType.CustomAction
            };
            
            EventResponse foundCustomResponse = null;
            EventResponseType foundType = EventResponseType.ObserveAndLearn;
            
            foreach (var responseType in customResponses)
            {
                if (currentEvent.responses.ContainsKey(responseType))
                {
                    foundCustomResponse = currentEvent.responses[responseType];
                    foundType = responseType;
                    break;
                }
            }
            
            if (foundCustomResponse != null)
            {
                customResponseButton.gameObject.SetActive(true);
                
                if (customResponseText != null)
                    customResponseText.text = foundCustomResponse.responseText;
                if (customResponseCost != null)
                    customResponseCost.text = FormatCost(foundCustomResponse.rpCost, foundCustomResponse.cpCost);
                    
                // Store the response type for the button click
                customResponseButton.onClick.RemoveAllListeners();
                customResponseButton.onClick.AddListener(() => RespondToEvent(foundType));
            }
            else
            {
                customResponseButton.gameObject.SetActive(false);
            }
        }
        
        private void UpdateButtonAffordability()
        {
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null || currentEvent == null) return;
            
            // Emergency Fix Button
            UpdateButtonAffordability(emergencyFixButton, EventResponseType.EmergencyFix, resourceManager);
            
            // Community Management Button
            UpdateButtonAffordability(communityManagementButton, EventResponseType.CommunityManagement, resourceManager);
            
            // Observe button is usually always affordable (free)
            if (observeAndLearnButton != null)
            {
                observeAndLearnButton.interactable = true;
                SetButtonAffordabilityVisuals(observeAndLearnButton, true);
            }
            
            // Custom Response Button
            UpdateCustomButtonAffordability(resourceManager);
        }
        
        private void UpdateButtonAffordability(Button button, EventResponseType responseType, Core.ResourceManager resourceManager)
        {
            if (button == null || !currentEvent.responses.ContainsKey(responseType)) return;
            
            var response = currentEvent.responses[responseType];
            bool canAfford = resourceManager.CanSpend(response.rpCost, response.cpCost);
            
            button.interactable = canAfford;
            SetButtonAffordabilityVisuals(button, canAfford);
        }
        
        private void UpdateCustomButtonAffordability(Core.ResourceManager resourceManager)
        {
            if (customResponseButton == null || !customResponseButton.gameObject.activeSelf) return;
            
            // Find the active custom response
            var customResponses = new[] { EventResponseType.IgnoreEvent, EventResponseType.DelayResponse, EventResponseType.SeekAdvice, EventResponseType.CustomAction };
            
            foreach (var responseType in customResponses)
            {
                if (currentEvent.responses.ContainsKey(responseType))
                {
                    var response = currentEvent.responses[responseType];
                    bool canAfford = resourceManager.CanSpend(response.rpCost, response.cpCost);
                    
                    customResponseButton.interactable = canAfford;
                    SetButtonAffordabilityVisuals(customResponseButton, canAfford);
                    break;
                }
            }
        }
        
        private void SetButtonAffordabilityVisuals(Button button, bool canAfford)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canAfford ? Color.white : new Color(1f, 1f, 1f, 0.6f);
            }
            
            // Update text colors for cost displays
            var textComponents = button.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in textComponents)
            {
                if (text.name.ToLower().Contains("cost"))
                {
                    text.color = canAfford ? Color.white : Color.red;
                }
            }
        }
        
        private void UpdateTimer()
        {
            if (currentEvent == null) return;
            
            timeRemaining -= Time.deltaTime;
            
            if (responseWindowText != null)
            {
                if (timeRemaining > 0)
                {
                    responseWindowText.text = $"Response Window: {timeRemaining:F1}s";
                    
                    // Update urgency color based on time remaining
                    if (timeRemaining < currentEvent.responseTimeLimit * 0.3f) // Last 30%
                    {
                        responseWindowText.color = urgentColor;
                    }
                    else if (timeRemaining < currentEvent.responseTimeLimit * 0.6f) // Last 60%
                    {
                        responseWindowText.color = Color.yellow;
                    }
                    else
                    {
                        responseWindowText.color = Color.white;
                    }
                }
                else
                {
                    responseWindowText.text = "Time Expired!";
                    responseWindowText.color = Color.red;
                    ExpireEvent();
                }
            }
        }
        
        private void UpdateEventTypeVisuals()
        {
            Color typeColor = currentEvent.eventType switch
            {
                EventType.Crisis => crisisColor,
                EventType.Opportunity => opportunityColor,
                EventType.CommunityEvent => neutralColor,
                EventType.MetaShift => Color.cyan,
                EventType.TournamentEvent => Color.magenta,
                _ => neutralColor
            };
            
            if (eventTypeIcon != null)
                eventTypeIcon.color = typeColor;
                
            // Update title color to match event type
            if (eventTitleText != null)
                eventTitleText.color = typeColor;
        }
        
        private void UpdateUrgencyVisuals()
        {
            if (urgencyIndicator == null) return;
            
            Color urgencyColor = currentEvent.urgencyLevel switch
            {
                EventUrgency.Low => Color.green,
                EventUrgency.Medium => Color.yellow,
                EventUrgency.High => Color.orange,
                EventUrgency.Critical => Color.red,
                _ => Color.white
            };
            
            urgencyIndicator.color = urgencyColor;
            
            // Add pulsing effect for critical events
            if (currentEvent.urgencyLevel == EventUrgency.Critical)
            {
                float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                urgencyIndicator.color = Color.Lerp(Color.red, Color.white, pulse);
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void SetupButtonListeners()
        {
            if (emergencyFixButton != null)
                emergencyFixButton.onClick.AddListener(() => RespondToEvent(EventResponseType.EmergencyFix));
                
            if (communityManagementButton != null)
                communityManagementButton.onClick.AddListener(() => RespondToEvent(EventResponseType.CommunityManagement));
                
            if (observeAndLearnButton != null)
                observeAndLearnButton.onClick.AddListener(() => RespondToEvent(EventResponseType.ObserveAndLearn));
                
            // Custom response button listener is set dynamically in UpdateCustomResponseButton()
        }
        
        private void RespondToEvent(EventResponseType responseType)
        {
            if (currentEvent == null || !currentEvent.responses.ContainsKey(responseType))
            {
                Debug.LogError($"Invalid response type: {responseType}");
                return;
            }
            
            var response = currentEvent.responses[responseType];
            var resourceManager = Core.ResourceManager.Instance;
            
            // Check if we can afford the response
            if (resourceManager != null && !resourceManager.CanSpend(response.rpCost, response.cpCost))
            {
                Debug.Log($"Cannot afford response: {response.responseText} (Cost: {response.rpCost} RP, {response.cpCost} CP)");
                ShowInsufficientResourcesEffect();
                return;
            }
            
            // Spend resources
            if (resourceManager != null)
            {
                resourceManager.SpendResources(response.rpCost, response.cpCost);
            }
            
            // Execute the response effects
            ExecuteResponseEffects(response);
            
            // Notify the event system
            OnEventResponseSelected.Invoke(currentEvent, responseType);
            
            // Generate community feedback about the response
            GenerateResponseFeedback(responseType, response);
            
            Debug.Log($"✅ Event response executed: {response.responseText}");
            
            // Close the event
            CloseEvent();
        }
        
        private void ExecuteResponseEffects(EventResponse response)
        {
            // Apply character stat changes
            if (response.characterEffects != null && Characters.CharacterManager.Instance != null)
            {
                var characterManager = Characters.CharacterManager.Instance;
                foreach (var effect in response.characterEffects)
                {
                    characterManager.ModifyStat(effect.character, effect.stat, effect.changeAmount);
                }
            }
            
            // Apply community sentiment changes
            if (response.communitySentimentChange != 0f && Community.CommunityFeedbackManager.Instance != null)
            {
                // The community system will pick this up automatically through the CharacterManager events
                Debug.Log($"Community sentiment effect: {response.communitySentimentChange:+0.0;-0.0}");
            }
            
            // Apply any special effects
            if (!string.IsNullOrEmpty(response.specialEffect))
            {
                ExecuteSpecialEffect(response.specialEffect);
            }
        }
        
        private void ExecuteSpecialEffect(string effectName)
        {
            // Handle special effects based on name
            switch (effectName.ToLower())
            {
                case "boost_rp_generation":
                    // Could boost resource generation temporarily
                    Debug.Log("🔋 RP generation boosted!");
                    break;
                    
                case "unlock_emergency_cards":
                    // Could unlock special cards in the card manager
                    Debug.Log("🎴 Emergency cards unlocked!");
                    break;
                    
                case "tournament_delay":
                    // Could delay tournament events
                    Debug.Log("🏆 Tournament delayed!");
                    break;
                    
                default:
                    Debug.Log($"🎭 Special effect executed: {effectName}");
                    break;
            }
        }
        
        private void GenerateResponseFeedback(EventResponseType responseType, EventResponse response)
        {
            // Generate appropriate community feedback based on the response
            var feedbackManager = Community.CommunityFeedbackManager.Instance;
            if (feedbackManager == null) return;
            
            // Create contextual feedback about the event response
            var responseFeedback = new Community.CommunityFeedback
            {
                author = GetResponseFeedbackAuthor(responseType),
                content = GetResponseFeedbackContent(responseType, response),
                sentiment = CalculateResponseSentiment(responseType, response),
                feedbackType = Community.FeedbackType.BalanceReaction,
                communitySegment = GetResponseFeedbackSegment(responseType),
                timestamp = System.DateTime.Now,
                upvotes = Random.Range(10, 50),
                replies = Random.Range(5, 25),
                isOrganic = false
            };
            
            // Add the feedback to the system
            feedbackManager.OnNewFeedbackAdded.Invoke(responseFeedback);
        }
        
        private void ShowInsufficientResourcesEffect()
        {
            // Visual feedback for insufficient resources
            if (eventTitleText != null)
            {
                var originalColor = eventTitleText.color;
                eventTitleText.color = Color.red;
                
                // Flash effect
                LeanTween.color(eventTitleText.rectTransform, originalColor, 1f).setEaseOutQuad();
            }
            
            Debug.Log("❌ Insufficient resources to execute response");
        }
        
        private void ExpireEvent()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Disable all buttons
            SetAllButtonsInteractable(false);
            
            // Apply expiration penalties if any
            if (currentEvent.expirationPenalty != null)
            {
                ExecuteResponseEffects(currentEvent.expirationPenalty);
            }
            
            OnEventExpired.Invoke(currentEvent);
            
            // Auto-close after a delay to show the expiration
            Invoke(nameof(CloseEvent), 2f);
        }
        
        #endregion
        
        #region Utility Methods
        
        private void SetAllButtonsInteractable(bool interactable)
        {
            if (emergencyFixButton != null) emergencyFixButton.interactable = interactable;
            if (communityManagementButton != null) communityManagementButton.interactable = interactable;
            if (observeAndLearnButton != null) observeAndLearnButton.interactable = interactable;
            if (customResponseButton != null) customResponseButton.interactable = interactable;
        }
        
        private string FormatExpectedImpact(List<string> impacts)
        {
            if (impacts == null || impacts.Count == 0) return "Unknown impact";
            return string.Join("\n• ", impacts.ToArray());
        }
        
        private string FormatCost(int rpCost, int cpCost)
        {
            if (rpCost > 0 && cpCost > 0)
                return $"{rpCost} RP, {cpCost} CP";
            else if (rpCost > 0)
                return $"{rpCost} RP";
            else if (cpCost > 0)
                return $"{cpCost} CP";
            else
                return "Free";
        }
        
        private string GetResponseFeedbackAuthor(EventResponseType responseType)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => "DevTeam_Response",
                EventResponseType.CommunityManagement => "CommunityManager",
                EventResponseType.ObserveAndLearn => "DataAnalyst",
                EventResponseType.IgnoreEvent => "Community_Watcher",
                _ => "EventObserver"
            };
        }
        
        private string GetResponseFeedbackContent(EventResponseType responseType, EventResponse response)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => "Quick hotfix deployed! ⚡ Hope this addresses the immediate issues",
                EventResponseType.CommunityManagement => "Devs are communicating well about this situation 📢",
                EventResponseType.ObserveAndLearn => "Interesting approach - let's see how this plays out 👀",
                EventResponseType.IgnoreEvent => "No response to the situation... concerning 🤔",
                _ => $"Response to recent events: {response.responseText}"
            };
        }
        
        private float CalculateResponseSentiment(EventResponseType responseType, EventResponse response)
        {
            float baseSentiment = responseType switch
            {
                EventResponseType.EmergencyFix => 0.6f,      // Generally positive
                EventResponseType.CommunityManagement => 0.4f, // Moderately positive
                EventResponseType.ObserveAndLearn => 0.0f,   // Neutral
                EventResponseType.IgnoreEvent => -0.5f,      // Negative
                _ => 0.0f
            };
            
            // Add some variance
            return baseSentiment + Random.Range(-0.2f, 0.2f);
        }
        
        private string GetResponseFeedbackSegment(EventResponseType responseType)
        {
            return responseType switch
            {
                EventResponseType.EmergencyFix => "Competitive",
                EventResponseType.CommunityManagement => "Casual Players",
                EventResponseType.ObserveAndLearn => "Pro Players",
                _ => "General"
            };
        }
        
        #endregion
        
        #region Public Getters
        
        public EventData GetCurrentEvent() => currentEvent;
        public bool IsActive() => isActive;
        public float GetTimeRemaining() => timeRemaining;
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("🧪 Test Crisis Event")]
        public void TestCrisisEvent()
        {
            var testEvent = EventDataFactory.CreateSupportExploitCrisis();
            DisplayEvent(testEvent);
        }
        
        [ContextMenu("🧪 Test Opportunity Event")]
        public void TestOpportunityEvent()
        {
            var testEvent = EventDataFactory.CreateTournamentOpportunity();
            DisplayEvent(testEvent);
        }
        
        [ContextMenu("🔍 Debug Current Event")]
        public void DebugCurrentEvent()
        {
            if (currentEvent == null)
            {
                Debug.Log("❌ No current event");
                return;
            }
            
            Debug.Log("=== 🔍 CURRENT EVENT DEBUG ===");
            Debug.Log($"Title: {currentEvent.eventTitle}");
            Debug.Log($"Type: {currentEvent.eventType}");
            Debug.Log($"Urgency: {currentEvent.urgencyLevel}");
            Debug.Log($"Time Remaining: {timeRemaining:F1}s");
            Debug.Log($"Responses Available: {currentEvent.responses.Count}");
            
            foreach (var response in currentEvent.responses)
            {
                Debug.Log($"  {response.Key}: {response.Value.responseText} (Cost: {response.Value.rpCost} RP, {response.Value.cpCost} CP)");
            }
        }
        
        #endregion
    }