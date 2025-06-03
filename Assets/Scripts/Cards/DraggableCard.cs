using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Fixed DraggableCard with proper drop zone cleanup
    /// </summary>
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Card Data")]
        [SerializeField] private CardData cardData;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI rpCostText;
        [SerializeField] private TextMeshProUGUI cpCostText;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private Image rarityIndicator;
        
        [Header("Drag Settings")]
        [SerializeField] private Canvas canvas;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;
        private bool draggingEnabled = true;
        
        // Track which drop zone we were in (if any)
        private CardDropZone currentDropZone = null;

        public CardData CardData => cardData;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
                
            AutoFindUIComponents();
        }
        
        private void Start()
        {
            if (cardData != null)
            {
                UpdateCardDisplay();
            }
            
            // Initialize current drop zone reference
            UpdateCurrentDropZoneReference();
        }

        private void AutoFindUIComponents()
        {
            if (cardNameText == null)
                cardNameText = transform.Find("CardName")?.GetComponent<TextMeshProUGUI>();
            if (descriptionText == null)
                descriptionText = transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (cardTypeText == null)
                cardTypeText = transform.Find("CardType")?.GetComponent<TextMeshProUGUI>();
            if (rpCostText == null)
                rpCostText = transform.Find("Costs/RP")?.GetComponent<TextMeshProUGUI>();
            if (cpCostText == null)
                cpCostText = transform.Find("Costs/CP")?.GetComponent<TextMeshProUGUI>();
            if (cardBackgroundImage == null)
                cardBackgroundImage = GetComponent<Image>();
        }

        public void SetCardData(CardData data)
        {
            cardData = data;
            UpdateCardDisplay();
        }

        public void SetDraggingEnabled(bool enabled)
        {
            draggingEnabled = enabled;
        }
        
        public bool IsDraggingEnabled()
        {
            return draggingEnabled;
        }

        private void UpdateCardDisplay()
        {
            if (cardData == null) return;
            
            // Update basic info
            if (cardNameText != null)
                cardNameText.text = cardData.cardName;
            
            if (descriptionText != null)
                descriptionText.text = cardData.description;
            
            // Update card type with color
            if (cardTypeText != null)
            {
                cardTypeText.text = GetCardTypeDisplayName(cardData.cardType);
                cardTypeText.color = GetCardTypeColor(cardData.cardType);
            }
            
            // Update costs
            UpdateCostDisplay();
            
            // Update rarity (only affects dedicated rarity indicator)
            UpdateRarityDisplay();
            
            // Update affordability
            UpdateAffordabilityDisplay();
        }

        private void UpdateCostDisplay()
        {
            if (cardData == null) return;
            
            // RP cost
            if (rpCostText != null)
            {
                if (cardData.researchPointCost > 0)
                {
                    rpCostText.text = $"{cardData.researchPointCost} RP";
                    rpCostText.gameObject.SetActive(true);
                }
                else
                {
                    rpCostText.gameObject.SetActive(false);
                }
            }
            
            // CP cost
            if (cpCostText != null)
            {
                if (cardData.communityPointCost > 0)
                {
                    cpCostText.text = $"{cardData.communityPointCost} CP";
                    cpCostText.gameObject.SetActive(true);
                }
                else
                {
                    cpCostText.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateRarityDisplay()
        {
            if (cardData == null) return;
            
            Color rarityColor = GetRarityColor(cardData.rarity);
            
            // Only update dedicated rarity indicator
            if (rarityIndicator != null)
            {
                rarityIndicator.color = rarityColor;
            }
        }

        private void UpdateAffordabilityDisplay()
        {
            if (cardData == null) return;
            
            bool canAfford = CanAffordCard();
            var resourceManager = Core.ResourceManager.Instance;
            
            // Transparency for affordability
            if (canvasGroup != null)
            {
                canvasGroup.alpha = canAfford ? 1f : 0.6f;
            }
            
            // Cost text colors
            if (resourceManager != null)
            {
                if (rpCostText != null && cardData.researchPointCost > 0)
                {
                    rpCostText.color = resourceManager.ResearchPoints >= cardData.researchPointCost ? Color.black : Color.red;
                }
                
                if (cpCostText != null && cardData.communityPointCost > 0)
                {
                    cpCostText.color = resourceManager.CommunityPoints >= cardData.communityPointCost ? Color.black : Color.red;
                }
            }
        }

        private bool CanAffordCard()
        {
            if (cardData == null) return false;
            
            var resourceManager = Core.ResourceManager.Instance;
            return resourceManager != null &&
                   resourceManager.CanSpend(cardData.researchPointCost, cardData.communityPointCost);
        }

        private string GetCardTypeDisplayName(CardType cardType)
        {
            return cardType switch
            {
                CardType.BalanceChange => "Balance",
                CardType.MetaShift => "Meta Shift",
                CardType.Community => "Community",
                CardType.CrisisResponse => "Crisis",
                CardType.Special => "Special",
                _ => cardType.ToString()
            };
        }

        private Color GetCardTypeColor(CardType cardType)
        {
            return cardType switch
            {
                CardType.BalanceChange => new Color(1f, 0.6f, 0.2f), // Orange
                CardType.MetaShift => new Color(0.2f, 0.8f, 0.8f), // Teal
                CardType.Community => new Color(0.8f, 0.2f, 0.8f), // Purple
                CardType.CrisisResponse => new Color(0.8f, 0.2f, 0.2f), // Red
                CardType.Special => new Color(1f, 0.8f, 0.2f), // Gold
                _ => Color.white
            };
        }

        private Color GetRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => new Color(0.7f, 0.7f, 0.7f), // Gray
                CardRarity.Uncommon => new Color(0.0f, 1f, 0.0f), // Green
                CardRarity.Rare => new Color(0.0f, 0.5f, 1f), // Blue
                CardRarity.Epic => new Color(0.8f, 0.0f, 1f), // Purple
                CardRarity.Special => new Color(1f, 0.8f, 0.0f), // Gold
                _ => Color.white
            };
        }

        public void RefreshDisplay()
        {
            UpdateCardDisplay();
        }
        
        public void RefreshAffordabilityOnly()
        {
            if (cardData == null) return;
            UpdateAffordabilityDisplay();
        }

        /// <summary>
        /// Update our reference to which drop zone we're currently in (if any)
        /// </summary>
        private void UpdateCurrentDropZoneReference()
        {
            currentDropZone = GetParentDropZone();
            
            if (currentDropZone != null)
            {
                Debug.Log($"🎯 {cardData?.cardName} is now in drop zone: {currentDropZone.name}");
            }
            else
            {
                Debug.Log($"🏠 {cardData?.cardName} is now in hand/other location");
            }
        }

        /// <summary>
        /// Get the drop zone that contains this card (if any)
        /// </summary>
        private CardDropZone GetParentDropZone()
        {
            Transform current = transform.parent;
            while (current != null)
            {
                var dropZone = current.GetComponent<CardDropZone>();
                if (dropZone != null)
                    return dropZone;
                current = current.parent;
            }
            return null;
        }

        // Drag and Drop Implementation
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!draggingEnabled) 
            {
                Debug.Log($"❌ Drag blocked: dragging disabled for {cardData?.cardName}");
                return;
            }
            
            if (Core.PhaseManager.Instance?.GetCurrentPhase() != Core.GamePhase.Planning)
            {
                Debug.Log($"❌ Drag blocked: not in planning phase for {cardData?.cardName}");
                return;
            }
            
            // Store current drop zone reference
            CardDropZone startingDropZone = GetParentDropZone();
            bool isInDropZone = startingDropZone != null;
            
            // Check affordability only if we're starting from hand (not drop zone)
            if (!isInDropZone && !CanAffordCard())
            {
                Debug.Log($"❌ Drag blocked: cannot afford {cardData?.cardName}");
                return;
            }
            
            Debug.Log($"🚀 Starting drag for {cardData?.cardName} from {(isInDropZone ? "drop zone" : "hand")}");
            
            isDragging = true;
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            currentDropZone = startingDropZone; // Store reference to starting drop zone
            
            // Visual feedback during drag
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            
            // Bring to front
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            rectTransform.position = eventData.position;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            Debug.Log($"🎯 Ending drag for {cardData?.cardName}");
            
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Determine what happened during the drag
            CardDropZone startingDropZone = currentDropZone; // Where we started
            CardDropZone newDropZone = GetParentDropZone(); // Where we ended up
            bool isInHand = transform.parent.GetComponent<HandDropZone>() != null;
            bool droppedOnCanvas = transform.parent == canvas.transform;
            
            Debug.Log($"🔍 Drag analysis for {cardData?.cardName}:");
            Debug.Log($"  - Started from: {(startingDropZone != null ? startingDropZone.name : "hand")}");
            Debug.Log($"  - Ended in: {(newDropZone != null ? newDropZone.name : (isInHand ? "hand" : "canvas/other"))}");
            Debug.Log($"  - Dropped on canvas: {droppedOnCanvas}");
            
            if (droppedOnCanvas)
            {
                // Card was dropped somewhere invalid - return to original location
                Debug.Log($"🔄 Returning {cardData?.cardName} to original position");
                ReturnToOriginalPosition();
            }
            else if (startingDropZone != null && newDropZone == null && isInHand)
            {
                // CRITICAL: Card moved from drop zone to hand - must remove from queue!
                Debug.Log($"🚨 CRITICAL: {cardData?.cardName} moved from drop zone to hand - removing from queue");
                
                // Remove from the original drop zone's queue
                startingDropZone.ForceRemoveCardFromQueue(this);
                
                // Ensure CardManager knows this card is back in hand
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.ReturnCardToHand(this);
                }
                
                // Update our drop zone reference
                currentDropZone = null;
                
                Debug.Log($"✅ {cardData?.cardName} successfully unqueued and returned to hand");
            }
            else if (startingDropZone == null && newDropZone != null)
            {
                // Card moved from hand to drop zone
                Debug.Log($"📥 {cardData?.cardName} moved from hand to drop zone");
                
                // Remove from hand manager
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.RemoveCardFromHand(this);
                }
                
                // Update our drop zone reference
                currentDropZone = newDropZone;
            }
            else if (startingDropZone != null && newDropZone != null && startingDropZone != newDropZone)
            {
                // Card moved from one drop zone to another
                Debug.Log($"🔄 {cardData?.cardName} moved between drop zones");
                
                // Remove from old drop zone
                startingDropZone.ForceRemoveCardFromQueue(this);
                
                // The new drop zone should handle adding it via OnDrop
                currentDropZone = newDropZone;
            }
            
            // Final verification
            VerifyCardState();
        }
        
        /// <summary>
        /// Verify that the card's state is consistent after drag operations
        /// </summary>
        private void VerifyCardState()
        {
            CardDropZone actualDropZone = GetParentDropZone();
            bool isInHand = transform.parent.GetComponent<HandDropZone>() != null;
            
            // Update our reference to match reality
            currentDropZone = actualDropZone;
            
            Debug.Log($"🔍 Final state verification for {cardData?.cardName}:");
            Debug.Log($"  - Actually in drop zone: {actualDropZone?.name ?? "none"}");
            Debug.Log($"  - Is in hand zone: {isInHand}");
            Debug.Log($"  - Parent: {transform.parent?.name ?? "null"}");
            
            // If we're in a drop zone, make sure we're in its queue
            if (actualDropZone != null && !actualDropZone.IsCardInQueue(this))
            {
                Debug.LogWarning($"⚠️ {cardData?.cardName} is visually in drop zone but not in queue - this may cause issues");
            }
            
            // If we're not in a drop zone, make sure no drop zone has us in their queue
            if (actualDropZone == null)
            {
                var allDropZones = FindObjectsOfType<CardDropZone>();
                foreach (var dropZone in allDropZones)
                {
                    if (dropZone.IsCardInQueue(this))
                    {
                        Debug.LogWarning($"⚠️ {cardData?.cardName} is not in drop zone but {dropZone.name} still has it queued - fixing");
                        dropZone.ForceRemoveCardFromQueue(this);
                    }
                }
            }
        }
        
        private bool IsCardInDropZone()
        {
            return GetParentDropZone() != null;
        }
        
        private bool IsCardInDropZone(Transform parent)
        {
            if (parent == null) return false;
            
            Transform current = parent;
            while (current != null)
            {
                if (current.GetComponent<CardDropZone>() != null)
                    return true;
                current = current.parent;
            }
            
            return false;
        }
        
        private bool IsCardInHand()
        {
            return transform.parent.GetComponent<HandDropZone>() != null;
        }
        
        private void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent, false);
            rectTransform.position = originalPosition;
            
            // Restore our drop zone reference to what it was
            currentDropZone = GetParentDropZone();
        }
        
        public void ReturnToHand()
        {
            // Find the hand drop zone
            var handDropZone = FindObjectOfType<HandDropZone>();
            if (handDropZone != null)
            {
                transform.SetParent(handDropZone.transform, false);
            }
            else if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
            }
            
            draggingEnabled = true;
            currentDropZone = null;
            
            Debug.Log($"🏠 {cardData?.cardName} returned to hand");
        }

        // Debug Methods
        [ContextMenu("Debug: Force Enable Dragging")]
        public void DebugForceEnableDragging()
        {
            draggingEnabled = true;
            Debug.Log($"🔓 FORCE ENABLED dragging for {cardData?.cardName}");
        }

        [ContextMenu("Debug: Test Drag Conditions")]
        public void DebugTestDragConditions()
        {
            Debug.Log("=== DRAG CONDITIONS TEST ===");
            Debug.Log($"Card: {cardData?.cardName ?? "NULL"}");
            Debug.Log($"draggingEnabled: {draggingEnabled}");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"Is Planning Phase: {Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning}");
            Debug.Log($"Parent: {transform.parent?.name ?? "NULL"}");
            Debug.Log($"Current Drop Zone: {currentDropZone?.name ?? "none"}");
            Debug.Log($"IsInDropZone: {IsCardInDropZone()}");
            Debug.Log($"CanAffordCard: {CanAffordCard()}");
        }

        [ContextMenu("Debug: Verify Card State")]
        public void DebugVerifyCardState()
        {
            VerifyCardState();
        }

        [ContextMenu("Debug: Force Update Drop Zone Reference")]
        public void DebugForceUpdateDropZoneReference()
        {
            CardDropZone oldRef = currentDropZone;
            UpdateCurrentDropZoneReference();
            Debug.Log($"🔄 Updated drop zone reference: {oldRef?.name ?? "null"} → {currentDropZone?.name ?? "null"}");
        }
    }
}