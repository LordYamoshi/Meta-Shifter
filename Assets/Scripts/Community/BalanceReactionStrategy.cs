using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Concrete Strategy: Generates direct reactions to balance changes
    /// Focuses on immediate player responses to character stat modifications
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
                "Finally! {CHARACTER} feels balanced now {EMOJI}",
                "Great changes to {CHARACTER}! This is what we needed ✅",
                "Perfect adjustment to {CHARACTER} {STAT} - well done devs!",
                "{CHARACTER} is in a much better spot after these changes 👍",
                "Love the {CHARACTER} rework - feels fair to play against now",
                "These {CHARACTER} changes fix everything that was wrong 🎯",
                "Excellent balance work on {CHARACTER} - more like this please!",
                "{CHARACTER} finally viable again! Thank you devs 🙌"
            };
            
            negativeTemplates = new[]
            {
                "{CHARACTER} is completely broken now 😠",
                "Why did you nerf {CHARACTER}? This ruins the game 😢",
                "Another patch, another character ruined...",
                "{CHARACTER} was fine before, now they're useless 👎",
                "Whoever balanced {CHARACTER} doesn't play the game",
                "RIP {CHARACTER} - from hero to zero in one patch 💔",
                "These {CHARACTER} changes are way too harsh",
                "Can we revert the {CHARACTER} changes please? This is awful",
                "{CHARACTER} mains on suicide watch 😭"
            };
            
            neutralTemplates = new[]
            {
                "Interesting changes to {CHARACTER} - let's see how it plays out",
                "{CHARACTER} feels different but need more time to judge",
                "Not sure about these {CHARACTER} adjustments yet 🤔",
                "Mixed feelings about the {CHARACTER} changes",
                "{CHARACTER} changes are... unexpected",
                "Time will tell if these {CHARACTER} changes are good",
                "Neutral on the {CHARACTER} rework - could go either way",
                "The {CHARACTER} changes are definitely noticeable"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.BalanceReaction;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // High priority for significant balance changes
            float maxMagnitude = changes.Max(c => c.magnitude);
            float basePriority = Mathf.Clamp01(maxMagnitude / 20f);
            
            // Boost priority for core stat changes (Health, Damage)
            bool hasCoreStatChange = changes.Any(c => 
                c.stat == Characters.CharacterStat.Health || 
                c.stat == Characters.CharacterStat.Damage);
            
            if (hasCoreStatChange)
                basePriority += 0.3f;
            
            // Boost priority for extreme sentiment
            if (sentiment < 30f || sentiment > 70f)
                basePriority += 0.2f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            return changes.Any(c => c.IsSignificantChange());
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes to core stats
            var coreStatChanges = changes.Where(c => 
                c.stat == Characters.CharacterStat.Health || 
                c.stat == Characters.CharacterStat.Damage ||
                c.stat == Characters.CharacterStat.WinRate).ToList();
            
            if (coreStatChanges.Count > 0)
            {
                return coreStatChanges.OrderByDescending(c => c.magnitude).First();
            }
            
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 50f;
            
            if (change != null)
            {
                if (change.IsPositiveChange())
                {
                    baseSentiment += Random.Range(0.2f, 0.6f);
                }
                else if (change.IsSignificantChange())
                {
                    baseSentiment -= Random.Range(0.3f, 0.8f);
                }
                
                if (IsPopularCharacter(change.character))
                {
                    baseSentiment -= 0.2f;
                }
            }
            
            baseSentiment += Random.Range(-0.3f, 0.3f);
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            float engagementMultiplier = (Mathf.Abs(sentiment) + 0.5f) * 1.5f;
            
            int upvotes = (int)(Random.Range(5, 30) * engagementMultiplier);
            int replies = (int)(Random.Range(2, 15) * engagementMultiplier);
            
            if (sentiment < -0.5f)
            {
                replies = (int)(replies * 1.5f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            var weights = new Dictionary<string, float>
            {
                { "Competitive", 0.4f },
                { "Pro Players", 0.3f },
                { "Casual Players", 0.2f },
                { "Content Creators", 0.1f }
            };
            
            return SelectWeightedSegment(segments, weights);
        }
        
        private bool IsPopularCharacter(Characters.CharacterType character)
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return false;
            
            float popularity = characterManager.GetStat(character, Characters.CharacterStat.Popularity);
            return popularity > 60f;
        }
        
        private string SelectWeightedSegment(List<CommunitySegmentData> segments, Dictionary<string, float> weights)
        {
            if (segments.Count == 0) return "General";
            
            float totalWeight = 0f;
            var weightedSegments = new List<(CommunitySegmentData segment, float weight)>();
            
            foreach (var segment in segments)
            {
                float weight = weights.ContainsKey(segment.segmentName) ? weights[segment.segmentName] : 0.1f;
                weight *= segment.activityLevel;
                
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