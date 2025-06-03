using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MetaBalance.Effects
{
    /// <summary>
    /// Simple implementation effect script that plays highlight effect and SFX when a card is being implemented
    /// No animations - just background color highlight and audio
    /// </summary>
    public class ImplementationEffect : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip implementationSFX;
        [SerializeField] private float volume = 1f;
        
        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightDuration = 1.5f;
        [SerializeField] private bool useCustomHighlightColor = false;
        
        [Header("Settings")]
        [SerializeField] private bool autoDestroyAfterEffect = false;
        
        private bool isPlaying = false;
        private Image cardBackground;
        private Color originalColor;
        
        private void Awake()
        {
            // Auto-find components if not assigned
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            // Get card background image
            cardBackground = GetComponent<Image>();
            if (cardBackground != null)
            {
                originalColor = cardBackground.color;
            }
                
            // Create audio source if it doesn't exist
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = volume;
            }
        }
        
        /// <summary>
        /// Main function to call when implementing a card - plays highlight effect and SFX
        /// </summary>
        /// <param name="cardData">The card being implemented (optional, for future card-specific effects)</param>
        public void PlayImplementationEffect(Cards.CardData cardData = null)
        {
            if (isPlaying)
            {
                Debug.LogWarning("Implementation effect already playing!");
                return;
            }
            
            StartCoroutine(PlayHighlightEffectCoroutine(cardData));
        }
        
        /// <summary>
        /// Alternative function name for clarity
        /// </summary>
        public void PlayEffect(Cards.CardData cardData = null)
        {
            PlayImplementationEffect(cardData);
        }
        
        private IEnumerator PlayHighlightEffectCoroutine(Cards.CardData cardData)
        {
            isPlaying = true;
            
            Debug.Log($"Playing highlight implementation effect for card: {cardData?.cardName ?? "Unknown"}");
            
            // Play sound effect
            if (audioSource != null && implementationSFX != null)
            {
                audioSource.clip = implementationSFX;
                audioSource.volume = volume;
                audioSource.Play();
                Debug.Log("Playing implementation SFX");
            }
            else
            {
                Debug.LogWarning("No audio source or SFX clip set!");
            }
            
            // Play highlight effect if we have a background image
            if (cardBackground != null)
            {
                yield return StartCoroutine(PlayHighlightEffect(cardData));
            }
            else
            {
                Debug.LogWarning("No Image component found for highlight effect!");
                // Just wait for the duration without visual effect
                yield return new WaitForSeconds(highlightDuration);
            }
            
            isPlaying = false;
            
            Debug.Log("Implementation highlight effect completed");
            
            // Auto-destroy if enabled
            if (autoDestroyAfterEffect)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Play the highlight effect on the card background
        /// </summary>
        private IEnumerator PlayHighlightEffect(Cards.CardData cardData)
        {
            // Determine highlight color
            Color targetHighlightColor = useCustomHighlightColor ? highlightColor : GetCardTypeHighlightColor(cardData);
            
            Debug.Log($"Highlighting with color: {targetHighlightColor}");
            
            float fadeInDuration = 0.3f;
            float holdDuration = highlightDuration - (fadeInDuration * 2);
            float fadeOutDuration = 0.3f;
            
            // Phase 1: Fade to highlight color
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeInDuration;
                
                cardBackground.color = Color.Lerp(originalColor, targetHighlightColor, t);
                yield return null;
            }
            
            // Ensure we're at full highlight color
            cardBackground.color = targetHighlightColor;
            
            // Phase 2: Hold highlight color
            if (holdDuration > 0)
            {
                yield return new WaitForSeconds(holdDuration);
            }
            
            // Phase 3: Fade back to original color
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                
                cardBackground.color = Color.Lerp(targetHighlightColor, originalColor, t);
                yield return null;
            }
            
            // Ensure we're back to original color
            cardBackground.color = originalColor;
        }
        
        /// <summary>
        /// Get highlight color based on card type (if not using custom color)
        /// </summary>
        private Color GetCardTypeHighlightColor(Cards.CardData cardData)
        {
            if (cardData == null) return highlightColor;
            
            return cardData.cardType switch
            {
                Cards.CardType.BalanceChange => new Color(1f, 0.8f, 0.2f, 1f),  // Orange/Gold
                Cards.CardType.MetaShift => new Color(0.2f, 0.8f, 1f, 1f),      // Cyan
                Cards.CardType.Community => new Color(0.8f, 0.2f, 1f, 1f),      // Purple
                Cards.CardType.CrisisResponse => new Color(1f, 0.2f, 0.2f, 1f), // Red
                Cards.CardType.Special => new Color(1f, 1f, 0.2f, 1f),          // Bright Yellow
                _ => highlightColor // Default fallback
            };
        }
        
        /// <summary>
        /// Stop the effect if it's currently playing
        /// </summary>
        public void StopEffect()
        {
            if (isPlaying)
            {
                StopAllCoroutines();
                
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                
                // Reset background color
                if (cardBackground != null)
                {
                    cardBackground.color = originalColor;
                }
                
                isPlaying = false;
                Debug.Log("Implementation highlight effect stopped");
            }
        }
        
        /// <summary>
        /// Check if the effect is currently playing
        /// </summary>
        public bool IsPlaying()
        {
            return isPlaying;
        }
        
        /// <summary>
        /// Set the highlight color at runtime
        /// </summary>
        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
            useCustomHighlightColor = true;
        }
        
        /// <summary>
        /// Set the SFX clip at runtime
        /// </summary>
        public void SetSFX(AudioClip clip)
        {
            implementationSFX = clip;
        }
        
        /// <summary>
        /// Set the effect duration at runtime
        /// </summary>
        public void SetDuration(float duration)
        {
            highlightDuration = duration;
        }
        
        /// <summary>
        /// Enable/disable automatic card type color detection
        /// </summary>
        public void SetUseCardTypeColors(bool useCardTypeColors)
        {
            useCustomHighlightColor = !useCardTypeColors;
        }
        
        /// <summary>
        /// Reset to original color (useful if effect is interrupted)
        /// </summary>
        public void ResetToOriginalColor()
        {
            if (cardBackground != null)
            {
                cardBackground.color = originalColor;
            }
        }
        
        // Debug methods for testing in editor
        [ContextMenu("Test Highlight Effect")]
        public void TestEffect()
        {
            PlayImplementationEffect();
        }
        
        [ContextMenu("Stop Effect")]
        public void TestStopEffect()
        {
            StopEffect();
        }
        
        [ContextMenu("Reset Color")]
        public void TestResetColor()
        {
            ResetToOriginalColor();
        }
    }
}