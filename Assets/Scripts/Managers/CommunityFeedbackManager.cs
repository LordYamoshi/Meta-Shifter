using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Community
{
    /// <summary>
    /// Main Community Feedback Manager using Strategy Pattern and Observer Pattern
    /// </summary>
    public class CommunityFeedbackManager : MonoBehaviour
    {
        public static CommunityFeedbackManager Instance { get; private set; }
        
        [Header("Feedback Generation")]
        [SerializeField] private FeedbackGenerationSettings settings;
        [SerializeField] private List<FeedbackTemplate> feedbackTemplates;
        
        [Header("Community Segments")]
        [SerializeField] private List<CommunitySegmentData> communitySegments;
        
        [Header("Events")]
        public UnityEvent<List<CommunityFeedback>> OnFeedbackGenerated;
        public UnityEvent<CommunityFeedback> OnNewFeedbackAdded;
        public UnityEvent<float> OnCommunitySentimentChanged;
        
        // Strategy Pattern: Different feedback generation strategies
        private Dictionary<FeedbackType, IFeedbackStrategy> feedbackStrategies;
        
        // Observer Pattern: Track changes that affect community sentiment
        private List<BalanceChange> recentChanges = new List<BalanceChange>();
        private Queue<CommunityFeedback> activeFeedback = new Queue<CommunityFeedback>();
        private float currentCommunitySentiment = 65f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeFeedbackStrategies();
        }
        
        private void Start()
        {
            SubscribeToEvents();
            InitializeCommunitySegments();
            
            // Create default settings if missing
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<FeedbackGenerationSettings>();
                settings.InitializeDefaults();
            }
        }
        
        private void InitializeFeedbackStrategies()
        {
            feedbackStrategies = new Dictionary<FeedbackType, IFeedbackStrategy>();
            
            // Add strategies one by one to avoid overload resolution issues
            feedbackStrategies.Add(FeedbackType.BalanceReaction, new BalanceReactionStrategy());
            feedbackStrategies.Add(FeedbackType.PopularityShift, new PopularityShiftStrategy());
            feedbackStrategies.Add(FeedbackType.MetaAnalysis, new MetaAnalysisStrategy());
            feedbackStrategies.Add(FeedbackType.ProPlayerOpinion, new ProPlayerStrategy());
            feedbackStrategies.Add(FeedbackType.CasualPlayerFeedback, new CasualPlayerStrategy());
            feedbackStrategies.Add(FeedbackType.ContentCreator, new ContentCreatorStrategy());
            feedbackStrategies.Add(FeedbackType.CompetitiveScene, new CompetitiveStrategy());
            
            Debug.Log($"✅ Initialized {feedbackStrategies.Count} feedback strategies");
        }
        
        private void SubscribeToEvents()
        {
            if (Core.ImplementationManager.Instance != null)
            {
                Core.ImplementationManager.Instance.OnImplementationCompleted.AddListener(GenerateFeedbackForImplementedChanges);
            }
            
            if (Core.PhaseManager.Instance != null)
            {
                Core.PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
            
            if (Characters.CharacterManager.Instance != null)
            {
                Characters.CharacterManager.Instance.OnStatChanged.AddListener(OnCharacterStatChanged);
                Characters.CharacterManager.Instance.OnOverallBalanceChanged.AddListener(OnOverallBalanceChanged);
            }
        }
        
        private void OnPhaseChanged(Core.GamePhase newPhase)
        {
            if (newPhase == Core.GamePhase.Feedback)
            {
                StartFeedbackPhase();
            }
        }
        
        private void StartFeedbackPhase()
        {
            Debug.Log("🎭 Starting Feedback Phase - Generating community reactions...");
            Invoke(nameof(GenerateFeedbackForImplementedChanges), 0.5f);
        }
        
        private void OnCharacterStatChanged(Characters.CharacterType character, Characters.CharacterStat stat, float newValue)
        {
            var change = new BalanceChange(character, stat, GetPreviousValue(character, stat), newValue);
            recentChanges.Add(change);
            
            // Keep only recent changes (last 10 seconds)
            recentChanges.RemoveAll(c => Time.time - c.timestamp > 10f);
        }
        
        private float GetPreviousValue(Characters.CharacterType character, Characters.CharacterStat stat)
        {
            // Try to find the previous value from recent changes
            var previousChange = recentChanges
                .Where(c => c.character == character && c.stat == stat)
                .OrderByDescending(c => c.timestamp)
                .FirstOrDefault();
            
            return previousChange?.newValue ?? 50f; // Default to 50 if no previous value
        }
        
        private void OnOverallBalanceChanged(float newBalance)
        {
            float targetSentiment = CalculateTargetSentiment(newBalance);
            currentCommunitySentiment = Mathf.Lerp(currentCommunitySentiment, targetSentiment, 0.3f);
            
            OnCommunitySentimentChanged.Invoke(currentCommunitySentiment);
        }
        
        public void GenerateFeedbackForImplementedChanges()
        {
            Debug.Log("📝 Generating community feedback for recent changes...");
            
            if (recentChanges.Count == 0)
            {
                Debug.Log("No recent changes to generate feedback for");
                return;
            }
            
            var newFeedback = new List<CommunityFeedback>();
            
            // Use different strategies for different types of feedback
            foreach (var strategy in feedbackStrategies.Values)
            {
                if (strategy.ShouldApply(recentChanges, currentCommunitySentiment))
                {
                    var feedback = strategy.GenerateFeedback(recentChanges, currentCommunitySentiment, communitySegments);
                    if (feedback != null)
                    {
                        newFeedback.Add(feedback);
                        activeFeedback.Enqueue(feedback);
                    }
                }
            }
            
            // Add some random organic feedback
            newFeedback.AddRange(GenerateOrganicFeedback());
            
            // Limit active feedback to prevent UI overflow
            while (activeFeedback.Count > settings.maxActiveFeedback)
            {
                activeFeedback.Dequeue();
            }
            
            // Update community sentiment based on generated feedback
            UpdateCommunitySentimentFromFeedback(newFeedback);
            
            // Notify UI systems
            OnFeedbackGenerated.Invoke(newFeedback);
            
            foreach (var feedback in newFeedback)
            {
                OnNewFeedbackAdded.Invoke(feedback);
            }
            
            Debug.Log($"✅ Generated {newFeedback.Count} community feedback items");
        }
        
        private List<CommunityFeedback> GenerateOrganicFeedback()
        {
            var organicFeedback = new List<CommunityFeedback>();
            
            int count = Random.Range(1, 4);
            
            for (int i = 0; i < count; i++)
            {
                var template = GetRandomTemplate();
                var segment = GetRandomSegment();
                
                var feedback = new CommunityFeedback
                {
                    author = GenerateAuthorName(segment),
                    content = ProcessTemplate(template.content, null),
                    sentiment = Random.Range(-1f, 1f),
                    feedbackType = template.feedbackType,
                    communitySegment = segment.segmentName,
                    timestamp = System.DateTime.Now,
                    isOrganic = true,
                    upvotes = Random.Range(1, 30),
                    replies = Random.Range(0, 10)
                };
                
                organicFeedback.Add(feedback);
            }
            
            return organicFeedback;
        }
        
        private FeedbackTemplate GetRandomTemplate()
        {
            if (feedbackTemplates != null && feedbackTemplates.Count > 0)
            {
                return feedbackTemplates[Random.Range(0, feedbackTemplates.Count)];
            }
            
            // Default templates if none are assigned
            var defaultTemplates = new[]
            {
                new FeedbackTemplate("Great changes! The game feels more balanced now 👍", FeedbackType.BalanceReaction, 0.5f),
                new FeedbackTemplate("Not sure about these changes... 🤔", FeedbackType.BalanceReaction, 0f),
                new FeedbackTemplate("This completely ruins my favorite character 😢", FeedbackType.BalanceReaction, -0.7f)
            };
            
            return defaultTemplates[Random.Range(0, defaultTemplates.Length)];
        }
        
        private CommunitySegmentData GetRandomSegment()
        {
            if (communitySegments != null && communitySegments.Count > 0)
            {
                return communitySegments[Random.Range(0, communitySegments.Count)];
            }
            
            return new CommunitySegmentData("General", 0.5f);
        }
        
        private float CalculateTargetSentiment(float overallBalance)
        {
            return overallBalance switch
            {
                >= 80f => Random.Range(70f, 90f),
                >= 60f => Random.Range(50f, 75f),
                >= 40f => Random.Range(30f, 55f),
                >= 20f => Random.Range(15f, 40f),
                _ => Random.Range(10f, 25f)
            };
        }
        
        private void UpdateCommunitySentimentFromFeedback(List<CommunityFeedback> feedback)
        {
            if (feedback.Count == 0) return;
            
            float averageSentiment = feedback.Average(f => f.sentiment);
            float sentimentWeight = feedback.Count * 0.1f;
            
            currentCommunitySentiment = Mathf.Lerp(
                currentCommunitySentiment, 
                (averageSentiment + 1f) * 50f,
                sentimentWeight
            );
            
            currentCommunitySentiment = Mathf.Clamp(currentCommunitySentiment, 0f, 100f);
            OnCommunitySentimentChanged.Invoke(currentCommunitySentiment);
        }
        
        private string GenerateAuthorName(CommunitySegmentData segment)
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
        
        private string GetProPlayerName()
        {
            var teams = new[] { "FaZe", "TSM", "TL", "C9", "G2" };
            var names = new[] { "ProGamer", "Ace", "Champion", "Legend", "Elite" };
            return $"{teams[Random.Range(0, teams.Length)]}_{names[Random.Range(0, names.Length)]}";
        }
        
        private string GetContentCreatorName()
        {
            var names = new[] { "StreamMaster", "YouTubeGuru", "TwitchKing", "ContentQueen", "GameGuruYT" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetCompetitiveName()
        {
            var names = new[] { "RankedClimber", "EsportsHopeful", "TryHardPlayer", "CompetitiveMind" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetCasualName()
        {
            var names = new[] { "CasualGamer42", "FunPlayer", "WeekendWarrior", "ChillGamer" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string GetGenericName()
        {
            var names = new[] { "GameFan", "PlayerOne", "Community_Voice", "BalanceWatcher" };
            return names[Random.Range(0, names.Length)];
        }
        
        private string ProcessTemplate(string template, BalanceChange change)
        {
            if (change != null)
            {
                template = template.Replace("{CHARACTER}", change.character.ToString());
                template = template.Replace("{STAT}", change.stat.ToString());
                template = template.Replace("{VALUE}", change.newValue.ToString("F1"));
            }
            
            template = template.Replace("{RANDOM_EMOJI}", GetRandomEmoji());
            
            return template;
        }
        
        private string GetRandomEmoji()
        {
            var emojis = new[] { "😊", "😢", "😠", "🎯", "⚖️", "🔥", "💯", "❌", "✅", "🤔" };
            return emojis[Random.Range(0, emojis.Length)];
        }
        
        private void InitializeCommunitySegments()
        {
            if (communitySegments == null || communitySegments.Count == 0)
            {
                communitySegments = new List<CommunitySegmentData>
                {
                    new CommunitySegmentData("Pro Players", 0.8f),
                    new CommunitySegmentData("Content Creators", 0.7f),
                    new CommunitySegmentData("Competitive", 0.6f),
                    new CommunitySegmentData("Casual Players", 0.4f)
                };
            }
        }
        
        // Public getters for UI systems
        public List<CommunityFeedback> GetActiveFeedback()
        {
            return activeFeedback.ToList();
        }
        
        public float GetCommunitySentiment()
        {
            return currentCommunitySentiment;
        }
        
        public List<BalanceChange> GetRecentChanges()
        {
            return new List<BalanceChange>(recentChanges);
        }
        
        // Debug methods
        [ContextMenu("Generate Test Feedback")]
        public void DebugGenerateTestFeedback()
        {
            GenerateFeedbackForImplementedChanges();
        }
        
        [ContextMenu("Show Community State")]
        public void DebugShowCommunityState()
        {
            Debug.Log("=== COMMUNITY STATE ===");
            Debug.Log($"Current Sentiment: {currentCommunitySentiment:F1}%");
            Debug.Log($"Active Feedback Items: {activeFeedback.Count}");
            Debug.Log($"Recent Changes: {recentChanges.Count}");
            Debug.Log($"Community Segments: {communitySegments.Count}");
        }
    }
}