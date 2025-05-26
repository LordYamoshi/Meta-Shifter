using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Visual representation of a card in the 2.5D space
    /// </summary>
    public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler, IEndDragHandler
    {
        [Header("Card Components")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image cardArtImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image frameImage;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Image typeIconImage;
        
        [Header("Card Type Icons")]
        [SerializeField] private Sprite balanceChangeIcon;
        [SerializeField] private Sprite metaShiftIcon;
        [SerializeField] private Sprite communityIcon;
        [SerializeField] private Sprite crisisResponseIcon;
        [SerializeField] private Sprite specialIcon;
        
        [Header("Card Frame Colors")]
        [SerializeField] private Color commonFrameColor = Color.grey;
        [SerializeField] private Color uncommonFrameColor = Color.green;
        [SerializeField] private Color rareFrameColor = Color.blue;
        [SerializeField] private Color epicFrameColor = Color.magenta;
        [SerializeField] private Color specialFrameColor = Color.yellow;
        
        [Header("Card Type Colors")]
        [SerializeField] private Color balanceColor = new Color(1.0f, 0.6f, 0.2f); // Orange
        [SerializeField] private Color metaShiftColor = new Color(0.2f, 0.8f, 0.8f); // Teal
        [SerializeField] private Color communityColor = new Color(0.8f, 0.2f, 0.8f); // Purple
        [SerializeField] private Color crisisColor = new Color(0.8f, 0.2f, 0.2f); // Red
        [SerializeField] private Color specialColor = new Color(1.0f, 0.8f, 0.2f); // Gold
        
        [Header("Animation")]
        [SerializeField] private float hoverHeight = 0.5f;
        [SerializeField] private float hoverScale = 1.2f;
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private Ease animationEase = Ease.OutQuint;
        
        // References
        private CardInstance _cardInstance;
        private CardManager _cardManager;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        
        // State
        private bool _isHovering = false;
        private bool _isDragging = false;
        private Vector3 _originalPosition;
        private Vector3 _targetPosition;
        private Quaternion _originalRotation;
        private Quaternion _targetRotation;
        private Vector3 _originalScale;
        private Vector3 _targetScale;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            
            // Store original transform values
            _originalPosition = _rectTransform.localPosition;
            _originalRotation = _rectTransform.localRotation;
            _originalScale = _rectTransform.localScale;
            
            // Initialize targets
            _targetPosition = _originalPosition;
            _targetRotation = _originalRotation;
            _targetScale = _originalScale;
        }
        
        public void SetupCard(CardInstance cardInstance, CardManager cardManager)
        {
            _cardInstance = cardInstance;
            _cardManager = cardManager;
            
            CardData cardData = cardInstance.cardData;
            
            // Set texts
            nameText.text = cardData.cardName;
            descriptionText.text = cardData.description;
            
            // Set costs
            string costString = "";
            if (cardData.researchPointCost > 0)
                costString += $"RP: {cardData.researchPointCost} ";
            if (cardData.communityPointCost > 0)
                costString += $"CP: {cardData.communityPointCost}";
                
            costText.text = costString;
            
            // Set art
            if (cardData.cardArt != null)
            {
                cardArtImage.sprite = cardData.cardArt;
                cardArtImage.enabled = true;
            }
            else
            {
                cardArtImage.enabled = false;
            }
            
            // Set background color based on card type
            backgroundImage.color = GetCardColor(cardData.cardType);
            
            // Set frame color based on rarity
            frameImage.color = GetFrameColor(cardData.rarity);
            
            // Set icon based on type
            typeIconImage.sprite = GetTypeIcon(cardData.cardType);
        }
        
        private Color GetCardColor(CardType cardType)
        {
            return cardType switch
            {
                CardType.BalanceChange => balanceColor,
                CardType.MetaShift => metaShiftColor,
                CardType.Community => communityColor,
                CardType.CrisisResponse => crisisColor,
                CardType.Special => specialColor,
                _ => Color.white
            };
        }
        
        private Color GetFrameColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Common => commonFrameColor,
                CardRarity.Uncommon => uncommonFrameColor,
                CardRarity.Rare => rareFrameColor,
                CardRarity.Epic => epicFrameColor,
                CardRarity.Special => specialFrameColor,
                _ => Color.white
            };
        }
        
        private Sprite GetTypeIcon(CardType cardType)
        {
            return cardType switch
            {
                CardType.BalanceChange => balanceChangeIcon,
                CardType.MetaShift => metaShiftIcon,
                CardType.Community => communityIcon,
                CardType.CrisisResponse => crisisResponseIcon,
                CardType.Special => specialIcon,
                _ => null
            };
        }
        
        public void SetTargetPosition(Vector3 position)
        {
            _originalPosition = position;
            
            if (!_isHovering && !_isDragging)
            {
                _targetPosition = position;
                
                // Animate to position
                _rectTransform.DOLocalMove(_targetPosition, animationDuration).SetEase(animationEase);
            }
        }
        
        public void SetTargetRotation(Quaternion rotation)
        {
            _originalRotation = rotation;
            
            if (!_isHovering && !_isDragging)
            {
                _targetRotation = rotation;
                
                // Animate to rotation
                _rectTransform.DOLocalRotateQuaternion(_targetRotation, animationDuration).SetEase(animationEase);
            }
        }
        
        public void SetDepthOffset(float offset)
        {
            // Adjust local position Z
            Vector3 pos = _originalPosition;
            pos.z = offset;
            _originalPosition = pos;
            
            if (!_isHovering && !_isDragging)
            {
                _targetPosition = pos;
                _rectTransform.DOLocalMove(_targetPosition, animationDuration).SetEase(animationEase);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDragging)
                return;
                
            _isHovering = true;
            
            // Raise and scale up the card
            _targetPosition = _originalPosition + new Vector3(0, hoverHeight, -0.1f);
            _targetScale = _originalScale * hoverScale;
            
            // Animate
            _rectTransform.DOLocalMove(_targetPosition, animationDuration).SetEase(animationEase);
            _rectTransform.DOScale(_targetScale, animationDuration).SetEase(animationEase);
            _rectTransform.DOLocalRotateQuaternion(Quaternion.identity, animationDuration).SetEase(animationEase);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDragging)
                return;
                
            _isHovering = false;
            
            // Return to original position and scale
            _targetPosition = _originalPosition;
            _targetScale = _originalScale;
            _targetRotation = _originalRotation;
            
            // Animate
            _rectTransform.DOLocalMove(_targetPosition, animationDuration).SetEase(animationEase);
            _rectTransform.DOScale(_targetScale, animationDuration).SetEase(animationEase);
            _rectTransform.DOLocalRotateQuaternion(_targetRotation, animationDuration).SetEase(animationEase);
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging || _cardInstance == null || _cardManager == null)
                return;
                
            // Play the card
            _cardManager.PlayCard(_cardInstance);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_cardInstance == null || _cardManager == null)
                return;
                
            _isDragging = true;
            
            // Move card with cursor
            if (_canvas != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                _rectTransform.localPosition = new Vector3(localPoint.x, localPoint.y, -0.2f);
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            
            // Check if dropped on play area
            // (In a full implementation, this would use raycasting to detect the play area)
            
            // For now, just return to hand
            if (_isHovering)
            {
                // Still hovering, go back to hover state
                _targetPosition = _originalPosition + new Vector3(0, hoverHeight, -0.1f);
                _targetScale = _originalScale * hoverScale;
                _targetRotation = Quaternion.identity;
            }
            else
            {
                // No longer hovering, return to original state
                _targetPosition = _originalPosition;
                _targetScale = _originalScale;
                _targetRotation = _originalRotation;
            }
            
            // Animate
            _rectTransform.DOLocalMove(_targetPosition, animationDuration).SetEase(animationEase);
            _rectTransform.DOScale(_targetScale, animationDuration).SetEase(animationEase);
            _rectTransform.DOLocalRotateQuaternion(_targetRotation, animationDuration).SetEase(animationEase);
        }
        
        public void PlayAnimation()
        {
            // Sequence for card play animation
            Sequence sequence = DOTween.Sequence();
            
            // Move to center, scale up, then fade out
            sequence.Append(_rectTransform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad));
            sequence.Join(_rectTransform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad));
            sequence.Append(_rectTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InQuad));
            sequence.Join(GetComponent<CanvasGroup>().DOFade(0, 0.2f));
            
            // Destroy after animation
            sequence.OnComplete(() => Destroy(gameObject));
        }
        
        public void DiscardAnimation()
        {
            // Sequence for discard animation
            Sequence sequence = DOTween.Sequence();
            
            // Rotate and fade out while moving down
            sequence.Append(_rectTransform.DOLocalMoveY(-200, 0.5f).SetEase(Ease.InQuad));
            sequence.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, Random.Range(-90f, 90f)), 0.5f).SetEase(Ease.InQuad));
            sequence.Join(GetComponent<CanvasGroup>().DOFade(0, 0.5f));
            
            // Destroy after animation
            sequence.OnComplete(() => Destroy(gameObject));
        }
    }
}