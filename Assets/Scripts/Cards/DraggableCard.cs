using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Simplified DraggableCard without position preservation complexity
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

        // Drag and Drop Implementation
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!draggingEnabled) return;
            
            if (Core.PhaseManager.Instance?.GetCurrentPhase() != Core.GamePhase.Planning)
                return;
            
            bool isInDropZone = IsCardInDropZone();
            if (!isInDropZone && !CanAffordCard())
                return;
            
            isDragging = true;
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            
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
            
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Check what happened
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
            transform.SetParent(originalParent, false);
            rectTransform.position = originalPosition;
        }
        
        private void RemoveFromDropZone()
        {
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
            
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.ReturnCardToHand(this);
            }
        }
        
        public void ReturnToHand()
        {
            if (originalParent != null)
            {
                transform.SetParent(originalParent, false);
                draggingEnabled = true;
            }
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
            Debug.Log($"IsInDropZone: {IsCardInDropZone()}");
            Debug.Log($"CanAffordCard: {CanAffordCard()}");
        }
    }
}