using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MetaBalance.Core;
using MetaBalance.Characters;
using MetaBalance.Cards;
using MetaBalance.Events;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Manages all UI elements and interactions
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Main UI Panels")]
        [SerializeField] private GameObject mainGamePanel;
        [SerializeField] private GameObject characterPanel;
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private GameObject analyticsPanel;
        
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI weekText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI researchPointsText;
        [SerializeField] private TextMeshProUGUI communityPointsText;
        [SerializeField] private Slider satisfactionSlider;
        [SerializeField] private Image satisfactionFill;
        [SerializeField] private Gradient satisfactionGradient;
        
        [Header("Character Panel")]
        [SerializeField] private Transform characterTabsContainer;
        [SerializeField] private GameObject characterTabPrefab;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI characterDescriptionText;
        [SerializeField] private Transform statBarContainer;
        [SerializeField] private GameObject statBarPrefab;
        [SerializeField] private Image winRateGauge;
        [SerializeField] private TextMeshProUGUI winRateText;
        [SerializeField] private Image pickRateGauge;
        [SerializeField] private TextMeshProUGUI pickRateText;
        
        [Header("Control Buttons")]
        [SerializeField] private Button nextPhaseButton;
        [SerializeField] private Button analyticsButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button helpButton;
        
        [Header("Event Panel")]
        [SerializeField] private TextMeshProUGUI eventTitleText;
        [SerializeField] private TextMeshProUGUI eventDescriptionText;
        [SerializeField] private Image eventImage;
        [SerializeField] private Transform eventOptionsContainer;
        [SerializeField] private GameObject eventOptionPrefab;
        
        [Header("Analytics Panel")]
        [SerializeField] private Button closeAnalyticsButton;
        [SerializeField] private Button charactersTabButton;
        [SerializeField] private Button communityTabButton;
        [SerializeField] private Button metaTabButton;
        
        // References to managers
        private GameManager _gameManager;
        private CharacterManager _characterManager;
        private ResourceManager _resourceManager;
        private EventManager _eventManager;
        
        // Character UI references
        private Dictionary<CharacterType, GameObject> _characterTabs = new Dictionary<CharacterType, GameObject>();
        private Dictionary<CharacterStat, GameObject> _statBars = new Dictionary<CharacterStat, GameObject>();
        
        // Event option buttons
        private List<GameObject> _eventOptionButtons = new List<GameObject>();
        
        private void Start()
        {
            // Get references to managers
            _gameManager = GameManager.Instance;
            _characterManager = CharacterManager.Instance;
            _resourceManager = ResourceManager.Instance;
            _eventManager = EventManager.Instance;
            
            // Subscribe to events
            SubscribeToEvents();
            
            // Set up UI elements
            SetupCharacterPanel();
            SetupButtonHandlers();
            
            // Show initial panels
            ShowPanel(mainGamePanel);
            HidePanel(eventPanel);
            HidePanel(analyticsPanel);
            
            // Update UI for initial state
            UpdateTopBar(_gameManager.GetGameState());
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            if (_gameManager != null)
            {
                _gameManager.onGameStateChanged.AddListener(OnGameStateChanged);
                _gameManager.onPhaseChanged.AddListener(OnPhaseChanged);
                _gameManager.onWeekChanged.AddListener(OnWeekChanged);
            }
            
            if (_characterManager != null)
            {
                _characterManager.onCharacterSelected.AddListener(OnCharacterSelected);
                _characterManager.onCharacterStatsChanged.AddListener(OnCharacterStatsChanged);
                _characterManager.onAllCharactersUpdated.AddListener(OnAllCharactersUpdated);
            }
            
            if (_resourceManager != null)
            {
                _resourceManager.onResourceChanged.AddListener(OnResourceChanged);
            }
            
            if (_eventManager != null)
            {
                _eventManager.onEventTriggered.AddListener(OnEventTriggered);
                _eventManager.onEventResolved.AddListener(OnEventResolved);
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (_gameManager != null)
            {
                _gameManager.onGameStateChanged.RemoveListener(OnGameStateChanged);
                _gameManager.onPhaseChanged.RemoveListener(OnPhaseChanged);
                _gameManager.onWeekChanged.RemoveListener(OnWeekChanged);
            }
            
            if (_characterManager != null)
            {
                _characterManager.onCharacterSelected.RemoveListener(OnCharacterSelected);
                _characterManager.onCharacterStatsChanged.RemoveListener(OnCharacterStatsChanged);
                _characterManager.onAllCharactersUpdated.RemoveListener(OnAllCharactersUpdated);
            }
            
            if (_resourceManager != null)
            {
                _resourceManager.onResourceChanged.RemoveListener(OnResourceChanged);
            }
            
            if (_eventManager != null)
            {
                _eventManager.onEventTriggered.RemoveListener(OnEventTriggered);
                _eventManager.onEventResolved.RemoveListener(OnEventResolved);
            }
        }
        
        private void SetupCharacterPanel()
        {
            // Create character tabs
            foreach (CharacterType characterType in System.Enum.GetValues(typeof(CharacterType)))
            {
                GameObject tabObj = Instantiate(characterTabPrefab, characterTabsContainer);
                Button tabButton = tabObj.GetComponent<Button>();
                TextMeshProUGUI tabText = tabObj.GetComponentInChildren<TextMeshProUGUI>();
                
                // Set tab text
                tabText.text = characterType.ToString();
                
                // Set tab color based on character type
                Image tabImage = tabObj.GetComponent<Image>();
                switch (characterType)
                {
                    case CharacterType.Warrior:
                        tabImage.color = new Color(0.8f, 0.2f, 0.2f);
                        break;
                    case CharacterType.Mage:
                        tabImage.color = new Color(0.2f, 0.2f, 0.8f);
                        break;
                    case CharacterType.Support:
                        tabImage.color = new Color(0.2f, 0.8f, 0.2f);
                        break;
                    case CharacterType.Tank:
                        tabImage.color = new Color(0.8f, 0.8f, 0.2f);
                        break;
                }
                
                // Set tab click handler
                tabButton.onClick.AddListener(() => {
                    if (_characterManager != null)
                    {
                        _characterManager.SelectCharacter(characterType);
                    }
                });
                
                // Store tab reference
                _characterTabs[characterType] = tabObj;
            }
            
            // Create stat bars
            foreach (CharacterStat stat in new[] { CharacterStat.Health, CharacterStat.Damage, CharacterStat.Speed, CharacterStat.Utility })
            {
                GameObject barObj = Instantiate(statBarPrefab, statBarContainer);
                
                // Set bar label
                TextMeshProUGUI labelText = barObj.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                labelText.text = stat.ToString();
                
                // Set bar color based on stat type
                Image barFill = barObj.transform.Find("Bar/Fill").GetComponent<Image>();
                switch (stat)
                {
                    case CharacterStat.Health:
                        barFill.color = new Color(0.2f, 0.8f, 0.2f); // Green
                        break;
                    case CharacterStat.Damage:
                        barFill.color = new Color(0.8f, 0.2f, 0.2f); // Red
                        break;
                    case CharacterStat.Speed:
                        barFill.color = new Color(0.2f, 0.2f, 0.8f); // Blue
                        break;
                    case CharacterStat.Utility:
                        barFill.color = new Color(0.8f, 0.8f, 0.2f); // Yellow
                        break;
                }
                
                // Store bar reference
                _statBars[stat] = barObj;
            }
        }
        
        private void SetupButtonHandlers()
        {
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.AddListener(() => {
                    if (_gameManager != null)
                    {
                        _gameManager.AdvanceToNextPhase();
                    }
                });
            }
            
            if (analyticsButton != null)
            {
                analyticsButton.onClick.AddListener(() => {
                    ShowAnalyticsPanel();
                });
            }
            
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(() => {
                    if (_gameManager != null)
                    {
                        _gameManager.SaveGame("slot1");
                    }
                });
            }
            
            if (helpButton != null)
            {
                helpButton.onClick.AddListener(() => {
                    ShowHelpPanel();
                });
            }
            
            if (closeAnalyticsButton != null)
            {
                closeAnalyticsButton.onClick.AddListener(() => {
                    HidePanel(analyticsPanel);
                    ShowPanel(mainGamePanel);
                });
            }
            
            if (charactersTabButton != null)
            {
                charactersTabButton.onClick.AddListener(() => {
                    // Switch to characters tab in analytics
                });
            }
            
            if (communityTabButton != null)
            {
                communityTabButton.onClick.AddListener(() => {
                    // Switch to community tab in analytics
                });
            }
            
            if (metaTabButton != null)
            {
                metaTabButton.onClick.AddListener(() => {
                    // Switch to meta tab in analytics
                });
            }
        }
        
        // Event handlers
        
        private void OnGameStateChanged(GameState newState)
        {
            UpdateTopBar(newState);
        }
        
        private void OnPhaseChanged(GamePhase newPhase)
        {
            // Update phase text
            if (phaseText != null)
            {
                phaseText.text = $"Phase: {newPhase}";
            }
            
            // Update button text based on phase
            if (nextPhaseButton != null)
            {
                TextMeshProUGUI buttonText = nextPhaseButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    switch (newPhase)
                    {
                        case GamePhase.Planning:
                            buttonText.text = "Implement Changes";
                            break;
                        case GamePhase.Implementation:
                            buttonText.text = "View Feedback";
                            break;
                        case GamePhase.Feedback:
                            buttonText.text = "Check Events";
                            break;
                        case GamePhase.Event:
                            buttonText.text = "Next Week";
                            break;
                    }
                }
            }
            
            // Enable/disable button during event phase
            if (newPhase == GamePhase.Event && _eventManager != null && _eventManager.IsEventPending())
            {
                nextPhaseButton.interactable = false;
            }
            else
            {
                nextPhaseButton.interactable = true;
            }
            
            // Show/hide panels based on phase
            if (newPhase == GamePhase.Event)
            {
                // Event panel will be shown by OnEventTriggered
            }
            else
            {
                ShowPanel(mainGamePanel);
                HidePanel(eventPanel);
            }
        }
        
        private void OnWeekChanged(int newWeek)
        {
            // Update week text
            if (weekText != null)
            {
                weekText.text = $"Week {newWeek}";
            }
        }
        
        private void OnCharacterSelected(GameCharacter character)
        {
            // Update character panel
            UpdateCharacterPanel(character);
            
            // Update tab selection visual
            UpdateCharacterTabs(character.GetCharacterType());
        }
        
        private void OnCharacterStatsChanged(GameCharacter character)
        {
            // Update character panel if this is the selected character
            if (_characterManager != null && _characterManager.GetSelectedCharacter() == character)
            {
                UpdateCharacterPanel(character);
            }
            
            // Update overall satisfaction meter
            UpdateSatisfactionMeter();
        }
        
        private void OnAllCharactersUpdated()
        {
            // Update satisfaction meter
            UpdateSatisfactionMeter();
        }
        
        private void OnResourceChanged(ResourceChangeEvent changeEvent)
        {
            // Update resource display
            if (changeEvent.ResourceType == ResourceType.ResearchPoints)
            {
                if (researchPointsText != null)
                {
                    researchPointsText.text = $"RP: {changeEvent.NewValue} (+{changeEvent.GenerationRate})";
                }
            }
            else
            {
                if (communityPointsText != null)
                {
                    communityPointsText.text = $"CP: {changeEvent.NewValue} (+{changeEvent.GenerationRate})";
                }
            }
        }
        
        private void OnEventTriggered(GameEvent gameEvent)
        {
            ShowEventPanel(gameEvent);
        }
        
        private void OnEventResolved(GameEvent gameEvent, EventOption option)
        {
            // Hide event panel
            HidePanel(eventPanel);
            ShowPanel(mainGamePanel);
        }
        
        // UI update methods
        
        private void UpdateTopBar(GameState state)
        {
            // Update week and phase
            if (weekText != null)
            {
                weekText.text = $"Week {state.CurrentWeek}";
            }
            
            if (phaseText != null)
            {
                phaseText.text = $"Phase: {state.CurrentPhase}";
            }
        }
        
        private void UpdateCharacterPanel(GameCharacter character)
        {
            // Update name and description
            if (characterNameText != null)
            {
                characterNameText.text = character.GetCharacterName();
            }
            
            if (characterDescriptionText != null)
            {
                characterDescriptionText.text = character.Data.description;
            }
            
            // Update stat bars
            foreach (CharacterStat stat in new[] { CharacterStat.Health, CharacterStat.Damage, CharacterStat.Speed, CharacterStat.Utility })
            {
                if (_statBars.TryGetValue(stat, out GameObject barObj))
                {
                    Slider barSlider = barObj.transform.Find("Bar").GetComponent<Slider>();
                    TextMeshProUGUI valueText = barObj.transform.Find("Value").GetComponent<TextMeshProUGUI>();
                    
                    // Update bar value (normalized to 0-1 range)
                    float statValue = character.GetStat(stat);
                    float normalizedValue = statValue / 100f;
                    barSlider.value = normalizedValue;
                    
                    // Update value text
                    valueText.text = $"{statValue:F0}";
                }
            }
            
            // Update win rate and pick rate
            float winRate = character.GetStat(CharacterStat.WinRate);
            float popularity = character.GetStat(CharacterStat.Popularity);
            
            if (winRateText != null)
            {
                winRateText.text = $"{winRate:F1}%";
            }
            
            if (pickRateText != null)
            {
                pickRateText.text = $"{popularity:F1}%";
            }
            
            if (winRateGauge != null)
            {
                winRateGauge.fillAmount = winRate / 100f;
                
                // Color based on balance state
                if (winRate > 55)
                {
                    winRateGauge.color = new Color(0.8f, 0.2f, 0.2f); // Red for overpowered
                }
                else if (winRate < 45)
                {
                    winRateGauge.color = new Color(0.2f, 0.2f, 0.8f); // Blue for underpowered
                }
                else
                {
                    winRateGauge.color = new Color(0.2f, 0.8f, 0.2f); // Green for balanced
                }
            }
            
            if (pickRateGauge != null)
            {
                pickRateGauge.fillAmount = popularity / 100f;
            }
        }

        private void UpdateCharacterTabs(CharacterType selectedType)
        {
            foreach (var kvp in _characterTabs)
            {
                // Extract the key (CharacterType) and value (GameObject) for clarity
                CharacterType tabCharacterType = kvp.Key;
                GameObject tabGameObject = kvp.Value;

                // Get the tab's button and image components from the GameObject
                Button tabButton = tabGameObject.GetComponent<Button>();
                Image tabImage = tabGameObject.GetComponent<Image>();

                if (tabButton == null || tabImage == null)
                    continue;

                // Highlight selected tab
                if (tabCharacterType == selectedType)
                {
                    // Make selected tab brighter
                    ColorBlock colors = tabButton.colors;
                    colors.normalColor = tabImage.color;
                    colors.highlightedColor = tabImage.color;
                    colors.selectedColor = tabImage.color;
                    tabButton.colors = colors;

                    // Add selected visual effect
                    tabGameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }
                else
                {
                    // Dim non-selected tabs
                    ColorBlock colors = tabButton.colors;
                    colors.normalColor = new Color(tabImage.color.r * 0.7f, tabImage.color.g * 0.7f,
                        tabImage.color.b * 0.7f);
                    tabButton.colors = colors;

                    // Remove selected visual effect
                    tabGameObject.transform.localScale = Vector3.one;
                }
            }
        }

        private void UpdateSatisfactionMeter()
        {
            if (_characterManager == null || satisfactionSlider == null)
                return;
                
            // Calculate satisfaction
            float satisfaction = _characterManager.CalculatePlayerSatisfaction();
            
            // Update slider
            satisfactionSlider.value = satisfaction;
            
            // Update color
            if (satisfactionFill != null && satisfactionGradient != null)
            {
                satisfactionFill.color = satisfactionGradient.Evaluate(satisfaction);
            }
        }
        
        private void ShowEventPanel(GameEvent gameEvent)
        {
            // Hide main game panel and show event panel
            HidePanel(mainGamePanel);
            ShowPanel(eventPanel);
            
            // Update event information
            if (eventTitleText != null)
            {
                eventTitleText.text = gameEvent.eventTitle;
            }
            
            if (eventDescriptionText != null)
            {
                eventDescriptionText.text = gameEvent.eventDescription;
            }
            
            if (eventImage != null && gameEvent.eventImage != null)
            {
                eventImage.sprite = gameEvent.eventImage;
                eventImage.enabled = true;
            }
            else if (eventImage != null)
            {
                eventImage.enabled = false;
            }
            
            // Clear previous option buttons
            foreach (GameObject button in _eventOptionButtons)
            {
                Destroy(button);
            }
            _eventOptionButtons.Clear();
            
            // Create new option buttons
            foreach (EventOption option in gameEvent.options)
            {
                GameObject optionObj = Instantiate(eventOptionPrefab, eventOptionsContainer);
                
                // Set up button text
                TextMeshProUGUI optionText = optionObj.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                if (optionText != null)
                {
                    optionText.text = option.optionText;
                }
                
                // Set up button description
                TextMeshProUGUI resultText = optionObj.transform.Find("Result").GetComponent<TextMeshProUGUI>();
                if (resultText != null)
                {
                    resultText.text = option.resultText;
                }
                
                // Set up cost display
                TextMeshProUGUI costText = optionObj.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
                if (costText != null)
                {
                    string costString = "";
                    if (option.researchPointsCost > 0)
                    {
                        costString += $"RP: {option.researchPointsCost} ";
                    }
                    if (option.communityPointsCost > 0)
                    {
                        costString += $"CP: {option.communityPointsCost}";
                    }
                    costText.text = costString;
                }
                
                // Set up button click handler
                Button optionButton = optionObj.GetComponent<Button>();
                if (optionButton != null)
                {
                    optionButton.onClick.AddListener(() => {
                        if (_eventManager != null)
                        {
                            _eventManager.ResolveEvent(option);
                        }
                    });
                    
                    // Enable/disable based on resource availability
                    if (_resourceManager != null)
                    {
                        bool canAfford = _resourceManager.CanSpend(option.researchPointsCost, option.communityPointsCost);
                        optionButton.interactable = canAfford;
                    }
                }
                
                // Add to list for cleanup
                _eventOptionButtons.Add(optionObj);
            }
        }
        
        public void ShowAnalyticsPanel()
        {
            HidePanel(mainGamePanel);
            HidePanel(eventPanel);
            ShowPanel(analyticsPanel);
            
            // Update analytics data
            UpdateAnalyticsData();
        }
        
        private void UpdateAnalyticsData()
        {
            // In a full implementation, this would update all the graphs and analytics data
            Debug.Log("UpdateAnalyticsData called");
        }
        
        private void ShowHelpPanel()
        {
            // In a full implementation, this would show a help/tutorial panel
            Debug.Log("ShowHelpPanel called");
        }
        
        // Helper methods
        
        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }
        
        private void HidePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}