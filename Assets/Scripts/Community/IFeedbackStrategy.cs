using System.Collections.Generic;

namespace MetaBalance.Community
{
    /// <summary>
    /// Strategy Pattern: Interface for different feedback generation strategies
    /// Follows Open/Closed Principle - open for extension, closed for modification
    /// </summary>
    public interface IFeedbackStrategy
    {
        /// <summary>
        /// Generate feedback based on balance changes and current community sentiment
        /// </summary>
        /// <param name="changes">Recent balance changes that triggered feedback</param>
        /// <param name="sentiment">Current community sentiment (0-100)</param>
        /// <param name="segments">Available community segments</param>
        /// <returns>Generated feedback or null if strategy doesn't apply</returns>
        CommunityFeedback GenerateFeedback(List<BalanceChange> changes, float sentiment, List<CommunitySegmentData> segments);
        
        /// <summary>
        /// Check if this strategy should be used given the current context
        /// </summary>
        /// <param name="changes">Recent balance changes</param>
        /// <param name="sentiment">Current community sentiment</param>
        /// <returns>True if this strategy is applicable</returns>
        bool ShouldApply(List<BalanceChange> changes, float sentiment);
        
        /// <summary>
        /// Get the priority of this strategy (higher = more likely to be chosen)
        /// </summary>
        /// <param name="changes">Recent balance changes</param>
        /// <param name="sentiment">Current community sentiment</param>
        /// <returns>Priority value (0-1)</returns>
        float GetPriority(List<BalanceChange> changes, float sentiment);
        
        /// <summary>
        /// Get the feedback type this strategy generates
        /// </summary>
        FeedbackType GetFeedbackType();
    }
    
    /// <summary>
    /// Abstract base class for feedback strategies implementing common functionality
    /// Template Method Pattern
    /// </summary>
    public abstract class BaseFeedbackStrategy : IFeedbackStrategy
    {
        protected string[] positiveTemplates;
        protected string[] negativeTemplates;
        protected string[] neutralTemplates;
        
        public abstract FeedbackType GetFeedbackType();
        
        public virtual CommunityFeedback GenerateFeedback(List<BalanceChange> changes, float sentiment, List<CommunitySegmentData> segments)
        {
            if (!ShouldApply(changes, sentiment))
                return null;
            
            // Template Method Pattern: Define algorithm structure, let subclasses customize steps
            var selectedChange = SelectRelevantChange(changes);
            var template = SelectTemplate(selectedChange, sentiment);
            var author = GenerateAuthor(segments);
            var processedContent = ProcessTemplate(template, selectedChange);
            var feedbackSentiment = CalculateFeedbackSentiment(selectedChange, sentiment);
            var engagement = GenerateEngagement(feedbackSentiment);
            
            return new CommunityFeedback
            {
                author = author,
                content = processedContent,
                sentiment = feedbackSentiment,
                feedbackType = GetFeedbackType(),
                communitySegment = GetTargetSegment(segments),
                timestamp = System.DateTime.Now,
                upvotes = engagement.upvotes,
                replies = engagement.replies,
                isOrganic = false
            };
        }
        
        public virtual bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            return GetPriority(changes, sentiment) > 0.1f;
        }
        
        public abstract float GetPriority(List<BalanceChange> changes, float sentiment);
        
        // Template methods for subclasses to override
        protected virtual BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Default: select most significant change
            BalanceChange mostSignificant = changes[0];
            foreach (var change in changes)
            {
                if (change.magnitude > mostSignificant.magnitude)
                    mostSignificant = change;
            }
            return mostSignificant;
        }
        
        protected virtual string SelectTemplate(BalanceChange change, float sentiment)
        {
            string[] templates;
            
            if (change != null && change.IsPositiveChange())
            {
                templates = positiveTemplates ?? new[] { "Positive change to {CHARACTER}!" };
            }
            else if (sentiment < 40f)
            {
                templates = negativeTemplates ?? new[] { "Not happy about {CHARACTER} changes..." };
            }
            else
            {
                templates = neutralTemplates ?? new[] { "Interesting changes to {CHARACTER}" };
            }
            
            return templates[UnityEngine.Random.Range(0, templates.Length)];
        }
        
        protected virtual string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            var targetSegment = GetTargetSegment(segments);
            return GenerateAuthorName(targetSegment);
        }
        
        protected virtual string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", change.stat.ToString())
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", change.GetChangeDescription())
                .Replace("{EMOJI}", GetRandomEmoji());
        }
        
        protected virtual float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 50f; // Convert to -1,1 range
            
            if (change != null)
            {
                float changeImpact = change.IsPositiveChange() ? 0.3f : -0.3f;
                baseSentiment += changeImpact;
            }
            
            // Add some randomness
            baseSentiment += UnityEngine.Random.Range(-0.2f, 0.2f);
            
            return UnityEngine.Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected virtual (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Higher absolute sentiment = more engagement
            float engagementMultiplier = UnityEngine.Mathf.Abs(sentiment) + 0.5f;
            
            int upvotes = UnityEngine.Random.Range(1, (int)(20 * engagementMultiplier));
            int replies = UnityEngine.Random.Range(0, (int)(10 * engagementMultiplier));
            
            return (upvotes, replies);
        }
        
        protected virtual string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            if (segments.Count == 0) return "General";
            return segments[UnityEngine.Random.Range(0, segments.Count)].segmentName;
        }
        
        protected string GenerateAuthorName(string segment)
        {
            return segment switch
            {
                "Pro Players" => GetProPlayerName(),
                "Content Creators" => GetContentCreatorName(),
                "Competitive" => GetCompetitiveName(),
                "Casual Players" => GetCasualName(),
                _ => GetGenericName()
            };
        }
        
        private string GetProPlayerName()
        {
            var teams = new[] { "FaZe", "TSM", "TL", "C9", "G2", "Fnatic" };
            var names = new[] { "ProGamer", "Ace", "Champion", "Legend", "Elite", "Master" };
            return $"{teams[UnityEngine.Random.Range(0, teams.Length)]}_{names[UnityEngine.Random.Range(0, names.Length)]}";
        }
        
        private string GetContentCreatorName()
        {
            var names = new[] { "StreamMaster", "YouTubeGuru", "TwitchKing", "ContentQueen", "GameGuruYT", "MetaMasterTV" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }
        
        private string GetCompetitiveName()
        {
            var names = new[] { "RankedClimber", "EsportsHopeful", "TryHardPlayer", "CompetitiveMind", "LadderWarrior" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }
        
        private string GetCasualName()
        {
            var names = new[] { "CasualGamer42", "FunPlayer", "WeekendWarrior", "ChillGamer", "JustForFun" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }
        
        private string GetGenericName()
        {
            var names = new[] { "GameFan", "PlayerOne", "Community_Voice", "BalanceWatcher", "MetaObserver" };
            return names[UnityEngine.Random.Range(0, names.Length)];
        }
        
        protected string GetRandomEmoji()
        {
            var emojis = new[] { "😊", "😢", "😠", "🎯", "⚖️", "🔥", "💯", "❌", "✅", "🤔", "💪", "👍", "👎", "🚀" };
            return emojis[UnityEngine.Random.Range(0, emojis.Length)];
        }
    }
}