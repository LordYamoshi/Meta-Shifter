using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Settings for feedback generation system
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackGenerationSettings", menuName = "Meta Balance/Feedback Generation Settings")]
    public class FeedbackGenerationSettings : ScriptableObject
    {
        [Header("Generation Control")]
        [Range(1, 20)]
        public int maxActiveFeedback = 10;
        
        [Range(0.1f, 1f)]
        public float organicFeedbackChance = 0.6f;
        
        [Range(1, 10)]
        public int feedbackPerImplementation = 5;
        
        [Header("Sentiment Weights")]
        [Range(0f, 1f)]
        public float balanceImpactWeight = 0.4f;
        
        [Range(0f, 1f)]
        public float popularityImpactWeight = 0.3f;
        
        [Range(0f, 1f)]
        public float randomFactorWeight = 0.3f;
        
        [Header("Community Response")]
        [Range(0f, 100f)]
        public float highSentimentThreshold = 70f;
        
        [Range(0f, 100f)]
        public float lowSentimentThreshold = 30f;
        
        [Header("Timing")]
        [Range(0.1f, 5f)]
        public float feedbackDelayMin = 0.5f;
        
        [Range(0.1f, 5f)]
        public float feedbackDelayMax = 2f;
        
        [Range(1f, 10f)]
        public float feedbackLifetime = 30f; // How long feedback stays active
        
        public void InitializeDefaults()
        {
            maxActiveFeedback = 10;
            organicFeedbackChance = 0.6f;
            feedbackPerImplementation = 5;
            balanceImpactWeight = 0.4f;
            popularityImpactWeight = 0.3f;
            randomFactorWeight = 0.3f;
            highSentimentThreshold = 70f;
            lowSentimentThreshold = 30f;
            feedbackDelayMin = 0.5f;
            feedbackDelayMax = 2f;
            feedbackLifetime = 30f;
        }
        
        [ContextMenu("Reset to Defaults")]
        public void ResetToDefaults()
        {
            InitializeDefaults();
        }
    }
}