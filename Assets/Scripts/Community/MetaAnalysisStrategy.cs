using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: High-level meta analysis and strategic discussions
    /// Focuses on overall game health, diversity, and strategic implications
    /// Analytical tone with measured reactions and strategic thinking
    /// </summary>
    public class MetaAnalysisStrategy : BaseFeedbackStrategy
    {
        private readonly string[] analysts = { 
            "MetaAnalyst", "StrategicMind", "BalanceExpert", "MetaTheory", "GameHealthGuru",
            "StrategistPro", "MetaPhilosopher", "BalanceScientist", "MetaArchitect", "SystemThinker"
        };
        
        public MetaAnalysisStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "The current meta is becoming more diverse with these changes ⚖",
                "These adjustments should shake up the competitive scene ►",
                "Finally seeing some rock-paper-scissors balance in character picks ♦",
                "This patch promotes healthy meta diversity ★",
                "Strategic depth increases with these character changes ▲",
                "The meta ecosystem is evolving in positive directions ✓",
                "These changes create interesting strategic options ►",
                "Meta health indicator: improved character variety ⚖",
                "Strategic landscape expanding with these adjustments ♦",
                "This patch addresses core meta imbalances effectively ★"
            };
            
            negativeTemplates = new[]
            {
                "The meta is stale. These changes don't address core issues ◆",
                "This meta shift feels forced and unnatural ✗",
                "Strategic diversity declining with these adjustments ↓",
                "Meta centralization worsening despite balance attempts ■",
                "These changes create unhealthy meta patterns ◆",
                "The strategic landscape is becoming one-dimensional ✗",
                "Meta stagnation continues with these insufficient changes ↓",
                "Balance philosophy seems inconsistent with these updates ■",
                "Strategic rock-paper-scissors breaking down ◆",
                "Meta health declining - need more comprehensive changes ✗"
            };
            
            neutralTemplates = new[]
            {
                "Interesting direction for the meta - let's see how it develops ●",
                "Meta prediction: things are about to change significantly ►",
                "The strategic implications of these changes are complex ♦",
                "Meta analysis: transitional period beginning ●",
                "Strategic equilibrium shifting with these adjustments ►",
                "Long-term meta effects remain to be determined ♦",
                "The meta is entering an experimental phase ●",
                "Strategic paradigm potentially shifting ►",
                "Meta stability testing period initiated ♦",
                "Analytical perspective: meta flux incoming ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.MetaAnalysis;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // Meta analysis is less frequent but more thoughtful
            float basePriority = 0.3f;
            
            // Higher priority for multiple character changes (affects meta more)
            if (changes.Count >= 3)
                basePriority += 0.3f;
            
            // Boost for changes that affect overall balance/diversity
            bool hasMetaImplications = changes.Any(c => 
                c.stat == Characters.CharacterStat.WinRate ||
                (c.magnitude > 12f && IsMetaRelevantStat(c.stat)));
            
            if (hasMetaImplications)
                basePriority += 0.25f;
            
            // Higher priority during meta transitions
            if (IsMetaTransitionPeriod())
                basePriority += 0.2f;
            
            // Boost for extreme community sentiment (meta health indicator)
            if (sentiment < 35f || sentiment > 75f)
                basePriority += 0.15f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Meta analysis applies to broader strategic implications
            return changes.Count >= 2 || // Multiple changes needed for meta analysis
                   changes.Any(c => c.magnitude > 10f && IsMetaRelevantStat(c.stat));
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // For meta analysis, focus on the most strategically significant change
            var metaRelevantChanges = changes.Where(c => IsMetaRelevantStat(c.stat))
                .OrderByDescending(c => GetMetaImpactScore(c)).ToList();
            
            if (metaRelevantChanges.Count > 0)
            {
                return metaRelevantChanges.First();
            }
            
            // Fall back to largest change
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            // Meta analysts are less emotional, more strategic
            float baseSentiment = (communitySentiment - 50f) / 70f; // Dampened emotional response
            
            if (change != null)
            {
                // Focus on meta health rather than character preference
                float metaHealthScore = CalculateMetaHealthImpact(change);
                baseSentiment += metaHealthScore * 0.6f;
                
                // Analysts appreciate strategic depth
                if (IncreasesStrategicDepth(change))
                {
                    baseSentiment += Random.Range(0.3f, 0.5f);
                }
                else if (ReducesStrategicDepth(change))
                {
                    baseSentiment -= Random.Range(0.2f, 0.4f);
                }
                
                // Consider long-term implications
                if (HasLongTermImplications(change))
                {
                    baseSentiment += Random.Range(-0.2f, 0.3f); // Could be positive or negative
                }
            }
            
            // Low variance - analysts are consistent
            baseSentiment += Random.Range(-0.1f, 0.1f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // Meta analysis generates thoughtful discussion
            float engagementMultiplier = (Mathf.Abs(sentiment) + 1f) * 1.8f;
            
            int upvotes = (int)(Random.Range(15, 50) * engagementMultiplier);
            int replies = (int)(Random.Range(8, 30) * engagementMultiplier);
            
            // Strategic discussions generate lots of replies
            replies = (int)(replies * 1.4f);
            
            // Thoughtful analysis gets upvoted by strategic players
            if (sentiment > 0.2f || Mathf.Abs(sentiment) < 0.3f) // Positive or neutral analysis
            {
                upvotes = (int)(upvotes * 1.3f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            // Meta analysis comes from strategic segments
            var weights = new Dictionary<string, float>
            {
                { "Pro Players", 0.35f },       // Professional strategic insight
                { "Competitive", 0.30f },       // Competitive strategic thinking
                { "Content Creators", 0.25f },  // Analysis content
                { "Casual Players", 0.10f }     // Some strategic casuals
            };
            
            return SelectWeightedSegment(segments, weights);
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            var segment = GetTargetSegment(segments);
            
            // 50% chance of using analyst name regardless of segment
            if (Random.Range(0f, 1f) < 0.5f)
            {
                return analysts[Random.Range(0, analysts.Length)];
            }
            
            // Otherwise use segment-specific names with analytical flair
            return segment switch
            {
                "Pro Players" => GetAnalyticalProName(),
                "Content Creators" => GetAnalyticalCreatorName(),
                "Competitive" => GetAnalyticalCompName(),
                _ => GetGenericAnalystName()
            };
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetStrategicStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", GetStrategicChangeDescription(change))
                .Replace("{META_IMPACT}", GetMetaImpactDescription(change))
                .Replace("{STRATEGIC_DEPTH}", GetStrategicDepthDescription(change));
        }
        
        private bool IsMetaRelevantStat(Characters.CharacterStat stat)
        {
            // Stats that significantly impact meta game
            return stat switch
            {
                Characters.CharacterStat.WinRate => true,     // Directly affects meta
                Characters.CharacterStat.Damage => true,     // Affects kill trades
                Characters.CharacterStat.Health => true,     // Affects survivability meta
                Characters.CharacterStat.Speed => true,      // Affects positioning meta
                Characters.CharacterStat.Utility => true,    // Affects team comp meta
                Characters.CharacterStat.Popularity => false, // Effect, not cause
                _ => false
            };
        }
        
        private float GetMetaImpactScore(BalanceChange change)
        {
            float score = change.magnitude;
            
            // Weight by meta relevance
            score *= change.stat switch
            {
                Characters.CharacterStat.WinRate => 2.5f,  // Highest meta impact
                Characters.CharacterStat.Damage => 2.0f,   // High impact on trades
                Characters.CharacterStat.Health => 1.8f,   // High impact on durability meta
                Characters.CharacterStat.Utility => 1.6f,  // Team comp impact
                Characters.CharacterStat.Speed => 1.4f,    // Positioning meta impact
                _ => 1.0f
            };
            
            return score;
        }
        
        private float CalculateMetaHealthImpact(BalanceChange change)
        {
            // Positive score = healthier meta, negative = less healthy
            float healthImpact = 0f;
            
            if (change.stat == Characters.CharacterStat.WinRate)
            {
                float targetWinRate = 50f;
                float beforeDistance = Mathf.Abs(change.previousValue - targetWinRate);
                float afterDistance = Mathf.Abs(change.newValue - targetWinRate);
                
                if (afterDistance < beforeDistance)
                    healthImpact += 0.5f; // Moving toward balance
                else
                    healthImpact -= 0.3f; // Moving away from balance
            }
            
            // Changes that reduce extreme outliers are healthy
            if (change.magnitude > 15f && change.IsPositiveChange())
                healthImpact += 0.3f;
            
            return healthImpact;
        }
        
        private bool IncreasesStrategicDepth(BalanceChange change)
        {
            // Changes that add viable options increase depth
            return (change.stat == Characters.CharacterStat.WinRate && 
                    change.newValue > 45f && change.newValue < 55f) || // Brings into viable range
                   (change.stat == Characters.CharacterStat.Utility && change.newValue > change.previousValue);
        }
        
        private bool ReducesStrategicDepth(BalanceChange change)
        {
            // Changes that reduce viable options reduce depth
            return (change.stat == Characters.CharacterStat.WinRate && 
                    (change.newValue < 40f || change.newValue > 60f)) || // Pushes out of viable range
                   (change.magnitude > 25f); // Extreme changes often reduce options
        }
        
        private bool HasLongTermImplications(BalanceChange change)
        {
            // Major changes have lasting meta effects
            return change.magnitude > 15f || 
                   (change.stat == Characters.CharacterStat.WinRate && change.magnitude > 8f);
        }
        
        private bool IsMetaTransitionPeriod()
        {
            // Simulate meta cycles - some periods have more changes
            int currentWeek = Core.PhaseManager.Instance?.GetCurrentWeek() ?? 1;
            return (currentWeek % 6 == 0) || (currentWeek % 6 == 1); // Transition periods
        }
        
        private string SelectWeightedSegment(List<CommunitySegmentData> segments, Dictionary<string, float> weights)
        {
            if (segments.Count == 0) return "Competitive";
            
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
        
        private string GetStrategicStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "survivability framework",
                Characters.CharacterStat.Damage => "damage paradigm",
                Characters.CharacterStat.Speed => "mobility meta",
                Characters.CharacterStat.Utility => "strategic utility",
                Characters.CharacterStat.WinRate => "competitive viability",
                Characters.CharacterStat.Popularity => "meta presence",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetStrategicChangeDescription(BalanceChange change)
        {
            float delta = change.newValue - change.previousValue;
            string direction = delta > 0 ? "enhanced" : "reduced";
            string scope = Mathf.Abs(delta) switch
            {
                > 20f => "fundamentally restructured",
                > 12f => "significantly adjusted",
                > 6f => "strategically modified",
                _ => "fine-tuned"
            };
            
            return $"{change.character} {scope} their {GetStrategicStatName(change.stat)}";
        }
        
        private string GetMetaImpactDescription(BalanceChange change)
        {
            float impact = GetMetaImpactScore(change);
            return impact switch
            {
                > 40f => "meta-defining impact",
                > 25f => "significant meta implications",
                > 15f => "notable strategic effects",
                > 8f => "moderate meta influence",
                _ => "minor strategic adjustment"
            };
        }
        
        private string GetStrategicDepthDescription(BalanceChange change)
        {
            if (IncreasesStrategicDepth(change))
                return "expanding strategic options";
            else if (ReducesStrategicDepth(change))
                return "narrowing strategic choices";
            else
                return "maintaining strategic equilibrium";
        }
        
        private string GetAnalyticalProName()
        {
            var teams = new[] { "TSM", "C9", "TL" };
            var titles = new[] { "Analyst", "Strategist", "TheoryMaster" };
            return $"{teams[Random.Range(0, teams.Length)]}_{titles[Random.Range(0, titles.Length)]}";
        }
        
        private string GetAnalyticalCreatorName()
        {
            var names = new[] { "MetaAnalysisYT", "StrategyGuruTV", "BalanceTheoryYT", "MetaPhilosophyTV" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetAnalyticalCompName()
        {
            var names = new[] { "StrategicMind", "MetaTheorist", "BalanceStudent", "StrategistPro" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetGenericAnalystName()
        {
            return analysts[Random.Range(0, analysts.Length)];
        }
    }
}