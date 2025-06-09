// Assets/Scripts/UI/EventUIItem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MetaBalance.UI
{
    public class EventUIItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI severityText;
        [SerializeField] private TextMeshProUGUI timeRemainingText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image severityIcon;
        [SerializeField] private Button viewButton;
        
        private Events.GameEvent gameEvent;
        private System.Action onViewClicked;
        
        public void Setup(Events.GameEvent evt, System.Action onView)
        {
            gameEvent = evt;
            onViewClicked = onView;
            
            if (viewButton != null)
            {
                viewButton.onClick.RemoveAllListeners();
                viewButton.onClick.AddListener(() => onViewClicked?.Invoke());
            }
            
            RefreshDisplay();
        }
        
        public void RefreshDisplay()
        {
            if (gameEvent == null) return;
            
            if (titleText != null)
                titleText.text = gameEvent.eventTitle;
            
            if (severityText != null)
            {
                severityText.text = $"{gameEvent.GetSeverityEmoji()} {gameEvent.severity}";
                severityText.color = gameEvent.GetSeverityColor();
            }
            
            if (timeRemainingText != null)
                timeRemainingText.text = gameEvent.GetTimeRemaining();
            
            if (backgroundImage != null)
            {
                backgroundImage.color = gameEvent.eventColor;
                
                // Add urgency effects
                if (gameEvent.isUrgent)
                {
                    // Add pulsing effect for urgent events
                    StartCoroutine(PulseBackground());
                }
            }
            
            if (severityIcon != null)
                severityIcon.color = gameEvent.GetSeverityColor();
        }
        
        private System.Collections.IEnumerator PulseBackground()
        {
            if (backgroundImage == null) yield break;
            
            Color originalColor = backgroundImage.color;
            Color pulseColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            
            while (gameEvent != null && gameEvent.isUrgent && gameObject.activeInHierarchy)
            {
                // Fade out
                float elapsed = 0f;
                float duration = 0.8f;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    backgroundImage.color = Color.Lerp(originalColor, pulseColor, t);
                    yield return null;
                }
                
                // Fade in
                elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    backgroundImage.color = Color.Lerp(pulseColor, originalColor, t);
                    yield return null;
                }
            }
        }
        
        public Events.GameEvent GetEvent() => gameEvent;
    }
}