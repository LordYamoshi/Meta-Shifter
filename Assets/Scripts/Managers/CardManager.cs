using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Manages player hand, deck, and card playing
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance { get; private set; }
        
        [Header("Card Database")]
        [SerializeField] private List<CardData> allAvailableCards = new List<CardData>();
        
        [Header("Player Collection")]
        [SerializeField] private List<CardData> playerDeck = new List<CardData>();
        [SerializeField] private List<CardData> currentHand = new List<CardData>();
        
        [Header("Settings")]
        [SerializeField] private int maxHandSize = 7;
        [SerializeField] private int cardsPerPhase = 2;
        [SerializeField] private int startingDeckSize = 20;
        
        [Header("Events")]
        public UnityEvent<List<CardData>> OnHandChanged;
        public UnityEvent<CardData> OnCardPlayed;
        public UnityEvent<List<CardData>> OnCardsEarned;
        
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
            
            BuildStartingDeck();
            DrawInitialHand();
        }
        
        private void BuildStartingDeck()
        {
            playerDeck.Clear();
            
            // Add basic cards of each type
            var commonCards = allAvailableCards.Where(card => card.rarity == CardRarity.Common).ToList();
            
            // Add random common cards to reach starting deck size
            for (int i = 0; i < startingDeckSize && commonCards.Count > 0; i++)
            {
                var randomCard = commonCards[Random.Range(0, commonCards.Count)];
                playerDeck.Add(randomCard);
            }
            
            ShuffleDeck();
            Debug.Log($"Built starting deck with {playerDeck.Count} cards");
        }
        
        private void DrawInitialHand()
        {
            DrawCards(maxHandSize);
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Planning)
            {
                // Draw new cards and earn cards each planning phase
                EarnNewCards();
                DrawCards(cardsPerPhase);
            }
        }
        
        private void EarnNewCards()
        {
            List<CardData> earnedCards = new List<CardData>();
            
            // Determine how many cards to earn (base + performance bonus)
            int cardsToEarn = cardsPerPhase;
            
            // Get current week to determine what rarities are available
            int currentWeek = Core.PhaseManager.Instance?.GetCurrentWeek() ?? 1;
            
            for (int i = 0; i < cardsToEarn; i++)
            {
                CardData newCard = SelectRandomCard(currentWeek);
                if (newCard != null)
                {
                    playerDeck.Add(newCard);
                    earnedCards.Add(newCard);
                }
            }
            
            if (earnedCards.Count > 0)
            {
                OnCardsEarned.Invoke(earnedCards);
                Debug.Log($"Earned {earnedCards.Count} new cards: {string.Join(", ", earnedCards.Select(c => c.cardName))}");
            }
        }
        
        private CardData SelectRandomCard(int currentWeek)
        {
            // Filter cards by what should be available at current week
            var availableCards = allAvailableCards.Where(card => IsCardAvailable(card, currentWeek)).ToList();
            
            if (availableCards.Count == 0) return null;
            
            // Weight by rarity (common cards more likely)
            var weightedCards = new List<(CardData card, float weight)>();
            
            foreach (var card in availableCards)
            {
                float weight = card.rarity switch
                {
                    CardRarity.Common => 0.5f,
                    CardRarity.Uncommon => 0.3f,
                    CardRarity.Rare => 0.15f,
                    CardRarity.Epic => 0.04f,
                    CardRarity.Special => 0.01f,
                    _ => 0.1f
                };
                
                weightedCards.Add((card, weight));
            }
            
            // Select based on weighted random
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
            // Unlock cards based on week and rarity
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
        
        public void DrawCards(int count)
        {
            for (int i = 0; i < count && currentHand.Count < maxHandSize; i++)
            {
                if (playerDeck.Count == 0)
                {
                    Debug.Log("No more cards in deck to draw");
                    break;
                }
                
                // Draw from deck
                var drawnCard = playerDeck[0];
                playerDeck.RemoveAt(0);
                currentHand.Add(drawnCard);
            }
            
            OnHandChanged.Invoke(currentHand);
            Debug.Log($"Drew {count} cards. Hand size: {currentHand.Count}");
        }
        
        public bool PlayCard(CardData card)
        {
            if (!currentHand.Contains(card))
            {
                Debug.Log("Card not in hand!");
                return false;
            }
            
            // Check if player can afford the card
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager == null || !resourceManager.CanSpend(card.researchPointCost, card.communityPointCost))
            {
                Debug.Log($"Cannot afford {card.cardName}: needs {card.researchPointCost} RP, {card.communityPointCost} CP");
                return false;
            }
            
            // Pay the cost
            resourceManager.SpendResources(card.researchPointCost, card.communityPointCost);
            
            // Play the card effect
            card.PlayCard();
            
            // Remove from hand
            currentHand.Remove(card);
            
            OnCardPlayed.Invoke(card);
            OnHandChanged.Invoke(currentHand);
            
            Debug.Log($"Played {card.cardName}");
            return true;
        }
        
        public bool PlayCardByIndex(int handIndex)
        {
            if (handIndex < 0 || handIndex >= currentHand.Count)
            {
                Debug.Log($"Invalid hand index: {handIndex}");
                return false;
            }
            
            return PlayCard(currentHand[handIndex]);
        }
        
        private void ShuffleDeck()
        {
            for (int i = playerDeck.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                var temp = playerDeck[i];
                playerDeck[i] = playerDeck[randomIndex];
                playerDeck[randomIndex] = temp;
            }
        }
        
        // Public getters
        public List<CardData> GetCurrentHand() => new List<CardData>(currentHand);
        public List<CardData> GetDeck() => new List<CardData>(playerDeck);
        public int GetHandSize() => currentHand.Count;
        public int GetDeckSize() => playerDeck.Count;
        
        // Debug methods
        [ContextMenu("Draw Card")]
        public void DebugDrawCard()
        {
            DrawCards(1);
        }
        
        [ContextMenu("Play First Card")]
        public void DebugPlayFirstCard()
        {
            if (currentHand.Count > 0)
            {
                PlayCard(currentHand[0]);
            }
        }
        
        [ContextMenu("Add Random Card to Deck")]
        public void DebugAddRandomCard()
        {
            if (allAvailableCards.Count > 0)
            {
                var randomCard = allAvailableCards[Random.Range(0, allAvailableCards.Count)];
                playerDeck.Add(randomCard);
                Debug.Log($"Added {randomCard.cardName} to deck");
            }
        }
    }
}