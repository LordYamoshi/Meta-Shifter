using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Clean DraggableCard with proper initialization and cost display
    /// </summary>
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Card Data")]
        [SerializeField] private CardData cardData;

        [Header("UI References - Link these to your prefab components")]
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

        public CardData CardData => cardData;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
                
            // Auto-find UI components if not assigned
            AutoFindUIComponents();
        }
        
        private void Start()
        {
            // Initialize the card display when the object starts
            if (cardData != null)
            {
                UpdateCardDisplay();
            }
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
            Debug.Log($"SetCardData called for {data.cardName}: RP={data.researchPointCost}, CP={data.communityPointCost}");
            
            // Force immediate update
            UpdateCardDisplay();
            
            // Also force update on next frame to ensure UI components are ready
            Invoke(nameof(ForceDisplayUpdate), 0.1f);
        }
        
        private void ForceDisplayUpdate()
        {
            if (cardData != null)
            {
                Debug.Log($"ForceDisplayUpdate for {cardData.cardName}");
                UpdateCardDisplay();
            }
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
            
            // Update card name
            if (cardNameText != null)
                cardNameText.text = cardData.cardName;
            
            // Update description
            if (descriptionText != null)
                descriptionText.text = cardData.description;
            
            // Update card type
            if (cardTypeText != null)
            {
                cardTypeText.text = GetCardTypeDisplayName(cardData.cardType);
                cardTypeText.color = GetCardTypeColor(cardData.cardType);
            }
            
            // Update costs - ALWAYS from CardData
            UpdateCostDisplay();
            
            // Update rarity color
            UpdateRarityDisplay();
            
            // Update affordability
            UpdateAffordabilityDisplay();
        }

        private void UpdateCostDisplay()
        {
            if (cardData == null) 
            {
                Debug.LogWarning("UpdateCostDisplay called but cardData is null!");
                return;
            }
            
            Debug.Log($"UpdateCostDisplay for {cardData.cardName}: RP={cardData.researchPointCost}, CP={cardData.communityPointCost}");
            
            // Update RP cost (left aligned) - ALWAYS from CardData
            if (rpCostText != null)
            {
                if (cardData.researchPointCost > 0)
                {
                    rpCostText.text = $"{cardData.researchPointCost} RP";
                    rpCostText.color = Color.black; // Set default color to black
                    rpCostText.gameObject.SetActive(true);
                    Debug.Log($"Set RP text to: '{rpCostText.text}' with black color");
                }
                else
                {
                    rpCostText.gameObject.SetActive(false);
                    Debug.Log("Hidden RP cost (cost is 0)");
                }
            }
            else
            {
                Debug.LogWarning("rpCostText is null!");
            }
            
            // Update CP cost (right aligned) - ALWAYS from CardData
            if (cpCostText != null)
            {
                if (cardData.communityPointCost > 0)
                {
                    cpCostText.text = $"{cardData.communityPointCost} CP";
                    cpCostText.color = Color.black; // Set default color to black
                    cpCostText.gameObject.SetActive(true);
                    Debug.Log($"Set CP text to: '{cpCostText.text}' with black color");
                }
                else
                {
                    cpCostText.gameObject.SetActive(false);
                    Debug.Log("Hidden CP cost (cost is 0)");
                }
            }
            else
            {
                Debug.LogWarning("cpCostText is null!");
            }
        }

        private void UpdateRarityDisplay()
        {
            if (cardData == null) return;
            
            Color rarityColor = GetRarityColor(cardData.rarity);
            
            if (rarityIndicator != null)
            {
                rarityIndicator.color = rarityColor;
            }
            else if (cardBackgroundImage != null)
            {
                cardBackgroundImage.color = Color.Lerp(Color.white, rarityColor, 0.3f);
            }
        }

        private void UpdateAffordabilityDisplay()
        {
            if (cardData == null) return;
            
            bool canAfford = CanAffordCard();
            var resourceManager = Core.ResourceManager.Instance;
            
            Debug.Log($"💰 {cardData.cardName}: RP cost={cardData.researchPointCost}, CP cost={cardData.communityPointCost}, Player RP={resourceManager?.ResearchPoints}, Player CP={resourceManager?.CommunityPoints}, CanAfford={canAfford}");
            
            // Make card semi-transparent if can't afford
            if (canvasGroup != null)
            {
                canvasGroup.alpha = canAfford ? 1f : 0.6f;
            }
            
            // Update cost text colors - ONLY colors, never text content
            if (resourceManager != null)
            {
                // RP cost color - black when affordable, red when not
                if (rpCostText != null && cardData.researchPointCost > 0)
                {
                    Color newColor = resourceManager.ResearchPoints >= cardData.researchPointCost ? Color.black : Color.red;
                    rpCostText.color = newColor;
                    Debug.Log($"   RP text color set to: {(newColor == Color.black ? "BLACK" : "RED")}");
                }
                
                // CP cost color - black when affordable, red when not
                if (cpCostText != null && cardData.communityPointCost > 0)
                {
                    Color newColor = resourceManager.CommunityPoints >= cardData.communityPointCost ? Color.black : Color.red;
                    cpCostText.color = newColor;
                    Debug.Log($"   CP text color set to: {(newColor == Color.black ? "BLACK" : "RED")}");
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
            
            Debug.Log($"🔄 Refreshing affordability for {cardData.cardName}");
            UpdateAffordabilityDisplay();
        }
        
        [ContextMenu("Debug: Test Drag Conditions")]
        public void DebugTestDragConditions()
        {
            Debug.Log("=== DRAG CONDITIONS TEST ===");
            Debug.Log($"Card: {cardData?.cardName ?? "NULL"}");
            Debug.Log($"1. draggingEnabled: {draggingEnabled}");
            Debug.Log($"2. Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"3. Is Planning Phase: {Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning}");
            Debug.Log($"4. Parent: {transform.parent?.name ?? "NULL"}");
            Debug.Log($"5. IsInDropZone: {IsCardInDropZone()}");
            Debug.Log($"6. CanAffordCard: {CanAffordCard()}");
            Debug.Log($"7. Should be draggable: {draggingEnabled && Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning && (IsCardInDropZone() || CanAffordCard())}");
        }
        
        [ContextMenu("Debug: Force Enable Dragging")]
        public void DebugForceEnableDragging()
        {
            draggingEnabled = true;
            Debug.Log($"🔓 FORCE ENABLED dragging for {cardData?.cardName}");
        }

        // Drag and Drop Implementation
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"🔍 OnBeginDrag called for {cardData?.cardName ?? "unknown card"}");
            Debug.Log($"   - draggingEnabled: {draggingEnabled}");
            Debug.Log($"   - Current phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"   - Parent: {transform.parent?.name ?? "null"}");
            
            if (!draggingEnabled) 
            {
                Debug.LogError($"❌ DRAGGING BLOCKED: draggingEnabled = false for {cardData?.cardName}");
                return;
            }
            
            // Only allow dragging in Planning phase
            if (Core.PhaseManager.Instance?.GetCurrentPhase() != Core.GamePhase.Planning)
            {
                Debug.LogError($"❌ DRAGGING BLOCKED: Not in planning phase (current: {Core.PhaseManager.Instance?.GetCurrentPhase()})");
                return;
            }
            
            // Only check affordability if card is in hand
            bool isInDropZone = IsCardInDropZone();
            Debug.Log($"   - isInDropZone: {isInDropZone}");
            Debug.Log($"   - Can afford: {CanAffordCard()}");
            
            if (!isInDropZone && !CanAffordCard())
            {
                Debug.LogError($"❌ DRAGGING BLOCKED: Cannot afford card {cardData?.cardName}");
                return;
            }
            
            isDragging = true;
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            
            // STORE COMPLETE ORIGINAL TRANSFORM STATE
            StoreOriginalTransformState();
            
            string location = isInDropZone ? "drop zone" : "hand";
            Debug.Log($"✅ SUCCESS: Started dragging {cardData?.cardName} from {location}");
            
            // Visual feedback during drag - ONLY change opacity and raycast blocking
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            
            // Bring to front for dragging - use worldPositionStays to maintain position
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
            
            // RESTORE original transform state after parent change
            RestoreOriginalTransformState();
        }
        
        private Vector2 storedAnchoredPos;
        private Vector2 storedAnchorMin;
        private Vector2 storedAnchorMax;
        private Vector2 storedPivot;
        private Vector2 storedSizeDelta;
        private Vector3 storedScale;
        private Quaternion storedRotation;
        private Vector2 storedOffsetMin;
        private Vector2 storedOffsetMax;
        
        private void StoreOriginalTransformState()
        {
            storedAnchoredPos = rectTransform.anchoredPosition;
            storedAnchorMin = rectTransform.anchorMin;
            storedAnchorMax = rectTransform.anchorMax;
            storedPivot = rectTransform.pivot;
            storedSizeDelta = rectTransform.sizeDelta;
            storedScale = rectTransform.localScale;
            storedRotation = rectTransform.localRotation;
            storedOffsetMin = rectTransform.offsetMin;
            storedOffsetMax = rectTransform.offsetMax;
        }
        
        private void RestoreOriginalTransformState()
        {
            rectTransform.anchorMin = storedAnchorMin;
            rectTransform.anchorMax = storedAnchorMax;
            rectTransform.pivot = storedPivot;
            rectTransform.sizeDelta = storedSizeDelta;
            rectTransform.localScale = storedScale;
            rectTransform.localRotation = storedRotation;
            rectTransform.offsetMin = storedOffsetMin;
            rectTransform.offsetMax = storedOffsetMax;
            rectTransform.anchoredPosition = storedAnchoredPos;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            rectTransform.position = eventData.position;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Check what we were dragged from and to
            bool wasInDropZone = IsCardInDropZone(originalParent);
            bool isInDropZone = transform.parent != canvas.transform && IsCardInDropZone();
            bool isInHand = transform.parent != canvas.transform && IsCardInHand();
            
            if (transform.parent == canvas.transform)
            {
                // Not accepted, return to original location
                ReturnToOriginalPosition();
            }
            else if (wasInDropZone && isInHand)
            {
                // Moved from drop zone to hand
                RemoveFromDropZone();
            }
            else if (!wasInDropZone && isInDropZone)
            {
                // Moved from hand to drop zone
                if (Cards.CardManager.Instance != null)
                {
                    Cards.CardManager.Instance.RemoveCardFromHand(this);
                }
            }
        }
        
        private bool IsCardInDropZone()
        {
            return IsCardInDropZone(transform.parent);
        }
        
        private bool IsCardInDropZone(Transform parent)
        {
            if (parent == null) return false;
            
            // Check parent hierarchy for CardDropZone
            Transform current = parent;
            while (current != null)
            {
                if (current.GetComponent<Cards.CardDropZone>() != null)
                    return true;
                current = current.parent;
            }
            
            return false;
        }
        
        private bool IsCardInHand()
        {
            return transform.parent.GetComponent<Cards.HandDropZone>() != null;
        }
        
        private void ReturnToOriginalPosition()
        {
            // Store current transform properties to preserve them
            var rect = GetComponent<RectTransform>();
            Vector2 currentAnchoredPos = rect.anchoredPosition;
            Vector2 currentAnchorMin = rect.anchorMin;
            Vector2 currentAnchorMax = rect.anchorMax;
            Vector2 currentPivot = rect.pivot;
            Vector2 currentSizeDelta = rect.sizeDelta;
            Vector3 currentScale = rect.localScale;
            Quaternion currentRotation = rect.localRotation;
            Vector2 currentOffsetMin = rect.offsetMin;
            Vector2 currentOffsetMax = rect.offsetMax;
            
            // Return to original parent
            transform.SetParent(originalParent, true); // Keep world position
            
            // Restore ALL transform properties to maintain exact same appearance
            rect.anchorMin = currentAnchorMin;
            rect.anchorMax = currentAnchorMax;
            rect.pivot = currentPivot;
            rect.sizeDelta = currentSizeDelta;
            rect.localScale = currentScale;
            rect.localRotation = currentRotation;
            rect.offsetMin = currentOffsetMin;
            rect.offsetMax = currentOffsetMax;
            rect.anchoredPosition = currentAnchoredPos;
            rect.position = originalPosition; // Restore exact world position
            
            Debug.Log($"✅ Returned {cardData?.cardName} to original position with EXACT same appearance preserved");
        }
        
        private void RemoveFromDropZone()
        {
            // Find drop zone and remove card
            Transform current = originalParent;
            while (current != null)
            {
                var dropZone = current.GetComponent<Cards.CardDropZone>();
                if (dropZone != null)
                {
                    dropZone.RemoveCard(this);
                    return;
                }
                current = current.parent;
            }
            
            // Fallback
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.ReturnCardToHand(this);
            }
        }
        
        public void ReturnToHand()
        {
            if (originalParent != null)
            {
                // Store current transform properties to preserve them
                var rect = GetComponent<RectTransform>();
                Vector2 currentAnchoredPos = rect.anchoredPosition;
                Vector2 currentAnchorMin = rect.anchorMin;
                Vector2 currentAnchorMax = rect.anchorMax;
                Vector2 currentPivot = rect.pivot;
                Vector2 currentSizeDelta = rect.sizeDelta;
                Vector3 currentScale = rect.localScale;
                Quaternion currentRotation = rect.localRotation;
                Vector2 currentOffsetMin = rect.offsetMin;
                Vector2 currentOffsetMax = rect.offsetMax;
                Vector3 currentWorldPosition = rect.position;
                
                // Return to original parent
                transform.SetParent(originalParent, true); // Keep world position
                
                // Restore ALL transform properties to maintain exact same appearance
                rect.anchorMin = currentAnchorMin;
                rect.anchorMax = currentAnchorMax;
                rect.pivot = currentPivot;
                rect.sizeDelta = currentSizeDelta;
                rect.localScale = currentScale;
                rect.localRotation = currentRotation;
                rect.offsetMin = currentOffsetMin;
                rect.offsetMax = currentOffsetMax;
                rect.anchoredPosition = currentAnchoredPos;
                rect.position = currentWorldPosition; // Restore exact world position
                
                draggingEnabled = true;
                
                Debug.Log($"✅ Returned {cardData?.cardName} to hand with EXACT same appearance preserved");
            }
        }
    }
}