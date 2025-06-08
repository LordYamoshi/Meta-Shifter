using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: Generates immediate reactions to balance changes
    /// Highest activity level - most common feedback type
    /// Focuses on core stat changes and player frustration/satisfaction
    /// </summary>
    public class BalanceReactionStrategy : BaseFeedbackStrategy
    {
        public BalanceReactionStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "Finally! {CHARACTER} feels balanced now ✓ Thank you devs!",
                "Great changes to {CHARACTER}! This is what we needed ★",
                "Perfect adjustment to {CHARACTER} {STAT} - well done team!",
                "{CHARACTER} is in a much better spot after these changes ♦",
                "Love the {CHARACTER} rework - feels fair to play against now ►",
                "These {CHARACTER} changes fix everything that was wrong ⚖",
                "Excellent balance work on {CHARACTER} - more like this please!",
                "{CHARACTER} finally viable again! Amazing work ▲",
                "This {CHARACTER} update brings them back to relevance ★",
                "Spot on with the {CHARACTER} adjustments - perfect balance ✓"
            };
            
            negativeTemplates = new[]
            {
                "{CHARACTER} is completely broken now ✗ What were you thinking?",
                "Why did you nerf {CHARACTER}? This ruins everything ↓",
                "Another patch, another character ruined... ✗",
                "{CHARACTER} was fine before, now they're useless ↓",
                "Whoever balanced {CHARACTER} doesn't understand the game ◆",
                "RIP {CHARACTER} - from hero to zero in one patch †",
                "These {CHARACTER} changes are way too harsh ■",
                "Can we revert the {CHARACTER} changes please? This is awful ✗",
                "{CHARACTER} mains on suicide watch - completely gutted ↓",
                "This {CHARACTER} nerf makes no sense at all ◆"
            };
            
            neutralTemplates = new[]
            {
                "Interesting changes to {CHARACTER} - let's see how it plays out ►",
                "{CHARACTER} feels different but need more time to judge ♦",
                "Not sure about these {CHARACTER} adjustments yet ●",
                "Mixed feelings about the {CHARACTER} changes ◆",
                "{CHARACTER} changes are... unexpected to say the least ►",
                "Time will tell if these {CHARACTER} changes are good ●",
                "Neutral on the {CHARACTER} rework - could go either way ♦",
                "The {CHARACTER} changes are definitely noticeable ■",
                "These {CHARACTER} adjustments require adaptation ►",
                "Cautiously optimistic about the {CHARACTER} updates ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.BalanceReaction;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // HIGHEST priority strategy - balance reactions are most common
            float basePriority = 0.85f; // Very high base priority
            
            // Major boost for significant changes
            float maxMagnitude = changes.Max(c => c.magnitude);
            basePriority += Mathf.Clamp01(maxMagnitude / 15f) * 0.15f;
            
            // Extra boost for core stat changes (Health, Damage, WinRate)
            bool hasCoreStatChange = changes.Any(c => 
                c.stat == Characters.CharacterStat.Health || 
                c.stat == Characters.CharacterStat.Damage ||
                c.stat == Characters.CharacterStat.WinRate);
            
            if (hasCoreStatChange)
                basePriority += 0.1f;
            
            // Boost for extreme sentiment (people react strongly)
            if (sentiment < 25f || sentiment > 75f)
                basePriority += 0.05f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Almost always applies - balance reactions are universal
            return changes.Any(c => c.magnitude > 3f); // Very low threshold
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes to core stats that players notice immediately
            var coreStatChanges = changes.Where(c => 
                c.stat == Characters.CharacterStat.Health || 
                c.stat == Characters.CharacterStat.Damage ||
                c.stat == Characters.CharacterStat.WinRate).ToList();
            
            if (coreStatChanges.Count > 0)
            {
                return coreStatChanges.OrderByDescending(c => c.magnitude).First();
            }
            
            // Fall back to any significant change
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 50f; // Convert to -1,1 range
            
            if (change != null)
            {
                // Strong reactions to balance changes
                if (change.IsPositiveChange())
                {
                    baseSentiment += Random.Range(0.3f, 0.8f); // Strong positive reaction
                }
                else if (change.magnitude > 10f)
                {
                    baseSentiment -= Random.Range(0.4f, 0.9f); // Strong negative reaction
                }
                
                // Popular characters get more emotional reactions
                if (IsPopularCharacter(change.character))
                {
                    baseSentiment *= 1.3f; // Amplify emotion for popular characters
                }
                
                // Nerfs to damage/health get especially strong reactions
                if ((change.stat == Characters.CharacterStat.Damage || 
                     change.stat == Characters.CharacterStat.Health) && 
                    change.newValue < change.previousValue)
                {
                    baseSentiment -= 0.3f; // Extra negative reaction to nerfs
                }
            }
            
            // Add emotional variance
            baseSentiment += Random.Range(-0.3f, 0.3f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Balance reactions get high engagement due to emotional nature
            float engagementMultiplier = (Mathf.Abs(sentiment) + 0.7f) * 2f;
            
            int upvotes = (int)(Random.Range(8, 45) * engagementMultiplier);
            int replies = (int)(Random.Range(3, 25) * engagementMultiplier);
            
            // Negative reactions generate more discussion/arguments
            if (sentiment < -0.5f)
            {
                replies = (int)(replies * 1.6f);
            }
            
            // Positive reactions get more upvotes
            if (sentiment > 0.5f)
            {
                upvotes = (int)(upvotes * 1.4f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            // Balance reactions come from all segments, weighted by activity
            var weights = new Dictionary<string, float>
            {
                { "Competitive", 0.35f },      // Most vocal about balance
                { "Casual Players", 0.30f },   // Large population
                { "Pro Players", 0.20f },      // High influence
                { "Content Creators", 0.15f }  // Create discussion
            };
            
            return SelectWeightedSegment(segments, weights);
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetFriendlyStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", change.GetChangeDescription())
                .Replace("{MAGNITUDE}", GetMagnitudeDescription(change.magnitude));
        }
        
        private string GetFriendlyStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "health",
                Characters.CharacterStat.Damage => "damage",
                Characters.CharacterStat.Speed => "mobility",
                Characters.CharacterStat.Utility => "utility",
                Characters.CharacterStat.WinRate => "performance",
                Characters.CharacterStat.Popularity => "pick rate",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetMagnitudeDescription(float magnitude)
        {
            return magnitude switch
            {
                > 25f => "massive",
                > 15f => "huge",
                > 10f => "significant", 
                > 5f => "noticeable",
                _ => "minor"
            };
        }
        
        private bool IsPopularCharacter(Characters.CharacterType character)
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return false;
            
            float popularity = characterManager.GetStat(character, Characters.CharacterStat.Popularity);
            return popularity > 55f; // Lower threshold for reactions
        }
        
        private string SelectWeightedSegment(List<CommunitySegmentData> segments, Dictionary<string, float> weights)
        {
            if (segments.Count == 0) return "General";
            
            float totalWeight = 0f;
            var weightedSegments = new List<(CommunitySegmentData segment, float weight)>();
            
            foreach (var segment in segments)
            {
                float weight = weights.ContainsKey(segment.segmentName) ? weights[segment.segmentName] : 0.1f;
                weight *= segment.activityLevel; // Apply segment activity multiplier
                
                weightedSegments.Add((segment, weight));
                totalWeight += weight;
            }
            
            if (totalWeight == 0f) return segments[0].segmentName;
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var (segment, weight) in weightedSegments)
            {
                currentWeight += weight;
                if (randomValue <= currentWeight)
                {
                    return segment.segmentName;
                }
            }
            
            return segments[0].segmentName;
        }
    }
}