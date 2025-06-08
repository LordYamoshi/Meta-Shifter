using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: Content creator perspectives focusing on content opportunities
    /// High engagement multiplier, creator-focused language (videos, streams, guides)
    /// Emphasizes content creation potential and audience engagement
    /// </summary>
    public class ContentCreatorStrategy : BaseFeedbackStrategy
    {
        private readonly string[] creatorNames = { 
            "StreamMaster", "YouTubeGuru", "TwitchKing", "ContentQueen", "GameGuruYT", 
            "MetaMasterTV", "BalanceWatchYT", "ProGuideGamer", "TierListLord", "PatchNotesTV" 
        };
        
        private readonly string[] contentTypes = { 
            "guide", "tier list", "reaction video", "analysis stream", "breakdown", 
            "tutorial", "review", "discussion", "hot take", "deep dive" 
        };
        
        public ContentCreatorStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "New {CHARACTER} guide coming soon! These changes are HUGE ▲",
                "Stream tonight: Testing the new {CHARACTER} changes live! ►",
                "Subscribe for my reaction to these balance updates! ★",
                "Making a {CHARACTER} tier list with these amazing changes ✓",
                "This {CHARACTER} buff creates so much content potential! ♦",
                "Finally! {CHARACTER} is guide-worthy again - video dropping soon ▲",
                "These {CHARACTER} changes deserve a full analysis stream ►",
                "My viewers called these {CHARACTER} buffs perfectly! ★",
                "Content creators, we eating good with these {CHARACTER} updates ✓",
                "Tutorial Tuesday: New {CHARACTER} strategies after the patch ♦"
            };
            
            negativeTemplates = new[]
            {
                "Making a rant video about these {CHARACTER} changes ✗",
                "This {CHARACTER} nerf ruins all my content plans ↓",
                "Time for an emergency stream - {CHARACTER} got destroyed ■",
                "Another character gutted... making a farewell tribute video †",
                "My {CHARACTER} guide series is now completely outdated ◆",
                "Viewers are asking for refunds on my {CHARACTER} course ↓",
                "This {CHARACTER} nerf makes no content sense whatsoever ✗",
                "Stream title: 'RIP {CHARACTER} - What Were They Thinking?' ■",
                "My {CHARACTER} tier placement aged like milk in one patch †",
                "Making an angry reaction to these terrible {CHARACTER} changes ◆"
            };
            
            neutralTemplates = new[]
            {
                "Making a tier list video about these {CHARACTER} updates ●",
                "Hot take incoming: These {CHARACTER} changes will change everything ►",
                "Poll time: What do you think about the new {CHARACTER}? ♦",
                "Breaking down the {CHARACTER} changes in tonight's stream ●",
                "Tier list needs updating after these {CHARACTER} adjustments ►",
                "Analysis video coming: {CHARACTER} before vs after ♦",
                "Community poll: Rate the {CHARACTER} changes 1-10 ●",
                "Reaction stream to the {CHARACTER} patch notes tonight ►",
                "New content idea: {CHARACTER} strategies post-patch ♦",
                "Discussion time: Are these {CHARACTER} changes healthy? ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.ContentCreator;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // Content creators are very vocal about changes - high priority
            float basePriority = 0.6f;
            
            // Higher priority for changes that create content opportunities
            bool hasContentPotential = changes.Any(c => 
                c.magnitude > 8f || // Significant changes = content
                c.stat == Characters.CharacterStat.WinRate || // Meta shifts = content
                IsPopularCharacter(c.character)); // Popular characters = views
            
            if (hasContentPotential)
                basePriority += 0.3f;
            
            // Creators love controversial changes (more engagement)
            if (sentiment < 35f || sentiment > 75f)
                basePriority += 0.2f;
            
            // Multiple changes = compilation content opportunity
            if (changes.Count >= 3)
                basePriority += 0.15f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Content creators comment on most changes - they need content!
            return changes.Any(c => c.magnitude > 4f || IsPopularCharacter(c.character));
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes that generate the most content/views
            var contentWorthyChanges = changes.Where(c => 
                IsPopularCharacter(c.character) || 
                c.magnitude > 10f ||
                c.stat == Characters.CharacterStat.WinRate).ToList();
            
            if (contentWorthyChanges.Count > 0)
            {
                return contentWorthyChanges.OrderByDescending(c => GetContentPotentialScore(c)).First();
            }
            
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 60f; // Slightly less extreme than casual
            
            if (change != null)
            {
                // Content creators love big changes (positive or negative = content)
                if (change.magnitude > 15f)
                {
                    // Big changes are exciting for content creators regardless of direction
                    baseSentiment += Random.Range(0.2f, 0.5f);
                }
                else if (change.IsPositiveChange())
                {
                    baseSentiment += Random.Range(0.3f, 0.6f);
                }
                else if (change.magnitude > 8f)
                {
                    // Nerfs are content gold (rant videos, farewell tributes)
                    baseSentiment -= Random.Range(0.4f, 0.7f);
                }
                
                // Popular characters = more views = more emotional investment
                if (IsPopularCharacter(change.character))
                {
                    baseSentiment *= 1.4f; // Amplify reaction for popular characters
                }
                
                // Changes that affect content planning
                if (IsContentDisruptive(change))
                {
                    baseSentiment -= 0.3f; // Frustration when content plans disrupted
                }
            }
            
            // Content creators are more dramatic for engagement
            baseSentiment += Random.Range(-0.2f, 0.2f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Content creators get VERY high engagement - they have followers
            float engagementMultiplier = (Mathf.Abs(sentiment) + 1.2f) * 3.5f; // 3.5x multiplier
            
            int upvotes = (int)(Random.Range(30, 120) * engagementMultiplier);
            int replies = (int)(Random.Range(20, 60) * engagementMultiplier);
            
            // Controversial takes from creators generate massive discussion
            if (Mathf.Abs(sentiment) > 0.7f)
            {
                replies = (int)(replies * 2.2f); // Huge discussion
                upvotes = (int)(upvotes * 1.8f);
            }
            
            // Positive creator reactions get shared more
            if (sentiment > 0.5f)
            {
                upvotes = (int)(upvotes * 1.6f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Content Creators";
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            return creatorNames[Random.Range(0, creatorNames.Length)];
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetCreatorStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", GetCreatorChangeDescription(change))
                .Replace("{CONTENT_TYPE}", GetRandomContentType())
                .Replace("{ENGAGEMENT_HOOK}", GetEngagementHook(change));
        }
        
        private float GetContentPotentialScore(BalanceChange change)
        {
            float score = change.magnitude;
            
            // Popular characters = more views
            if (IsPopularCharacter(change.character))
                score *= 2f;
            
            // Win rate changes = tier list updates
            if (change.stat == Characters.CharacterStat.WinRate)
                score *= 1.8f;
            
            // Big damage/health changes = guide updates
            if ((change.stat == Characters.CharacterStat.Damage || 
                 change.stat == Characters.CharacterStat.Health) && 
                change.magnitude > 10f)
                score *= 1.6f;
            
            return score;
        }
        
        private bool IsPopularCharacter(Characters.CharacterType character)
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return false;
            
            float popularity = characterManager.GetStat(character, Characters.CharacterStat.Popularity);
            return popularity > 50f; // Content creators care about viewership
        }
        
        private bool IsContentDisruptive(BalanceChange change)
        {
            // Changes that force content creators to redo work
            return (change.stat == Characters.CharacterStat.WinRate && change.magnitude > 12f) ||
                   (change.magnitude > 20f); // Major reworks
        }
        
        private string GetCreatorStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "survivability",
                Characters.CharacterStat.Damage => "damage output", 
                Characters.CharacterStat.Speed => "mobility",
                Characters.CharacterStat.Utility => "utility kit",
                Characters.CharacterStat.WinRate => "tier placement",
                Characters.CharacterStat.Popularity => "pick rate",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetCreatorChangeDescription(BalanceChange change)
        {
            float delta = change.newValue - change.previousValue;
            string direction = delta > 0 ? "buffed" : "nerfed";
            string intensity = Mathf.Abs(delta) switch
            {
                > 25f => "completely reworked",
                > 15f => "massively",
                > 10f => "significantly", 
                > 5f => "notably",
                _ => "slightly"
            };
            
            return $"{change.character} {intensity} {direction}";
        }
        
        private string GetRandomContentType()
        {
            return contentTypes[Random.Range(0, contentTypes.Length)];
        }
        
        private string GetEngagementHook(BalanceChange change)
        {
            var hooks = new[]
            {
                "Let me know your thoughts below!",
                "Drop your predictions in the comments!",
                "What's your take on this change?",
                "Hit that like if you agree!",
                "Subscribe for more balance updates!",
                "Ring the bell for patch reactions!",
                "Share this if you're hyped!",
                "Comment your tier list predictions!"
            };
            
            return hooks[Random.Range(0, hooks.Length)];
        }
    }
}