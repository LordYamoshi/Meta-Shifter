using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Makes cards draggable from hand to center area
    /// </summary>
    public class DraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Card Data")] [SerializeField] private CardData cardData;

        [Header("Drag Settings")] [SerializeField]
        private Canvas canvas;

        [SerializeField] private GraphicRaycaster raycaster;

        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private Image cardBackgroundImage;
        [SerializeField] private Image cardArtImage;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector3 originalPosition;
        private Transform originalParent;
        private bool isDragging = false;

        public CardData CardData => cardData;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        public void SetCardData(CardData data)
        {
            cardData = data;
            UpdateCardDisplay();
        }

        /// <summary>
        /// Enhanced card display update that handles multiple ways to find UI elements
        /// </summary>
        private void UpdateCardDisplay()
        {
            if (cardData == null) return;
            
            UpdateCardName();
            UpdateDescription();
            UpdateCost();
            UpdateCardType();
            UpdateCardArt();
            UpdateCardBackground();

            Debug.Log($"Updated card display for: {cardData.cardName}");
        }

        private void UpdateCardName()
        {
            var nameText = cardNameText ??
                           transform.Find("CardName")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Name")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Title")?.GetComponent<TextMeshProUGUI>();

            if (nameText != null)
            {
                nameText.text = cardData.cardName;
                nameText.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"Could not find card name text component on {gameObject.name}");
            }
        }

        private void UpdateDescription()
        {
            var descText = descriptionText ??
                           transform.Find("Description")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Effect")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            if (descText != null)
            {
                descText.text = cardData.description;
                descText.color = new Color(0.9f, 0.9f, 0.9f);
            }
        }

        private void UpdateCost()
        {
            var costText = this.costText ??
                           transform.Find("Cost")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Resources")?.GetComponent<TextMeshProUGUI>();

            if (costText != null)
            {
                string costString = "";

                if (cardData.researchPointCost > 0)
                    costString += $"{cardData.researchPointCost} RP";

                if (cardData.communityPointCost > 0)
                {
                    if (costString.Length > 0) costString += " ";
                    costString += $"{cardData.communityPointCost} CP";
                }

                if (costString.Length == 0)
                    costString = "Free";

                costText.text = costString;
                costText.color = CanAffordCard() ? Color.white : Color.red;
            }
        }

        private void UpdateCardType()
        {
            var typeText = cardTypeText ??
                           transform.Find("CardType")?.GetComponent<TextMeshProUGUI>() ??
                           transform.Find("Type")?.GetComponent<TextMeshProUGUI>();

            if (typeText != null)
            {
                typeText.text = FormatCardType(cardData.cardType);
                typeText.color = GetCardTypeColor(cardData.cardType);
            }
        }

        private void UpdateCardArt()
        {
            var artImage = cardArtImage ??
                           transform.Find("CardArt")?.GetComponent<Image>() ??
                           transform.Find("Art")?.GetComponent<Image>() ??
                           transform.Find("Icon")?.GetComponent<Image>();

            if (artImage != null && cardData.cardArt != null)
            {
                artImage.sprite = cardData.cardArt;
                artImage.color = Color.white;
            }
        }

        private void UpdateCardBackground()
        {
            var bgImage = cardBackgroundImage ?? GetComponent<Image>();

            if (bgImage != null)
            {
                // Set background color based on rarity
                bgImage.color = GetRarityColor(cardData.rarity);
            }
        }

        private bool CanAffordCard()
        {
            var resourceManager = Core.ResourceManager.Instance;
            return resourceManager != null &&
                   resourceManager.CanSpend(cardData.researchPointCost, cardData.communityPointCost);
        }

        private string FormatCardType(CardType cardType)
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
                CardRarity.Common => new Color(0.8f, 0.8f, 0.8f, 0.9f), // Light gray
                CardRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f, 0.9f), // Green
                CardRarity.Rare => new Color(0.2f, 0.2f, 1f, 0.9f), // Blue
                CardRarity.Epic => new Color(0.8f, 0.2f, 0.8f, 0.9f), // Purple
                CardRarity.Special => new Color(1f, 0.8f, 0.2f, 0.9f), // Gold
                _ => new Color(1f, 1f, 1f, 0.9f)
            };
        }

        /// <summary>
        /// Call this to refresh the card display (useful when resources change)
        /// </summary>
        public void RefreshDisplay()
        {
            UpdateCardDisplay();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only allow dragging in Planning phase
            if (Core.PhaseManager.Instance?.GetCurrentPhase() != Core.GamePhase.Planning)
                return;
            
            isDragging = true;
            originalPosition = rectTransform.position;
            originalParent = transform.parent;
            
            // Make card semi-transparent while dragging
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            
            // Bring card to front
            transform.SetParent(canvas.transform, true);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // Move card with mouse/touch
            rectTransform.position = eventData.position;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Check if dropped on valid drop zone
            var dropZone = GetDropZoneUnderPointer(eventData);
            
            if (dropZone != null && dropZone.CanAcceptCard(cardData))
            {
                // Card accepted by drop zone
                dropZone.AcceptCard(this);
            }
            else
            {
                // Return to original position
                transform.SetParent(originalParent, true);
                rectTransform.position = originalPosition;
            }
        }
        
        private CardDropZone GetDropZoneUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                var dropZone = result.gameObject.GetComponent<CardDropZone>();
                if (dropZone != null)
                    return dropZone;
            }
            
            return null;
        }
        
        public void ReturnToHand()
        {
            transform.SetParent(originalParent, true);
            rectTransform.position = originalPosition;
        }
    }
}