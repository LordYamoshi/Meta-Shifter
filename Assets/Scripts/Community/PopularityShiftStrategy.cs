using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Strategy for popularity shift feedback
    /// </summary>
    public class PopularityShiftStrategy : BaseFeedbackStrategy
    {
        public PopularityShiftStrategy()
        {
            positiveTemplates = new[]
            {
                "Everyone's playing {CHARACTER} now! 🔥",
                "Is it just me or is {CHARACTER} everywhere?",
                "{CHARACTER} is the new meta pick, calling it now"
            };
            
            negativeTemplates = new[]
            {
                "Nobody plays {CHARACTER} anymore 😢",
                "RIP {CHARACTER} pickrate... you will be missed",
                "When was the last time you saw {CHARACTER} in a game?"
            };
            
            neutralTemplates = new[]
            {
                "{CHARACTER} usage seems to be shifting",
                "Interesting changes in {CHARACTER} popularity"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.PopularityShift;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            var popularityChanges = changes.Where(c => c.stat == Characters.CharacterStat.Popularity).ToList();
            return popularityChanges.Count > 0 ? 0.6f : 0f;
        }
    }
    
    /// <summary>
    /// Strategy for meta analysis feedback
    /// </summary>
    public class MetaAnalysisStrategy : BaseFeedbackStrategy
    {
        public MetaAnalysisStrategy()
        {
            positiveTemplates = new[]
            {
                "The current meta is becoming more diverse with these changes ⚖️",
                "These adjustments should shake up the competitive scene",
                "Finally seeing some rock-paper-scissors balance in character picks"
            };
            
            negativeTemplates = new[]
            {
                "The meta is stale. These changes don't address the core issues",
                "This meta shift feels forced and unnatural"
            };
            
            neutralTemplates = new[]
            {
                "Interesting direction for the meta - let's see how it develops",
                "Meta prediction: things are about to change significantly"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.MetaAnalysis;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            return 0.3f; // Meta analysis is less frequent but consistent
        }
    }
    
    /// <summary>
    /// Strategy for casual player feedback
    /// </summary>
    public class CasualPlayerStrategy : BaseFeedbackStrategy
    {
        public CasualPlayerStrategy()
        {
            positiveTemplates = new[]
            {
                "{CHARACTER} is more fun to play now! 😊",
                "Cool, {CHARACTER} feels better to use",
                "I like these {CHARACTER} changes!"
            };
            
            negativeTemplates = new[]
            {
                "I just want to have fun with {CHARACTER}, why all these changes? 😅",
                "I liked {CHARACTER} the way they were before 😢",
                "{CHARACTER} feels different but I can't explain why"
            };
            
            neutralTemplates = new[]
            {
                "As a casual player, I don't really notice these {CHARACTER} changes",
                "These changes are too complicated... I just want to play"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.CasualPlayerFeedback;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            return 0.4f; // Casual players comment regularly but not as much as competitive
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Casual Players";
        }
    }
    
    /// <summary>
    /// Strategy for content creator feedback
    /// </summary>
    public class ContentCreatorStrategy : BaseFeedbackStrategy
    {
        public ContentCreatorStrategy()
        {
            positiveTemplates = new[]
            {
                "New {CHARACTER} guide coming soon! These changes are HUGE 🎥",
                "Stream tonight: Testing the new {CHARACTER} changes live!",
                "Subscribe for my reaction to these balance updates! 🔔"
            };
            
            negativeTemplates = new[]
            {
                "Making a rant video about these {CHARACTER} changes",
                "This {CHARACTER} nerf ruins all my content plans 😤"
            };
            
            neutralTemplates = new[]
            {
                "Making a tier list video about these {CHARACTER} updates",
                "Hot take: These {CHARACTER} changes will change everything"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.ContentCreator;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            return 0.5f; // Content creators are vocal about changes
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Content Creators";
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Content creators get high engagement
            var (baseUpvotes, baseReplies) = base.GenerateEngagement(sentiment);
            return (baseUpvotes * 2, baseReplies * 2);
        }
    }
    
    /// <summary>
    /// Strategy for competitive scene feedback
    /// </summary>
    public class CompetitiveStrategy : BaseFeedbackStrategy
    {
        public CompetitiveStrategy()
        {
            positiveTemplates = new[]
            {
                "These {CHARACTER} changes will make tournaments more exciting",
                "Competitive integrity > meta staleness. Good changes to {CHARACTER}",
                "Teams need to adapt their {CHARACTER} strategies ASAP"
            };
            
            negativeTemplates = new[]
            {
                "These {CHARACTER} changes kill competitive viability completely",
                "This patch drops right before the championship... bold move",
                "Tournament meta just got way less interesting"
            };
            
            neutralTemplates = new[]
            {
                "Expecting {CHARACTER} to be pick/ban priority now",
                "Another patch, another meta shift. Adapting is part of the game"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.CompetitiveScene;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            bool hasCompetitiveImpact = changes.Any(c => 
                c.magnitude > 10f || 
                c.stat == Characters.CharacterStat.WinRate);
            
            return hasCompetitiveImpact ? 0.7f : 0.2f;
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Competitive";
        }
    }
}