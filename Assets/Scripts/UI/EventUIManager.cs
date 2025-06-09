using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace MetaBalance.UI
{
    public class EventUIManager : MonoBehaviour
    {
        [Header("Event UI References")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private Transform eventContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private TextMeshProUGUI eventPhaseTitle;
        
        [Header("Event Modal")]
        [SerializeField] private GameObject eventModal;
        [SerializeField] private TextMeshProUGUI modalTitle;
        [SerializeField] private TextMeshProUGUI modalDescription;
        [SerializeField] private TextMeshProUGUI modalTimer;
        [SerializeField] private Transform responseContainer;
        [SerializeField] private GameObject responseButtonPrefab;
        
        private List<EventUIItem> activeEventItems = new List<EventUIItem>();
        private Events.GameEvent currentModalEvent;
        
        private void Start()
        {
            SubscribeToEvents();
            UpdateEventPanelVisibility();
        }
        
        private void SubscribeToEvents()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
            
            if (Events.EventManager.Instance != null)
            {
                Events.EventManager.Instance.OnEventGenerated.AddListener(OnEventGenerated);
                Events.EventManager.Instance.OnEventResolved.AddListener(OnEventResolved);
                Events.EventManager.Instance.OnActiveEventsChanged.AddListener(OnActiveEventsChanged);
            }
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            bool isEventPhase = newPhase == Core.GamePhase.Event;
            UpdateEventPanelVisibility(isEventPhase);
            
            if (eventPhaseTitle != null)
            {
                eventPhaseTitle.text = isEventPhase ? "Event Phase - Handle Community Events" : "";
            }
        }
        
        private void UpdateEventPanelVisibility(bool? forceVisibility = null)
        {
            bool shouldShow = forceVisibility ?? (Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Event);
            
            if (eventPanel != null)
                eventPanel.SetActive(shouldShow);
        }
        
        private void OnEventGenerated(Events.GameEvent gameEvent)
        {
            CreateEventItem(gameEvent);
        }
        
        private void OnEventResolved(Events.GameEvent gameEvent)
        {
            RemoveEventItem(gameEvent);
            
            if (currentModalEvent == gameEvent)
            {
                CloseEventModal();
            }
        }
        
        private void OnActiveEventsChanged(List<Events.GameEvent> activeEvents)
        {
            RefreshEventItems(activeEvents);
        }
        
        private void CreateEventItem(Events.GameEvent gameEvent)
        {
            if (eventItemPrefab == null || eventContainer == null) return;
            
            GameObject itemObj = Instantiate(eventItemPrefab, eventContainer);
            EventUIItem eventItem = itemObj.GetComponent<EventUIItem>();
            
            if (eventItem == null)
                eventItem = itemObj.AddComponent<EventUIItem>();
            
            eventItem.Setup(gameEvent, ShowEventModal);
            activeEventItems.Add(eventItem);
        }
        
        private void RemoveEventItem(Events.GameEvent gameEvent)
        {
            var itemToRemove = activeEventItems.FirstOrDefault(item => item.GameEvent.eventId == gameEvent.eventId);
            if (itemToRemove != null)
            {
                activeEventItems.Remove(itemToRemove);
                if (itemToRemove.gameObject != null)
                    Destroy(itemToRemove.gameObject);
            }
        }
        
        private void RefreshEventItems(List<Events.GameEvent> activeEvents)
        {
            // Clear existing items
            foreach (var item in activeEventItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            activeEventItems.Clear();
            
            // Create new items
            foreach (var gameEvent in activeEvents)
            {
                CreateEventItem(gameEvent);
            }
        }
        
        private void ShowEventModal(Events.GameEvent gameEvent)
        {
            currentModalEvent = gameEvent;
            
            if (eventModal != null)
                eventModal.SetActive(true);
            
            if (modalTitle != null)
                modalTitle.text = gameEvent.title;
            
            if (modalDescription != null)
                modalDescription.text = gameEvent.description;
            
            CreateResponseButtons(gameEvent);
        }
        
        private void CreateResponseButtons(Events.GameEvent gameEvent)
        {
            if (responseContainer == null || responseButtonPrefab == null) return;
            
            // Clear existing buttons
            foreach (Transform child in responseContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create response buttons
            foreach (var response in gameEvent.responses)
            {
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText != null)
                {
                    string costText = "";
                    if (response.rpCost > 0 || response.cpCost > 0)
                        costText = $" ({response.rpCost} RP, {response.cpCost} CP)";
                    
                    buttonText.text = response.responseText + costText;
                }
                
                if (button != null)
                {
                    button.onClick.AddListener(() => RespondToEvent(response));
                    
                    // Check affordability
                    bool canAfford = Core.ResourceManager.Instance?.CanSpend(response.rpCost, response.cpCost) ?? false;
                    button.interactable = canAfford;
                    
                    if (!canAfford)
                        buttonObj.GetComponent<Image>().color = Color.gray;
                }
            }
        }
        
        private void RespondToEvent(Events.EventResponse response)
        {
            if (currentModalEvent != null && Events.EventManager.Instance != null)
            {
                Events.EventManager.Instance.RespondToEvent(currentModalEvent, response);
                CloseEventModal();
            }
        }
        
        private void CloseEventModal()
        {
            if (eventModal != null)
                eventModal.SetActive(false);
            
            currentModalEvent = null;
        }
        
        private void Update()
        {
            // Update modal timer if showing
            if (currentModalEvent != null && modalTimer != null)
            {
                int minutes = Mathf.FloorToInt(currentModalEvent.timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(currentModalEvent.timeRemaining % 60f);
                modalTimer.text = $"Time: {minutes}:{seconds:00}";
            }
            
            // Update event item timers
            foreach (var item in activeEventItems)
            {
                item?.UpdateTimer();
            }
        }
        
        [ContextMenu("Test Crisis Event")]
        private void TestCrisisEvent()
        {
            var testEvent = new Events.GameEvent
            {
                title = "Test Crisis",
                description = "This is a test crisis event",
                eventType = Events.EventType.Crisis,
                severity = Events.EventSeverity.High,
                timeRemaining = 120f
            };
            
            testEvent.responses.Add(new Events.EventResponse("Test Response", 2, 1));
            OnEventGenerated(testEvent);
        }
    }
    
    public class EventUIItem : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI severityText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button respondButton;
        
        public Events.GameEvent GameEvent { get; private set; }
        private System.Action<Events.GameEvent> onRespond;
        
        public void Setup(Events.GameEvent gameEvent, System.Action<Events.GameEvent> respondCallback)
        {
            GameEvent = gameEvent;
            onRespond = respondCallback;
            
            FindComponents();
            UpdateDisplay();
            
            if (respondButton != null)
                respondButton.onClick.AddListener(() => onRespond?.Invoke(GameEvent));
        }
        
        private void FindComponents()
        {
            if (titleText == null)
                titleText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (respondButton == null)
                respondButton = GetComponentInChildren<Button>();
        }
        
        private void UpdateDisplay()
        {
            if (titleText != null)
                titleText.text = GameEvent.title;
            
            if (severityText != null)
                severityText.text = GameEvent.severity.ToString();
            
            if (backgroundImage != null)
            {
                backgroundImage.color = GameEvent.eventType switch
                {
                    Events.EventType.Crisis => new Color(0.8f, 0.2f, 0.2f, 0.8f),
                    Events.EventType.Opportunity => new Color(0.2f, 0.8f, 0.2f, 0.8f),
                    Events.EventType.Community => new Color(0.8f, 0.2f, 0.8f, 0.8f),
                    _ => new Color(0.5f, 0.5f, 0.5f, 0.8f)
                };
            }
        }
        
        public void UpdateTimer()
        {
            if (timerText != null && GameEvent != null)
            {
                int minutes = Mathf.FloorToInt(GameEvent.timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(GameEvent.timeRemaining % 60f);
                timerText.text = $"{minutes}:{seconds:00}";
                
                // Color code based on urgency
                if (GameEvent.timeRemaining < 30f)
                    timerText.color = Color.red;
                else if (GameEvent.timeRemaining < 60f)
                    timerText.color = Color.yellow;
                else
                    timerText.color = Color.white;
            }
        }
    }
}