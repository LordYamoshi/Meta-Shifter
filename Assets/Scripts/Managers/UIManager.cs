using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Simplified UIManager that reliably updates character stats
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
        
        private Characters.CharacterType selectedCharacter = Characters.CharacterType.Warrior;
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            InitializeCommunitySystem();
            
            // Force update after everything is ready
            Invoke(nameof(ForceCompleteUIUpdate), 0.3f);
            
            // Keep updating every second to ensure UI stays in sync
            InvokeRepeating(nameof(UpdateCharacterStats), 1f, 1f);
        }
        
        private void ForceCompleteUIUpdate()
        {
            FindAndSetupCharacterPanels();
            UpdateDisplay();
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.RecalculateWinRates();
            }
            
            UpdateCharacterStats();
        }
        
        private void FindAndSetupCharacterPanels()
        {
            // Clear and find all character panels
            characterPanels.Clear();
            
            // Method 1: Look in container
            if (characterStatsContainer != null)
            {
                var panelsInContainer = characterStatsContainer.GetComponentsInChildren<CharacterUIPanel>(true);
                characterPanels.AddRange(panelsInContainer);
            }
            
            // Method 2: Find all in scene if none found
            if (characterPanels.Count == 0)
            {
                var allPanels = FindObjectsOfType<CharacterUIPanel>(true);
                characterPanels.AddRange(allPanels);
            }
            
            // Setup each panel with correct character type
            for (int i = 0; i < characterPanels.Count && i < 4; i++)
            {
                if (characterPanels[i] != null)
                {
                    Characters.CharacterType characterType = (Characters.CharacterType)i;
                    characterPanels[i].ForceSetup(characterType, () => SelectCharacter(characterType));
                    
                    if (i == 0)
                    {
                        characterPanels[i].SetSelected(true);
                    }
                }
            }
            
            Debug.Log($"Found and setup {characterPanels.Count} character panels");
        }
        
        private void UpdateCharacterStats()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            // Update each panel with current data
            for (int i = 0; i < characterPanels.Count && i < 4; i++)
            {
                if (characterPanels[i] != null)
                {
                    Characters.CharacterType type = (Characters.CharacterType)i;
                    float winRate = characterManager.GetStat(type, Characters.CharacterStat.WinRate);
                    float popularity = characterManager.GetStat(type, Characters.CharacterStat.Popularity);
                    
                    characterPanels[i].ForceUpdateDisplay(winRate, popularity);
                }
            }
        }
        
        private void InitializeCommunitySystem()
        {
            if (satisfactionSlider != null)
                satisfactionSlider.value = 0.65f;
            if (satisfactionText != null)
                satisfactionText.text = "65.0%";
        }
        
        private void SetupUI()
        {
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.AddListener(() => {
                    if (Core.PhaseManager.Instance != null)
                        Core.PhaseManager.Instance.AdvancePhase();
                });
            }
            
            if (drawCardButton != null)
            {
                drawCardButton.onClick.AddListener(() => {
                    if (Cards.CardManager.Instance != null)
                        Cards.CardManager.Instance.DebugDrawCard();
                });
            }
            
            if (clearHandButton != null)
            {
                clearHandButton.onClick.AddListener(() => {
                    if (Cards.CardManager.Instance != null)
                        Cards.CardManager.Instance.DebugClearHand();
                });
            }
        }
        
        private void SelectCharacter(Characters.CharacterType character)
        {
            selectedCharacter = character;
            
            for (int i = 0; i < characterPanels.Count && i < 4; i++)
            {
                if (characterPanels[i] != null)
                {
                    characterPanels[i].SetSelected((Characters.CharacterType)i == character);
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnWeekChanged);
                Core.PhaseManager.Instance.OnPhaseButtonTextChanged.AddListener(OnPhaseButtonTextChanged);
            }
            
            if (Core.ResourceManager.Instance != null)
            {
                Core.ResourceManager.Instance.OnResourcesChanged.AddListener(OnResourcesChanged);
                Core.ResourceManager.Instance.OnResourcesGenerated.AddListener(OnResourcesGenerated);
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnOverallBalanceChanged);
            }
            
            if (Cards.CardManager.Instance != null)
            {
                Cards.CardManager.Instance.OnHandChanged.AddListener(OnHandChanged);
                Cards.CardManager.Instance.OnCardPlayed.AddListener(OnCardPlayed);
            }
            
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
            if (weekPhaseText != null)
            {
                var phaseManager = Core.PhaseManager.Instance;
                if (phaseManager != null)
                {
                    weekPhaseText.text = $"Week {phaseManager.GetCurrentWeek()} - {phaseManager.GetPhaseDisplayName()}";
                }
            }
            
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
            UpdateDisplay();
            if (Cards.CardManager.Instance != null)
                Cards.CardManager.Instance.RefreshAllCardsInHand();
        }
        
        private void OnWeekChanged(int newWeek)
        {
            UpdateTopUI();
        }
        
        private void OnPhaseButtonTextChanged(string buttonText)
        {
            if (nextPhaseButtonText != null)
                nextPhaseButtonText.text = buttonText;
        }
        
        private void OnResourcesChanged(int rp, int cp)
        {
            UpdateTopUI();
            if (Cards.CardManager.Instance != null)
                Cards.CardManager.Instance.RefreshAllCardsInHand();
        }
        
        private void OnResourcesGenerated(int rpGained, int cpGained)
        {
            UpdateDebugText($"Generated: +{rpGained} RP, +{cpGained} CP");
        }
        
        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            // Force immediate update
            UpdateCharacterStats();
            UpdateDebugText($"{character} {stat}: {newValue:F1}");
        }
        
        private void OnOverallBalanceChanged(float balanceScore)
        {
            float satisfactionPercentage = Mathf.Clamp(balanceScore, 0f, 100f);
            
            if (satisfactionSlider != null)
                satisfactionSlider.value = satisfactionPercentage / 100f;
            if (satisfactionText != null)
                satisfactionText.text = $"{satisfactionPercentage:F1}%";
        }
        
        private void OnHandChanged(List<Cards.CardData> newHand)
        {
            UpdateDebugText($"Hand updated: {newHand.Count} cards");
        }
        
        private void OnCardPlayed(Cards.CardData card)
        {
            UpdateDebugText($"Played: {card.cardName}");
            Invoke(nameof(UpdateCharacterStats), 0.1f);
        }
        
        private void OnDropZoneChanged(List<Cards.CardData> queuedCards)
        {
            UpdateDebugText($"Queued cards: {queuedCards.Count}");
        }
        
        private void UpdateDebugText(string message)
        {
            if (debugText != null)
                debugText.text = $"{System.DateTime.Now:HH:mm:ss} - {message}";
            Debug.Log(message);
        }
        
        // Public methods for other scripts
        public void UpdateResourceDisplay() => UpdateTopUI();
        public void RefreshUI() => UpdateDisplay();
        public void UpdateCharacterDisplay() => UpdateCharacterStats();
        public Characters.CharacterType GetSelectedCharacter() => selectedCharacter;
        public void SetSelectedCharacter(Characters.CharacterType character) => SelectCharacter(character);
        
        public void UpdateSatisfactionDisplay(float satisfactionPercentage)
        {
            if (satisfactionSlider != null)
                satisfactionSlider.value = satisfactionPercentage / 100f;
            if (satisfactionText != null)
                satisfactionText.text = $"{satisfactionPercentage:F1}%";
        }
        
        public void ShowDebugMessage(string message) => UpdateDebugText(message);
        
        // Debug methods
        [ContextMenu("Force Update Everything")]
        public void DebugForceUpdate()
        {
            ForceCompleteUIUpdate();
        }
    }
}