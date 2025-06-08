using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: Casual player perspectives focusing on fun and accessibility
    /// Emotional, fun-focused reactions with simple language
    /// Emphasizes enjoyment over competition, relatability over analysis
    /// </summary>
    public class CasualPlayerStrategy : BaseFeedbackStrategy
    {
        private readonly string[] casualNames = { 
            "CasualGamer42", "FunPlayer", "WeekendWarrior", "ChillGamer", "JustForFun",
            "RelaxedGamer", "FamilyPlayer", "EveningGamer", "SocialGamer", "FunSeeker",
            "CasualFan", "GameNight", "LeisurePlayer", "HobbyGamer", "EasyGoing"
        };
        
        public CasualPlayerStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "{CHARACTER} is more fun to play now! ♥",
                "Cool, {CHARACTER} feels better to use ✓",
                "I like these {CHARACTER} changes! ►",
                "Finally I can enjoy {CHARACTER} again ♦",
                "These {CHARACTER} buffs make the game more enjoyable ★",
                "My favorite {CHARACTER} got some love! ♥",
                "{CHARACTER} feels so much smoother now ✓",
                "Great changes! {CHARACTER} is actually fun again ►",
                "Love what they did to {CHARACTER} - feels great! ♦",
                "Thank you devs! {CHARACTER} is my main again ★"
            };
            
            negativeTemplates = new[]
            {
                "I just want to have fun with {CHARACTER}, why all these changes? ●",
                "I liked {CHARACTER} the way they were before ↓",
                "{CHARACTER} feels different but I can't explain why ◆",
                "Why did they change my favorite character? ✗",
                "I'm confused about these {CHARACTER} changes ●",
                "{CHARACTER} doesn't feel right anymore ↓",
                "Can we please leave {CHARACTER} alone? ◆",
                "I don't understand why they nerfed {CHARACTER} ✗",
                "These changes make {CHARACTER} less fun somehow ●",
                "I wish they would stop changing {CHARACTER} every patch ↓"
            };
            
            neutralTemplates = new[]
            {
                "As a casual player, I don't really notice these {CHARACTER} changes ●",
                "These changes are too complicated... I just want to play ►",
                "Not sure what to think about the {CHARACTER} updates ♦",
                "I guess {CHARACTER} is different now? ●",
                "Still learning how these {CHARACTER} changes affect me ►",
                "Maybe I need to try {CHARACTER} again after these changes ♦",
                "I play for fun so {CHARACTER} changes don't matter much ●",
                "Casual opinion: {CHARACTER} still seems okay to me ►",
                "Don't really get the {CHARACTER} changes but whatever ♦",
                "I'll adapt to the new {CHARACTER} eventually ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.CasualPlayerFeedback;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // Casual players comment regularly but less analytically
            float basePriority = 0.5f;
            
            // Higher priority for changes to fun/accessible aspects
            bool affectsCasualExperience = changes.Any(c => 
                c.stat == Characters.CharacterStat.Health || // Survivability matters to casuals
                (c.stat == Characters.CharacterStat.Damage && c.magnitude > 8f) || // Big damage changes noticed
                IsPopularWithCasuals(c.character)); // Popular casual characters
            
            if (affectsCasualExperience)
                basePriority += 0.25f;
            
            // Casual players react more to extreme sentiment
            if (sentiment < 40f || sentiment > 70f)
                basePriority += 0.15f;
            
            // Lower priority for technical changes casuals don't notice
            if (changes.All(c => c.stat == Characters.CharacterStat.Utility && c.magnitude < 8f))
                basePriority -= 0.2f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Casual players comment on changes they actually notice
            return changes.Any(c => c.magnitude > 5f && AffectsCasualGameplay(c));
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes casuals notice and care about
            var casualRelevantChanges = changes.Where(c => 
                IsPopularWithCasuals(c.character) || 
                c.stat == Characters.CharacterStat.Health ||
                (c.stat == Characters.CharacterStat.Damage && c.magnitude > 8f)).ToList();
            
            if (casualRelevantChanges.Count > 0)
            {
                return casualRelevantChanges.OrderByDescending(c => GetCasualImpactScore(c)).First();
            }
            
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 45f; // More emotional than pros
            
            if (change != null)
            {
                // Casual players care most about fun and ease of use
                if (change.IsPositiveChange() && MakesCasualFriendlier(change))
                {
                    baseSentiment += Random.Range(0.4f, 0.8f); // Strong positive for fun improvements
                }
                else if (MakesHarderForCasuals(change))
                {
                    baseSentiment -= Random.Range(0.3f, 0.7f); // Negative for difficulty increases
                }
                
                // Attachment to favorite characters
                if (IsPopularWithCasuals(change.character))
                {
                    if (change.newValue < change.previousValue) // Any nerf to popular character
                        baseSentiment -= 0.4f; // Emotional attachment
                    else
                        baseSentiment += 0.3f; // Happy when favorites buffed
                }
                
                // Confusion about complex changes
                if (IsComplexChange(change))
                {
                    baseSentiment -= 0.2f; // Slight negative for confusion
                }
            }
            
            // High emotional variance - casuals are less consistent
            baseSentiment += Random.Range(-0.4f, 0.4f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Moderate engagement - casuals are less vocal than competitive players
            float engagementMultiplier = (Mathf.Abs(sentiment) + 0.6f) * 1.2f;
            
            int upvotes = (int)(Random.Range(5, 25) * engagementMultiplier);
            int replies = (int)(Random.Range(2, 12) * engagementMultiplier);
            
            // Simple positive reactions get more upvotes from fellow casuals
            if (sentiment > 0.3f)
            {
                upvotes = (int)(upvotes * 1.4f);
            }
            
            // Negative reactions get supportive replies
            if (sentiment < -0.3f)
            {
                replies = (int)(replies * 1.3f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Casual Players";
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            return casualNames[Random.Range(0, casualNames.Length)];
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetCasualStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F0")) // No decimals for casuals
                .Replace("{CHANGE}", GetCasualChangeDescription(change))
                .Replace("{FUN_FACTOR}", GetFunFactorDescription(change));
        }
        
        private bool IsPopularWithCasuals(Characters.CharacterType character)
        {
            // Casual players tend to gravitate toward certain character archetypes
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return false;
            
            float popularity = characterManager.GetStat(character, Characters.CharacterStat.Popularity);
            
            // Different popularity threshold for casuals - they like accessible characters
            return character switch
            {
                Characters.CharacterType.Warrior => popularity > 45f, // Straightforward, popular with casuals
                Characters.CharacterType.Tank => popularity > 40f,    // Forgiving gameplay
                Characters.CharacterType.Mage => popularity > 55f,    // Needs to be quite popular
                Characters.CharacterType.Support => popularity > 35f, // Some casuals love support
                _ => popularity > 50f
            };
        }
        
        private bool AffectsCasualGameplay(BalanceChange change)
        {
            // Changes that casuals actually notice in their gameplay
            return change.stat switch
            {
                Characters.CharacterStat.Health => change.magnitude > 5f, // Survivability matters
                Characters.CharacterStat.Damage => change.magnitude > 8f, // Big damage changes noticed
                Characters.CharacterStat.Speed => change.magnitude > 12f, // Only major mobility changes
                Characters.CharacterStat.Utility => change.magnitude > 15f, // Only huge utility changes
                Characters.CharacterStat.WinRate => false, // Casuals don't track win rates
                Characters.CharacterStat.Popularity => false, // Casuals don't care about meta
                _ => false
            };
        }
        
        private bool MakesCasualFriendlier(BalanceChange change)
        {
            // Changes that make characters more accessible/forgiving
            return (change.stat == Characters.CharacterStat.Health && change.newValue > change.previousValue) ||
                   (change.stat == Characters.CharacterStat.Damage && change.newValue > change.previousValue && change.magnitude > 10f);
        }
        
        private bool MakesHarderForCasuals(BalanceChange change)
        {
            // Changes that make characters less forgiving
            return (change.stat == Characters.CharacterStat.Health && change.newValue < change.previousValue && change.magnitude > 8f) ||
                   (change.stat == Characters.CharacterStat.Damage && change.newValue < change.previousValue && change.magnitude > 12f);
        }
        
        private bool IsComplexChange(BalanceChange change)
        {
            // Changes that are hard for casuals to understand
            return (change.stat == Characters.CharacterStat.Utility && change.magnitude > 10f) ||
                   (change.magnitude > 20f); // Major reworks are confusing
        }
        
        private float GetCasualImpactScore(BalanceChange change)
        {
            float score = change.magnitude;
            
            // Weight by casual relevance
            score *= change.stat switch
            {
                Characters.CharacterStat.Health => 2.0f,     // Survivability most important
                Characters.CharacterStat.Damage => 1.6f,     // Damage second
                Characters.CharacterStat.Speed => 1.2f,      // Mobility less important
                Characters.CharacterStat.Utility => 0.8f,    // Utility least important
                Characters.CharacterStat.WinRate => 0.5f,    // Casuals don't track this
                Characters.CharacterStat.Popularity => 0.3f, // Don't care about meta
                _ => 1.0f
            };
            
            // Popular characters matter more to casuals
            if (IsPopularWithCasuals(change.character))
                score *= 1.8f;
            
            return score;
        }
        
        private string GetCasualStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "health",
                Characters.CharacterStat.Damage => "damage",
                Characters.CharacterStat.Speed => "speed",
                Characters.CharacterStat.Utility => "abilities",
                Characters.CharacterStat.WinRate => "performance",
                Characters.CharacterStat.Popularity => "how much people play them",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetCasualChangeDescription(BalanceChange change)
        {
            float delta = change.newValue - change.previousValue;
            string direction = delta > 0 ? "got better" : "got nerfed";
            string intensity = Mathf.Abs(delta) switch
            {
                > 20f => "completely changed",
                > 10f => "really",
                > 5f => "a bit",
                _ => "slightly"
            };
            
            return $"{change.character} {intensity} {direction}";
        }
        
        private string GetFunFactorDescription(BalanceChange change)
        {
            if (MakesCasualFriendlier(change))
                return "more fun to play";
            else if (MakesHarderForCasuals(change))
                return "harder to enjoy";
            else
                return "different somehow";
        }
    }
}