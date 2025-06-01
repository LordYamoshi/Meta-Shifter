using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace MetaBalance.UI
{
    /// <summary>
    /// Complete UI manager with resource preview integration
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Top UI")]
        [SerializeField] private TextMeshProUGUI weekPhaseText;
        [SerializeField] private TextMeshProUGUI rpText;
        [SerializeField] private TextMeshProUGUI cpText;
        [SerializeField] private TextMeshProUGUI rpGenerationText; // "+10/turn" text
        [SerializeField] private TextMeshProUGUI cpGenerationText; // "+5/turn" text
        [SerializeField] private TextMeshProUGUI rpGainText; // "+4" indicator text
        [SerializeField] private Image rpGainBackground; // Green circle background for "+4"
        [SerializeField] private TextMeshProUGUI cpGainText; // "+4" indicator text  
        [SerializeField] private Image cpGainBackground; // Green circle background for "+4"
        [SerializeField] private TextMeshProUGUI totalCostText; // "Total Costs: 5 RP, 3 CP"
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
            
            // Initial display update
            UpdateDisplay();
            
            // Force additional updates to catch any initialization timing issues
            Invoke(nameof(UpdateDisplay), 0.1f);
            Invoke(nameof(UpdateDisplay), 0.5f);
            Invoke(nameof(UpdateDisplay), 1f);
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
            
            // Subscribe to ALL drop zones (not just one)
            SubscribeToAllDropZones();
            
            // Set up recurring check for new drop zones
            InvokeRepeating(nameof(SubscribeToAllDropZones), 1f, 2f);
        }
        
        /// <summary>
        /// Subscribe to all drop zones for cost updates
        /// </summary>
        private void SubscribeToAllDropZones()
        {
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                if (dropZone != null)
                {
                    // Remove listener first to avoid duplicates
                    dropZone.OnCardsChanged.RemoveListener(OnDropZoneChanged);
                    // Add listener
                    dropZone.OnCardsChanged.AddListener(OnDropZoneChanged);
                    Debug.Log($"📡 UIManager subscribed to drop zone: {dropZone.name}");
                }
            }
            
            if (dropZones.Length > 0)
            {
                // Force update when new drop zones are found
                UpdateTopUI();
            }
        }
        
        private void UpdateDisplay()
        {
            UpdateTopUI();
            UpdateCharacterStats();
            UpdateDropZoneDisplay();
        }
        
        /// <summary>
        /// Public method to force update resource display - called by ResourcePreviewUI
        /// </summary>
        public void UpdateResourceDisplay()
        {
            UpdateTopUI();
            Debug.Log("🔄 UIManager resource display updated by ResourcePreviewUI");
        }
        
        /// <summary>
        /// Enhanced UpdateTopUI that gets dynamic generation rates AND total costs
        /// </summary>
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
            
            // Update resources display with DYNAMIC generation rates AND total costs
            var resourceManager = Core.ResourceManager.Instance;
            if (resourceManager != null)
            {
                // Get current resources
                int currentRP = resourceManager.ResearchPoints;
                int currentCP = resourceManager.CommunityPoints;
                
                // Get DYNAMIC generation rates (not hardcoded!)
                int rpPerWeek = resourceManager.RPPerWeek;
                int cpPerWeek = resourceManager.CPPerWeek;
                
                // Calculate total cost from all drop zones
                int totalRPCost = 0;
                int totalCPCost = 0;
                GetTotalQueuedCost(out totalRPCost, out totalCPCost);
                
                // Update main resource text
                if (rpText != null)
                    rpText.text = $"RP: {currentRP}";
                if (cpText != null)
                    cpText.text = $"CP: {currentCP}";
                
                // Update generation rate text (stays same color)
                if (rpGenerationText != null)
                    rpGenerationText.text = $"+{rpPerWeek}/turn";
                if (cpGenerationText != null)
                    cpGenerationText.text = $"+{cpPerWeek}/turn";
                
                // Update the gain indicators (+4, +4) with color changes
                UpdateGainIndicators(currentRP, currentCP, totalRPCost, totalCPCost);
                
                // Update total cost display (single text with no color changes)
                if (totalCostText != null)
                {
                    if (totalRPCost > 0 || totalCPCost > 0)
                    {
                        string costDisplay = "Total Costs: ";
                        
                        if (totalRPCost > 0)
                            costDisplay += $"{totalRPCost} RP";
                        
                        if (totalRPCost > 0 && totalCPCost > 0)
                            costDisplay += ", ";
                        
                        if (totalCPCost > 0)
                            costDisplay += $"{totalCPCost} CP";
                        
                        totalCostText.text = costDisplay;
                        totalCostText.gameObject.SetActive(true);
                        
                        // Keep total cost text color constant (white)
                        totalCostText.color = Color.white;
                    }
                    else
                    {
                        totalCostText.gameObject.SetActive(false);
                    }
                }
                
                Debug.Log($"📊 Main UI updated: RP {currentRP} (+{rpPerWeek}/turn), CP {currentCP} (+{cpPerWeek}/turn), Total cost: {totalRPCost} RP, {totalCPCost} CP");
            }
        }
        
        /// <summary>
        /// Update the gain indicators to show cost reduction (-5) with proper colors
        /// </summary>
        private void UpdateGainIndicators(int currentRP, int currentCP, int totalRPCost, int totalCPCost)
        {
            // Calculate what resources will be after spending
            int afterRP = currentRP - totalRPCost;
            int afterCP = currentCP - totalCPCost;
            
            Debug.Log($"🎨 UpdateGainIndicators called: RP {currentRP}-{totalRPCost}={afterRP}, CP {currentCP}-{totalCPCost}={afterCP}");
            
            // Update RP cost indicator (show reduction, not gain)
            if (rpGainText != null)
            {
                if (totalRPCost > 0)
                {
                    rpGainText.text = $"-{totalRPCost}"; // Show cost as negative
                    rpGainText.gameObject.SetActive(true);
                    
                    // Color based on resource situation
                    Color textColor;
                    Color bgColor;
                    
                    if (afterRP < 0)
                    {
                        // Can't afford - red
                        textColor = new Color(1f, 0.9f, 0.9f); // Light red text
                        bgColor = new Color(0.8f, 0.2f, 0.2f); // Red background
                        Debug.Log($"🔴 RP indicator: RED (after: {afterRP})");
                    }
                    else if (afterRP <= 5)
                    {
                        // Low resources - yellow
                        textColor = new Color(1f, 1f, 0.9f); // Light yellow text
                        bgColor = new Color(0.8f, 0.8f, 0.2f); // Yellow background
                        Debug.Log($"🟡 RP indicator: YELLOW (after: {afterRP})");
                    }
                    else
                    {
                        // Good resources - green
                        textColor = new Color(0.9f, 1f, 0.9f); // Light green text
                        bgColor = new Color(0.2f, 0.8f, 0.2f); // Green background
                        Debug.Log($"🟢 RP indicator: GREEN (after: {afterRP})");
                    }
                    
                    rpGainText.color = textColor;
                    if (rpGainBackground != null)
                        rpGainBackground.color = bgColor;
                }
                else
                {
                    // No cost - hide the indicator
                    rpGainText.gameObject.SetActive(false);
                    if (rpGainBackground != null)
                        rpGainBackground.gameObject.SetActive(false);
                    Debug.Log("🚫 RP indicator: HIDDEN (no cost)");
                }
            }
            
            // Update CP cost indicator (show reduction, not gain)
            if (cpGainText != null)
            {
                if (totalCPCost > 0)
                {
                    cpGainText.text = $"-{totalCPCost}"; // Show cost as negative
                    cpGainText.gameObject.SetActive(true);
                    
                    // Color based on resource situation
                    Color textColor;
                    Color bgColor;
                    
                    if (afterCP < 0)
                    {
                        // Can't afford - red
                        textColor = new Color(1f, 0.9f, 0.9f); // Light red text
                        bgColor = new Color(0.8f, 0.2f, 0.2f); // Red background
                        Debug.Log($"🔴 CP indicator: RED (after: {afterCP})");
                    }
                    else if (afterCP <= 2)
                    {
                        // Low resources - yellow
                        textColor = new Color(1f, 1f, 0.9f); // Light yellow text
                        bgColor = new Color(0.8f, 0.8f, 0.2f); // Yellow background
                        Debug.Log($"🟡 CP indicator: YELLOW (after: {afterCP})");
                    }
                    else
                    {
                        // Good resources - green
                        textColor = new Color(0.9f, 1f, 0.9f); // Light green text
                        bgColor = new Color(0.2f, 0.8f, 0.2f); // Green background
                        Debug.Log($"🟢 CP indicator: GREEN (after: {afterCP})");
                    }
                    
                    cpGainText.color = textColor;
                    if (cpGainBackground != null)
                        cpGainBackground.color = bgColor;
                }
                else
                {
                    // No cost - hide the indicator
                    cpGainText.gameObject.SetActive(false);
                    if (cpGainBackground != null)
                        cpGainBackground.gameObject.SetActive(false);
                    Debug.Log("🚫 CP indicator: HIDDEN (no cost)");
                }
            }
        }
        
        /// <summary>
        /// Get total cost of all queued cards from all drop zones
        /// </summary>
        private void GetTotalQueuedCost(out int totalRP, out int totalCP)
        {
            totalRP = 0;
            totalCP = 0;
            
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            foreach (var dropZone in dropZones)
            {
                if (dropZone != null)
                {
                    dropZone.GetTotalQueuedCost(out int rpCost, out int cpCost);
                    totalRP += rpCost;
                    totalCP += cpCost;
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
            Debug.Log($"🔄 UIManager: Drop zone changed - {queuedCards.Count} cards queued, updating total cost");
            UpdateTopUI(); // This will recalculate and update the total cost display
        }
        
        private void UpdateDebugText(string message)
        {
            if (debugText != null)
            {
                debugText.text = $"{System.DateTime.Now:HH:mm:ss} - {message}";
            }
            Debug.Log(message);
        }
        
        [ContextMenu("Debug: Force Update All UI")]
        public void DebugForceUpdateUI()
        {
            UpdateDisplay();
            Debug.Log("🔄 Forced complete UI update");
        }
        
        [ContextMenu("Debug: Check Resource Components")]
        public void DebugCheckResourceComponents()
        {
            Debug.Log("=== UI RESOURCE COMPONENTS CHECK ===");
            Debug.Log($"rpText: {(rpText != null ? "✅" : "❌")}");
            Debug.Log($"cpText: {(cpText != null ? "✅" : "❌")}");
            Debug.Log($"rpGenerationText: {(rpGenerationText != null ? "✅" : "❌")}");
            Debug.Log($"cpGenerationText: {(cpGenerationText != null ? "✅" : "❌")}");
            Debug.Log($"rpGainText: {(rpGainText != null ? "✅" : "❌")}");
            Debug.Log($"rpGainBackground: {(rpGainBackground != null ? "✅" : "❌")}");
            Debug.Log($"cpGainText: {(cpGainText != null ? "✅" : "❌")}");
            Debug.Log($"cpGainBackground: {(cpGainBackground != null ? "✅" : "❌")}");
            Debug.Log($"totalCostText: {(totalCostText != null ? "✅" : "❌")}");
            
            if (Core.ResourceManager.Instance != null)
            {
                var rm = Core.ResourceManager.Instance;
                Debug.Log($"ResourceManager RP: {rm.ResearchPoints} (+{rm.RPPerWeek}/turn)");
                Debug.Log($"ResourceManager CP: {rm.CommunityPoints} (+{rm.CPPerWeek}/turn)");
                
                GetTotalQueuedCost(out int totalRP, out int totalCP);
                Debug.Log($"Total queued cost: {totalRP} RP, {totalCP} CP");
            }
            else
            {
                Debug.Log("❌ ResourceManager not found");
            }
        }
        
        [ContextMenu("Debug: Test Gain Colors")]
        public void DebugTestGainColors()
        {
            Debug.Log("🎨 Testing gain indicator colors...");
            
            // Test different scenarios
            UpdateGainIndicators(5, 2, 0, 0); // Low resources
            Debug.Log("Applied low resource colors (should be yellow)");
        }
        
        [ContextMenu("Debug: Check Color Update Frequency")]
        public void DebugCheckColorUpdateFrequency()
        {
            Debug.Log("🔍 Checking for duplicate color updates...");
            Debug.Log("Watch the console for multiple '🎨 UpdateGainIndicators called' messages");
            Debug.Log("If you see the same message multiple times quickly, there's a duplicate call");
            
            // Force an update and watch for duplicates
            UpdateDisplay();
        }
        
        [ContextMenu("Debug: Force Update Total Cost")]
        public void DebugForceUpdateTotalCost()
        {
            GetTotalQueuedCost(out int totalRP, out int totalCP);
            Debug.Log($"🎯 Manual total cost check: {totalRP} RP, {totalCP} CP");
            UpdateTopUI();
        }
        
        [ContextMenu("Debug: Check Drop Zone Subscriptions")]
        public void DebugCheckDropZoneSubscriptions()
        {
            Debug.Log("=== DROP ZONE SUBSCRIPTION CHECK ===");
            var dropZones = FindObjectsOfType<Cards.CardDropZone>();
            Debug.Log($"Found {dropZones.Length} drop zones:");
            
            for (int i = 0; i < dropZones.Length; i++)
            {
                var dz = dropZones[i];
                int listenerCount = dz.OnCardsChanged.GetPersistentEventCount();
                dz.GetTotalQueuedCost(out int rpCost, out int cpCost);
                Debug.Log($"  Drop zone {i} ({dz.name}): {listenerCount} listeners, {dz.GetQueuedCardCount()} cards, costs {rpCost} RP + {cpCost} CP");
            }
        }
        
        [ContextMenu("Debug: Force Resubscribe to Drop Zones")]
        public void DebugForceResubscribeDropZones()
        {
            Debug.Log("🔄 Force resubscribing to all drop zones...");
            SubscribeToAllDropZones();
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