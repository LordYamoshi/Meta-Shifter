using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Simple UI manager for testing the core systems
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Top UI")]
        [SerializeField] private TextMeshProUGUI weekPhaseText;
        [SerializeField] private TextMeshProUGUI resourcesText;
        [SerializeField] private Button nextPhaseButton;
        
        [Header("Character Display")]
        [SerializeField] private Transform characterStatsContainer;
        [SerializeField] private GameObject characterStatPrefab;
        
        [Header("Card Hand")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private TextMeshProUGUI handInfoText;
        
        [Header("Debug")]
        [SerializeField] private Button drawCardButton;
        [SerializeField] private TextMeshProUGUI debugText;
        
        // UI References
        private Dictionary<Characters.CharacterType, GameObject> characterDisplays = new Dictionary<Characters.CharacterType, GameObject>();
        private List<GameObject> handButtons = new List<GameObject>();
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            UpdateDisplay();
        }
        
        private void SetupUI()
        {
            // Setup next phase button
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.AddListener(() => {
                    if (Core.PhaseManager.Instance != null)
                    {
                        Core.PhaseManager.Instance.AdvanceToNextPhase();
                    }
                });
            }
            
            // Setup draw card button
            if (drawCardButton != null)
            {
                drawCardButton.onClick.AddListener(() => {
                    if (Cards.CardManager.Instance != null)
                    {
                        Cards.CardManager.Instance.DebugDrawCard();
                    }
                });
            }
            
            CreateCharacterDisplays();
        }
        
        private void CreateCharacterDisplays()
        {
            if (characterStatsContainer == null) return;
            
            foreach (Characters.CharacterType type in System.Enum.GetValues(typeof(Characters.CharacterType)))
            {
                GameObject statDisplay = new GameObject($"{type}_Stats");
                statDisplay.transform.SetParent(characterStatsContainer, false);
                
                // Add vertical layout group
                var layoutGroup = statDisplay.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = 5f;
                layoutGroup.padding = new RectOffset(10, 10, 10, 10);
                
                // Add background
                var image = statDisplay.AddComponent<Image>();
                image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                // Character name
                GameObject nameObj = new GameObject("Name");
                nameObj.transform.SetParent(statDisplay.transform, false);
                var nameText = nameObj.AddComponent<TextMeshProUGUI>();
                nameText.text = type.ToString();
                nameText.fontSize = 18f;
                nameText.fontStyle = FontStyles.Bold;
                
                // Win rate (most important)
                GameObject winRateObj = new GameObject("WinRate");
                winRateObj.transform.SetParent(statDisplay.transform, false);
                var winRateText = winRateObj.AddComponent<TextMeshProUGUI>();
                winRateText.text = "Win Rate: 50%";
                winRateText.fontSize = 16f;
                
                // Popularity
                GameObject popObj = new GameObject("Popularity");
                popObj.transform.SetParent(statDisplay.transform, false);
                var popText = popObj.AddComponent<TextMeshProUGUI>();
                popText.text = "Popularity: 50%";
                popText.fontSize = 14f;
                
                characterDisplays[type] = statDisplay;
            }
        }
        
        private void SubscribeToEvents()
        {
            // Phase manager events
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
            }
            
            // Resource manager events
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
            }
            
            // Character manager events
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnCharacterStatChanged);
            }
            
            // Card manager events
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.OnHandChanged.AddListener(OnHandChanged);
                Cards.CardManager.Instance.OnCardPlayed.AddListener(OnCardPlayed);
                Cards.CardManager.Instance.OnCardsEarned.AddListener(OnCardsEarned);
            }
        }
        
        private void UpdateDisplay()
        {
            UpdateTopUI();
            UpdateCharacterStats();
            UpdateHandDisplay();
        }
        
        private void UpdateTopUI()
        {
            // Update week/phase display
            if (weekPhaseText != null)
            {
                var phaseManager = Core.PhaseManager.Instance;
                if (phaseManager != null)
                {
                    weekPhaseText.text = $"Week {phaseManager.GetCurrentWeek()} - {phaseManager.GetCurrentPhase()}";
                }
            }
            
            // Update resources display
            if (resourcesText != null)
            {
                var resourceManager = Core.ResourceManager.Instance;
                if (resourceManager != null)
                {
                    resourcesText.text = $"RP: {resourceManager.ResearchPoints} | CP: {resourceManager.CommunityPoints}";
                }
            }
        }
        
        private void UpdateCharacterStats()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            foreach (var kvp in characterDisplays)
            {
                Characters.CharacterType type = kvp.Key;
                GameObject display = kvp.Value;
                
                // Update win rate text
                var winRateText = display.transform.Find("WinRate")?.GetComponent<TextMeshProUGUI>();
                if (winRateText != null)
                {
                    float winRate = characterManager.GetStat(type, Characters.CharacterStat.WinRate);
                    winRateText.text = $"Win Rate: {winRate:F1}%";
                    
                    // Color code based on balance
                    winRateText.color = winRate switch
                    {
                        > 55f => Color.red,      // Overpowered
                        < 45f => Color.cyan,     // Underpowered  
                        _ => Color.green         // Balanced
                    };
                }
                
                // Update popularity text
                var popText = display.transform.Find("Popularity")?.GetComponent<TextMeshProUGUI>();
                if (popText != null)
                {
                    float popularity = characterManager.GetStat(type, Characters.CharacterStat.Popularity);
                    popText.text = $"Popularity: {popularity:F1}%";
                }
            }
        }
        
        private void UpdateHandDisplay()
        {
            // Clear existing hand buttons
            foreach (var button in handButtons)
            {
                if (button != null) Destroy(button);
            }
            handButtons.Clear();
            
            var cardManager = Cards.CardManager.Instance;
            if (cardManager == null || handContainer == null) return;
            
            var hand = cardManager.GetCurrentHand();
            
            // Update hand info
            if (handInfoText != null)
            {
                handInfoText.text = $"Hand: {hand.Count}/{cardManager.GetHandSize()} | Deck: {cardManager.GetDeckSize()}";
            }
            
            // Create button for each card in hand
            for (int i = 0; i < hand.Count; i++)
            {
                var card = hand[i];
                int cardIndex = i; // Capture for closure
                
                GameObject cardButton = new GameObject($"Card_{i}");
                cardButton.transform.SetParent(handContainer, false);
                
                // Add button component
                var button = cardButton.AddComponent<Button>();
                var image = cardButton.AddComponent<Image>();
                
                // Set color based on rarity
                image.color = card.rarity switch
                {
                    Cards.CardRarity.Common => new Color(0.8f, 0.8f, 0.8f),
                    Cards.CardRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                    Cards.CardRarity.Rare => new Color(0.2f, 0.2f, 1f),
                    Cards.CardRarity.Epic => new Color(0.8f, 0.2f, 0.8f),
                    Cards.CardRarity.Special => new Color(1f, 0.8f, 0.2f),
                    _ => Color.white
                };
                
                // Add text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(cardButton.transform, false);
                var cardText = textObj.AddComponent<TextMeshProUGUI>();
                cardText.text = $"{card.cardName}\n{card.researchPointCost}RP {card.communityPointCost}CP";
                cardText.fontSize = 12f;
                cardText.alignment = TextAlignmentOptions.Center;
                
                // Set text rect to fill button
                var textRect = cardText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                // Add click handler
                button.onClick.AddListener(() => {
                    cardManager.PlayCardByIndex(cardIndex);
                });
                
                // Check if card is playable
                var resourceManager = Core.ResourceManager.Instance;
                bool canPlay = resourceManager != null && 
                              resourceManager.CanSpend(card.researchPointCost, card.communityPointCost);
                
                button.interactable = canPlay;
                if (!canPlay)
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f);
                }
                
                handButtons.Add(cardButton);
            }
        }
        
        // Event handlers
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            UpdateTopUI();
            UpdateDebugText($"Phase changed to {newPhase}");
        }
        
        private void OnWeekChanged(int newWeek)
        {
            UpdateTopUI();
            UpdateDebugText($"Started week {newWeek}");
        }
        
        private void OnResourcesChanged(int rp, int cp)
        {
            UpdateTopUI();
            UpdateHandDisplay(); // Update card playability
        }
        
        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            UpdateCharacterStats();
            UpdateDebugText($"{character} {stat}: {newValue:F1}");
        }
        
        private void OnHandChanged(List<Cards.CardData> newHand)
        {
            UpdateHandDisplay();
        }
        
        private void OnCardPlayed(Cards.CardData card)
        {
            UpdateDebugText($"Played: {card.cardName}");
        }
        
        private void OnCardsEarned(List<Cards.CardData> earnedCards)
        {
            string cardNames = string.Join(", ", earnedCards.ConvertAll(c => c.cardName));
            UpdateDebugText($"Earned cards: {cardNames}");
        }
        
        private void UpdateDebugText(string message)
        {
            if (debugText != null)
            {
                debugText.text = $"{System.DateTime.Now:HH:mm:ss} - {message}";
            }
            Debug.Log(message);
        }
    }
}