using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Drop zone in the center where cards are queued for implementation
    /// </summary>
    public class CardDropZone : MonoBehaviour
    {
        [Header("Drop Zone Settings")]
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private GameObject cardSlotPrefab;
        [SerializeField] private int maxCards = 5;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.2f, 0.5f, 0.8f, 0.7f);
        
        [Header("Events")]
        public UnityEvent<List<CardData>> OnCardsChanged;
        
        private List<DraggableCard> queuedCards = new List<DraggableCard>();
        private List<GameObject> cardSlots = new List<GameObject>();
        
        private void Start()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (cardContainer == null)
                cardContainer = GetComponent<RectTransform>();
            
            SetNormalAppearance();
            CreateCardSlots();
            
            // Subscribe to phase changes
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        private void CreateCardSlots()
        {
            // Clear existing slots
            foreach (var slot in cardSlots)
            {
                if (slot != null) Destroy(slot);
            }
            cardSlots.Clear();
            
            // Create new slots
            for (int i = 0; i < maxCards; i++)
            {
                GameObject slot = new GameObject($"CardSlot_{i}");
                slot.transform.SetParent(cardContainer, false);
                
                // Add layout element
                var layoutElement = slot.AddComponent<LayoutElement>();
                layoutElement.preferredWidth = 120f;
                layoutElement.preferredHeight = 160f;
                
                // Add visual (optional border)
                var slotImage = slot.AddComponent<Image>();
                slotImage.color = new Color(1f, 1f, 1f, 0.1f);
                
                cardSlots.Add(slot);
            }
        }
        
        public bool CanAcceptCard(CardData cardData)
        {
            // Check if we have space and if we're in planning phase
            return queuedCards.Count < maxCards && 
                   Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
        }
        
        public void AcceptCard(DraggableCard card)
        {
            if (!CanAcceptCard(card.CardData)) return;
            
            // Add to queued cards
            queuedCards.Add(card);
            
            // Move card to appropriate slot
            int slotIndex = queuedCards.Count - 1;
            card.transform.SetParent(cardSlots[slotIndex].transform, false);
            card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            
            // Add remove button functionality
            AddRemoveButton(card);
            
            // Notify listeners
            OnCardsChanged.Invoke(GetQueuedCardData());
            
            Debug.Log($"Card {card.CardData.cardName} added to implementation queue");
        }
        
        private void AddRemoveButton(DraggableCard card)
        {
            // Add a small X button to remove card from queue
            GameObject removeButton = new GameObject("RemoveButton");
            removeButton.transform.SetParent(card.transform, false);
            
            var rectTransform = removeButton.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-5f, -5f);
            rectTransform.sizeDelta = new Vector2(20f, 20f);
            
            var buttonImage = removeButton.AddComponent<Image>();
            buttonImage.color = Color.red;
            
            var button = removeButton.AddComponent<Button>();
            button.onClick.AddListener(() => RemoveCard(card));
            
            // Add X text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(removeButton.transform, false);
            var text = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "×";
            text.fontSize = 16f;
            text.color = Color.white;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        public void RemoveCard(DraggableCard card)
        {
            if (queuedCards.Contains(card))
            {
                queuedCards.Remove(card);
                card.ReturnToHand();
                
                // Reorganize remaining cards
                ReorganizeCards();
                
                OnCardsChanged.Invoke(GetQueuedCardData());
                Debug.Log($"Card {card.CardData.cardName} removed from queue");
            }
        }
        
        private void ReorganizeCards()
        {
            for (int i = 0; i < queuedCards.Count; i++)
            {
                queuedCards[i].transform.SetParent(cardSlots[i].transform, false);
                queuedCards[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        
        public void ImplementAllCards()
        {
            if (Core.PhaseManager.Instance?.GetCurrentPhase() != Core.GamePhase.Implementation)
                return;
            
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null) return;
            
            // Calculate total cost
            int totalRP = 0, totalCP = 0;
            foreach (var card in queuedCards)
            {
                totalRP += card.CardData.researchPointCost;
                totalCP += card.CardData.communityPointCost;
            }
            
            // Check if player can afford all cards
            if (!resourceManager.CanSpend(totalRP, totalCP))
            {
                Debug.Log($"Cannot afford to implement all cards! Need {totalRP} RP, {totalCP} CP");
                return;
            }
            
            // Spend resources
            resourceManager.SpendResources(totalRP, totalCP);
            
            // Implement each card
            foreach (var card in queuedCards)
            {
                card.CardData.PlayCard();
                Debug.Log($"Implemented: {card.CardData.cardName}");
            }
            
            // Clear the queue
            ClearQueue();
            
            Debug.Log($"Implemented {queuedCards.Count} cards for {totalRP} RP, {totalCP} CP");
        }
        
        public void ClearQueue()
        {
            foreach (var card in queuedCards)
            {
                Destroy(card.gameObject);
            }
            queuedCards.Clear();
            OnCardsChanged.Invoke(GetQueuedCardData());
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            switch (newPhase)
            {
                case Core.GamePhase.Planning:
                    SetNormalAppearance();
                    break;
                    
                case Core.GamePhase.Implementation:
                    SetImplementationAppearance();
                    // Auto-implement cards
                    if (queuedCards.Count > 0)
                    {
                        ImplementAllCards();
                    }
                    break;
                    
                case Core.GamePhase.Feedback:
                    SetNormalAppearance();
                    break;
            }
        }
        
        private void SetNormalAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }
        
        private void SetImplementationAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = highlightColor;
        }
        
        public List<CardData> GetQueuedCardData()
        {
            var cardDataList = new List<CardData>();
            foreach (var card in queuedCards)
            {
                cardDataList.Add(card.CardData);
            }
            return cardDataList;
        }
        
        public int GetQueuedCardCount() => queuedCards.Count;
        
        public bool HasQueuedCards() => queuedCards.Count > 0;
        
        private void OnDestroy()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
        }
    }
}