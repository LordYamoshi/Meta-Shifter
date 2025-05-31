using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Updated UI manager that works with the prefab-based card system
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Top UI")]
        [SerializeField] private TextMeshProUGUI weekPhaseText;
        [SerializeField] private TextMeshProUGUI rpText;
        [SerializeField] private TextMeshProUGUI cpText;
        [SerializeField] private Slider satisfactionSlider;
        [SerializeField] private TextMeshProUGUI satisfactionText;
        [SerializeField] private Button nextPhaseButton;
        [SerializeField] private TextMeshProUGUI nextPhaseButtonText;
        
        [Header("Character Display")]
        [SerializeField] private Transform characterStatsContainer;
        [SerializeField] private List<CharacterUIPanel> characterPanels = new List<CharacterUIPanel>();
        
        [Header("Drop Zone")]
        [SerializeField] private Cards.CardDropZone dropZone;
        [SerializeField] private TextMeshProUGUI dropZoneInstructionText;
        
        [Header("Debug")]
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private Button drawCardButton;
        [SerializeField] private Button clearHandButton;
        
        // Track current selection
        private Characters.CharacterType selectedCharacter = Characters.CharacterType.Warrior;
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            
            // Initialize community satisfaction
            InitializeCommunitySystem();
            
            UpdateDisplay();
        }
        
        private void InitializeCommunitySystem()
        {
            // Initialize satisfaction slider to 65% (good starting value)
            if (satisfactionSlider != null)
            {
                satisfactionSlider.value = 0.65f;
            }
            
            if (satisfactionText != null)
            {
                satisfactionText.text = "65.0%";
            }
            
            Debug.Log("Initialized community satisfaction to 65%");
        }
        
        private void SetupUI()
        {
            // Setup next phase button
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.AddListener(() => {
                    if (Core.PhaseManager.Instance != null)
                    {
                        Core.PhaseManager.Instance.AdvancePhase();
                    }
                });
            }
            
            // Setup debug buttons
            if (drawCardButton != null)
            {
                drawCardButton.onClick.AddListener(() => {
                    if (Cards.CardManager.Instance != null)
                    {
                        Cards.CardManager.Instance.DebugDrawCard();
                    }
                });
            }
            
            if (clearHandButton != null)
            {
                clearHandButton.onClick.AddListener(() => {
                    if (Cards.CardManager.Instance != null)
                    {
                        Cards.CardManager.Instance.DebugClearHand();
                    }
                });
            }
            
            // Setup character panel click handlers
            SetupCharacterPanels();
            
            // Initialize satisfaction slider
            if (satisfactionSlider != null)
            {
                satisfactionSlider.value = 0.65f; // 65% starting satisfaction
            }
        }
        
        private void SetupCharacterPanels()
        {
            for (int i = 0; i < characterPanels.Count; i++)
            {
                if (characterPanels[i] != null)
                {
                    Characters.CharacterType characterType = (Characters.CharacterType)i;
                    characterPanels[i].Setup(characterType, () => SelectCharacter(characterType));
                    
                    // Set first character as selected
                    if (i == 0)
                    {
                        characterPanels[i].SetSelected(true);
                    }
                }
            }
        }
        
        private void SelectCharacter(Characters.CharacterType character)
        {
            selectedCharacter = character;
            
            // Update visual selection
            for (int i = 0; i < characterPanels.Count; i++)
            {
                if (characterPanels[i] != null)
                {
                    characterPanels[i].SetSelected((Characters.CharacterType)i == character);
                }
            }
            
            UpdateDebugText($"Selected {character}");
        }
        
        private void SubscribeToEvents()
        {
            // Phase manager events
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
                Core.PhaseManager.Instance.OnPhaseButtonTextChanged.AddListener(OnPhaseButtonTextChanged);
            }
            
            // Resource manager events
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
                Core.ResourceManager.Instance.OnResourcesGenerated.AddListener(OnResourcesGenerated);
            }
            
            // Character manager events
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnOverallBalanceChanged);
            }
            
            // Card manager events
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.OnHandChanged.AddListener(OnHandChanged);
                Cards.CardManager.Instance.OnCardPlayed.AddListener(OnCardPlayed);
            }
            
            // Drop zone events
            if (dropZone != null)
            {
                dropZone.OnCardsChanged.AddListener(OnDropZoneChanged);
            }
        }
        
        private void UpdateDisplay()
        {
            UpdateTopUI();
            UpdateCharacterStats();
            UpdateDropZoneDisplay();
        }
        
        private void UpdateTopUI()
        {
            // Update week/phase display
            if (weekPhaseText != null)
            {
                var phaseManager = Core.PhaseManager.Instance;
                if (phaseManager != null)
                {
                    weekPhaseText.text = $"Week {phaseManager.GetCurrentWeek()} - {phaseManager.GetPhaseDisplayName()}";
                }
            }
            
            // Update resources display
            if (rpText != null || cpText != null)
            {
                var resourceManager = Core.ResourceManager.Instance;
                if (resourceManager != null)
                {
                    if (rpText != null)
                        rpText.text = $"RP: {resourceManager.ResearchPoints}";
                    if (cpText != null)
                        cpText.text = $"CP: {resourceManager.CommunityPoints}";
                }
            }
        }
        
        private void UpdateCharacterStats()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            for (int i = 0; i < characterPanels.Count; i++)
            {
                if (characterPanels[i] != null)
                {
                    Characters.CharacterType type = (Characters.CharacterType)i;
                    float winRate = characterManager.GetStat(type, Characters.CharacterStat.WinRate);
                    float popularity = characterManager.GetStat(type, Characters.CharacterStat.Popularity);
                    
                    characterPanels[i].UpdateStats(winRate, popularity);
                }
            }
        }
        
        private void UpdateDropZoneDisplay()
        {
            if (dropZoneInstructionText == null) return;
            
            var phaseManager = Core.PhaseManager.Instance;
            if (phaseManager == null) return;
            
            string instructionText = phaseManager.GetCurrentPhase() switch
            {
                Core.GamePhase.Planning => "Drop cards here to implement balance changes",
                Core.GamePhase.Implementation => "Cards are being implemented...",
                Core.GamePhase.Feedback => "Review the results of your changes",
                Core.GamePhase.Event => "Handle community events",
                _ => "Drop cards here to implement balance changes"
            };
            
            dropZoneInstructionText.text = instructionText;
        }
        
        // Event handlers
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            UpdateTopUI();
            UpdateDropZoneDisplay();
            UpdateDebugText($"Phase changed to {newPhase}");
            
            // Update card affordability when phase changes
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.RefreshAllCardsInHand();
            }
        }
        
        private void OnWeekChanged(int newWeek)
        {
            UpdateTopUI();
            UpdateDebugText($"Started week {newWeek}");
        }
        
        private void OnPhaseButtonTextChanged(string buttonText)
        {
            if (nextPhaseButtonText != null)
            {
                nextPhaseButtonText.text = buttonText;
            }
        }
        
        private void OnResourcesChanged(int rp, int cp)
        {
            UpdateTopUI();
            
            // Update card affordability when resources change
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.RefreshAllCardsInHand();
            }
        }
        
        private void OnResourcesGenerated(int rpGained, int cpGained)
        {
            UpdateDebugText($"Generated: +{rpGained} RP, +{cpGained} CP");
        }
        
        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            UpdateCharacterStats();
            UpdateDebugText($"{character} {stat}: {newValue:F1}");
        }
        
        private void OnOverallBalanceChanged(float balanceScore)
        {
            // Convert balance score (0-100) to satisfaction percentage
            float satisfactionPercentage = Mathf.Clamp(balanceScore, 0f, 100f);
            
            if (satisfactionSlider != null)
            {
                satisfactionSlider.value = satisfactionPercentage / 100f; // Convert to 0-1 range
            }
            
            if (satisfactionText != null)
            {
                satisfactionText.text = $"{satisfactionPercentage:F1}%";
            }
            
            Debug.Log($"Community satisfaction updated to {satisfactionPercentage:F1}%");
        }
        
        private void OnHandChanged(List<Cards.CardData> newHand)
        {
            UpdateDebugText($"Hand updated: {newHand.Count} cards");
        }
        
        private void OnCardPlayed(Cards.CardData card)
        {
            UpdateDebugText($"Played: {card.cardName}");
        }
        
        private void OnDropZoneChanged(List<Cards.CardData> queuedCards)
        {
            UpdateDebugText($"Queued cards: {queuedCards.Count}");
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
    
    /// <summary>
    /// Component for individual character UI panels
    /// </summary>
    [System.Serializable]
    public class CharacterUIPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI roleText;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private TextMeshProUGUI popularityText;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button selectButton;
        
        [Header("Visual Settings")]
        [SerializeField] private Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color selectedBackgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.9f);
        
        private Characters.CharacterType characterType;
        private System.Action onSelected;
        
        public void Setup(Characters.CharacterType type, System.Action onSelectCallback)
        {
            characterType = type;
            onSelected = onSelectCallback;
            
            // Setup UI
            if (characterNameText != null)
                characterNameText.text = type.ToString();
            
            if (roleText != null)
            {
                roleText.text = type switch
                {
                    Characters.CharacterType.Warrior => "Melee Fighter",
                    Characters.CharacterType.Mage => "Ranged Caster",
                    Characters.CharacterType.Support => "Utility",
                    Characters.CharacterType.Tank => "Defensive",
                    _ => "Unknown"
                };
            }
            
            // Setup button
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => onSelected?.Invoke());
            }
            
            // Setup icon color
            if (characterIcon != null)
            {
                characterIcon.color = type switch
                {
                    Characters.CharacterType.Warrior => new Color(0.9f, 0.3f, 0.3f), // Red
                    Characters.CharacterType.Mage => new Color(0.3f, 0.5f, 0.9f), // Blue
                    Characters.CharacterType.Support => new Color(0.3f, 0.9f, 0.5f), // Green
                    Characters.CharacterType.Tank => new Color(0.9f, 0.7f, 0.3f), // Orange
                    _ => Color.white
                };
            }
        }
        
        public void UpdateStats(float winRate, float popularity)
        {
            if (winRateText != null)
            {
                winRateText.text = $"{winRate:F1}%";
                
                // Color code win rate
                winRateText.color = winRate switch
                {
                    > 55f => Color.red,      // Overpowered
                    < 45f => Color.cyan,     // Underpowered  
                    _ => Color.green         // Balanced
                };
            }
            
            if (popularityText != null)
            {
                popularityText.text = $"Pop: {popularity:F0}%";
            }
        }
        
        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedBackgroundColor : normalBackgroundColor;
            }
        }
    }
}