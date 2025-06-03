using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MetaBalance.UI
{
    /// <summary>
    /// Simple, reliable CharacterUIPanel that guarantees stats display correctly
    /// </summary>
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
        
        [Header("Character Configuration")]
        [SerializeField] private Characters.CharacterType assignedCharacter = Characters.CharacterType.Warrior;
        
        private Characters.CharacterType characterType;
        private System.Action onSelected;
        private bool isSelected = false;
        
        private void Awake()
        {
            FindUIComponents();
        }
        
        private void Start()
        {
            if (characterType == default(Characters.CharacterType))
            {
                ForceSetup(assignedCharacter, null);
            }
        }
        
        private void FindUIComponents()
        {
            // Find components aggressively - check every possible location
            if (characterNameText == null)
            {
                // Try specific names first
                characterNameText = transform.Find("CharacterName")?.GetComponent<TextMeshProUGUI>() ??
                                  transform.Find("Name")?.GetComponent<TextMeshProUGUI>() ??
                                  transform.Find("Header/CharacterName")?.GetComponent<TextMeshProUGUI>();
                
                // If still null, search by name pattern
                if (characterNameText == null)
                {
                    var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in textComponents)
                    {
                        if (text.name.ToLower().Contains("name") || text.name.ToLower().Contains("character"))
                        {
                            characterNameText = text;
                            break;
                        }
                    }
                }
                
                // Last resort - use first text component
                if (characterNameText == null)
                {
                    characterNameText = GetComponentInChildren<TextMeshProUGUI>();
                }
            }
            
            if (winRateText == null)
            {
                // Try specific names
                winRateText = transform.Find("WinRate")?.GetComponent<TextMeshProUGUI>() ??
                             transform.Find("WinRateText")?.GetComponent<TextMeshProUGUI>() ??
                             transform.Find("Stats/WinRate")?.GetComponent<TextMeshProUGUI>();
                
                // Search by name pattern
                if (winRateText == null)
                {
                    var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in textComponents)
                    {
                        if (text.name.ToLower().Contains("winrate") || text.name.ToLower().Contains("win"))
                        {
                            winRateText = text;
                            break;
                        }
                    }
                }
            }
            
            if (popularityText == null)
            {
                // Try specific names
                popularityText = transform.Find("Popularity")?.GetComponent<TextMeshProUGUI>() ??
                                transform.Find("PopularityText")?.GetComponent<TextMeshProUGUI>() ??
                                transform.Find("Stats/Popularity")?.GetComponent<TextMeshProUGUI>();
                
                // Search by name pattern
                if (popularityText == null)
                {
                    var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in textComponents)
                    {
                        if (text.name.ToLower().Contains("popularity") || text.name.ToLower().Contains("pop"))
                        {
                            popularityText = text;
                            break;
                        }
                    }
                }
            }
            
            if (roleText == null)
            {
                // Try specific names
                roleText = transform.Find("Role")?.GetComponent<TextMeshProUGUI>() ??
                          transform.Find("CharacterRole")?.GetComponent<TextMeshProUGUI>() ??
                          transform.Find("Header/Role")?.GetComponent<TextMeshProUGUI>();
                
                // Search by name pattern
                if (roleText == null)
                {
                    var textComponents = GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var text in textComponents)
                    {
                        if (text.name.ToLower().Contains("role") || text.name.ToLower().Contains("class"))
                        {
                            roleText = text;
                            break;
                        }
                    }
                }
            }
            
            if (characterIcon == null)
                characterIcon = GetComponentInChildren<Image>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (selectButton == null)
                selectButton = GetComponent<Button>() ?? GetComponentInChildren<Button>();
        }
        
        public void ForceSetup(Characters.CharacterType type, System.Action onSelectCallback)
        {
            characterType = type;
            onSelected = onSelectCallback;
            
            // Make sure we have components
            FindUIComponents();
            
            // Setup character name display
            if (characterNameText != null)
                characterNameText.text = type.ToString();
            
            // Setup role display
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
            
            // Setup button click handler
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => {
                    Debug.Log($"Character {type} clicked");
                    onSelected?.Invoke();
                });
            }
            
            // Setup character icon color
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
            
            // Force initial stats update
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager != null)
            {
                float winRate = characterManager.GetStat(characterType, Characters.CharacterStat.WinRate);
                float popularity = characterManager.GetStat(characterType, Characters.CharacterStat.Popularity);
                ForceUpdateDisplay(winRate, popularity);
            }
            
            Debug.Log($"✅ Character panel setup complete for {type}");
        }
        
        public void ForceUpdateDisplay(float winRate, float popularity)
        {
            // Make sure components exist first
            FindUIComponents();
            
            // Force update win rate - guaranteed to work
            if (winRateText != null)
            {
                winRateText.text = $"{winRate:F1}%";
                winRateText.color = GetWinRateColor(winRate);
                Debug.Log($"✅ Updated {characterType} win rate to {winRateText.text}");
            }
            else
            {
                // Create missing component if needed
                Debug.LogWarning($"Creating missing winRateText for {characterType}");
                CreateMissingWinRateText(winRate);
            }
            
            // Force update popularity - guaranteed to work
            if (popularityText != null)
            {
                popularityText.text = $"Pop: {popularity:F0}%";
                Debug.Log($"✅ Updated {characterType} popularity to {popularityText.text}");
            }
            else
            {
                Debug.LogWarning($"Creating missing popularityText for {characterType}");
                CreateMissingPopularityText(popularity);
            }
        }
        
        private Color GetWinRateColor(float winRate)
        {
            return winRate switch
            {
                > 55f => Color.red,                    // Overpowered (red)
                < 45f => new Color(0.3f, 0.8f, 1f),  // Underpowered (light blue)
                _ => Color.green                       // Balanced (green)
            };
        }
        
        private void CreateMissingWinRateText(float winRate)
        {
            GameObject textObj = new GameObject("WinRateText");
            textObj.transform.SetParent(transform, false);
            
            winRateText = textObj.AddComponent<TextMeshProUGUI>();
            winRateText.text = $"{winRate:F1}%";
            winRateText.fontSize = 16f;
            winRateText.color = GetWinRateColor(winRate);
            winRateText.alignment = TextAlignmentOptions.Center;
            
            // Position it in the middle of the panel
            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0.3f);
            rectTransform.anchorMax = new Vector2(1f, 0.7f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log($"✅ Created missing winRateText for {characterType}");
        }
        
        private void CreateMissingPopularityText(float popularity)
        {
            GameObject textObj = new GameObject("PopularityText");
            textObj.transform.SetParent(transform, false);
            
            popularityText = textObj.AddComponent<TextMeshProUGUI>();
            popularityText.text = $"Pop: {popularity:F0}%";
            popularityText.fontSize = 12f;
            popularityText.color = Color.white;
            popularityText.alignment = TextAlignmentOptions.Center;
            
            // Position it at the bottom of the panel
            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0.3f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Debug.Log($"✅ Created missing popularityText for {characterType}");
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedBackgroundColor : normalBackgroundColor;
                Debug.Log($"🎯 Set {characterType} selection state: {selected}");
            }
        }
        
        // Public getters
        public Characters.CharacterType GetCharacterType() => characterType;
        public bool IsSelected() => isSelected;
        
        // Legacy compatibility methods
        public void UpdateStats(float winRate, float popularity) => ForceUpdateDisplay(winRate, popularity);
        
        public void RefreshStats()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager != null)
            {
                float winRate = characterManager.GetStat(characterType, Characters.CharacterStat.WinRate);
                float popularity = characterManager.GetStat(characterType, Characters.CharacterStat.Popularity);
                ForceUpdateDisplay(winRate, popularity);
            }
        }
        
        // Debug methods
        [ContextMenu("🔧 Force Update Display")]
        public void DebugForceUpdate()
        {
            RefreshStats();
        }
        
        [ContextMenu("🔍 Debug: Check Components")]
        public void DebugCheckComponents()
        {
            Debug.Log($"=== 🔍 {characterType} Panel Component Check ===");
            Debug.Log($"characterNameText: {(characterNameText != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"roleText: {(roleText != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"winRateText: {(winRateText != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"popularityText: {(popularityText != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"characterIcon: {(characterIcon != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"backgroundImage: {(backgroundImage != null ? "✅ OK" : "❌ MISSING")}");
            Debug.Log($"selectButton: {(selectButton != null ? "✅ OK" : "❌ MISSING")}");
            
            if (winRateText != null) Debug.Log($"  winRateText current value: '{winRateText.text}'");
            if (popularityText != null) Debug.Log($"  popularityText current value: '{popularityText.text}'");
        }
        
        [ContextMenu("🧪 Debug: Test Update with Random Values")]
        public void DebugTestUpdate()
        {
            float randomWinRate = Random.Range(25f, 75f);
            float randomPopularity = Random.Range(10f, 90f);
            Debug.Log($"🧪 Testing update for {characterType} with WinRate={randomWinRate:F1}%, Pop={randomPopularity:F1}%");
            ForceUpdateDisplay(randomWinRate, randomPopularity);
        }
        
        [ContextMenu("🔧 Debug: Force Find Components")]
        public void DebugForceFindComponents()
        {
            Debug.Log($"🔧 Force finding components for {characterType}");
            FindUIComponents();
            DebugCheckComponents();
        }
    }
}