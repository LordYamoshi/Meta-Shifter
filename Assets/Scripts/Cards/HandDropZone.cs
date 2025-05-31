using UnityEngine;
using UnityEngine.EventSystems;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Add this script to your Hand Container to allow cards to be dragged back from the drop zone
    /// </summary>
    public class HandDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hand Container")]
        [SerializeField] private Transform handContainer;
        
        [Header("Visual Feedback (Optional)")]
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0f); // Transparent
        [SerializeField] private Color highlightColor = new Color(0.2f, 0.8f, 0.2f, 0.3f); // Green with transparency
        
        private void Start()
        {
            // Use this object as hand container if not set
            if (handContainer == null)
                handContainer = transform;
                
            // Get background image if available
            if (backgroundImage == null)
                backgroundImage = GetComponent<UnityEngine.UI.Image>();
                
            Debug.Log($"HandDropZone initialized on {gameObject.name}");
        }
        
        public bool CanAcceptCard(CardData cardData)
        {
            // Only accept cards during planning phase
            return Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("HandDropZone: OnDrop called");
            
            // Get the dragged card
            var draggedObject = eventData.pointerDrag;
            if (draggedObject == null) 
            {
                Debug.Log("HandDropZone: No dragged object found");
                return;
            }
            
            var draggableCard = draggedObject.GetComponent<DraggableCard>();
            if (draggableCard == null) 
            {
                Debug.Log("HandDropZone: Dragged object is not a DraggableCard");
                return;
            }
            
            if (!CanAcceptCard(draggableCard.CardData))
            {
                Debug.Log("HandDropZone: Cannot accept card - not in planning phase");
                return;
            }
            
            Debug.Log($"HandDropZone: Accepting card {draggableCard.CardData.cardName}");
            
            // ABSOLUTE POSITION PRESERVATION: Store world position and size
            var rect = draggableCard.GetComponent<RectTransform>();
            Vector3 originalWorldPosition = rect.position;
            Vector2 originalSize = rect.rect.size;
            Vector2 originalSizeDelta = rect.sizeDelta;
            Vector3 originalScale = rect.localScale;
            
            Debug.Log($"🔒 BEFORE SetParent: WorldPos={originalWorldPosition}, Size={originalSize}, SizeDelta={originalSizeDelta}, Scale={originalScale}");
            
            // Set parent to hand container
            draggableCard.transform.SetParent(handContainer, true); // Keep world position
            
            // FORCE restore exact same visual properties
            rect.position = originalWorldPosition;
            rect.sizeDelta = originalSizeDelta;
            rect.localScale = originalScale;
            
            Debug.Log($"🔒 AFTER restore: WorldPos={rect.position}, Size={rect.rect.size}, SizeDelta={rect.sizeDelta}, Scale={rect.localScale}");
            
            // Return card to hand via CardManager (this will handle the hand list management)
            if (CardManager.Instance != null)
            {
                CardManager.Instance.ReturnCardToHand(draggableCard);
            }
            else
            {
                Debug.LogError("HandDropZone: CardManager.Instance is null!");
            }
            
            Debug.Log($"✅ HandDropZone: Card {draggableCard.CardData.cardName} returned to hand with EXACT same appearance");
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Show visual feedback when dragging a card over the hand area
            if (eventData.pointerDrag != null)
            {
                var draggableCard = eventData.pointerDrag.GetComponent<DraggableCard>();
                if (draggableCard != null && CanAcceptCard(draggableCard.CardData))
                {
                    SetHighlightAppearance();
                }
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            // Remove visual feedback
            SetNormalAppearance();
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
    }
}