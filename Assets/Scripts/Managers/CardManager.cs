using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Updated CardManager that works with Unity prefabs and your drag-drop system
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance { get; private set; }
        
        [Header("Card Database")]
        [SerializeField] private List<CardData> allAvailableCards = new List<CardData>();
        
        [Header("Hand Management")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private int maxHandSize = 7;
        [SerializeField] private int cardsDrawnPerWeek = 2;
        
        [Header("Starting Resources")]
        [SerializeField] private int startingRP = 24;
        [SerializeField] private int startingCP = 0;
        
        [Header("Events")]
        public UnityEvent<List<CardData>> OnHandChanged;
        public UnityEvent<CardData> OnCardPlayed;
        
        private List<DraggableCard> currentHandCards = new List<DraggableCard>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // Subscribe to phase changes
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
            
            // Subscribe to resource changes for real-time updates
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
                Debug.Log("✅ Subscribed to resource changes for real-time affordability updates");
            }
            
            // Set starting resources
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.SetStartingResources(startingRP, startingCP);
            }
            
            // Draw initial hand
            DrawInitialHand();
        }
        
        private void OnResourcesChanged(int rp, int cp)
        {
            Debug.Log($"🔄 Resources changed: RP={rp}, CP={cp} - updating all card affordability");
            UpdateAllCardAffordability();
        }
        
        private void DrawInitialHand()
        {
            DrawCards(maxHandSize);
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Planning)
            {
                // Draw new cards each week
                DrawCards(cardsDrawnPerWeek);
                
                // UNLOCK ALL CARDS - enable dragging for all cards everywhere during planning
                UnlockAllCardsForDragging();
                
                // Update all cards in hand to show current affordability
                RefreshAllCardsInHand();
                
                // Start real-time affordability updates
                StartAffordabilityUpdates();
            }
            else
            {
                // LOCK ALL CARDS - disable dragging when not in planning phase
                LockAllCardsFromDragging();
                
                // Stop real-time updates
                StopAffordabilityUpdates();
            }
        }
        
        private void StartAffordabilityUpdates()
        {
            // Update affordability every 0.2 seconds during planning phase
            InvokeRepeating(nameof(UpdateAllCardAffordability), 0f, 0.2f);
        }
        
        private void StopAffordabilityUpdates()
        {
            CancelInvoke(nameof(UpdateAllCardAffordability));
        }
        
        private void UpdateAllCardAffordability()
        {
            Debug.Log("🔄 Updating affordability for all cards...");
            
            // Update all cards in hand
            int handUpdated = 0;
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    card.RefreshAffordabilityOnly();
                    handUpdated++;
                }
            }
            
            // Update all cards in drop zones
            int dropZoneUpdated = 0;
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in allDropZones)
            {
                var cardsInDropZone = dropZone.GetQueuedCards();
                foreach (var card in cardsInDropZone)
                {
                    if (card != null)
                    {
                        card.RefreshAffordabilityOnly();
                        dropZoneUpdated++;
                    }
                }
            }
            
            Debug.Log($"✅ Updated affordability: {handUpdated} hand cards + {dropZoneUpdated} drop zone cards");
        }
        
        private void UnlockAllCardsForDragging()
        {
            Debug.Log("🔓 UNLOCKING ALL CARDS FOR PLANNING PHASE");
            
            int handCardsUnlocked = 0;
            int dropZoneCardsUnlocked = 0;
            
            // Enable dragging for cards in hand
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    Debug.Log($"   🔓 Unlocking {card.CardData.cardName} in hand (was: {card.IsDraggingEnabled()})");
                    card.SetDraggingEnabled(true);
                    handCardsUnlocked++;
                }
            }
            
            // Enable dragging for cards in drop zones
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            Debug.Log($"   🔍 Found {allDropZones.Length} drop zones");
            
            foreach (var dropZone in allDropZones)
            {
                var cardsInDropZone = dropZone.GetQueuedCards();
                Debug.Log($"   🔍 Drop zone {dropZone.name} has {cardsInDropZone.Count} cards");
                
                foreach (var card in cardsInDropZone)
                {
                    if (card != null)
                    {
                        Debug.Log($"   🔓 Unlocking {card.CardData.cardName} in drop zone (was: {card.IsDraggingEnabled()})");
                        card.SetDraggingEnabled(true);
                        dropZoneCardsUnlocked++;
                    }
                }
            }
            
            Debug.Log($"✅ PLANNING PHASE UNLOCK COMPLETE: {handCardsUnlocked} hand cards + {dropZoneCardsUnlocked} drop zone cards = {handCardsUnlocked + dropZoneCardsUnlocked} total cards unlocked");
        }
        
        private void LockAllCardsFromDragging()
        {
            // Disable dragging for cards in hand
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    card.SetDraggingEnabled(false);
                }
            }
            
            // Disable dragging for cards in drop zones
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in allDropZones)
            {
                var cardsInDropZone = dropZone.GetQueuedCards();
                foreach (var card in cardsInDropZone)
                {
                    if (card != null)
                    {
                        card.SetDraggingEnabled(false);
                    }
                }
            }
            
            Debug.Log("NON-PLANNING PHASE: All cards locked from dragging");
        }
        
        public void DrawCards(int count)
        {
            if (handContainer == null || cardPrefab == null)
            {
                Debug.LogError("Hand container or card prefab not set!");
                return;
            }
            
            for (int i = 0; i < count && currentHandCards.Count < maxHandSize; i++)
            {
                // Get random card from available cards
                CardData randomCard = GetRandomAvailableCard();
                if (randomCard == null) continue;
                
                // Instantiate card prefab
                GameObject cardObject = Instantiate(cardPrefab, handContainer);
                DraggableCard draggableCard = cardObject.GetComponent<DraggableCard>();
                
                if (draggableCard != null)
                {
                    // Set the card data
                    draggableCard.SetCardData(randomCard);
                    currentHandCards.Add(draggableCard);
                    
                    // Set dragging state based on current phase
                    bool isPlanning = Core.PhaseManager.Instance?.GetCurrentPhase() == Core.GamePhase.Planning;
                    draggableCard.SetDraggingEnabled(isPlanning);
                }
                else
                {
                    Debug.LogError("Card prefab missing DraggableCard component!");
                    Destroy(cardObject);
                }
            }
            
            Debug.Log($"Drew {count} cards. Hand size: {currentHandCards.Count}");
        }
        
        private CardData GetRandomAvailableCard()
        {
            if (allAvailableCards.Count == 0) return null;
            
            // Get current week to determine available rarities
            int currentWeek = Core.PhaseManager.Instance?.GetCurrentWeek() ?? 1;
            
            // Filter cards by availability
            var availableCards = allAvailableCards.Where(card => IsCardAvailable(card, currentWeek)).ToList();
            if (availableCards.Count == 0) return allAvailableCards[Random.Range(0, allAvailableCards.Count)];
            
            // Weighted selection by rarity
            var weightedCards = new List<(CardData card, float weight)>();
            
            foreach (var card in availableCards)
            {
                float weight = card.rarity switch
                {
                    CardRarity.Common => 50f,
                    CardRarity.Uncommon => 30f,
                    CardRarity.Rare => 15f,
                    CardRarity.Epic => 4f,
                    CardRarity.Special => 1f,
                    _ => 10f
                };
                
                weightedCards.Add((card, weight));
            }
            
            // Select based on weight
            float totalWeight = weightedCards.Sum(w => w.weight);
            float randomValue = Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var (card, weight) in weightedCards)
            {
                currentWeight += weight;
                if (randomValue <= currentWeight)
                {
                    return card;
                }
            }
            
            return availableCards[Random.Range(0, availableCards.Count)];
        }
        
        private bool IsCardAvailable(CardData card, int currentWeek)
        {
            return card.rarity switch
            {
                CardRarity.Common => true,
                CardRarity.Uncommon => currentWeek >= 2,
                CardRarity.Rare => currentWeek >= 4,
                CardRarity.Epic => currentWeek >= 6,
                CardRarity.Special => currentWeek >= 8,
                _ => false
            };
        }
        
        public void RemoveCardFromHand(DraggableCard card)
        {
            if (currentHandCards.Contains(card))
            {
                currentHandCards.Remove(card);
                Debug.Log($"Removed {card.CardData.cardName} from hand list (but not destroyed)");
            }
        }
        
        public void ReturnCardToHand(DraggableCard card)
        {
            // Add back to hand list if not already there
            if (!currentHandCards.Contains(card))
            {
                currentHandCards.Add(card);
            }
            
            // Store the card's COMPLETE original transform properties BEFORE any changes
            var rectTransform = card.GetComponent<RectTransform>();
            Vector2 originalAnchoredPos = rectTransform.anchoredPosition;
            Vector2 originalAnchorMin = rectTransform.anchorMin;
            Vector2 originalAnchorMax = rectTransform.anchorMax;
            Vector2 originalPivot = rectTransform.pivot;
            Vector2 originalSizeDelta = rectTransform.sizeDelta;
            Vector3 originalScale = rectTransform.localScale;
            Quaternion originalRotation = rectTransform.localRotation;
            Vector2 originalOffsetMin = rectTransform.offsetMin;
            Vector2 originalOffsetMax = rectTransform.offsetMax;
            
            // Return to hand container
            if (handContainer != null)
            {
                card.transform.SetParent(handContainer, true); // Use worldPositionStays = true
                
                // COMPLETELY RESTORE all transform properties to maintain exact same appearance
                rectTransform.anchorMin = originalAnchorMin;
                rectTransform.anchorMax = originalAnchorMax;
                rectTransform.pivot = originalPivot;
                rectTransform.sizeDelta = originalSizeDelta;
                rectTransform.localScale = originalScale;
                rectTransform.localRotation = originalRotation;
                rectTransform.offsetMin = originalOffsetMin;
                rectTransform.offsetMax = originalOffsetMax;
                rectTransform.anchoredPosition = originalAnchoredPos;
                
                // Re-enable dragging
                card.SetDraggingEnabled(true);
                
                Debug.Log($"Returned {card.CardData.cardName} to hand with completely preserved transform properties");
            }
        }
        
        public void RefreshAllCardsInHand()
        {
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    card.RefreshAffordabilityOnly();
                }
            }
        }
        
        // Public getters
        public List<CardData> GetCurrentHandData()
        {
            return currentHandCards.Where(c => c != null).Select(c => c.CardData).ToList();
        }
        
        public int GetHandSize() => currentHandCards.Count;
        
        // Debug methods
        [ContextMenu("Draw Card")]
        public void DebugDrawCard()
        {
            DrawCards(1);
        }
        
        [ContextMenu("Clear Hand")]
        public void DebugClearHand()
        {
            foreach (var card in currentHandCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            currentHandCards.Clear();
        }
        
        [ContextMenu("Refresh Hand Display")]
        public void DebugRefreshHand()
        {
            RefreshAllCardsInHand();
        }
        
        [ContextMenu("Force Update All Card Displays")]
        public void DebugForceUpdateAllCardDisplays()
        {
            Debug.Log("=== FORCE UPDATING ALL CARD DISPLAYS ===");
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    Debug.Log($"Force updating display for {card.CardData?.cardName ?? "null cardData"}");
                    card.RefreshDisplay();
                }
            }
        }
        
        [ContextMenu("Check All Card States")]
        public void DebugCheckAllCardStates()
        {
            Debug.Log("=== CHECKING ALL CARD STATES ===");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"Hand Cards: {currentHandCards.Count}");
            
            foreach (var card in currentHandCards)
            {
                if (card != null)
                {
                    Debug.Log($"  Hand Card: {card.CardData?.cardName ?? "null"} - Dragging: {card.IsDraggingEnabled()} - Parent: {card.transform.parent.name}");
                }
            }
            
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in allDropZones)
            {
                var cardsInDropZone = dropZone.GetQueuedCards();
                Debug.Log($"Drop Zone {dropZone.name}: {cardsInDropZone.Count} cards");
                
                foreach (var card in cardsInDropZone)
                {
                    if (card != null)
                    {
                        Debug.Log($"  Drop Zone Card: {card.CardData?.cardName ?? "null"} - Dragging: {card.IsDraggingEnabled()} - Parent: {card.transform.parent.name}");
                    }
                }
            }
        }
    }
}