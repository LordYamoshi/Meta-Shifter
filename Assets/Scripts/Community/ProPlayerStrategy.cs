using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Concrete Strategy: Generates professional player perspectives on balance changes
    /// Pro players focus on competitive impact and high-level play implications
    /// </summary>
    public class ProPlayerStrategy : BaseFeedbackStrategy
    {
        public ProPlayerStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "As a pro player, these {CHARACTER} changes improve competitive diversity 💪",
                "The {CHARACTER} adjustments will make tournaments more exciting to watch",
                "Finally! {CHARACTER} is viable in competitive play again 🏆",
                "These {CHARACTER} changes reward skill expression - love it",
                "From a pro perspective, {CHARACTER} needed these buffs for high-level play",
                "Excellent work on {CHARACTER} - opens up new team compositions",
                "The {CHARACTER} rework adds depth to competitive strategy",
                "Pro scene will definitely see more {CHARACTER} picks now 🎯"
            };
            
            negativeTemplates = new[]
            {
                "These {CHARACTER} changes kill competitive viability completely",
                "As a pro, I'm concerned about {CHARACTER}'s impact on tournaments",
                "The {CHARACTER} nerfs remove skill expression from high-level play",
                "This {CHARACTER} rework dumbs down competitive strategy 😕",
                "Pro players invested months learning {CHARACTER} - now it's wasted",
                "These {CHARACTER} changes favor low-skill gameplay unfortunately",
                "The {CHARACTER} adjustments create unhealthy competitive patterns",
                "Tournament meta just got way less interesting with these {CHARACTER} changes"
            };
            
            neutralTemplates = new[]
            {
                "Interesting {CHARACTER} changes - need to test in scrims first 🤔",
                "The {CHARACTER} adjustments require adaptation at pro level",
                "Mixed feelings about {CHARACTER} changes from competitive standpoint",
                "Need more time to evaluate {CHARACTER} impact on team strategies",
                "The {CHARACTER} rework changes everything we know about the matchup",
                "Pro teams will need to completely rethink {CHARACTER} positioning",
                "These {CHARACTER} changes shift the competitive paradigm",
                "Curious to see how {CHARACTER} performs in next tournament"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.ProPlayerOpinion;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // Pro players comment less frequently but with high impact
            float basePriority = 0.3f;
            
            // Higher priority for changes that affect competitive play
            bool hasCompetitiveImpact = changes.Any(c => 
                c.magnitude > 10f || // Significant changes
                c.stat == Characters.CharacterStat.WinRate || // Win rate changes matter most
                c.stat == Characters.CharacterStat.Damage ||  // Damage affects kill potential
                c.stat == Characters.CharacterStat.Speed);    // Speed affects positioning
            
            if (hasCompetitiveImpact)
                basePriority += 0.4f;
            
            // Pro players speak up more when balance is concerning
            if (sentiment < 35f || sentiment > 75f)
                basePriority += 0.2f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Pro players only comment on significant competitive changes
            return changes.Any(c => c.magnitude > 5f && IsCompetitivelyRelevant(c));
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Pro Players";
        }
        
        private bool IsCompetitivelyRelevant(BalanceChange change)
        {
            // Changes that matter in competitive play
            return change.stat switch
            {
                Characters.CharacterStat.WinRate => true,      // Win rate always matters
                Characters.CharacterStat.Damage => true,      // Affects kill potential
                Characters.CharacterStat.Health => true,      // Affects survivability
                Characters.CharacterStat.Speed => change.magnitude > 8f, // Speed matters if significant
                Characters.CharacterStat.Utility => change.magnitude > 10f, // Utility matters if major
                _ => false
            };
        }
    }
}