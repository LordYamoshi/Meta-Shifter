using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Strategy: Competitive/Ranked player perspectives
    /// Focuses on ladder climbing, ranked viability, and competitive integrity
    /// Emphasis on skill expression, carry potential, and ranked meta
    /// </summary>
    public class CompetitiveStrategy : BaseFeedbackStrategy
    {
        private readonly string[] competitiveNames = { 
            "RankedClimber", "EsportsHopeful", "TryHardPlayer", "CompetitiveMind", "LadderWarrior",
            "RankedGrinder", "ClimbingHard", "CompetitiveEdge", "SkillExpression", "RankedAce",
            "TournamentBound", "CompetitiveFocus", "RankedAspire", "ProspectPlayer", "SkillPursuit"
        };
        
        private readonly string[] ranks = { "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Grandmaster" };
        
        public CompetitiveStrategy()
        {
            InitializeTemplates();
        }
        
        private void InitializeTemplates()
        {
            positiveTemplates = new[]
            {
                "These {CHARACTER} changes will make ranked more exciting ▲",
                "Competitive integrity > meta staleness. Good changes to {CHARACTER} ⚖",
                "Teams need to adapt their {CHARACTER} strategies ASAP ►",
                "Finally! {CHARACTER} is viable in ranked again ✓",
                "These {CHARACTER} buffs reward skilled players ★",
                "Ranked meta just got way more interesting ▲",
                "{CHARACTER} changes promote skill expression in competitive ♦",
                "This {CHARACTER} adjustment improves ladder balance ⚖",
                "Competitive players will love these {CHARACTER} changes ►",
                "Ranked viability restored for {CHARACTER} - excellent work ✓"
            };
            
            negativeTemplates = new[]
            {
                "These {CHARACTER} changes kill competitive viability completely ✗",
                "This patch drops right before ranked season... questionable timing ↓",
                "Ranked meta just got way less skill-based ◆",
                "{CHARACTER} nerfs remove all carry potential ■",
                "Competitive integrity compromised with these changes ✗",
                "Another character made unviable in ranked play ↓",
                "These {CHARACTER} changes favor luck over skill ◆",
                "Ranked ladder becomes less competitive with this patch ■",
                "Skill ceiling lowered - not good for competitive scene ✗",
                "Tournament viability destroyed for {CHARACTER} ↓"
            };
            
            neutralTemplates = new[]
            {
                "Expecting {CHARACTER} to be pick/ban priority now ●",
                "Another patch, another meta shift. Adapting is part of the game ►",
                "Ranked implications of {CHARACTER} changes need testing ♦",
                "Competitive scene entering adaptation phase ●",
                "Meta shifts require strategic thinking in ranked ►",
                "Tournament formats may need adjustment after these changes ♦",
                "Competitive balance is always evolving ●",
                "Ranked meta stability testing these {CHARACTER} changes ►",
                "Professional scene will determine {CHARACTER} viability ♦",
                "Skill-based adaptation required for {CHARACTER} changes ●"
            };
        }
        
        public override FeedbackType GetFeedbackType()
        {
            return FeedbackType.CompetitiveScene;
        }
        
        public override float GetPriority(List<BalanceChange> changes, float sentiment)
        {
            if (changes.Count == 0) return 0f;
            
            // Competitive players are very vocal about balance
            float basePriority = 0.6f;
            
            // Much higher priority for changes affecting competitive play
            bool hasCompetitiveImpact = changes.Any(c => 
                c.magnitude > 8f || 
                c.stat == Characters.CharacterStat.WinRate ||
                c.stat == Characters.CharacterStat.Damage ||
                c.stat == Characters.CharacterStat.Health);
            
            if (hasCompetitiveImpact)
                basePriority += 0.3f;
            
            // Boost during ranked seasons
            if (IsRankedSeason())
                basePriority += 0.2f;
            
            // Competitive players react strongly to balance concerns
            if (sentiment < 40f || sentiment > 70f)
                basePriority += 0.15f;
            
            // Extra priority for skill expression changes
            if (changes.Any(c => AffectsCarryPotential(c)))
                basePriority += 0.1f;
            
            return Mathf.Clamp01(basePriority);
        }
        
        public override bool ShouldApply(List<BalanceChange> changes, float sentiment)
        {
            // Competitive players comment on changes affecting ranked viability
            return changes.Any(c => c.magnitude > 6f && IsRankedRelevant(c));
        }
        
        protected override BalanceChange SelectRelevantChange(List<BalanceChange> changes)
        {
            if (changes.Count == 0) return null;
            
            // Prioritize changes most relevant to competitive/ranked play
            var rankedRelevantChanges = changes.Where(IsRankedRelevant)
                .OrderByDescending(c => GetCompetitiveRelevanceScore(c)).ToList();
            
            if (rankedRelevantChanges.Count > 0)
            {
                return rankedRelevantChanges.First();
            }
            
            return changes.OrderByDescending(c => c.magnitude).First();
        }
        
        protected override float CalculateFeedbackSentiment(BalanceChange change, float communitySentiment)
        {
            float baseSentiment = (communitySentiment - 50f) / 60f; // Moderate emotional response
            
            if (change != null)
            {
                // Competitive players focus on ranked viability and skill expression
                if (change.IsPositiveChange() && ImprovesMeta(change))
                {
                    baseSentiment += Random.Range(0.4f, 0.7f);
                }
                else if (HurtsCompetitiveIntegrity(change))
                {
                    baseSentiment -= Random.Range(0.5f, 0.8f);
                }
                
                // Carry potential is crucial for competitive players
                if (AffectsCarryPotential(change))
                {
                    if (change.newValue > change.previousValue) // Increased carry potential
                        baseSentiment += 0.3f;
                    else
                        baseSentiment -= 0.4f; // Reduced carry potential
                }
                
                // Skill expression matters
                if (AffectsSkillExpression(change))
                {
                    if (IncreasesSkillCeiling(change))
                        baseSentiment += 0.3f;
                    else
                        baseSentiment -= 0.4f;
                }
                
                // Timing matters for competitive players
                if (IsRankedSeason() && change.magnitude > 12f)
                {
                    baseSentiment -= 0.2f; // Don't like big changes during ranked season
                }
            }
            
            // Moderate variance - competitive but not as analytical as pros
            baseSentiment += Random.Range(-0.2f, 0.2f);
            
            return Mathf.Clamp(baseSentiment, -1f, 1f);
        }
        
        protected override (int upvotes, int replies) GenerateEngagement(float sentiment)
        {
            // High engagement - competitive players are passionate
            float engagementMultiplier = (Mathf.Abs(sentiment) + 0.9f) * 2.5f;
            
            int upvotes = (int)(Random.Range(20, 70) * engagementMultiplier);
            int replies = (int)(Random.Range(10, 35) * engagementMultiplier);
            
            // Controversial competitive opinions generate heated discussion
            if (Mathf.Abs(sentiment) > 0.6f)
            {
                replies = (int)(replies * 1.7f); // Lots of debate
                upvotes = (int)(upvotes * 1.4f);
            }
            
            // Positive competitive changes get strong support
            if (sentiment > 0.4f)
            {
                upvotes = (int)(upvotes * 1.5f);
            }
            
            return (upvotes, replies);
        }
        
        protected override string GetTargetSegment(List<CommunitySegmentData> segments)
        {
            return "Competitive";
        }
        
        protected override string GenerateAuthor(List<CommunitySegmentData> segments)
        {
            // 30% chance of rank-specific name
            if (Random.Range(0f, 1f) < 0.3f)
            {
                var rank = ranks[Random.Range(0, ranks.Length)];
                var baseName = competitiveNames[Random.Range(0, competitiveNames.Length)];
                return $"{rank}_{baseName}";
            }
            
            return competitiveNames[Random.Range(0, competitiveNames.Length)];
        }
        
        protected override string ProcessTemplate(string template, BalanceChange change)
        {
            if (change == null) return template;
            
            return template
                .Replace("{CHARACTER}", change.character.ToString())
                .Replace("{STAT}", GetCompetitiveStatName(change.stat))
                .Replace("{VALUE}", change.newValue.ToString("F1"))
                .Replace("{CHANGE}", GetCompetitiveChangeDescription(change))
                .Replace("{RANK_IMPACT}", GetRankImpactDescription(change))
                .Replace("{SKILL_FACTOR}", GetSkillFactorDescription(change));
        }
        
        private bool IsRankedRelevant(BalanceChange change)
        {
            // Changes that significantly impact ranked/competitive gameplay
            return change.stat switch
            {
                Characters.CharacterStat.WinRate => true,      // Always relevant
                Characters.CharacterStat.Damage => true,      // Affects trades and carries
                Characters.CharacterStat.Health => true,      // Affects survivability
                Characters.CharacterStat.Speed => change.magnitude > 7f, // Positioning matters
                Characters.CharacterStat.Utility => change.magnitude > 10f, // Team utility
                Characters.CharacterStat.Popularity => false, // Effect, not cause
                _ => false
            };
        }
        
        private float GetCompetitiveRelevanceScore(BalanceChange change)
        {
            float score = change.magnitude;
            
            // Weight by competitive importance
            score *= change.stat switch
            {
                Characters.CharacterStat.WinRate => 2.2f,   // High impact on ranked success
                Characters.CharacterStat.Damage => 2.0f,    // Carry potential
                Characters.CharacterStat.Health => 1.8f,    // Survivability in teamfights
                Characters.CharacterStat.Speed => 1.5f,     // Positioning and escapes
                Characters.CharacterStat.Utility => 1.3f,   // Team contribution
                _ => 1.0f
            };
            
            // Boost for characters good in competitive
            if (IsCompetitiveCharacter(change.character))
                score *= 1.4f;
            
            return score;
        }
        
        private bool AffectsCarryPotential(BalanceChange change)
        {
            // Changes that affect ability to carry games
            return (change.stat == Characters.CharacterStat.Damage && change.magnitude > 8f) ||
                   (change.stat == Characters.CharacterStat.WinRate && change.magnitude > 6f) ||
                   (change.stat == Characters.CharacterStat.Health && change.magnitude > 10f);
        }
        
        private bool AffectsSkillExpression(BalanceChange change)
        {
            // Changes that affect skill ceiling/expression
            return (change.stat == Characters.CharacterStat.Damage && change.magnitude > 12f) ||
                   (change.stat == Characters.CharacterStat.Speed && change.magnitude > 10f) ||
                   (change.stat == Characters.CharacterStat.Utility && change.magnitude > 15f);
        }
        
        private bool IncreasesSkillCeiling(BalanceChange change)
        {
            // Buffs to complex mechanics increase skill ceiling
            return change.newValue > change.previousValue && AffectsSkillExpression(change);
        }
        
        private bool ImprovesMeta(BalanceChange change)
        {
            // Changes that improve competitive meta health
            if (change.stat == Characters.CharacterStat.WinRate)
            {
                return change.newValue >= 47f && change.newValue <= 53f; // Competitive range
            }
            
            return change.IsPositiveChange();
        }
        
        private bool HurtsCompetitiveIntegrity(BalanceChange change)
        {
            // Changes that damage competitive balance
            return (change.stat == Characters.CharacterStat.WinRate && 
                    (change.newValue < 42f || change.newValue > 58f)) ||
                   (change.magnitude > 25f); // Extreme changes hurt stability
        }
        
        private bool IsRankedSeason()
        {
            // Simulate ranked season timing
            int currentWeek = Core.PhaseManager.Instance?.GetCurrentWeek() ?? 1;
            return (currentWeek % 10 >= 3 && currentWeek % 10 <= 8); // Weeks 3-8 of 10-week cycles
        }
        
        private bool IsCompetitiveCharacter(Characters.CharacterType character)
        {
            // Some characters are more common in competitive play
            return character switch
            {
                Characters.CharacterType.Warrior => true,  // Versatile in competitive
                Characters.CharacterType.Mage => true,     // High skill ceiling
                Characters.CharacterType.Support => false, // Team dependent
                Characters.CharacterType.Tank => false,    // Less carry potential
                _ => true
            };
        }
        
        private string GetCompetitiveStatName(Characters.CharacterStat stat)
        {
            return stat switch
            {
                Characters.CharacterStat.Health => "survivability",
                Characters.CharacterStat.Damage => "carry potential",
                Characters.CharacterStat.Speed => "positioning ability",
                Characters.CharacterStat.Utility => "team contribution",
                Characters.CharacterStat.WinRate => "ranked viability",
                Characters.CharacterStat.Popularity => "pick priority",
                _ => stat.ToString().ToLower()
            };
        }
        
        private string GetCompetitiveChangeDescription(BalanceChange change)
        {
            float delta = change.newValue - change.previousValue;
            string direction = delta > 0 ? "buffed" : "nerfed";
            string intensity = Mathf.Abs(delta) switch
            {
                > 20f => "drastically",
                > 12f => "heavily",
                > 6f => "significantly",
                _ => "moderately"
            };
            
            return $"{change.character} {intensity} {direction} for competitive play";
        }
        
        private string GetRankImpactDescription(BalanceChange change)
        {
            float impact = GetCompetitiveRelevanceScore(change);
            return impact switch
            {
                > 35f => "game-changing for ranked",
                > 25f => "major ranked implications",
                > 15f => "notable competitive impact",
                > 8f => "moderate ranked effect",
                _ => "minor competitive adjustment"
            };
        }
        
        private string GetSkillFactorDescription(BalanceChange change)
        {
            if (AffectsSkillExpression(change))
            {
                if (IncreasesSkillCeiling(change))
                    return "rewards skilled play";
                else
                    return "lowers skill requirement";
            }
            return "skill-neutral change";
        }
    }
}