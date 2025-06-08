using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: Professional player perspectives on balance changes
    /// High-impact, low-frequency feedback focusing on competitive viability
    /// Emphasizes skill expression, tournament meta, and high-level gameplay
    /// </summary>
    public class ProPlayerStrategy : BaseFeedbackStrategy
    {
        private readonly string[] teamPrefixes = { "TSM", "FaZe", "C9", "TL", "G2", "Fnatic", "NRG", "100T" };
        private readonly string[] proNames = { "Legend", "Ace", "Champion", "Elite", "Master", "Pro", "King", "Alpha" };
        
        public ProPlayerStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "As a pro player, these {CHARACTER} changes improve competitive diversity ★",
                "The {CHARACTER} adjustments will make tournaments more exciting to watch ▲",
                "Finally! {CHARACTER} is viable in competitive play again ✓",
                "These {CHARACTER} changes reward skill expression - love it ♦",
                "From a pro perspective, {CHARACTER} needed these buffs for high-level play ►",
                "Excellent work on {CHARACTER} - opens up new team compositions ⚖",
                "The {CHARACTER} rework adds depth to competitive strategy ●",
                "Pro scene will definitely see more {CHARACTER} picks now ▲",
                "These {CHARACTER} buffs bring much-needed balance to tournaments ★",
                "Perfect competitive adjustment to {CHARACTER} - well executed ✓"
            };
            
            negativeTemplates = new[]
            {
                "These {CHARACTER} changes kill competitive viability completely ✗",
                "As a pro, I'm concerned about {CHARACTER}'s impact on tournaments ↓",
                "The {CHARACTER} nerfs remove skill expression from high-level play ◆",
                "This {CHARACTER} rework dumbs down competitive strategy ■",
                "Pro players invested months learning {CHARACTER} - now it's wasted †",
                "These {CHARACTER} changes favor low-skill gameplay unfortunately ↓",
                "The {CHARACTER} adjustments create unhealthy competitive patterns ✗",
                "Tournament meta just got way less interesting with these changes ◆",
                "This {CHARACTER} nerf makes no sense from a competitive standpoint ■",
                "RIP high-level {CHARACTER} play - skill ceiling completely removed †"
            };
            
            neutralTemplates = new[]
            {
                "Interesting {CHARACTER} changes - need to test in scrims first ●",
                "The {CHARACTER} adjustments require adaptation at pro level ►",
                "Mixed feelings about {CHARACTER} changes from competitive standpoint ♦",
                "Need more time to evaluate {CHARACTER} impact on team strategies ●",
                "The {CHARACTER} rework changes everything we know about matchups ►",
                "Pro teams will need to completely rethink {CHARACTER} positioning ♦",
                "These {CHARACTER} changes shift the competitive paradigm ●",
                "Curious to see how {CHARACTER} performs in next tournament ►",
                "Professional scene needs time to adapt to these {CHARACTER} changes ♦",
                "Tournament implications of {CHARACTER} rework remain unclear ●"
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
            float basePriority = 0.25f; // Lower frequency than casual reactions
            
            // MUCH higher priority for changes that affect competitive play
            bool hasCompetitiveImpact = changes.Any(c => 
                c.magnitude > 8f || // Significant changes only
                c.stat == Characters.CharacterStat.WinRate || 
                c.stat == Characters.CharacterStat.Damage ||  
                c.stat == Characters.CharacterStat.Speed ||
                c.stat == Characters.CharacterStat.Utility);
            
            if (hasCompetitiveImpact)
                basePriority += 0.5f; // Major boost for competitive relevance
            
            // Pro players speak up when balance is concerning for tournaments
            if (sentiment < 30f || sentiment > 80f)
                basePriority += 0.3f;
            
            // Extra priority for changes that affect skill expression
            if (changes.Any(c => AffectsSkillExpression(c)))
                basePriority += 0.2f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Pro players only comment on competitively significant changes
            return changes.Any(c => c.magnitude > 6f && IsCompetitivelyRelevant(c));
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes that most affect competitive play
            var competitiveChanges = changes.Where(IsCompetitivelyRelevant)
                .OrderByDescending(c => GetCompetitiveImpactScore(c)).ToList();
            
            return competitiveChanges.FirstOrDefault() ?? changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 80f; // Less emotional than casual players
            
            if (change != null)
            {
                // Pro players focus on competitive health over personal preference
                if (change.IsPositiveChange() && IsCompetitivelyHealthy(change))
                {
                    baseSentiment += Random.Range(0.4f, 0.7f);
                }
                else if (!IsCompetitivelyHealthy(change))
                {
                    baseSentiment -= Random.Range(0.5f, 0.8f);
                }
                
                // Skill expression changes get strong reactions
                if (AffectsSkillExpression(change))
                {
                    if (change.newValue > change.previousValue) // Skill ceiling increased
                        baseSentiment += 0.3f;
                    else
                        baseSentiment -= 0.4f; // Skill ceiling lowered
                }
                
                // Tournament timing matters
                if (IsNearTournamentSeason())
                {
                    baseSentiment -= 0.2f; // Pros don't like changes before tournaments
                }
            }
            
            // Less random variance - more analytical
            baseSentiment += Random.Range(-0.15f, 0.15f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Pro player feedback gets HIGH engagement due to authority
            float engagementMultiplier = (Mathf.Abs(sentiment) + 1f) * 3f; // 3x multiplier for pro players
            
            int upvotes = (int)(Random.Range(25, 80) * engagementMultiplier);
            int replies = (int)(Random.Range(15, 40) * engagementMultiplier);
            
            // Controversial pro opinions generate massive discussion
            if (Mathf.Abs(sentiment) > 0.6f)
            {
                replies = (int)(replies * 1.8f);
                upvotes = (int)(upvotes * 1.5f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Pro Players";
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            var team = teamPrefixes[Random.Range(0, teamPrefixes.Length)];
            var name = proNames[Random.Range(0, proNames.Length)];
            return $"{team}_{name}";
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetCompetitiveStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", GetCompetitiveChangeDescription(change))
                .Replace("{SKILL_IMPACT}", GetSkillImpactDescription(change));
        }
        
        private bool IsCompetitivelyRelevant(BalanceChange change)
        {
            // Changes that matter in competitive/professional play
            return change.stat switch
            {
                Characters.CharacterStat.WinRate => true,      // Always competitively relevant
                Characters.CharacterStat.Damage => true,      // Affects kill potential/trades
                Characters.CharacterStat.Health => true,      // Affects survivability/positioning
                Characters.CharacterStat.Speed => change.magnitude > 6f, // Mobility matters if significant
                Characters.CharacterStat.Utility => change.magnitude > 8f, // Utility matters if major
                Characters.CharacterStat.Popularity => false, // Pros don't care about popularity directly
                _ => false
            };
        }
        
        private bool AffectsSkillExpression(BalanceChange change)
        {
            // Changes that affect skill ceiling and expression
            return (change.stat == Characters.CharacterStat.Damage && change.magnitude > 10f) ||
                   (change.stat == Characters.CharacterStat.Speed && change.magnitude > 8f) ||
                   (change.stat == Characters.CharacterStat.Utility && change.magnitude > 12f);
        }
        
        private bool IsCompetitivelyHealthy(BalanceChange change)
        {
            // Determine if change promotes competitive health
            if (change.stat == Characters.CharacterStat.WinRate)
            {
                // Win rates between 45-55% are competitively healthy
                return change.newValue >= 45f && change.newValue <= 55f;
            }
            
            // Changes that bring characters closer to balance are healthy
            return change.IsPositiveChange();
        }
        
        private float GetCompetitiveImpactScore(BalanceChange change)
        {
            float score = change.magnitude;
            
            // Weight by competitive importance
            score *= change.stat switch
            {
                Characters.CharacterStat.WinRate => 2.0f,
                Characters.CharacterStat.Damage => 1.8f,
                Characters.CharacterStat.Health => 1.6f,
                Characters.CharacterStat.Speed => 1.4f,
                Characters.CharacterStat.Utility => 1.2f,
                _ => 1.0f
            };
            
            return score;
        }
        
        private bool IsNearTournamentSeason()
        {
            // Simulate tournament calendar - pros hate changes before big events
            int currentWeek = Core.PhaseManager.Instance?.GetCurrentWeek() ?? 1;
            return (currentWeek % 8 == 7) || (currentWeek % 8 == 0); // Tournament weeks
        }
        
        private string GetCompetitiveStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "survivability",
                Characters.CharacterStat.Damage => "damage output",
                Characters.CharacterStat.Speed => "mobility/positioning",
                Characters.CharacterStat.Utility => "team utility",
                Characters.CharacterStat.WinRate => "competitive viability",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetCompetitiveChangeDescription(BalanceChange change)
        {
            float delta = change.newValue - change.previousValue;
            string direction = delta > 0 ? "buffed" : "nerfed";
            string intensity = Mathf.Abs(delta) switch
            {
                > 20f => "heavily",
                > 10f => "significantly", 
                > 5f => "moderately",
                _ => "slightly"
            };
            
            return $"{change.character} {intensity} {direction} in competitive play";
        }
        
        private string GetSkillImpactDescription(BalanceChange change)
        {
            if (!AffectsSkillExpression(change)) return "minimal skill impact";
            
            bool increases = change.newValue > change.previousValue;
            return increases ? "increases skill ceiling" : "lowers skill ceiling";
        }
    }
}