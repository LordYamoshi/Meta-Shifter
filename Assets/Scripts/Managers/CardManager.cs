using UnityEngine;
using System.Collections.Generic;
using MetaBalance.Core;
using UnityEngine.Events;
using System.Linq;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Manages cards in the game
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        [Header("Card Collections")]
        [SerializeField] private List<CardData> cardDatabase = new List<CardData>();
        [SerializeField] private CardPoolSettings cardPoolSettings;
        
        [Header("Card Display")]
        [SerializeField] private CardDisplay cardDisplayPrefab;
        [SerializeField] private Transform handContainer;
        [SerializeField] private Transform playArea;
        
        [Header("Draw Settings")]
        [SerializeField] private int maxHandSize = 7;
        [SerializeField] private int cardsPerDraw = 3;
        
        [Header("Events")]
        public UnityEvent<CardInstance> onCardPlayed;
        public UnityEvent<CardInstance> onCardDiscarded;
        public UnityEvent<List<CardInstance>> onHandUpdated;
        
        // Runtime state
        private List<CardData> _availableCardPool = new List<CardData>();
        private List<CardInstance> _hand = new List<CardInstance>();
        private List<CardInstance> _playedCards = new List<CardInstance>();
        private List<CardInstance> _discardedCards = new List<CardInstance>();
        
        // Card displays
        private Dictionary<string, CardDisplay> _cardDisplays = new Dictionary<string, CardDisplay>();
        
        private void Start()
        {
            // Subscribe to game phase changes
            GameManager.Instance.onPhaseChanged.AddListener(OnGamePhaseChanged);
            
            // Initialize card pool
            InitializeCardPool();
            
            // Draw initial hand
            DrawCards(maxHandSize);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from game phase changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.onPhaseChanged.RemoveListener(OnGamePhaseChanged);
            }
        }
        
        private void InitializeCardPool()
        {
            // Clear pool
            _availableCardPool.Clear();
            
            // Filter database by settings
            foreach (CardData card in cardDatabase)
            {
                // Add appropriate number of copies based on rarity
                int copies = GetCardCopiesByRarity(card.rarity);
                for (int i = 0; i < copies; i++)
                {
                    _availableCardPool.Add(card);
                }
            }
            
            // Shuffle the pool
            ShuffleCardPool();
        }
        
        private int GetCardCopiesByRarity(CardRarity rarity)
        {
            // Higher rarities have fewer copies
            return rarity switch
            {
                CardRarity.Common => cardPoolSettings.commonCardCopies,
                CardRarity.Uncommon => cardPoolSettings.uncommonCardCopies,
                CardRarity.Rare => cardPoolSettings.rareCardCopies,
                CardRarity.Epic => cardPoolSettings.epicCardCopies,
                CardRarity.Special => cardPoolSettings.specialCardCopies,
                _ => 1
            };
        }
        
        private void ShuffleCardPool()
        {
            // Fisher-Yates shuffle
            for (int i = _availableCardPool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                CardData temp = _availableCardPool[i];
                _availableCardPool[i] = _availableCardPool[j];
                _availableCardPool[j] = temp;
            }
        }
        
        private void OnGamePhaseChanged(GamePhase newPhase)
        {
            if (newPhase == GamePhase.Planning)
            {
                // Draw new cards at the start of Planning phase
                DrawCards(cardsPerDraw);
            }
        }
        
        public void DrawCards(int count)
        {
            // Don't draw more than max hand size
            int cardsToDraw = Mathf.Min(count, maxHandSize - _hand.Count);
            
            if (cardsToDraw <= 0)
                return;
                
            // Check if we need to reshuffle
            if (_availableCardPool.Count < cardsToDraw)
            {
                // Move discarded cards back to available pool
                _availableCardPool.AddRange(_discardedCards.Select(c => c.cardData));
                _discardedCards.Clear();
                
                // Shuffle the pool
                ShuffleCardPool();
                
                // If still not enough cards, adjust draw count
                cardsToDraw = Mathf.Min(cardsToDraw, _availableCardPool.Count);
            }
            
            // Draw cards
            for (int i = 0; i < cardsToDraw; i++)
            {
                // Draw a card from the pool
                CardData cardData = _availableCardPool[0];
                _availableCardPool.RemoveAt(0);
                
                // Create card instance
                CardInstance cardInstance = new CardInstance(cardData);
                _hand.Add(cardInstance);
                
                // Create visual
                CreateCardVisual(cardInstance);
            }
            
            // Notify listeners
            onHandUpdated.Invoke(_hand);
            
            // Arrange hand
            ArrangeHand();
        }
        
        private void CreateCardVisual(CardInstance cardInstance)
        {
            // Instantiate card visual
            CardDisplay cardDisplay = Instantiate(cardDisplayPrefab, handContainer);
            
            // Set up the card
            cardDisplay.SetupCard(cardInstance, this);
            
            // Store reference
            _cardDisplays[cardInstance.id] = cardDisplay;
        }
        
        private void ArrangeHand()
        {
            // Position cards in an arc
            float arcWidth = _hand.Count * 1.2f;
            float arcHeight = 0.5f;
            
            for (int i = 0; i < _hand.Count; i++)
            {
                CardInstance card = _hand[i];
                if (_cardDisplays.TryGetValue(card.id, out CardDisplay display))
                {
                    // Calculate position in arc
                    float t = _hand.Count > 1 ? i / (_hand.Count - 1f) : 0.5f;
                    float x = Mathf.Lerp(-arcWidth/2, arcWidth/2, t);
                    float y = arcHeight * Mathf.Sin(Mathf.PI * t);
                    
                    // Set position
                    display.SetTargetPosition(new Vector3(x, y, 0));
                    
                    // Set rotation
                    float angle = Mathf.Lerp(-15, 15, t);
                    display.SetTargetRotation(Quaternion.Euler(0, 0, angle));
                    
                    // Set depth order
                    display.SetDepthOffset(-i * 0.01f);
                }
            }
        }
        
        public void PlayCard(CardInstance card)
        {
            if (!_hand.Contains(card))
                return;
                
            // Try to spend resources
            ResourceManager resourceManager = ResourceManager.Instance;
            if (!resourceManager.CanSpend(card.cardData.researchPointCost, card.cardData.communityPointCost))
            {
                // Not enough resources
                Debug.Log($"Not enough resources to play {card.cardData.cardName}");
                return;
            }
            
            // Spend resources
            resourceManager.SpendResources(card.cardData.researchPointCost, card.cardData.communityPointCost);
            
            // Play the card
            if (card.Play())
            {
                // Move from hand to played pile
                _hand.Remove(card);
                _playedCards.Add(card);
                
                // Handle visual
                if (_cardDisplays.TryGetValue(card.id, out CardDisplay display))
                {
                    display.PlayAnimation();
                }
                
                // Notify listeners
                onCardPlayed.Invoke(card);
                onHandUpdated.Invoke(_hand);
                
                // Rearrange hand
                ArrangeHand();
            }
            else
            {
                // Failed to play, refund resources
                resourceManager.AddResources(card.cardData.researchPointCost, card.cardData.communityPointCost);
            }
        }
        
        public void DiscardCard(CardInstance card)
        {
            if (!_hand.Contains(card))
                return;
                
            // Move from hand to discard pile
            _hand.Remove(card);
            _discardedCards.Add(card);
            
            // Handle visual
            if (_cardDisplays.TryGetValue(card.id, out CardDisplay display))
            {
                display.DiscardAnimation();
                _cardDisplays.Remove(card.id);
            }
            
            // Notify listeners
            onCardDiscarded.Invoke(card);
            onHandUpdated.Invoke(_hand);
            
            // Rearrange hand
            ArrangeHand();
        }
    }
}