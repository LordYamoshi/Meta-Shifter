using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// CLEAN PopularityShiftStrategy - ONLY this class, no duplicates
    /// Observes and comments on character popularity trends and meta shifts
    /// Only triggers on actual popularity changes in the data
    /// </summary>
    public class PopularityShiftStrategy : BaseFeedbackStrategy
    {
        private readonly string[] trendWatchers = { 
            "MetaWatcher", "TrendAnalyst", "PickRateTracker", "MetaObserver", "UsageAnalyzer",
            "PopularityHawk", "MetaTrends", "StatWatcher", "TrendSpotter", "MetaShiftAlert"
        };
        
        public PopularityShiftStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "Everyone's playing {CHARACTER} now! ▲ Pick rate skyrocketing",
                "Is it just me or is {CHARACTER} everywhere? ★",
                "{CHARACTER} is the new meta pick, calling it now ►",
                "Finally seeing {CHARACTER} in every game ✓ Meta shift incoming",
                "{CHARACTER} usage went through the roof this patch ▲",
                "Meta update: {CHARACTER} is becoming the new favorite ♦",
                "Pick rate alert: {CHARACTER} climbing fast! ★",
                "The {CHARACTER} renaissance is real - everyone's trying them ►",
                "From zero to hero: {CHARACTER} pick rate doubled overnight ✓",
                "Trend confirmed: {CHARACTER} is the new meta darling ▲"
            };
            
            negativeTemplates = new[]
            {
                "Nobody plays {CHARACTER} anymore ↓ Pick rate in free fall",
                "RIP {CHARACTER} pickrate... you will be missed †",
                "When was the last time you saw {CHARACTER} in a game? ✗",
                "{CHARACTER} usage collapsed after the changes ↓",
                "Meta obituary: {CHARACTER} pick rate died this patch †",
                "From hero to zero: {CHARACTER} usage plummeted ✗",
                "Pick rate tracker: {CHARACTER} abandoned by players ↓",
                "The great {CHARACTER} exodus continues... sad to see †",
                "Usage statistics: {CHARACTER} becoming extinct ✗",
                "Meta funeral: {CHARACTER} pick rate buried six feet under ↓"
            };
            
            neutralTemplates = new[]
            {
                "{CHARACTER} usage seems to be shifting ●",
                "Interesting changes in {CHARACTER} popularity trends ►",
                "Meta analysis: {CHARACTER} pick rate stabilizing ♦",
                "{CHARACTER} finding their new place in the meta ●",
                "Pick rate report: {CHARACTER} usage adjusting post-patch ►",
                "Trend watch: {CHARACTER} popularity in transition ♦",
                "Usage patterns for {CHARACTER} are evolving ●",
                "Meta snapshot: {CHARACTER} pick rate finding equilibrium ►",
                "Popularity update: {CHARACTER} usage rebalancing ♦",
                "Statistical note: {CHARACTER} trends moderating ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.PopularityShift;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // ONLY triggers on actual popularity changes
            var popularityChanges = changes.Where(c => c.stat == Characters.CharacterStat.Popularity).ToList();
            
            if (popularityChanges.Count == 0) return 0f;
            
            // Base priority for popularity tracking
            float basePriority = 0.4f;
            
            // Higher priority for significant popularity shifts
            float maxPopularityChange = popularityChanges.Max(c => c.magnitude);
            if (maxPopularityChange > 15f)
                basePriority += 0.4f; // Major shift
            else if (maxPopularityChange > 8f)
                basePriority += 0.2f; // Noticeable shift
            
            // Boost for characters reaching extreme popularity
            bool hasExtremePopularity = popularityChanges.Any(c => 
                c.newValue > 80f || c.newValue < 20f);
            
            if (hasExtremePopularity)
                basePriority += 0.3f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // ONLY applies when there are actual popularity changes
            return changes.Any(c => c.stat == Characters.CharacterStat.Popularity && c.magnitude > 5f);
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Only consider popularity changes
            var popularityChanges = changes.Where(c => c.stat == Characters.CharacterStat.Popularity).ToList();
            
            if (popularityChanges.Count == 0) return null;
            
            // Select the most dramatic popularity shift
            return popularityChanges.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            // Popularity tracking comes from analytical segments
            var weights = new Dictionary<string, float>
            {
                { "Competitive", 0.4f },      // Track meta trends
                { "Content Creators", 0.3f }, // Content about trends
                { "Pro Players", 0.2f },      // Professional meta awareness
                { "Casual Players", 0.1f }    // Some casuals notice trends
            };
            
            return SelectWeightedSegment(segments, weights);
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            // Mix of segment-specific names and trend watcher names
            var segment = GetTargetSegment(segments);
            
            if (Random.Range(0f, 1f) < 0.4f) // 40% chance of generic trend watcher name
            {
                return trendWatchers[Random.Range(0, trendWatchers.Length)];
            }
            
            // Otherwise use segment-specific name
            return segment switch
            {
                "Pro Players" => GetProPlayerName(),
                "Content Creators" => GetContentCreatorName(),
                "Competitive" => GetCompetitiveName(),
                _ => GetGenericName()
            };
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
        
        private string GetProPlayerName()
        {
            var teams = new[] { "TSM", "FaZe", "C9", "TL", "G2" };
            var names = new[] { "ProAnalyst", "MetaReader", "TrendSpotter" };
            return $"{teams[Random.Range(0, teams.Length)]}_{names[Random.Range(0, names.Length)]}";
        }
        
        private string GetContentCreatorName()
        {
            var names = new[] { "MetaTrendsYT", "TierListGuru", "PickRateTracker", "MetaWatchTV" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetCompetitiveName()
        {
            var names = new[] { "MetaAnalyst", "TrendWatcher", "CompetitiveObserver", "RankedTracker" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetGenericName()
        {
            var names = new[] { "StatObserver", "TrendFinder", "MetaMonitor", "UsageTracker" };
            return names[Random.Range(0, names.Length)];
        }
    }
}