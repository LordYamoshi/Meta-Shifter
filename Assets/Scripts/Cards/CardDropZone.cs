using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Complete CardDropZone with proper queue management and individual card effects
    /// </summary>
    public class CardDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Drop Zone Settings")]
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private int maxCards = 5;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.2f, 0.5f, 0.8f, 0.7f);
        [SerializeField] private Color acceptColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
        [SerializeField] private Color rejectColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        [SerializeField] private Color implementationColor = new Color(0.8f, 0.4f, 0.1f, 0.7f);
        
        [Header("Layout")]
        [SerializeField] private float cardSpacing = 10f;
        [SerializeField] private float cardWidth = 120f;
        [SerializeField] private float cardHeight = 160f;
        
        [Header("Implementation Effects")]
        [SerializeField] private float delayBetweenCardEffects = 0.5f;
        [SerializeField] private float implementationHighlightDuration = 1.5f;
        [SerializeField] private Color implementationHighlightColor = Color.yellow;
        [SerializeField] private float fadeOutDuration = 1.0f;
        
        [Header("Events")]
        public UnityEvent<List<CardData>> OnCardsChanged;
        public UnityEvent<CardData> OnCardImplemented;
        public UnityEvent OnImplementationStarted;
        public UnityEvent OnImplementationCompleted;
        
        private List<DraggableCard> queuedCards = new List<DraggableCard>();
        private bool isImplementing = false;
        
        private void Start()
        {
            InitializeDropZone();
            SubscribeToEvents();
        }
        
        private void InitializeDropZone()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (cardContainer == null)
                cardContainer = GetComponent<RectTransform>();
            
            SetNormalAppearance();
            
            var graphicRaycaster = GetComponentInParent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                Debug.LogWarning("CardDropZone needs a GraphicRaycaster in parent hierarchy!");
            }
            
            Debug.Log($"CardDropZone initialized on {gameObject.name}, using container: {cardContainer?.name ?? "null"}");
        }
        
        private void SubscribeToEvents()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        public bool CanAcceptCard(CardData cardData)
        {
            bool hasSpace = queuedCards.Count < maxCards;
            bool isPlanning = Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
            bool notImplementing = !isImplementing;
            bool canAfford = Core.ResourceManager.Instance?.CanSpend(cardData.researchPointCost, cardData.communityPointCost) ?? false;
            bool canAffordTotal = CanAffordAllQueuedCards(cardData);
            
            Debug.Log($"CanAcceptCard {cardData.cardName}: hasSpace={hasSpace}, isPlanning={isPlanning}, notImplementing={notImplementing}, canAfford={canAfford}, canAffordTotal={canAffordTotal}");
            
            return hasSpace && isPlanning && notImplementing && canAfford && canAffordTotal;
        }
        
        private bool CanAffordAllQueuedCards(CardData additionalCard = null)
        {
            int totalRP = additionalCard?.researchPointCost ?? 0;
            int totalCP = additionalCard?.communityPointCost ?? 0;
            
            foreach (var card in queuedCards)
            {
                if (card?.CardData != null)
                {
                    totalRP += card.CardData.researchPointCost;
                    totalCP += card.CardData.communityPointCost;
                }
            }
            
            return Core.ResourceManager.Instance?.CanSpend(totalRP, totalCP) ?? false;
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop called!");
            
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
            if (!isImplementing)
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
            
            // Check if card is already in our queue (to prevent duplicates)
            if (queuedCards.Contains(card))
            {
                Debug.LogWarning($"⚠️ Card {card.CardData.cardName} already in queue, not adding again");
                return;
            }
            
            queuedCards.Add(card);
            
            // ABSOLUTE POSITION PRESERVATION
            var rect = card.GetComponent<RectTransform>();
            Vector3 originalWorldPosition = rect.position;
            Vector2 originalSizeDelta = rect.sizeDelta;
            Vector3 originalScale = rect.localScale;
            
            Transform targetParent = cardContainer != null ? cardContainer.transform : this.transform;
            card.transform.SetParent(targetParent, true);
            
            // FORCE restore exact same visual properties
            rect.position = originalWorldPosition;
            rect.sizeDelta = originalSizeDelta;
            rect.localScale = originalScale;
            
            bool isPlanning = Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
            card.SetDraggingEnabled(isPlanning);
            
            // Remove from hand
            if (CardManager.Instance != null)
            {
                CardManager.Instance.RemoveCardFromHand(card);
            }
            
            OnCardsChanged.Invoke(GetQueuedCardData());
            
            Debug.Log($"✅ Card {card.CardData.cardName} added to drop zone with EXACT same appearance");
        }
        
        public void RemoveCard(DraggableCard card)
        {
            if (queuedCards.Contains(card))
            {
                queuedCards.Remove(card);
                
                Debug.Log($"🗑️ Removed {card.CardData.cardName} from implementation queue");
                
                if (CardManager.Instance != null)
                {
                    CardManager.Instance.ReturnCardToHand(card);
                }
                else
                {
                    card.ReturnToHand();
                }
                
                ReorganizeCards();
                OnCardsChanged.Invoke(GetQueuedCardData());
                Debug.Log($"✅ {card.CardData.cardName} successfully unqueued and returned to hand");
            }
            else
            {
                Debug.LogWarning($"⚠️ Tried to remove {card.CardData?.cardName ?? "unknown card"} but it wasn't in the queue");
            }
        }
        
        /// <summary>
        /// Force remove a card from queue - used when card is dragged out but still in queue
        /// This ensures the card is properly removed from implementation queue
        /// </summary>
        public void ForceRemoveCardFromQueue(DraggableCard card)
        {
            if (card == null) return;

            bool wasInQueue = queuedCards.Contains(card);
            
            if (wasInQueue)
            {
                queuedCards.Remove(card);
                OnCardsChanged.Invoke(GetQueuedCardData());
                Debug.Log($"🔧 Force removed {card.CardData.cardName} from {name} implementation queue");
                
                // Also clean up any visual indicators
                if (!isImplementing)
                    SetNormalAppearance();
            }
            else
            {
                Debug.Log($"⚠️ Attempted to force remove {card.CardData?.cardName} from {name} but it wasn't in queue");
            }
        }
        
        /// <summary>
        /// Check if a specific card is in the implementation queue
        /// </summary>
        public bool IsCardInQueue(DraggableCard card)
        {
            bool inQueue = queuedCards.Contains(card);
            Debug.Log($"🔍 Card {card.CardData?.cardName ?? "unknown"} in {name} queue: {inQueue}");
            return inQueue;
        }
        
        private void ReorganizeCards()
        {
            Debug.Log("Grid Layout Group handling card reorganization automatically");
        }
        
        /// <summary>
        /// Main implementation method - implements all cards with individual effects
        /// </summary>
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
                if (card?.CardData != null)
                {
                    totalRP += card.CardData.researchPointCost;
                    totalCP += card.CardData.communityPointCost;
                }
            }
            
            // Check if player can afford all cards
            if (!resourceManager.CanSpend(totalRP, totalCP))
            {
                Debug.Log($"Cannot afford to implement all cards! Need {totalRP} RP, {totalCP} CP");
                return;
            }
            
            // Spend resources
            resourceManager.SpendResources(totalRP, totalCP);
            
            // Start implementation with individual card effects
            StartCoroutine(ImplementCardsWithIndividualEffects());
        }
        
        /// <summary>
        /// Implement cards with individual effects - one card completely finishes before next starts
        /// </summary>
        private IEnumerator ImplementCardsWithIndividualEffects()
        {
            isImplementing = true;
            OnImplementationStarted.Invoke();
            SetImplementationAppearance();
            
            Debug.Log($"🎯 Starting sequential implementation of {queuedCards.Count} cards");
            
            // Create a copy of the list to avoid modification during iteration
            var cardsToImplement = new List<DraggableCard>(queuedCards);
            
            for (int i = 0; i < cardsToImplement.Count; i++)
            {
                var card = cardsToImplement[i];
                if (card?.CardData != null)
                {
                    Debug.Log($"🎯 Implementing card {i + 1}/{cardsToImplement.Count}: {card.CardData.cardName}");
                    
                    // Get or add ImplementationEffect to the card
                    var cardEffect = card.GetComponent<Effects.ImplementationEffect>();
                    if (cardEffect == null)
                    {
                        Debug.Log($"⚠️ No ImplementationEffect on {card.CardData.cardName}, adding one dynamically");
                        cardEffect = card.gameObject.AddComponent<Effects.ImplementationEffect>();
                        
                        // Set default values for dynamically added effect
                        cardEffect.SetDuration(implementationHighlightDuration);
                    }
                    
                    // STEP 1: Start highlight and implementation effect
                    Debug.Log($"🌟 Step 1: Starting highlight for {card.CardData.cardName}");
                    yield return StartCoroutine(HighlightCardDuringImplementation(card, cardEffect));
                    
                    Debug.Log($"✅ Step 1 Complete: {card.CardData.cardName} implementation finished");
                    
                    // STEP 2: Fade out the card completely
                    Debug.Log($"💨 Step 2: Starting fade out for {card.CardData.cardName}");
                    yield return StartCoroutine(FadeOutCard(card));
                    
                    Debug.Log($"✅ Step 2 Complete: {card.CardData.cardName} completely hidden");
                    
                    // STEP 3: Small delay before next card (optional)
                    if (i < cardsToImplement.Count - 1) // Only delay if there's a next card
                    {
                        Debug.Log($"⏱️ Step 3: Waiting {delayBetweenCardEffects}s before next card");
                        yield return new WaitForSeconds(delayBetweenCardEffects);
                    }
                    
                    Debug.Log($"🔄 Ready for next card ({i + 2}/{cardsToImplement.Count})");
                }
            }
            
            Debug.Log($"🏁 All cards have been individually processed and hidden");
            
            // Clear the queue after all implementations and fade-outs are complete
            ClearQueue();
            
            isImplementing = false;
            OnImplementationCompleted.Invoke();
            SetNormalAppearance();
            
            Debug.Log($"✅ Sequential implementation completed successfully");
        }
        
        /// <summary>
        /// Highlight card with background color during implementation
        /// </summary>
        private IEnumerator HighlightCardDuringImplementation(DraggableCard card, Effects.ImplementationEffect cardEffect)
        {
            if (card == null) yield break;
            
            Debug.Log($"✨ Starting highlight for {card.CardData.cardName}");
            
            // Get card background image
            var cardBackground = card.GetComponent<Image>();
            if (cardBackground == null)
            {
                Debug.LogWarning($"No Image component found on {card.CardData.cardName} for highlighting");
                yield break;
            }
            
            // Store original color
            Color originalColor = cardBackground.color;
            
            // Play SFX if available
            if (cardEffect != null)
            {
                cardEffect.PlayImplementationEffect(card.CardData);
            }
            
            float elapsed = 0f;
            
            // Phase 1: Fade to highlight color (0.3s)
            float fadeInDuration = 0.3f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                // Smooth transition to highlight color
                cardBackground.color = Color.Lerp(originalColor, implementationHighlightColor, t);
                yield return null;
            }
            
            // Ensure we're at full highlight color
            cardBackground.color = implementationHighlightColor;
            
            // Apply the card logic during highlight
            card.CardData.PlayCard();
            OnCardImplemented.Invoke(card.CardData);
            
            // Phase 2: Hold highlight color (main duration - fade times)
            float holdDuration = implementationHighlightDuration - (fadeInDuration * 2);
            if (holdDuration > 0)
            {
                yield return new WaitForSeconds(holdDuration);
            }
            
            // Phase 3: Fade back to original color (0.3s)
            elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                // Smooth transition back to original color
                cardBackground.color = Color.Lerp(implementationHighlightColor, originalColor, t);
                yield return null;
            }
            
            // Ensure we're back to original color
            cardBackground.color = originalColor;
            
            Debug.Log($"✨ Highlight completed for {card.CardData.cardName}");
        }
        
        /// <summary>
        /// Fade out card smoothly after implementation - BLOCKS until completely hidden
        /// </summary>
        private IEnumerator FadeOutCard(DraggableCard card)
        {
            if (card == null) yield break;
            
            Debug.Log($"💨 Starting fade out for {card.CardData.cardName}");
            
            // Get or add CanvasGroup for fading
            var canvasGroup = card.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Store original values
            float startAlpha = canvasGroup.alpha;
            Vector3 startScale = card.transform.localScale;
            Vector3 startPosition = card.transform.localPosition;
            
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration && card != null)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                
                // Smooth fade out curve (ease in)
                float fadeT = 1f - Mathf.Pow(1f - t, 2f);
                
                // Fade alpha
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, fadeT);
                
                // Slight scale down
                float scaleMultiplier = Mathf.Lerp(1f, 0.8f, fadeT);
                card.transform.localScale = startScale * scaleMultiplier;
                
                // Optional: Slight upward drift
                Vector3 driftOffset = Vector3.up * (fadeT * 10f);
                card.transform.localPosition = startPosition + driftOffset;
                
                yield return null;
            }
            
            // Ensure card is completely faded
            if (card != null && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                card.transform.localScale = startScale * 0.8f;
                
                Debug.Log($"💨 {card.CardData.cardName} fade out completed - card is now completely hidden");
                
                // Disable the card GameObject - it's now completely hidden
                card.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Clear queue - handles faded out cards properly
        /// </summary>
        public void ClearQueue()
        {
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
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
                    isImplementing = false;
                    EnableCardDragging();
                    break;
                    
                case Core.GamePhase.Implementation:
                    SetImplementationAppearance();
                    DisableCardDragging();
                    // Auto-implement cards if there are any
                    if (queuedCards.Count > 0)
                    {
                        ImplementAllCards();
                    }
                    break;
                    
                case Core.GamePhase.Feedback:
                case Core.GamePhase.Event:
                    if (!isImplementing)
                        SetNormalAppearance();
                    DisableCardDragging();
                    break;
            }
        }
        
        private void EnableCardDragging()
        {
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
                    card.SetDraggingEnabled(true);
                    Debug.Log($"🔓 Planning phase: Unlocked {card.CardData.cardName} in drop zone for dragging");
                }
            }
            Debug.Log($"✅ Drop zone unlocked {queuedCards.Count} cards for planning phase");
        }
        
        private void DisableCardDragging()
        {
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
                    card.SetDraggingEnabled(false);
                }
            }
        }
        
        // Visual appearance methods
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
            
            Invoke(nameof(SetNormalAppearance), 0.5f);
        }
        
        private void SetImplementationAppearance()
        {
            if (backgroundImage != null)
                backgroundImage.color = implementationColor;
        }
        
        // Public getters
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
        
        /// <summary>
        /// Get the position of a card in the implementation queue (-1 if not found)
        /// </summary>
        public int GetCardQueuePosition(DraggableCard card)
        {
            int position = queuedCards.IndexOf(card);
            Debug.Log($"🔍 Card {card.CardData?.cardName ?? "unknown"} queue position: {position}");
            return position;
        }
        
        /// <summary>
        /// Call this method when you know a card has been moved to hand but might still be in queue
        /// This ensures the card is properly removed from implementation queue
        /// </summary>
        public void CleanUpOrphanedCards()
        {
            Debug.Log("🧹 Starting cleanup of orphaned cards...");
            
            List<DraggableCard> cardsToRemove = new List<DraggableCard>();
            
            foreach (var card in queuedCards)
            {
                if (card == null)
                {
                    cardsToRemove.Add(card);
                    Debug.Log("🗑️ Found null card in queue");
                }
                else if (!IsCardInDropZone(card.transform.parent))
                {
                    cardsToRemove.Add(card);
                    Debug.Log($"🗑️ Found orphaned card: {card.CardData.cardName} (parent: {card.transform.parent?.name ?? "null"})");
                }
            }
            
            foreach (var cardToRemove in cardsToRemove)
            {
                queuedCards.Remove(cardToRemove);
                Debug.Log($"🧹 Cleaned up orphaned card: {cardToRemove?.CardData?.cardName ?? "null card"}");
            }
            
            if (cardsToRemove.Count > 0)
            {
                OnCardsChanged.Invoke(GetQueuedCardData());
                Debug.Log($"✅ Cleanup complete: Removed {cardsToRemove.Count} orphaned cards");
            }
            else
            {
                Debug.Log("✅ Cleanup complete: No orphaned cards found");
            }
        }
        
        /// <summary>
        /// Helper method to check if a transform is within a drop zone
        /// </summary>
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
        
        // Debug methods
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
        }
        
        [ContextMenu("Debug: Check All Cards in Drop Zone")]
        public void DebugCheckAllCards()
        {
            Debug.Log($"=== DROP ZONE CARD STATUS ({queuedCards.Count} cards) ===");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"Is Implementing: {isImplementing}");
            
            if (queuedCards.Count == 0)
            {
                Debug.Log("📭 No cards in implementation queue");
                return;
            }
            
            for (int i = 0; i < queuedCards.Count; i++)
            {
                var card = queuedCards[i];
                if (card != null)
                {
                    var effect = card.GetComponent<Effects.ImplementationEffect>();
                    string parentName = card.transform.parent?.name ?? "null";
                    bool isInDropZone = IsCardInDropZone(card.transform.parent);
                    
                    Debug.Log($"  [{i}] {card.CardData.cardName}");
                    Debug.Log($"      - Dragging Enabled: {card.IsDraggingEnabled()}");
                    Debug.Log($"      - Has Effect: {effect != null}");
                    Debug.Log($"      - Parent: {parentName}");
                    Debug.Log($"      - Is In Drop Zone: {isInDropZone}");
                    Debug.Log($"      - GameObject Active: {card.gameObject.activeInHierarchy}");
                }
                else
                {
                    Debug.Log($"  [{i}] NULL CARD - This shouldn't happen!");
                }
            }
        }
        
        [ContextMenu("Debug: Clean Up Orphaned Cards")]
        public void DebugCleanUpOrphanedCards()
        {
            CleanUpOrphanedCards();
        }
        
        [ContextMenu("Debug: Remove First Card from Queue")]
        public void DebugRemoveFirstCard()
        {
            if (queuedCards.Count > 0)
            {
                var firstCard = queuedCards[0];
                Debug.Log($"🧪 Manually removing first card: {firstCard.CardData.cardName}");
                RemoveCard(firstCard);
            }
            else
            {
                Debug.Log("🧪 No cards in queue to remove");
            }
        }
        
        [ContextMenu("Add Effects to All Cards")]
        public void AddEffectsToAllCards()
        {
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
                    var effect = card.GetComponent<Effects.ImplementationEffect>();
                    if (effect == null)
                    {
                        effect = card.gameObject.AddComponent<Effects.ImplementationEffect>();
                        Debug.Log($"➕ Added ImplementationEffect to {card.CardData.cardName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Test implementation effects - now tests sequential implementation
        /// </summary>
        [ContextMenu("Test Implementation Effects")]
        public void TestImplementationEffects()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(TestSequentialEffectsCoroutine());
            }
        }
        
        private IEnumerator TestSequentialEffectsCoroutine()
        {
            Debug.Log("🧪 Testing sequential highlight effects...");
            
            for (int i = 0; i < queuedCards.Count; i++)
            {
                var card = queuedCards[i];
                if (card != null)
                {
                    var effect = card.GetComponent<Effects.ImplementationEffect>();
                    if (effect == null)
                    {
                        effect = card.gameObject.AddComponent<Effects.ImplementationEffect>();
                        effect.SetDuration(implementationHighlightDuration);
                    }
                    
                    Debug.Log($"🧪 Testing card {i + 1}/{queuedCards.Count}: {card.CardData.cardName}");
                    
                    // Step 1: Test highlight effect
                    Debug.Log($"  Step 1: Testing highlight for {card.CardData.cardName}");
                    yield return StartCoroutine(HighlightCardDuringImplementation(card, effect));
                    
                    // Step 2: Test fade out
                    Debug.Log($"  Step 2: Testing fade out for {card.CardData.cardName}");
                    yield return StartCoroutine(FadeOutCard(card));
                    
                    // Step 3: Small delay
                    if (i < queuedCards.Count - 1)
                    {
                        Debug.Log($"  Step 3: Waiting before next card...");
                        yield return new WaitForSeconds(delayBetweenCardEffects);
                    }
                    
                    Debug.Log($"✅ Card {i + 1} test completed");
                }
            }
            
            Debug.Log("🧪 Sequential test completed - all cards processed one by one");
        }
        
        /// <summary>
        /// Force implementation for testing (bypasses phase checks)
        /// </summary>
        [ContextMenu("Debug: Force Implementation")]
        public void DebugForceImplementation()
        {
            if (queuedCards.Count == 0)
            {
                Debug.Log("🧪 No cards to implement");
                return;
            }
            
            Debug.Log($"🧪 Force implementing {queuedCards.Count} cards...");
            StartCoroutine(ImplementCardsWithIndividualEffects());
        }
        
        /// <summary>
        /// Check queue consistency - useful for debugging
        /// </summary>
        [ContextMenu("Debug: Check Queue Consistency")]
        public void DebugCheckQueueConsistency()
        {
            Debug.Log("=== QUEUE CONSISTENCY CHECK ===");
            Debug.Log($"Queue contains {queuedCards.Count} cards");
            
            int inconsistencies = 0;
            
            for (int i = 0; i < queuedCards.Count; i++)
            {
                var card = queuedCards[i];
                if (card == null)
                {
                    Debug.LogError($"❌ Queue position {i}: NULL CARD");
                    inconsistencies++;
                    continue;
                }
                
                bool isVisuallyInDropZone = IsCardInDropZone(card.transform.parent);
                bool isInThisDropZone = card.transform.IsChildOf(this.transform);
                
                Debug.Log($"✅ Queue position {i}: {card.CardData.cardName}");
                Debug.Log($"    - Visually in drop zone: {isVisuallyInDropZone}");
                Debug.Log($"    - In this drop zone: {isInThisDropZone}");
                Debug.Log($"    - GameObject active: {card.gameObject.activeInHierarchy}");
                Debug.Log($"    - Parent: {card.transform.parent?.name ?? "null"}");
                
                if (!isVisuallyInDropZone)
                {
                    Debug.LogWarning($"⚠️ Inconsistency: Card {card.CardData.cardName} is in queue but not visually in drop zone");
                    inconsistencies++;
                }
            }
            
            if (inconsistencies == 0)
            {
                Debug.Log("✅ Queue is consistent - all cards in queue are properly positioned");
            }
            else
            {
                Debug.LogWarning($"⚠️ Found {inconsistencies} inconsistencies in queue");
            }
        }
        
        /// <summary>
        /// Validate all cards in queue can still be afforded
        /// </summary>
        [ContextMenu("Debug: Validate Queue Affordability")]
        public void DebugValidateQueueAffordability()
        {
            Debug.Log("=== QUEUE AFFORDABILITY CHECK ===");
            
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null)
            {
                Debug.LogError("❌ ResourceManager not found");
                return;
            }
            
            GetTotalQueuedCost(out int totalRP, out int totalCP);
            bool canAffordAll = resourceManager.CanSpend(totalRP, totalCP);
            
            Debug.Log($"Current Resources: {resourceManager.ResearchPoints} RP, {resourceManager.CommunityPoints} CP");
            Debug.Log($"Total Queue Cost: {totalRP} RP, {totalCP} CP");
            Debug.Log($"Can Afford All: {canAffordAll}");
            
            if (!canAffordAll)
            {
                Debug.LogWarning("⚠️ Cannot afford all queued cards! This may cause implementation to fail.");
                
                int affordableRP = resourceManager.ResearchPoints;
                int affordableCP = resourceManager.CommunityPoints;
                
                Debug.Log("Individual card affordability:");
                foreach (var card in queuedCards)
                {
                    if (card?.CardData != null)
                    {
                        bool canAffordThis = resourceManager.CanSpend(card.CardData.researchPointCost, card.CardData.communityPointCost);
                        string status = canAffordThis ? "✅ AFFORDABLE" : "❌ TOO EXPENSIVE";
                        Debug.Log($"  {card.CardData.cardName} ({card.CardData.researchPointCost} RP, {card.CardData.communityPointCost} CP): {status}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Emergency method to clear queue without effects
        /// </summary>
        [ContextMenu("Debug: Emergency Clear Queue")]
        public void DebugEmergencyClearQueue()
        {
            Debug.Log($"🚨 Emergency clearing {queuedCards.Count} cards from queue");
            
            foreach (var card in queuedCards)
            {
                if (card != null)
                {
                    Debug.Log($"🗑️ Emergency removing: {card.CardData?.cardName ?? "unknown card"}");
                    
                    // Return to hand if possible
                    if (CardManager.Instance != null)
                    {
                        CardManager.Instance.ReturnCardToHand(card);
                    }
                    else
                    {
                        card.ReturnToHand();
                    }
                }
            }
            
            queuedCards.Clear();
            OnCardsChanged.Invoke(GetQueuedCardData());
            
            Debug.Log("🚨 Emergency clear completed");
        }
        
        /// <summary>
        /// Show detailed information about the drop zone state
        /// </summary>
        [ContextMenu("Debug: Show Drop Zone Info")]
        public void DebugShowDropZoneInfo()
        {
            Debug.Log("=== DROP ZONE INFORMATION ===");
            Debug.Log($"Name: {name}");
            Debug.Log($"Max Cards: {maxCards}");
            Debug.Log($"Current Card Count: {queuedCards.Count}");
            Debug.Log($"Is Implementing: {isImplementing}");
            Debug.Log($"Has Space: {queuedCards.Count < maxCards}");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"Card Container: {cardContainer?.name ?? "null"}");
            Debug.Log($"Background Image: {backgroundImage?.name ?? "null"}");
            
            GetTotalQueuedCost(out int totalRP, out int totalCP);
            Debug.Log($"Total Queued Cost: {totalRP} RP, {totalCP} CP");
            
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null)
            {
                Debug.Log($"Current Resources: {resourceManager.ResearchPoints} RP, {resourceManager.CommunityPoints} CP");
                Debug.Log($"Can Afford Queue: {resourceManager.CanSpend(totalRP, totalCP)}");
            }
            
            // Check for components
            Debug.Log("Component Status:");
            Debug.Log($"  - Image: {GetComponent<Image>() != null}");
            Debug.Log($"  - RectTransform: {GetComponent<RectTransform>() != null}");
            Debug.Log($"  - GraphicRaycaster in parent: {GetComponentInParent<GraphicRaycaster>() != null}");
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnPhaseChanged);
            }
            
            // Stop any running coroutines
            StopAllCoroutines();
            
            Debug.Log($"CardDropZone {name} destroyed");
        }
        
        /// <summary>
        /// Called when component is reset in editor - useful for setting up defaults
        /// </summary>
        private void Reset()
        {
            // Set up default values when component is added
            maxCards = 5;
            cardSpacing = 10f;
            cardWidth = 120f;
            cardHeight = 160f;
            delayBetweenCardEffects = 0.5f;
            implementationHighlightDuration = 1.5f;
            fadeOutDuration = 1.0f;
            
            // Set up default colors
            normalColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            highlightColor = new Color(0.2f, 0.5f, 0.8f, 0.7f);
            acceptColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
            rejectColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
            implementationColor = new Color(0.8f, 0.4f, 0.1f, 0.7f);
            implementationHighlightColor = Color.yellow;
            
            Debug.Log("CardDropZone reset with default values");
        }
    }
}