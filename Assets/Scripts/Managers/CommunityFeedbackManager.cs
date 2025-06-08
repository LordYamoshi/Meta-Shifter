using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Community
{
    /// <summary>
    /// COMPLETELY REWRITTEN Enhanced Community Feedback Manager
    /// Features all 7 enhanced strategies with smart priority system, realistic engagement patterns,
    /// context-aware reactions, and comprehensive feedback generation
    /// </summary>
    public class CommunityFeedbackManager : MonoBehaviour
    {
        public static CommunityFeedbackManager Instance { get; private set; }
        
        [Header("Enhanced Feedback Generation")]
        [SerializeField] private FeedbackGenerationSettings settings;
        [SerializeField] private List<FeedbackTemplate> legacyFeedbackTemplates; // For backward compatibility
        
        [Header("Enhanced Community Segments")]
        [SerializeField] private List<CommunitySegmentData> communitySegments;
        
        [Header("Strategy Configuration")]
        [Range(0f, 2f)]
        [SerializeField] private float balanceReactionWeight = 1.5f;
        [Range(0f, 2f)]
        [SerializeField] private float proPlayerWeight = 0.8f;
        [Range(0f, 2f)]
        [SerializeField] private float contentCreatorWeight = 1.2f;
        [Range(0f, 2f)]
        [SerializeField] private float casualPlayerWeight = 1.0f;
        [Range(0f, 2f)]
        [SerializeField] private float popularityShiftWeight = 0.6f;
        [Range(0f, 2f)]
        [SerializeField] private float metaAnalysisWeight = 0.7f;
        [Range(0f, 2f)]
        [SerializeField] private float competitiveWeight = 1.1f;
        
        [Header("Enhanced Events")]
        public UnityEvent<List<CommunityFeedback>> OnFeedbackGenerated;
        public UnityEvent<CommunityFeedback> OnNewFeedbackAdded;
        public UnityEvent<float> OnCommunitySentimentChanged;
        public UnityEvent<string> OnStrategyActivated; // New: Track which strategies trigger
        public UnityEvent<FeedbackEventData> OnViralFeedbackGenerated; // New: Track viral content
        
        // Enhanced Strategy Pattern: All 7 strategies with smart management
        private Dictionary<FeedbackType, IFeedbackStrategy> feedbackStrategies;
        private Dictionary<FeedbackType, float> strategyWeights;
        private Dictionary<FeedbackType, int> strategyUsageCount;
        private Dictionary<FeedbackType, float> strategyLastUsed;
        
        // Enhanced Observer Pattern: Comprehensive change tracking
        private List<BalanceChange> recentChanges = new List<BalanceChange>();
        private Queue<CommunityFeedback> activeFeedback = new Queue<CommunityFeedback>();
        private List<CommunityFeedback> viralFeedback = new List<CommunityFeedback>();
        
        // Enhanced sentiment and meta tracking
        private float currentCommunitySentiment = 65f;
        private float sentimentTrend = 0f;
        private int totalFeedbackGenerated = 0;
        private float lastFeedbackGenerationTime = 0f;
        
        // Enhanced context awareness
        private bool isRankedSeason = false;
        private bool isTournamentSeason = false;
        private int currentGameWeek = 1;
        private float metaStabilityScore = 75f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeEnhancedFeedbackSystem();
        }
        
        private void Start()
        {
            SubscribeToEnhancedEvents();
            InitializeEnhancedCommunitySegments();
            InitializeContextualData();
            
            // Create enhanced settings if missing
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<FeedbackGenerationSettings>();
                settings.InitializeDefaults();
                Debug.Log("🔧 Created default FeedbackGenerationSettings");
            }
            
            Debug.Log("🎉 Enhanced Community Feedback Manager fully initialized!");
        }
        
        #region Enhanced Strategy System
        
        private void InitializeEnhancedFeedbackSystem()
        {
            feedbackStrategies = new Dictionary<FeedbackType, IFeedbackStrategy>();
            strategyWeights = new Dictionary<FeedbackType, float>();
            strategyUsageCount = new Dictionary<FeedbackType, int>();
            strategyLastUsed = new Dictionary<FeedbackType, float>();
            
            try 
            {
                // Initialize all 7 enhanced strategies with error handling
                InitializeStrategy(FeedbackType.BalanceReaction, new BalanceReactionStrategy(), balanceReactionWeight);
                InitializeStrategy(FeedbackType.ProPlayerOpinion, new ProPlayerStrategy(), proPlayerWeight);
                InitializeStrategy(FeedbackType.ContentCreator, new ContentCreatorStrategy(), contentCreatorWeight);
                InitializeStrategy(FeedbackType.CasualPlayerFeedback, new CasualPlayerStrategy(), casualPlayerWeight);
                InitializeStrategy(FeedbackType.PopularityShift, new PopularityShiftStrategy(), popularityShiftWeight);
                InitializeStrategy(FeedbackType.MetaAnalysis, new MetaAnalysisStrategy(), metaAnalysisWeight);
                InitializeStrategy(FeedbackType.CompetitiveScene, new CompetitiveStrategy(), competitiveWeight);
                
                Debug.Log($"🎯 Successfully initialized {feedbackStrategies.Count} enhanced feedback strategies!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Critical error initializing enhanced feedback strategies: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
        
        private void InitializeStrategy(FeedbackType type, IFeedbackStrategy strategy, float weight)
        {
            feedbackStrategies.Add(type, strategy);
            strategyWeights.Add(type, weight);
            strategyUsageCount.Add(type, 0);
            strategyLastUsed.Add(type, 0f);
            
            Debug.Log($"✅ {type} strategy initialized with weight {weight:F1}");
        }
        
        #endregion
        
        #region Enhanced Event Subscription
        
        private void SubscribeToEnhancedEvents()
        {
            // Core system events
            if (Core.ImplementationManager.Instance != null)
            {
                Core.ImplementationManager.Instance.OnImplementationCompleted.AddListener(GenerateEnhancedFeedbackForImplementedChanges);
                Debug.Log("📡 Subscribed to ImplementationManager events");
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnEnhancedPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.AddListener(OnEnhancedWeekChanged);
                Debug.Log("📡 Subscribed to PhaseManager events");
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnEnhancedCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnEnhancedOverallBalanceChanged);
                Debug.Log("📡 Subscribed to CharacterManager events");
            }
        }
        
        #endregion
        
        #region Enhanced Event Handlers
        
        private void OnEnhancedPhaseChanged(Core.GamePhase newPhase)
        {
            Debug.Log($"🎭 Enhanced phase change: {newPhase}");
            
            if (newPhase == Core.GamePhase.Feedback)
            {
                StartEnhancedFeedbackPhase();
            }
            
            // Update contextual data
            UpdateSeasonalContext();
        }
        
        private void StartEnhancedFeedbackPhase()
        {
            Debug.Log("🎭 Starting Enhanced Feedback Phase - Generating authentic community reactions...");
            
            // Delay feedback generation for realistic timing
            float delay = Random.Range(settings.feedbackDelayMin, settings.feedbackDelayMax);
            Invoke(nameof(GenerateEnhancedFeedbackForImplementedChanges), delay);
        }
        
        private void OnEnhancedWeekChanged(int newWeek)
        {
            currentGameWeek = newWeek;
            UpdateSeasonalContext();
            
            // Generate some organic weekly feedback
            if (Random.Range(0f, 1f) < 0.3f) // 30% chance
            {
                GenerateOrganicWeeklyFeedback();
            }
            
            Debug.Log($"📅 Week {newWeek} - Context updated");
        }
        
        private void OnEnhancedCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            var previousValue = GetPreviousStatValue(character, stat);
            var change = new BalanceChange(character, stat, previousValue, newValue);
            
            recentChanges.Add(change);
            
            // Keep only recent changes (last 30 seconds of game time)
            recentChanges.RemoveAll(c => Time.time - c.timestamp > 30f);
            
            Debug.Log($"📊 Enhanced stat change tracked: {character} {stat} {previousValue:F1} → {newValue:F1}");
            
            // Immediate feedback for significant changes
            if (change.magnitude > 15f)
            {
                GenerateImmediateFeedback(change);
            }
        }
        
        private void OnEnhancedOverallBalanceChanged(float newBalance)
        {
            float previousSentiment = currentCommunitySentiment;
            float targetSentiment = CalculateEnhancedTargetSentiment(newBalance);
            
            // Smooth sentiment transition
            currentCommunitySentiment = Mathf.Lerp(currentCommunitySentiment, targetSentiment, 0.25f);
            sentimentTrend = currentCommunitySentiment - previousSentiment;
            
            // Update meta stability
            metaStabilityScore = newBalance;
            
            OnCommunitySentimentChanged.Invoke(currentCommunitySentiment);
            
            Debug.Log($"💭 Enhanced sentiment update: {previousSentiment:F1}% → {currentCommunitySentiment:F1}% (trend: {sentimentTrend:+0.1;-0.1})");
        }
        
        #endregion
        
        #region Enhanced Feedback Generation
        
        public void GenerateEnhancedFeedbackForImplementedChanges()
        {
            Debug.Log("📝 Generating enhanced community feedback with advanced strategy system...");
            
            if (recentChanges.Count == 0)
            {
                Debug.Log("ℹ️ No recent changes to generate feedback for");
                GenerateOrganicBackgroundFeedback();
                return;
            }
            
            var newFeedback = new List<CommunityFeedback>();
            lastFeedbackGenerationTime = Time.time;
            
            // ENHANCED: Smart strategy selection with priority system
            var applicableStrategies = GetApplicableStrategiesWithPriority();
            
            Debug.Log($"🎯 Found {applicableStrategies.Count} applicable strategies for {recentChanges.Count} changes");
            
            // Generate feedback from prioritized strategies
            newFeedback.AddRange(GenerateStrategicFeedback(applicableStrategies));
            
            // Add enhanced organic feedback
            newFeedback.AddRange(GenerateEnhancedOrganicFeedback());
            
            // Process and finalize feedback
            ProcessAndFinalizeFeedback(newFeedback);
            
            Debug.Log($"🎉 Generated {newFeedback.Count} enhanced feedback items using {applicableStrategies.Count} strategies");
        }
        
        private List<(IFeedbackStrategy strategy, float priority, FeedbackType type)> GetApplicableStrategiesWithPriority()
        {
            var applicableStrategies = new List<(IFeedbackStrategy strategy, float priority, FeedbackType type)>();
            
            foreach (var kvp in feedbackStrategies)
            {
                var strategy = kvp.Value;
                var type = kvp.Key;
                
                if (strategy.ShouldApply(recentChanges, currentCommunitySentiment))
                {
                    float basePriority = strategy.GetPriority(recentChanges, currentCommunitySentiment);
                    float enhancedPriority = CalculateEnhancedPriority(type, basePriority);
                    
                    applicableStrategies.Add((strategy, enhancedPriority, type));
                    
                    Debug.Log($"  {type}: Base Priority {basePriority:F2} → Enhanced Priority {enhancedPriority:F2}");
                }
            }
            
            return applicableStrategies.OrderByDescending(s => s.priority).ToList();
        }
        
        private float CalculateEnhancedPriority(FeedbackType type, float basePriority)
        {
            float enhancedPriority = basePriority;
            
            // Apply strategy weights
            enhancedPriority *= strategyWeights.GetValueOrDefault(type, 1f);
            
            // Reduce priority for recently used strategies (prevent spam)
            float timeSinceLastUse = Time.time - strategyLastUsed.GetValueOrDefault(type, 0f);
            if (timeSinceLastUse < 10f) // Within last 10 seconds
            {
                enhancedPriority *= 0.5f; // 50% reduction
            }
            
            // Boost priority for underused strategies (encourage diversity)
            int usageCount = strategyUsageCount.GetValueOrDefault(type, 0);
            int averageUsage = totalFeedbackGenerated / feedbackStrategies.Count;
            if (usageCount < averageUsage * 0.7f) // 30% below average
            {
                enhancedPriority *= 1.3f; // 30% boost
            }
            
            // Context-based adjustments
            enhancedPriority *= GetContextualPriorityMultiplier(type);
            
            return enhancedPriority;
        }
        
        private float GetContextualPriorityMultiplier(FeedbackType type)
        {
            return type switch
            {
                FeedbackType.ProPlayerOpinion => isTournamentSeason ? 1.5f : 1f,
                FeedbackType.CompetitiveScene => isRankedSeason ? 1.3f : 1f,
                FeedbackType.ContentCreator => (metaStabilityScore < 50f) ? 1.4f : 1f, // More content during chaos
                FeedbackType.CasualPlayerFeedback => (currentCommunitySentiment < 40f) ? 1.2f : 1f, // More vocal when unhappy
                FeedbackType.PopularityShift => 1f, // Always context-independent
                FeedbackType.MetaAnalysis => (recentChanges.Count >= 3) ? 1.3f : 1f, // More analysis during big changes
                FeedbackType.BalanceReaction => 1f, // Always high priority
                _ => 1f
            };
        }
        
        private List<CommunityFeedback> GenerateStrategicFeedback(List<(IFeedbackStrategy strategy, float priority, FeedbackType type)> applicableStrategies)
        {
            var strategicFeedback = new List<CommunityFeedback>();
            int maxStrategicFeedback = settings?.feedbackPerImplementation ?? 6;
            int strategicFeedbackCount = 0;
            
            foreach (var (strategy, priority, type) in applicableStrategies)
            {
                if (strategicFeedbackCount >= maxStrategicFeedback) break;
                
                // Probability based on enhanced priority
                float activationChance = Mathf.Clamp01(priority);
                if (Random.Range(0f, 1f) < activationChance)
                {
                    try
                    {
                        var feedback = strategy.GenerateFeedback(recentChanges, currentCommunitySentiment, communitySegments);
                        if (feedback != null)
                        {
                            // Enhanced feedback processing
                            EnhanceFeedbackData(feedback, type);
                            strategicFeedback.Add(feedback);
                            strategicFeedbackCount++;
                            
                            // Update strategy tracking
                            strategyUsageCount[type]++;
                            strategyLastUsed[type] = Time.time;
                            totalFeedbackGenerated++;
                            
                            OnStrategyActivated.Invoke($"{type} strategy activated");
                            
                            Debug.Log($"✅ Generated {type} feedback: \"{feedback.author}\" - {feedback.content.Substring(0, Mathf.Min(50, feedback.content.Length))}...");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"❌ Error generating {type} feedback: {e.Message}");
                    }
                }
            }
            
            return strategicFeedback;
        }
        
        private void EnhanceFeedbackData(CommunityFeedback feedback, FeedbackType type)
        {
            // Calculate impact score
            feedback.impactScore = CalculateFeedbackImpact(feedback);
            
            // Check viral potential
            feedback.isViralCandidate = IsViralCandidate(feedback);
            
            // Add contextual tags based on current game state
            AddContextualMetadata(feedback, type);
            
            // Track viral feedback
            if (feedback.isViralCandidate)
            {
                viralFeedback.Add(feedback);
                OnViralFeedbackGenerated.Invoke(new FeedbackEventData(feedback, "High engagement potential"));
            }
        }
        
        private float CalculateFeedbackImpact(CommunityFeedback feedback)
        {
            float baseImpact = Mathf.Abs(feedback.sentiment);
            float engagementMultiplier = (feedback.upvotes + feedback.replies * 2) / 100f;
            float segmentMultiplier = GetSegmentImpactMultiplier(feedback.communitySegment);
            
            return (baseImpact * segmentMultiplier) + engagementMultiplier;
        }
        
        private bool IsViralCandidate(CommunityFeedback feedback)
        {
            return feedback.upvotes > 60 || 
                   feedback.replies > 25 || 
                   Mathf.Abs(feedback.sentiment) > 0.8f ||
                   feedback.communitySegment == "Pro Players" ||
                   feedback.communitySegment == "Content Creators";
        }
        
        private float GetSegmentImpactMultiplier(string segment)
        {
            var segmentData = communitySegments.FirstOrDefault(s => s.segmentName == segment);
            return segmentData?.influence ?? 1f;
        }
        
        private void AddContextualMetadata(CommunityFeedback feedback, FeedbackType type)
        {
            // Add timing context
            if (isTournamentSeason && type == FeedbackType.ProPlayerOpinion)
            {
                feedback.content += " (Tournament Season)";
            }
            
            if (isRankedSeason && type == FeedbackType.CompetitiveScene)
            {
                feedback.content += " (Ranked Season)";
            }
            
            // Add meta context for analysis
            if (type == FeedbackType.MetaAnalysis && metaStabilityScore < 40f)
            {
                feedback.content += " (Meta Instability Period)";
            }
        }
        
        #endregion
        
        #region Enhanced Organic Feedback
        
        private List<CommunityFeedback> GenerateEnhancedOrganicFeedback()
        {
            var organicFeedback = new List<CommunityFeedback>();
            
            float organicChance = settings?.organicFeedbackChance ?? 0.4f;
            if (Random.Range(0f, 1f) > organicChance) return organicFeedback;
            
            int organicCount = Random.Range(1, 4);
            
            for (int i = 0; i < organicCount; i++)
            {
                var feedback = GenerateSingleOrganicFeedback();
                if (feedback != null)
                {
                    organicFeedback.Add(feedback);
                }
            }
            
            Debug.Log($"🌱 Generated {organicFeedback.Count} organic feedback items");
            return organicFeedback;
        }
        
        private CommunityFeedback GenerateSingleOrganicFeedback()
        {
            var template = GetRandomOrganicTemplate();
            var segment = GetRandomWeightedSegment();
            
            var feedback = new CommunityFeedback
            {
                author = GenerateContextualAuthorName(segment),
                content = ProcessOrganicTemplate(template.content),
                sentiment = CalculateOrganicSentiment(template.sentimentBias),
                feedbackType = template.feedbackType,
                communitySegment = segment.segmentName,
                timestamp = System.DateTime.Now,
                isOrganic = true,
                upvotes = Random.Range(1, 20),
                replies = Random.Range(0, 8)
            };
            
            return feedback;
        }
        
        private void GenerateOrganicBackgroundFeedback()
        {
            if (Random.Range(0f, 1f) < 0.2f) // 20% chance when no changes
            {
                var backgroundFeedback = GenerateEnhancedOrganicFeedback();
                ProcessAndFinalizeFeedback(backgroundFeedback);
                
                Debug.Log($"🌟 Generated {backgroundFeedback.Count} background organic feedback");
            }
        }
        
        private void GenerateOrganicWeeklyFeedback()
        {
            var weeklyFeedback = new List<CommunityFeedback>();
            
            // Generate meta state commentary
            var metaCommentary = GenerateMetaStateCommentary();
            if (metaCommentary != null) weeklyFeedback.Add(metaCommentary);
            
            // Generate seasonal commentary
            var seasonalCommentary = GenerateSeasonalCommentary();
            if (seasonalCommentary != null) weeklyFeedback.Add(seasonalCommentary);
            
            ProcessAndFinalizeFeedback(weeklyFeedback);
        }
        
        #endregion
        
        #region Enhanced Processing and Utilities
        
        private void ProcessAndFinalizeFeedback(List<CommunityFeedback> newFeedback)
        {
            if (newFeedback.Count == 0) return;
            
            // Add to active feedback queue
            foreach (var feedback in newFeedback)
            {
                activeFeedback.Enqueue(feedback);
            }
            
            // Maintain queue size
            while (activeFeedback.Count > (settings?.maxActiveFeedback ?? 15))
            {
                activeFeedback.Dequeue();
            }
            
            // Update community sentiment
            UpdateEnhancedCommunitySentiment(newFeedback);
            
            // Notify UI systems
            OnFeedbackGenerated.Invoke(newFeedback);
            
            foreach (var feedback in newFeedback)
            {
                OnNewFeedbackAdded.Invoke(feedback);
            }
            
            // Clean up old changes
            CleanupOldChanges();
        }
        
        private void UpdateEnhancedCommunitySentiment(List<CommunityFeedback> feedback)
        {
            if (feedback.Count == 0) return;
            
            float weightedSentimentSum = 0f;
            float totalWeight = 0f;
            
            foreach (var fb in feedback)
            {
                float segmentWeight = GetSegmentImpactMultiplier(fb.communitySegment);
                float engagementWeight = (fb.upvotes + fb.replies) / 100f + 1f;
                float weight = segmentWeight * engagementWeight;
                
                weightedSentimentSum += (fb.sentiment + 1f) * 50f * weight; // Convert to 0-100 scale
                totalWeight += weight;
            }
            
            if (totalWeight > 0f)
            {
                float averageSentiment = weightedSentimentSum / totalWeight;
                float sentimentInfluence = Mathf.Clamp(feedback.Count * 0.08f, 0.05f, 0.3f);
                
                float previousSentiment = currentCommunitySentiment;
                currentCommunitySentiment = Mathf.Lerp(currentCommunitySentiment, averageSentiment, sentimentInfluence);
                currentCommunitySentiment = Mathf.Clamp(currentCommunitySentiment, 10f, 90f);
                
                sentimentTrend = currentCommunitySentiment - previousSentiment;
                
                OnCommunitySentimentChanged.Invoke(currentCommunitySentiment);
                
                Debug.Log($"💭 Enhanced sentiment update: {previousSentiment:F1}% → {currentCommunitySentiment:F1}% (trend: {sentimentTrend:+0.1;-0.1})");
            }
        }
        
        private void CleanupOldChanges()
        {
            recentChanges.RemoveAll(c => Time.time - c.timestamp > 45f);
            
            // Also cleanup old viral feedback
            viralFeedback.RemoveAll(f => 
                (System.DateTime.Now - f.timestamp).TotalMinutes > 30);
        }
        
        #endregion
        
        #region Context and Utility Methods
        
        private void InitializeEnhancedCommunitySegments()
        {
            if (communitySegments == null || communitySegments.Count == 0)
            {
                communitySegments = new List<CommunitySegmentData>
                {
                    new CommunitySegmentData("Pro Players", 0.9f, 0.4f, 0.1f),        // Very high influence, low activity, slightly positive
                    new CommunitySegmentData("Content Creators", 0.8f, 1.3f, 0.2f),  // High influence, high activity, positive
                    new CommunitySegmentData("Competitive", 0.7f, 1.6f, -0.1f),      // Good influence, very active, slightly negative
                    new CommunitySegmentData("Casual Players", 0.5f, 1.0f, 0.3f)     // Moderate influence, normal activity, positive
                };
                
                Debug.Log($"✅ Initialized {communitySegments.Count} enhanced community segments");
            }
        }
        
        private void InitializeContextualData()
        {
            UpdateSeasonalContext();
        }
        
        private void UpdateSeasonalContext()
        {
            // Simulate competitive seasons based on game week
            int seasonCycle = currentGameWeek % 12; // 12-week cycles
            
            isRankedSeason = seasonCycle >= 2 && seasonCycle <= 9; // Weeks 2-9 are ranked season
            isTournamentSeason = seasonCycle == 10 || seasonCycle == 11; // Weeks 10-11 are tournament season
            
            Debug.Log($"🏆 Context Update - Week {currentGameWeek}: Ranked={isRankedSeason}, Tournament={isTournamentSeason}");
        }
        
        private float GetPreviousStatValue(Characters.CharacterType character, Characters.CharacterStat stat)
        {
            var previousChange = recentChanges
                .Where(c => c.character == character && c.stat == stat)
                .OrderByDescending(c => c.timestamp)
                .FirstOrDefault();
            
            return previousChange?.previousValue ?? 50f;
        }
        
        private float CalculateEnhancedTargetSentiment(float overallBalance)
        {
            float baseSentiment = overallBalance switch
            {
                >= 85f => Random.Range(75f, 85f),   // High satisfaction
                >= 70f => Random.Range(60f, 75f),   // Good satisfaction
                >= 50f => Random.Range(45f, 65f),   // Mixed satisfaction
                >= 30f => Random.Range(25f, 45f),   // Low satisfaction
                _ => Random.Range(15f, 30f)         // Very low satisfaction
            };
            
            // Apply contextual modifiers
            if (isTournamentSeason) baseSentiment -= 5f; // Tension before tournaments
            if (isRankedSeason && overallBalance < 60f) baseSentiment -= 8f; // Frustration during ranked with poor balance
            
            return Mathf.Clamp(baseSentiment, 15f, 85f);
        }
        
        private void GenerateImmediateFeedback(BalanceChange change)
        {
            // Generate immediate reaction for very significant changes
            var immediateStrategies = feedbackStrategies.Values
                .Where(s => s.ShouldApply(new List<BalanceChange> { change }, currentCommunitySentiment))
                .Take(2); // Limit to 2 immediate reactions
            
            var immediateFeedback = new List<CommunityFeedback>();
            
            foreach (var strategy in immediateStrategies)
            {
                if (Random.Range(0f, 1f) < 0.6f) // 60% chance for immediate reaction
                {
                    var feedback = strategy.GenerateFeedback(new List<BalanceChange> { change }, currentCommunitySentiment, communitySegments);
                    if (feedback != null)
                    {
                        feedback.content = "🚨 IMMEDIATE: " + feedback.content; // Mark as immediate
                        immediateFeedback.Add(feedback);
                    }
                }
            }
            
            if (immediateFeedback.Count > 0)
            {
                ProcessAndFinalizeFeedback(immediateFeedback);
                Debug.Log($"⚡ Generated {immediateFeedback.Count} immediate reactions to major {change.character} {change.stat} change");
            }
        }
        
        #endregion
        
        #region Template and Generation Utilities
        
        private FeedbackTemplate GetRandomOrganicTemplate()
        {
            if (legacyFeedbackTemplates != null && legacyFeedbackTemplates.Count > 0)
            {
                return legacyFeedbackTemplates[Random.Range(0, legacyFeedbackTemplates.Count)];
            }
            
            // Enhanced default templates with UTF-8 symbols
            var defaultTemplates = new[]
            {
                new FeedbackTemplate("Great balance changes! The game feels more balanced now ✓", FeedbackType.BalanceReaction, 0.6f),
                new FeedbackTemplate("Not sure about these changes... ● Need more time to judge", FeedbackType.BalanceReaction, 0f),
                new FeedbackTemplate("This completely ruins my favorite character ↓", FeedbackType.BalanceReaction, -0.7f),
                new FeedbackTemplate("Meta is evolving in interesting ways ►", FeedbackType.MetaAnalysis, 0.3f),
                new FeedbackTemplate("Loving the current state of the game ♥", FeedbackType.CasualPlayerFeedback, 0.8f),
                new FeedbackTemplate("Pick rates are shifting dramatically ▲", FeedbackType.PopularityShift, 0.2f)
            };
            
            return defaultTemplates[Random.Range(0, defaultTemplates.Length)];
        }
        
        private CommunitySegmentData GetRandomWeightedSegment()
        {
            if (communitySegments.Count == 0) return new CommunitySegmentData("General", 0.5f);
            
            float totalWeight = communitySegments.Sum(s => s.influence * s.activityLevel);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var segment in communitySegments)
            {
                currentWeight += segment.influence * segment.activityLevel;
                if (randomValue <= currentWeight)
                {
                    return segment;
                }
            }
            
            return communitySegments[0];
        }
        
        private string GenerateContextualAuthorName(CommunitySegmentData segment)
        {
            return segment.segmentName switch
            {
                "Pro Players" => GetProPlayerName(),
                "Content Creators" => GetContentCreatorName(),
                "Competitive" => GetCompetitiveName(),
                "Casual Players" => GetCasualName(),
                _ => GetGenericName()
            };
        }
        
        private string ProcessOrganicTemplate(string template)
        {
            // Replace any template variables with contextual information
            return template
                .Replace("{WEEK}", currentGameWeek.ToString())
                .Replace("{SEASON}", GetCurrentSeasonName())
                .Replace("{SENTIMENT}", GetSentimentDescription())
                .Replace("{META_STATE}", GetMetaStateDescription());
        }
        
        private float CalculateOrganicSentiment(float templateBias)
        {
            float baseSentiment = (currentCommunitySentiment - 50f) / 50f;
            float organicSentiment = baseSentiment + templateBias;
            organicSentiment += Random.Range(-0.3f, 0.3f); // Random variance
            
            return Mathf.Clamp(organicSentiment, -1f, 1f);
        }
        
        private CommunityFeedback GenerateMetaStateCommentary()
        {
            if (Random.Range(0f, 1f) > 0.4f) return null; // 40% chance
            
            string content = metaStabilityScore switch
            {
                > 80f => "Meta feels really stable right now ⚖ Good balance across the board",
                > 60f => "Current meta is in a decent place ● Some minor issues but overall good",
                > 40f => "Meta is a bit chaotic lately ► Lots of changes happening",
                > 20f => "Meta is pretty unstable right now ↓ Hope things improve soon",
                _ => "Meta is completely broken ✗ Need major fixes ASAP"
            };
            
            return new CommunityFeedback
            {
                author = "MetaObserver_Weekly",
                content = content,
                sentiment = (metaStabilityScore - 50f) / 50f,
                feedbackType = FeedbackType.MetaAnalysis,
                communitySegment = "Competitive",
                timestamp = System.DateTime.Now,
                isOrganic = true,
                upvotes = Random.Range(5, 25),
                replies = Random.Range(2, 12)
            };
        }
        
        private CommunityFeedback GenerateSeasonalCommentary()
        {
            if (!isRankedSeason && !isTournamentSeason) return null;
            
            string content = (isRankedSeason, isTournamentSeason) switch
            {
                (true, false) => "Ranked season is heating up! ▲ Time to climb the ladder",
                (false, true) => "Tournament season excitement! ★ Who's ready for the championships?",
                (true, true) => "Both ranked and tournament seasons! ♦ Intense competition ahead",
                _ => null
            };
            
            if (content == null) return null;
            
            return new CommunityFeedback
            {
                author = GetSeasonalCommentatorName(),
                content = content,
                sentiment = Random.Range(0.3f, 0.8f), // Generally positive about seasons
                feedbackType = FeedbackType.CompetitiveScene,
                communitySegment = isRankedSeason ? "Competitive" : "Pro Players",
                timestamp = System.DateTime.Now,
                isOrganic = true,
                upvotes = Random.Range(10, 40),
                replies = Random.Range(5, 20)
            };
        }
        
        #endregion
        
        #region Name Generation Utilities
        
        private string GetProPlayerName()
        {
            var teams = new[] { "TSM", "FaZe", "C9", "TL", "G2", "Fnatic", "NRG", "100T" };
            var names = new[] { "Legend", "Ace", "Champion", "Elite", "Master", "Pro", "King", "Alpha" };
            return $"{teams[Random.Range(0, teams.Length)]}_{names[Random.Range(0, names.Length)]}";
        }
        
        private string GetContentCreatorName()
        {
            var names = new[] { "StreamMaster", "YouTubeGuru", "TwitchKing", "ContentQueen", "GameGuruYT", 
                               "MetaMasterTV", "BalanceWatchYT", "ProGuideGamer", "TierListLord", "PatchNotesTV" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetCompetitiveName()
        {
            var names = new[] { "RankedClimber", "EsportsHopeful", "TryHardPlayer", "CompetitiveMind", "LadderWarrior",
                               "RankedGrinder", "ClimbingHard", "CompetitiveEdge", "SkillExpression", "RankedAce" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetCasualName()
        {
            var names = new[] { "CasualGamer42", "FunPlayer", "WeekendWarrior", "ChillGamer", "JustForFun",
                               "RelaxedGamer", "FamilyPlayer", "EveningGamer", "SocialGamer", "FunSeeker" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetGenericName()
        {
            var names = new[] { "GameFan", "PlayerOne", "Community_Voice", "BalanceWatcher", "MetaObserver",
                               "GameLover", "BalanceFan", "MetaFollower", "CommunityMember", "GameTracker" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetSeasonalCommentatorName()
        {
            return isTournamentSeason ? 
                   $"Tournament_Hype_{Random.Range(1, 100)}" : 
                   $"RankedSeason_Fan_{Random.Range(1, 100)}";
        }
        
        private string GetCurrentSeasonName()
        {
            return (isRankedSeason, isTournamentSeason) switch
            {
                (true, false) => "Ranked Season",
                (false, true) => "Tournament Season", 
                (true, true) => "Championship Period",
                _ => "Off Season"
            };
        }
        
        private string GetSentimentDescription()
        {
            return currentCommunitySentiment switch
            {
                > 75f => "very positive",
                > 60f => "positive",
                > 40f => "mixed",
                > 25f => "negative", 
                _ => "very negative"
            };
        }
        
        private string GetMetaStateDescription()
        {
            return metaStabilityScore switch
            {
                > 80f => "very stable",
                > 60f => "stable",
                > 40f => "transitional",
                > 20f => "unstable",
                _ => "chaotic"
            };
        }
        
        #endregion
        
        #region Public API and Debug Methods
        
        // Enhanced public getters
        public List<CommunityFeedback> GetActiveFeedback() => activeFeedback.ToList();
        public List<CommunityFeedback> GetViralFeedback() => viralFeedback.ToList();
        public float GetCommunitySentiment() => currentCommunitySentiment;
        public float GetSentimentTrend() => sentimentTrend;
        public List<BalanceChange> GetRecentChanges() => new List<BalanceChange>(recentChanges);
        public Dictionary<FeedbackType, int> GetStrategyUsageStats() => new Dictionary<FeedbackType, int>(strategyUsageCount);
        public bool IsRankedSeason() => isRankedSeason;
        public bool IsTournamentSeason() => isTournamentSeason;
        public float GetMetaStabilityScore() => metaStabilityScore;
        
        // Enhanced debug methods
        [ContextMenu("🧪 Test All Enhanced Strategies")]
        public void DebugTestAllEnhancedStrategies()
        {
            Debug.Log("=== 🧪 TESTING ALL ENHANCED STRATEGIES ===");
            
            // Create comprehensive test balance changes
            var testChanges = new List<BalanceChange>
            {
                new BalanceChange(Characters.CharacterType.Warrior, Characters.CharacterStat.Health, 50f, 70f),
                new BalanceChange(Characters.CharacterType.Mage, Characters.CharacterStat.Damage, 60f, 40f),
                new BalanceChange(Characters.CharacterType.Support, Characters.CharacterStat.Popularity, 35f, 65f),
                new BalanceChange(Characters.CharacterType.Tank, Characters.CharacterStat.WinRate, 48f, 52f)
            };
            
            recentChanges.AddRange(testChanges);
            
            foreach (var kvp in feedbackStrategies)
            {
                var strategy = kvp.Value;
                var type = kvp.Key;
                
                try
                {
                    bool shouldApply = strategy.ShouldApply(testChanges, currentCommunitySentiment);
                    float basePriority = strategy.GetPriority(testChanges, currentCommunitySentiment);
                    float enhancedPriority = CalculateEnhancedPriority(type, basePriority);
                    
                    Debug.Log($"Strategy: {type}");
                    Debug.Log($"  Should Apply: {shouldApply}");
                    Debug.Log($"  Base Priority: {basePriority:F2}");
                    Debug.Log($"  Enhanced Priority: {enhancedPriority:F2}");
                    Debug.Log($"  Usage Count: {strategyUsageCount.GetValueOrDefault(type, 0)}");
                    Debug.Log($"  Weight: {strategyWeights.GetValueOrDefault(type, 1f):F1}");
                    
                    if (shouldApply)
                    {
                        var feedback = strategy.GenerateFeedback(testChanges, currentCommunitySentiment, communitySegments);
                        if (feedback != null)
                        {
                            Debug.Log($"  ✅ Generated: {feedback.author}");
                            Debug.Log($"  Content: \"{feedback.content}\"");
                            Debug.Log($"  Sentiment: {feedback.sentiment:F2}");
                            Debug.Log($"  Engagement: {feedback.upvotes} upvotes, {feedback.replies} replies");
                            Debug.Log($"  Impact Score: {CalculateFeedbackImpact(feedback):F2}");
                            Debug.Log($"  Viral Candidate: {IsViralCandidate(feedback)}");
                        }
                        else
                        {
                            Debug.Log($"  ⚠️ Strategy returned null feedback");
                        }
                    }
                    
                    Debug.Log(""); // Empty line for readability
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Error testing {type}: {e.Message}");
                }
            }
            
            // Clean up test data
            recentChanges.RemoveAll(c => testChanges.Contains(c));
        }
        
        [ContextMenu("📊 Show Enhanced Statistics")]
        public void DebugShowEnhancedStatistics()
        {
            Debug.Log("=== 📊 ENHANCED COMMUNITY FEEDBACK STATISTICS ===");
            Debug.Log($"Total Strategies: {feedbackStrategies.Count}");
            Debug.Log($"Community Segments: {communitySegments.Count}");
            Debug.Log($"Active Feedback Items: {activeFeedback.Count}");
            Debug.Log($"Viral Feedback Items: {viralFeedback.Count}");
            Debug.Log($"Current Sentiment: {currentCommunitySentiment:F1}% (trend: {sentimentTrend:+0.1;-0.1})");
            Debug.Log($"Meta Stability: {metaStabilityScore:F1}%");
            Debug.Log($"Recent Changes: {recentChanges.Count}");
            Debug.Log($"Total Feedback Generated: {totalFeedbackGenerated}");
            Debug.Log($"Current Week: {currentGameWeek}");
            Debug.Log($"Season Context: Ranked={isRankedSeason}, Tournament={isTournamentSeason}");
            
            Debug.Log("\nStrategy Usage Statistics:");
            foreach (var kvp in strategyUsageCount.OrderByDescending(s => s.Value))
            {
                var type = kvp.Key;
                var count = kvp.Value;
                var weight = strategyWeights.GetValueOrDefault(type, 1f);
                var lastUsed = strategyLastUsed.GetValueOrDefault(type, 0f);
                var timeSinceLast = Time.time - lastUsed;
                
                Debug.Log($"  {type}: {count} uses, weight {weight:F1}, last used {timeSinceLast:F1}s ago");
            }
            
            Debug.Log("\nCommunity Segments:");
            foreach (var segment in communitySegments)
            {
                Debug.Log($"  {segment.segmentName}: Influence {segment.influence:F1}, Activity {segment.activityLevel:F1}, Bias {segment.baseSentimentBias:+0.1;-0.1}");
            }
        }
        
        [ContextMenu("🔄 Generate Enhanced Feedback Now")]
        public void DebugGenerateEnhancedFeedback()
        {
            Debug.Log("🔄 Manually triggering enhanced feedback generation...");
            GenerateEnhancedFeedbackForImplementedChanges();
        }
        
        [ContextMenu("🌟 Generate Viral Test Feedback")]
        public void DebugGenerateViralFeedback()
        {
            Debug.Log("🌟 Generating test viral feedback...");
            
            var viralTestFeedback = new CommunityFeedback
            {
                author = "TSM_Legend",
                content = "HUGE changes incoming! This patch will reshape the entire competitive scene ★",
                sentiment = 0.9f,
                feedbackType = FeedbackType.ProPlayerOpinion,
                communitySegment = "Pro Players",
                timestamp = System.DateTime.Now,
                upvotes = 150,
                replies = 45,
                isOrganic = false,
                isViralCandidate = true
            };
            
            viralTestFeedback.impactScore = CalculateFeedbackImpact(viralTestFeedback);
            viralFeedback.Add(viralTestFeedback);
            
            OnViralFeedbackGenerated.Invoke(new FeedbackEventData(viralTestFeedback, "Debug viral test"));
            OnNewFeedbackAdded.Invoke(viralTestFeedback);
            
            Debug.Log($"✅ Generated viral test feedback with {viralTestFeedback.upvotes} upvotes and impact score {viralTestFeedback.impactScore:F2}");
        }
        
        [ContextMenu("⚖️ Reset Enhanced System")]
        public void DebugResetEnhancedSystem()
        {
            Debug.Log("⚖️ Resetting enhanced community feedback system...");
            
            recentChanges.Clear();
            activeFeedback.Clear();
            viralFeedback.Clear();
            
            foreach (var type in strategyUsageCount.Keys.ToList())
            {
                strategyUsageCount[type] = 0;
                strategyLastUsed[type] = 0f;
            }
            
            totalFeedbackGenerated = 0;
            currentCommunitySentiment = 65f;
            sentimentTrend = 0f;
            metaStabilityScore = 75f;
            
            Debug.Log("✅ Enhanced system reset complete");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up subscriptions
            if (Core.ImplementationManager.Instance != null)
            {
                Core.ImplementationManager.Instance.OnImplementationCompleted.RemoveListener(GenerateEnhancedFeedbackForImplementedChanges);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.RemoveListener(OnEnhancedPhaseChanged);
                Core.PhaseManager.Instance.OnWeekChanged.RemoveListener(OnEnhancedWeekChanged);
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.RemoveListener(OnEnhancedCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.RemoveListener(OnEnhancedOverallBalanceChanged);
            }
            
            Debug.Log("🎭 Enhanced Community Feedback Manager destroyed and cleaned up");
        }
    }
}