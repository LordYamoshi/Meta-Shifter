// Assets/Scripts/UI/EventUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    public class EventUIManager : MonoBehaviour
    {
        [Header("Event Display")]
        [SerializeField] private Transform eventContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private int maxVisibleEvents = 5;
        
        [Header("Event Modal")]
        [SerializeField] private GameObject eventModal;
        [SerializeField] private TextMeshProUGUI modalTitle;
        [SerializeField] private TextMeshProUGUI modalDescription;
        [SerializeField] private TextMeshProUGUI modalSeverity;
        [SerializeField] private TextMeshProUGUI modalTimeRemaining;
        [SerializeField] private Transform modalButtonContainer;
        [SerializeField] private GameObject responseButtonPrefab;
        [SerializeField] private Button modalCloseButton;
        
        [Header("Event Notification")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationDuration = 3f;
        
        private List<EventUIItem> activeEventItems = new List<EventUIItem>();
        private Events.GameEvent currentModalEvent;
        
        private void Start()
        {
            SetupEventUI();
            SubscribeToEvents();
        }
        
        private void SetupEventUI()
        {
            if (modalCloseButton != null)
            {
                modalCloseButton.onClick.AddListener(CloseEventModal);
            }
            
            if (eventModal != null)
            {
                eventModal.SetActive(false);
            }
            
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
        
        private void SubscribeToEvents()
        {
            if (Events.EventManager.Instance != null)
            {
                Events.EventManager.Instance.OnEventTriggered.AddListener(OnEventTriggered);
                Events.EventManager.Instance.OnEventResolved.AddListener(OnEventResolved);
                Events.EventManager.Instance.OnActiveEventsChanged.AddListener(OnActiveEventsChanged);
                Events.EventManager.Instance.OnEventResponseChosen.AddListener(OnEventResponseChosen);
            }
        }
        
        private void OnEventTriggered(Events.GameEvent gameEvent)
        {
            ShowEventNotification($"New Event: {gameEvent.eventTitle}");
            CreateEventItem(gameEvent);
            
            // Auto-open modal for critical events
            if (gameEvent.severity == Events.EventSeverity.Critical)
            {
                ShowEventModal(gameEvent);
            }
        }
        
        private void OnEventResolved(Events.GameEvent gameEvent)
        {
            RemoveEventItem(gameEvent);
        }
        
        private void OnActiveEventsChanged(List<Events.GameEvent> activeEvents)
        {
            RefreshEventDisplay(activeEvents);
        }
        
        private void OnEventResponseChosen(Events.GameEvent gameEvent, Events.EventResponse response)
        {
            ShowEventNotification($"Responded to {gameEvent.eventTitle} with {response.buttonText}");
            CloseEventModal();
        }
        
        private void CreateEventItem(Events.GameEvent gameEvent)
        {
            if (eventItemPrefab == null || eventContainer == null) return;
            
            GameObject itemObj = Instantiate(eventItemPrefab, eventContainer);
            EventUIItem uiItem = itemObj.GetComponent<EventUIItem>();
            
            if (uiItem == null)
            {
                uiItem = itemObj.AddComponent<EventUIItem>();
            }
            
            uiItem.Setup(gameEvent, () => ShowEventModal(gameEvent));
            activeEventItems.Add(uiItem);
            
            // Remove oldest if too many
            while (activeEventItems.Count > maxVisibleEvents)
            {
                RemoveOldestEventItem();
            }
        }
        
        private void RemoveEventItem(Events.GameEvent gameEvent)
        {
            var item = activeEventItems.FirstOrDefault(i => i.GetEvent() == gameEvent);
            if (item != null)
            {
                activeEventItems.Remove(item);
                if (item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
        }
        
        private void RemoveOldestEventItem()
        {
            if (activeEventItems.Count > 0)
            {
                var oldest = activeEventItems[0];
                activeEventItems.RemoveAt(0);
                if (oldest.gameObject != null)
                {
                    Destroy(oldest.gameObject);
                }
            }
        }
        
        private void RefreshEventDisplay(List<Events.GameEvent> activeEvents)
        {
            // Remove items for events that are no longer active
            var itemsToRemove = activeEventItems.Where(item => 
                !activeEvents.Contains(item.GetEvent())).ToList();
            
            foreach (var item in itemsToRemove)
            {
                activeEventItems.Remove(item);
                if (item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            
            // Update remaining items
            foreach (var item in activeEventItems)
            {
                item.RefreshDisplay();
            }
        }
        
        public void ShowEventModal(Events.GameEvent gameEvent)
        {
            if (eventModal == null) return;
            
            currentModalEvent = gameEvent;
            
            // Set modal content
            if (modalTitle != null) modalTitle.text = gameEvent.eventTitle;
            if (modalDescription != null) modalDescription.text = gameEvent.eventDescription;
            if (modalSeverity != null) 
            {
                modalSeverity.text = $"{gameEvent.GetSeverityEmoji()} {gameEvent.severity}";
                modalSeverity.color = gameEvent.GetSeverityColor();
            }
            if (modalTimeRemaining != null) modalTimeRemaining.text = gameEvent.GetTimeRemaining();
            
            // Create response buttons
            CreateResponseButtons(gameEvent);
            
            // Show modal
            eventModal.SetActive(true);
        }
        
        private void CreateResponseButtons(Events.GameEvent gameEvent)
        {
            if (modalButtonContainer == null || responseButtonPrefab == null) return;
            
            // Clear existing buttons
            foreach (Transform child in modalButtonContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create buttons for each available response
            foreach (var response in gameEvent.availableResponses)
            {
                CreateResponseButton(response);
            }
        }
        
        private void CreateResponseButton(Events.EventResponse response)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, modalButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = response.buttonText;
                
                // Add cost information if applicable
                if (response.rpCost > 0 || response.cpCost > 0)
                {
                    string costText = "";
                    if (response.rpCost > 0) costText += $"{response.rpCost} RP";
                    if (response.cpCost > 0)
                    {
                        if (costText.Length > 0) costText += ", ";
                        costText += $"{response.cpCost} CP";
                    }
                    buttonText.text += $"\n({costText})";
                }
            }
            
            if (button != null)
            {
                // Check if response is available
                bool isAvailable = response.IsCurrentlyAvailable();
                button.interactable = isAvailable;
                
                if (isAvailable)
                {
                    button.onClick.AddListener(() => {
                        if (Events.EventManager.Instance != null && currentModalEvent != null)
                        {
                            Events.EventManager.Instance.RespondToEvent(currentModalEvent, response);
                        }
                    });
                }
                else
                {
                    // Show why it's not available
                    if (buttonText != null)
                    {
                        if (!response.CanAfford())
                        {
                            buttonText.text += "\n(Can't afford)";
                        }
                        else if (!response.HasRequiredCards())
                        {
                            buttonText.text += "\n(Missing cards)";
                        }
                        else
                        {
                            buttonText.text += "\n(Not available)";
                        }
                    }
                }
                
                // Color code button based on response type
                ColorBlock colors = button.colors;
                colors.normalColor = GetResponseTypeColor(response.responseType);
                button.colors = colors;
            }
        }
        
        private Color GetResponseTypeColor(Events.ResponseType responseType)
        {
            return responseType switch
            {
                Events.ResponseType.Emergency => new Color(0.8f, 0.1f, 0.1f),   // Red
                Events.ResponseType.Strategic => new Color(0.2f, 0.6f, 0.8f),  // Blue
                Events.ResponseType.Community => new Color(0.8f, 0.2f, 0.8f),  // Purple
                Events.ResponseType.Technical => new Color(0.2f, 0.8f, 0.2f),  // Green
                Events.ResponseType.Ignore => new Color(0.5f, 0.5f, 0.5f),     // Gray
                Events.ResponseType.Escalate => new Color(1f, 0.8f, 0.2f),     // Orange
                _ => Color.white
            };
        }
        
        public void CloseEventModal()
        {
            if (eventModal != null)
            {
                eventModal.SetActive(false);
            }
            currentModalEvent = null;
        }
        
        private void ShowEventNotification(string message)
        {
            if (notificationPanel == null || notificationText == null) return;
            
            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            // Auto-hide after duration
            Invoke(nameof(HideEventNotification), notificationDuration);
        }
        
        private void HideEventNotification()
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
        
        // Public methods
        public void ShowAllActiveEvents()
        {
            var eventManager = Events.EventManager.Instance;
            if (eventManager != null)
            {
                var activeEvents = eventManager.GetActiveEvents();
                RefreshEventDisplay(activeEvents);
            }
        }
        
        // Debug methods
        [ContextMenu("🧪 Test Event Modal")]
        public void DebugTestEventModal()
        {
            var testEvent = new Events.GameEvent
            {
                eventTitle = "Test Event",
                eventDescription = "This is a test event for debugging the modal system.",
                eventType = Events.EventType.Crisis,
                severity = Events.EventSeverity.High,
                duration = 2f,
                turnsRemaining = 2
            };
            
            testEvent.availableResponses.Add(new Events.EventResponse
            {
                buttonText = "Test Response",
                responseDescription = "A test response",
                responseType = Events.ResponseType.Emergency,
                rpCost = 5,
                cpCost = 2
            });
            
            ShowEventModal(testEvent);
        }
    }
}