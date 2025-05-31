using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Fixed drop zone that properly handles card dropping
    /// </summary>
    public class CardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Drop Zone Settings")]
        [SerializeField] private RectTransform cardContainer; // This should be THIS transform, not hand container
        [SerializeField] private int maxCards = 5;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.2f, 0.5f, 0.8f, 0.7f);
        [SerializeField] private Color acceptColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        [SerializeField] private Color rejectColor = new Color(0.8f, 0.2f, 0.2f, 0.7f); // Red for rejection
        
        [Header("Layout")]
        [SerializeField] private float cardSpacing = 10f;
        [SerializeField] private float cardWidth = 120f;
        [SerializeField] private float cardHeight = 160f;
        
        [Header("Events")]
        public UnityEvent<List<CardData>> OnCardsChanged;
        
        private List<DraggableCard> queuedCards = new List<DraggableCard>();
        
        private void Start()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            // If no card container is specified, use this transform
            if (cardContainer == null)
                cardContainer = GetComponent<RectTransform>();
            
            SetNormalAppearance();
            
            // Subscribe to phase changes
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
            
            // Ensure this object can receive drops
            var graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                Debug.LogWarning("CardDropZone needs a GraphicRaycaster in parent hierarchy!");
            }
            
            Debug.Log($"CardDropZone initialized on {gameObject.name}, using container: {cardContainer?.name ?? "null"}");
        }
        
        public bool CanAcceptCard(CardData cardData)
        {
            // Check if we have space
            bool hasSpace = queuedCards.Count < maxCards;
            
            // Check if we're in planning phase
            bool isPlanning = Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
            
            // Check if player can afford this card
            bool canAfford = Core.ResourceManager.Instance?.CanSpend(cardData.researchPointCost, cardData.communityPointCost) ?? false;
            
            // Calculate total cost of all queued cards + this new card
            int totalRP = cardData.researchPointCost;
            int totalCP = cardData.communityPointCost;
            
            foreach (var queuedCard in queuedCards)
            {
                if (queuedCard != null && queuedCard.CardData != null)
                {
                    totalRP += queuedCard.CardData.researchPointCost;
                    totalCP += queuedCard.CardData.communityPointCost;
                }
            }
            
            // Check if player can afford ALL queued cards including this one
            bool canAffordTotal = Core.ResourceManager.Instance?.CanSpend(totalRP, totalCP) ?? false;
            
            Debug.Log($"CanAcceptCard {cardData.cardName}: hasSpace={hasSpace}, isPlanning={isPlanning}, canAfford={canAfford}, canAffordTotal={canAffordTotal}");
            Debug.Log($"  Current resources: RP={Core.ResourceManager.Instance?.ResearchPoints}, CP={Core.ResourceManager.Instance?.CommunityPoints}");
            Debug.Log($"  Total cost if added: RP={totalRP}, CP={totalCP}");
            
            return hasSpace && isPlanning && canAfford && canAffordTotal;
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop called!");
            
            // Get the dragged card
            var draggedObject = eventData.pointerDrag;
            if (draggedObject == null)
            {
                Debug.Log("No dragged object found");
                return;
            }
            
            var draggableCard = draggedObject.GetComponent<DraggableCard>();
            if (draggableCard == null)
            {
                Debug.Log("Dragged object is not a DraggableCard");
                return;
            }
            
            Debug.Log($"Attempting to drop card: {draggableCard.CardData.cardName}");
            
            if (CanAcceptCard(draggableCard.CardData))
            {
                AcceptCard(draggableCard);
                SetAcceptAppearance();
            }
            else
            {
                Debug.Log("Cannot accept card - conditions not met");
                SetNormalAppearance();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                var draggableCard = eventData.pointerDrag.GetComponent<DraggableCard>();
                if (draggableCard != null)
                {
                    if (CanAcceptCard(draggableCard.CardData))
                    {
                        SetHighlightAppearance();
                        Debug.Log($"✅ Can accept {draggableCard.CardData.cardName} - highlighting drop zone");
                    }
                    else
                    {
                        SetRejectAppearance();
                        Debug.Log($"❌ Cannot accept {draggableCard.CardData.cardName} - showing rejection");
                    }
                }
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            SetNormalAppearance();
        }
        
        public void AcceptCard(DraggableCard card)
        {
            if (!CanAcceptCard(card.CardData)) 
            {
                Debug.Log("AcceptCard failed - cannot accept card");
                return;
            }
            
            Debug.Log($"CardDropZone: Accepting card {card.CardData.cardName}");
            
            // Add to queued cards FIRST
            queuedCards.Add(card);
            
            // ABSOLUTE POSITION PRESERVATION: Store world position and size
            var rect = card.GetComponent<RectTransform>();
            Vector3 originalWorldPosition = rect.position;
            Vector2 originalSize = rect.rect.size;
            Vector2 originalSizeDelta = rect.sizeDelta;
            Vector3 originalScale = rect.localScale;
            
            Debug.Log($"🔒 BEFORE SetParent: WorldPos={originalWorldPosition}, Size={originalSize}, SizeDelta={originalSizeDelta}, Scale={originalScale}");
            
            // Set parent to the cardContainer
            Transform targetParent = cardContainer != null ? cardContainer.transform : this.transform;
            card.transform.SetParent(targetParent, true); // Keep world position
            
            // FORCE restore exact same visual properties
            rect.position = originalWorldPosition;
            rect.sizeDelta = originalSizeDelta;
            rect.localScale = originalScale;
            
            Debug.Log($"🔒 AFTER restore: WorldPos={rect.position}, Size={rect.rect.size}, SizeDelta={rect.sizeDelta}, Scale={rect.localScale}");
            
            // Enable dragging if in planning phase
            bool isPlanning = Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
            card.SetDraggingEnabled(isPlanning);
            
            // Notify listeners
            OnCardsChanged.Invoke(GetQueuedCardData());
            
            Debug.Log($"✅ Card {card.CardData.cardName} added to drop zone with EXACT same appearance");
        }
        
        private void PositionCardInDropZone(DraggableCard card, int index)
        {
            // Check if the card container has a Grid Layout Group
            var gridLayout = cardContainer.GetComponent<GridLayoutGroup>();
            
            if (gridLayout != null)
            {
                // Grid Layout Group will handle positioning automatically
                // Just make sure the card is properly parented - no manual positioning needed
                Debug.Log($"Card {card.CardData.cardName} added to grid at index {index} - Grid Layout will handle positioning");
                return;
            }
            
            // Fallback: Manual positioning if no Grid Layout Group
            var rectTransform = card.GetComponent<RectTransform>();
            
            // COMPLETELY PRESERVE the card's transform - don't change ANYTHING
            // Just change the X position for side-by-side layout
            float cardActualWidth = rectTransform.rect.width;
            if (cardActualWidth <= 0) cardActualWidth = cardWidth; // fallback
            
            float totalWidth = (queuedCards.Count * cardActualWidth) + ((queuedCards.Count - 1) * cardSpacing);
            float startX = -totalWidth / 2f + cardActualWidth / 2f;
            float xPos = startX + (index * (cardActualWidth + cardSpacing));
            
            // ONLY change X position, preserve Y position and everything else
            Vector2 currentPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(xPos, currentPos.y);
            
            Debug.Log($"Positioned card {card.CardData.cardName} manually at index {index}, position ({xPos}, {currentPos.y}) - preserved all other properties");
        }
        
        // REMOVED AddRemoveButton method - no X buttons needed
        
        public void RemoveCard(DraggableCard card)
        {
            if (queuedCards.Contains(card))
            {
                queuedCards.Remove(card);
                
                // Return to hand - let CardManager handle re-adding to hand
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.ReturnCardToHand(card);
                }
                else
                {
                    card.ReturnToHand();
                }
                
                // Reorganize remaining cards
                ReorganizeCards();
                
                OnCardsChanged.Invoke(GetQueuedCardData());
                Debug.Log($"Card {card.CardData.cardName} removed from queue");
            }
        }
        
        private void ReorganizeCards()
        {
            // Grid Layout Group handles organization automatically - no manual work needed
            Debug.Log("Grid Layout Group handling card reorganization automatically");
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
                if (card != null)
                {
                    // Remove remove button
                    var removeButton = card.transform.Find("RemoveButton");
                    if (removeButton != null)
                        Destroy(removeButton.gameObject);
                    
                    Destroy(card.gameObject);
                }
            }
            queuedCards.Clear();
            OnCardsChanged.Invoke(GetQueuedCardData());
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            Debug.Log($"CardDropZone phase changed to: {newPhase}");
            
            switch (newPhase)
            {
                case Core.GamePhase.Planning:
                    SetNormalAppearance();
                    // UNLOCK all cards in this drop zone for dragging
                    foreach (var card in queuedCards)
                    {
                        if (card != null)
                        {
                            card.SetDraggingEnabled(true);
                            Debug.Log($"🔓 Planning phase: Unlocked {card.CardData.cardName} in drop zone for dragging");
                        }
                    }
                    Debug.Log($"✅ Drop zone unlocked {queuedCards.Count} cards for planning phase");
                    break;
                    
                case Core.GamePhase.Implementation:
                    SetImplementationAppearance();
                    // LOCK all cards during implementation
                    foreach (var card in queuedCards)
                    {
                        if (card != null)
                        {
                            card.SetDraggingEnabled(false);
                            Debug.Log($"🔒 Implementation phase: Locked {card.CardData.cardName}");
                        }
                    }
                    // Auto-implement cards
                    if (queuedCards.Count > 0)
                    {
                        ImplementAllCards();
                    }
                    break;
                    
                case Core.GamePhase.Feedback:
                    SetNormalAppearance();
                    // LOCK all cards during feedback
                    foreach (var card in queuedCards)
                    {
                        if (card != null)
                        {
                            card.SetDraggingEnabled(false);
                        }
                    }
                    break;
                    
                case Core.GamePhase.Event:
                    SetNormalAppearance();
                    // LOCK all cards during event phase
                    foreach (var card in queuedCards)
                    {
                        if (card != null)
                        {
                            card.SetDraggingEnabled(false);
                        }
                    }
                    break;
            }
        }
        
        private void SetNormalAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }
        
        private void SetHighlightAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = highlightColor;
        }
        
        private void SetRejectAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = rejectColor;
        }
        
        private void SetAcceptAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = acceptColor;
            
            // Return to normal after a short delay
            Invoke(nameof(SetNormalAppearance), 0.5f);
        }
        
        private void SetImplementationAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = new Color(0.8f, 0.4f, 0.1f, 0.7f); // Orange
        }
        
        public List<CardData> GetQueuedCardData()
        {
            var cardDataList = new List<CardData>();
            foreach (var card in queuedCards)
            {
                if (card != null)
                    cardDataList.Add(card.CardData);
            }
            return cardDataList;
        }
        
        public List<DraggableCard> GetQueuedCards()
        {
            return new List<DraggableCard>(queuedCards);
        }
        
        public int GetQueuedCardCount() => queuedCards.Count;
        public bool HasQueuedCards() => queuedCards.Count > 0;
        
        [ContextMenu("Debug: Force Unlock All Cards in Drop Zone")]
        public void DebugForceUnlockAllCards()
        {
            Debug.Log($"=== FORCE UNLOCKING {queuedCards.Count} CARDS IN DROP ZONE ===");
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
                    bool wasDraggingEnabled = card.IsDraggingEnabled();
                    card.SetDraggingEnabled(true);
                    Debug.Log($"🔓 Force unlocked {card.CardData.cardName} (was: {wasDraggingEnabled}, now: {card.IsDraggingEnabled()})");
                }
            }
        }
        
        public void GetTotalQueuedCost(out int totalRP, out int totalCP)
        {
            totalRP = 0;
            totalCP = 0;
            
            foreach (var card in queuedCards)
            {
                if (card != null && card.CardData != null)
                {
                    totalRP += card.CardData.researchPointCost;
                    totalCP += card.CardData.communityPointCost;
                }
            }
        }
        
        [ContextMenu("Debug: Show Total Queued Cost")]
        public void DebugShowTotalCost()
        {
            GetTotalQueuedCost(out int totalRP, out int totalCP);
            var resourceManager = Core.ResourceManager.Instance;
            
            Debug.Log("=== QUEUED CARDS COST ANALYSIS ===");
            Debug.Log($"Cards in queue: {queuedCards.Count}");
            Debug.Log($"Total RP cost: {totalRP}");
            Debug.Log($"Total CP cost: {totalCP}");
            Debug.Log($"Current RP: {resourceManager?.ResearchPoints ?? 0}");
            Debug.Log($"Current CP: {resourceManager?.CommunityPoints ?? 0}");
            Debug.Log($"Can afford all: {resourceManager?.CanSpend(totalRP, totalCP) ?? false}");
            
            for (int i = 0; i < queuedCards.Count; i++)
            {
                var card = queuedCards[i];
                if (card != null)
                {
                    Debug.Log($"  [{i}] {card.CardData.cardName}: {card.CardData.researchPointCost} RP, {card.CardData.communityPointCost} CP");
                }
            }
        }
        
        [ContextMenu("Debug: Check All Cards in Drop Zone")]
        public void DebugCheckAllCards()
        {
            Debug.Log($"=== DROP ZONE CARD STATUS ({queuedCards.Count} cards) ===");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            
            for (int i = 0; i < queuedCards.Count; i++)
            {
                var card = queuedCards[i];
                if (card != null)
                {
                    Debug.Log($"  [{i}] {card.CardData.cardName} - Dragging: {card.IsDraggingEnabled()} - Parent: {card.transform.parent.name}");
                }
                else
                {
                    Debug.Log($"  [{i}] NULL CARD");
                }
            }
        }
        
        private void OnDestroy()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
        }
    }
}