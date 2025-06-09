using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    public class EventUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform eventsContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private TextMeshProUGUI noEventsText;
        
        private List<EventItemUI> activeEventItems = new List<EventItemUI>();
        
        private void Start()
        {
            if (Events.EventManager.Instance != null)
            {
                Events.EventManager.Instance.OnActiveEventsChanged.AddListener(UpdateEventsDisplay);
            }
            
            // Show initial "no events" state
            UpdateNoEventsDisplay(true);
        }
        
        private void UpdateEventsDisplay(List<Events.GameEvent> events)
        {
            // Clear existing items
            ClearEventItems();
            
            if (events.Count == 0)
            {
                UpdateNoEventsDisplay(true);
                return;
            }
            
            UpdateNoEventsDisplay(false);
            
            // Create new event items
            foreach (var gameEvent in events)
            {
                CreateEventItem(gameEvent);
            }
        }
        
        private void ClearEventItems()
        {
            foreach (var item in activeEventItems)
            {
                if (item != null && item.gameObject != null)
                    Destroy(item.gameObject);
            }
            activeEventItems.Clear();
        }
        
        private void CreateEventItem(Events.GameEvent gameEvent)
        {
            if (eventItemPrefab == null || eventsContainer == null) return;
            
            GameObject itemObj = Instantiate(eventItemPrefab, eventsContainer);
            EventItemUI eventItem = itemObj.GetComponent<EventItemUI>();
            
            if (eventItem == null)
            {
                eventItem = itemObj.AddComponent<EventItemUI>();
            }
            
            eventItem.Setup(gameEvent);
            activeEventItems.Add(eventItem);
        }
        
        private void UpdateNoEventsDisplay(bool showNoEvents)
        {
            if (noEventsText != null)
            {
                noEventsText.gameObject.SetActive(showNoEvents);
                if (showNoEvents)
                {
                    noEventsText.text = "No active events\nEverything is running smoothly!";
                }
            }
        }
        
        [ContextMenu("Debug: Refresh Events Display")]
        public void DebugRefreshDisplay()
        {
            if (Events.EventManager.Instance != null)
            {
                var events = Events.EventManager.Instance.GetActiveEvents();
                UpdateEventsDisplay(events);
            }
        }
    }
}