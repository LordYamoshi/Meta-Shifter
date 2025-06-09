using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    public class EventItemUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Transform responseButtonsContainer;
        [SerializeField] private GameObject responseButtonPrefab;
        
        private Events.GameEvent currentEvent;
        private List<Button> responseButtons = new List<Button>();
        
        public void Setup(Events.GameEvent gameEvent)
        {
            currentEvent = gameEvent;
            
            // Set basic info
            if (titleText != null)
                titleText.text = $"{gameEvent.GetEventIcon()} {gameEvent.title}";
                
            if (descriptionText != null)
                descriptionText.text = gameEvent.description;
            
            // Set background color
            if (backgroundImage != null)
                backgroundImage.color = gameEvent.GetEventColor() * 0.3f; // Semi-transparent
            
            // Create response buttons
            CreateResponseButtons();
            
            // Start timer updates
            InvokeRepeating(nameof(UpdateTimer), 0f, 1f);
        }
        
        private void CreateResponseButtons()
        {
            if (responseButtonsContainer == null) return;
            
            var responseManager = Events.EventResponseManager.Instance;
            if (responseManager == null) return;
            
            var availableResponses = responseManager.GetAvailableResponses();
            
            foreach (var response in availableResponses)
            {
                CreateResponseButton(response);
            }
        }
        
        private void CreateResponseButton(Events.EventResponseManager.ResponseData response)
        {
            GameObject buttonObj;
            
            if (responseButtonPrefab != null)
            {
                buttonObj = Instantiate(responseButtonPrefab, responseButtonsContainer);
            }
            else
            {
                // Create simple button if no prefab
                buttonObj = new GameObject($"Response_{response.type}");
                buttonObj.transform.SetParent(responseButtonsContainer, false);
                buttonObj.AddComponent<Image>();
                buttonObj.AddComponent<Button>();
                
                var textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                textObj.AddComponent<TextMeshProUGUI>();
            }
            
            var button = buttonObj.GetComponent<Button>();
            var buttonImage = buttonObj.GetComponent<Image>();
            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // Setup button
            if (button != null)
            {
                button.onClick.AddListener(() => OnResponseSelected(response.type));
                responseButtons.Add(button);
            }
            
            // Setup appearance
            if (buttonImage != null)
                buttonImage.color = response.buttonColor;
                
            if (buttonText != null)
            {
                string costText = "";
                if (response.rpCost > 0 || response.cpCost > 0)
                {
                    costText = $" ({response.rpCost} RP, {response.cpCost} CP)";
                }
                buttonText.text = response.name + costText;
                buttonText.color = Color.white;
            }
            
            // Check affordability
            var responseManager = Events.EventResponseManager.Instance;
            if (responseManager != null)
            {
                bool canAfford = responseManager.CanAffordResponse(response.type);
                if (button != null)
                    button.interactable = canAfford;
                    
                if (!canAfford && buttonImage != null)
                    buttonImage.color = Color.gray;
            }
        }
        
        private void OnResponseSelected(Events.ResponseType responseType)
        {
            var responseManager = Events.EventResponseManager.Instance;
            if (responseManager != null)
            {
                bool success = responseManager.ExecuteResponse(currentEvent, responseType);
                if (success)
                {
                    // Disable all buttons after response
                    foreach (var button in responseButtons)
                    {
                        if (button != null)
                            button.interactable = false;
                    }
                    
                    // Update timer text to show resolved
                    if (timerText != null)
                        timerText.text = "Resolved";
                    
                    // Auto-remove after delay
                    Invoke(nameof(RemoveSelf), 2f);
                }
            }
        }
        
        private void UpdateTimer()
        {
            if (currentEvent == null || currentEvent.isResolved) return;
            
            if (timerText != null)
            {
                if (currentEvent.IsExpired())
                {
                    timerText.text = "Expired";
                    timerText.color = Color.red;
                    
                    // Disable buttons if expired
                    foreach (var button in responseButtons)
                    {
                        if (button != null)
                            button.interactable = false;
                    }
                }
                else
                {
                    timerText.text = $"Time: {currentEvent.GetTimeRemaining()}";
                    timerText.color = Color.white;
                }
            }
        }
        
        private void RemoveSelf()
        {
            CancelInvoke();
            Destroy(gameObject);
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}