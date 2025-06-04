using UnityEngine;
using System;

namespace MetaBalance.Community
{
    /// <summary>
    /// Core data structures for the community feedback system
    /// </summary>
    
    [System.Serializable]
    public class CommunityFeedback
    {
        public string author;
        public string content;
        public float sentiment; // -1 to 1 (negative to positive)
        public FeedbackType feedbackType;
        public string communitySegment;
        public DateTime timestamp;
        public bool isOrganic; // True if generated organically, false if response to specific change
        public int upvotes;
        public int replies;
        public float impactScore; // Calculated impact on community sentiment
        public bool isViralCandidate;
        
        public CommunityFeedback()
        {
            timestamp = DateTime.Now;
            upvotes = 0;
            replies = 0;
            impactScore = 0f;
            isViralCandidate = false;
            isOrganic = false;
        }
        
        public string GetTimeAgo()
        {
            var timeSpan = DateTime.Now - timestamp;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            else
                return $"{(int)timeSpan.TotalDays}d ago";
        }
        
        public Color GetSentimentColor()
        {
            return sentiment switch
            {
                > 0.5f => new Color(0.2f, 0.8f, 0.2f), // Green - positive
                > 0.1f => new Color(0.6f, 0.8f, 0.4f), // Light green - slightly positive
                > -0.1f => new Color(0.8f, 0.8f, 0.8f), // Gray - neutral
                > -0.5f => new Color(0.8f, 0.6f, 0.4f), // Orange - slightly negative
                _ => new Color(0.8f, 0.2f, 0.2f) // Red - negative
            };
        }
        
        public string GetSentimentEmoji()
        {
            return sentiment switch
            {
                > 0.7f => "😍",
                > 0.4f => "😊",
                > 0.1f => "🙂",
                > -0.1f => "😐",
                > -0.4f => "😕",
                > -0.7f => "😠",
                _ => "😡"
            };
        }
    }
    
    [System.Serializable]
    public class BalanceChange
    {
        public Characters.CharacterType character;
        public Characters.CharacterStat stat;
        public float previousValue;
        public float newValue;
        public float timestamp;
        public float magnitude; // How significant this change is
        
        public BalanceChange()
        {
            timestamp = Time.time;
        }
        
        public BalanceChange(Characters.CharacterType character, Characters.CharacterStat stat, float previousValue, float newValue)
        {
            this.character = character;
            this.stat = stat;
            this.previousValue = previousValue;
            this.newValue = newValue;
            this.timestamp = Time.time;
            this.magnitude = Mathf.Abs(newValue - previousValue);
        }
        
        public bool IsSignificantChange()
        {
            return magnitude > 5f; // Threshold for what's considered significant
        }
        
        public bool IsPositiveChange()
        {
            // Determine if change moves character closer to balanced state (around 50)
            float distanceFromBalanceBefore = Mathf.Abs(previousValue - 50f);
            float distanceFromBalanceAfter = Mathf.Abs(newValue - 50f);
            
            return distanceFromBalanceAfter < distanceFromBalanceBefore;
        }
        
        public string GetChangeDescription()
        {
            float change = newValue - previousValue;
            string direction = change > 0 ? "increased" : "decreased";
            string intensity = Mathf.Abs(change) switch
            {
                > 20f => "dramatically",
                > 10f => "significantly",
                > 5f => "moderately", 
                _ => "slightly"
            };
            
            return $"{character} {stat} {intensity} {direction}";
        }
    }
    
    [System.Serializable]
    public class CommunitySegmentData
    {
        public string segmentName;
        [Range(0f, 1f)]
        public float influence; // How much this segment affects overall sentiment
        [Range(0f, 2f)]
        public float activityLevel = 1f; // How often this segment posts (multiplier)
        [Range(-1f, 1f)]
        public float baseSentimentBias = 0f; // General optimism/pessimism of this segment
        public Color segmentColor = Color.white;
        
        public CommunitySegmentData()
        {
            influence = 0.5f;
            activityLevel = 1f;
            baseSentimentBias = 0f;
            segmentColor = Color.white;
        }
        
        public CommunitySegmentData(string name, float influence, float activity = 1f, float bias = 0f)
        {
            this.segmentName = name;
            this.influence = influence;
            this.activityLevel = activity;
            this.baseSentimentBias = bias;
            this.segmentColor = GetDefaultColorForSegment(name);
        }
        
        private Color GetDefaultColorForSegment(string segmentName)
        {
            return segmentName switch
            {
                "Pro Players" => new Color(1f, 0.8f, 0.2f), // Gold
                "Content Creators" => new Color(0.8f, 0.2f, 0.8f), // Purple
                "Competitive" => new Color(0.2f, 0.8f, 0.2f), // Green
                "Casual Players" => new Color(0.2f, 0.6f, 1f), // Blue
                _ => Color.white
            };
        }
    }
    
    [System.Serializable]
    public class FeedbackTemplate
    {
        public string content;
        public FeedbackType feedbackType;
        [Range(-1f, 1f)]
        public float sentimentBias; // -1 to 1
        public string[] requiredTags; // Tags that must be present for this template to be used
        public float weight = 1f; // Probability weight for template selection
        
        public FeedbackTemplate()
        {
            sentimentBias = 0f;
            weight = 1f;
            requiredTags = new string[0];
        }
        
        public FeedbackTemplate(string content, FeedbackType type, float bias = 0f)
        {
            this.content = content;
            this.feedbackType = type;
            this.sentimentBias = bias;
            this.weight = 1f;
            this.requiredTags = new string[0];
        }
    }
    
    public enum FeedbackType
    {
        BalanceReaction,    // Direct response to balance changes
        PopularityShift,    // Comments about character usage changes
        MetaAnalysis,       // Analytical comments about overall meta
        ProPlayerOpinion,   // Professional player perspectives
        CasualPlayerFeedback, // Casual player reactions
        ContentCreator,     // Content creator announcements/reactions
        CompetitiveScene,   // Tournament/competitive focused feedback
        Meme,              // Humorous/meme responses
        Bug,               // Bug reports or complaints
        Suggestion         // Player suggestions for future changes
    }
    
    /// <summary>
    /// Event data for feedback-related events
    /// </summary>
    [System.Serializable]
    public class FeedbackEventData
    {
        public CommunityFeedback feedback;
        public float communityImpact;
        public bool isViralCandidate;
        public DateTime timestamp;
        public string triggerReason; // What caused this feedback
        
        public FeedbackEventData(CommunityFeedback feedback, string reason = "")
        {
            this.feedback = feedback;
            this.communityImpact = CalculateImpact(feedback);
            this.isViralCandidate = CheckViralPotential(feedback);
            this.timestamp = DateTime.Now;
            this.triggerReason = reason;
        }
        
        private float CalculateImpact(CommunityFeedback feedback)
        {
            float baseImpact = Mathf.Abs(feedback.sentiment);
            float segmentMultiplier = GetSegmentMultiplier(feedback.communitySegment);
            float engagementBonus = (feedback.upvotes + feedback.replies) * 0.01f;
            
            return (baseImpact * segmentMultiplier) + engagementBonus;
        }
        
        private bool CheckViralPotential(CommunityFeedback feedback)
        {
            return feedback.upvotes > 50 || 
                   feedback.replies > 20 || 
                   Mathf.Abs(feedback.sentiment) > 0.8f ||
                   feedback.feedbackType == FeedbackType.Meme;
        }
        
        private float GetSegmentMultiplier(string segment)
        {
            return segment switch
            {
                "Pro Players" => 1.5f,
                "Content Creators" => 1.3f,
                "Competitive" => 1.2f,
                "Casual Players" => 1.0f,
                _ => 1.0f
            };
        }
    }
    
    /// <summary>
    /// Configuration for community sentiment tracking
    /// </summary>
    [System.Serializable]
    public class CommunitySentimentConfig
    {
        [Header("Sentiment Calculation")]
        [Range(0f, 1f)]
        public float recentFeedbackWeight = 0.6f; // Weight of recent feedback vs historical
        
        [Range(0f, 1f)]
        public float influencerFeedbackWeight = 0.8f; // Extra weight for influential segments
        
        [Range(1, 100)]
        public int sentimentSampleSize = 20; // Number of recent feedback items to consider
        
        [Header("Sentiment Decay")]
        [Range(0f, 1f)]
        public float sentimentDecayRate = 0.1f; // How quickly sentiment returns to neutral
        
        [Range(0f, 100f)]
        public float neutralSentimentTarget = 50f; // Target sentiment when no recent feedback
        
        public CommunitySentimentConfig()
        {
            recentFeedbackWeight = 0.6f;
            influencerFeedbackWeight = 0.8f;
            sentimentSampleSize = 20;
            sentimentDecayRate = 0.1f;
            neutralSentimentTarget = 50f;
        }
    }
}