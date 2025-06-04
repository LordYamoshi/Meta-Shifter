using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace MetaBalance.Community
{
    /// <summary>
    /// Enhanced Community Feedback Manager with sequential feedback display during feedback phase only
    /// </summary>
    public class CommunityFeedbackManager : MonoBehaviour
    {
        public static CommunityFeedbackManager Instance { get; private set; }
        
        [Header("Feedback Generation")]
        [SerializeField] private FeedbackGenerationSettings settings;
        [SerializeField] private List<FeedbackTemplate> feedbackTemplates;
        
        [Header("Sequential Display Settings")]
        [Range(0.5f, 5f)]
        [SerializeField] private float delayBetweenFeedback = 1.5f;
        [Range(1, 10)]
        [SerializeField] private int maxFeedbackPerPhase = 6;
        [Range(0f, 1f)]
        [SerializeField] private float feedbackShowChance = 0.8f; // Probability of showing feedback
        
        [Header("Community Segments")]
        [SerializeField] private List<CommunitySegmentData> communitySegments;
        
        [Header("Events")]
        public UnityEvent<List<CommunityFeedback>> OnFeedbackGenerated;
        public UnityEvent<CommunityFeedback> OnNewFeedbackAdded;
        public UnityEvent<float> OnCommunitySentimentChanged;
        public UnityEvent OnFeedbackSequenceStarted;
        public UnityEvent OnFeedbackSequenceCompleted;
        
        // Strategy Pattern: Different feedback generation strategies
        private Dictionary<FeedbackType, IFeedbackStrategy> feedbackStrategies;
        
        // Observer Pattern: Track changes that affect community sentiment
        private List<BalanceChange> recentChanges = new List<BalanceChange>();
        private Queue<CommunityFeedback> activeFeedback = new Queue<CommunityFeedback>();
        private Queue<CommunityFeedback> pendingFeedback = new Queue<CommunityFeedback>();
        private float currentCommunitySentiment = 65f;
        
        // Sequential display control
        private bool isDisplayingSequence = false;
        private bool isFeedbackPhase = false;
        private Coroutine currentSequenceCoroutine;
        
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
            
            // Add UTF-8 compatible strategies
            feedbackStrategies.Add(FeedbackType.BalanceReaction, new BalanceReactionStrategy());
            feedbackStrategies.Add(FeedbackType.PopularityShift, new PopularityShiftStrategy());
            feedbackStrategies.Add(FeedbackType.MetaAnalysis, new MetaAnalysisStrategy());
            feedbackStrategies.Add(FeedbackType.ProPlayerOpinion, new ProPlayerStrategy());
            feedbackStrategies.Add(FeedbackType.CasualPlayerFeedback, new CasualPlayerStrategy());
            feedbackStrategies.Add(FeedbackType.ContentCreator, new ContentCreatorStrategy());
            feedbackStrategies.Add(FeedbackType.CompetitiveScene, new CompetitiveStrategy());
            
            Debug.Log($"✅ Initialized {feedbackStrategies.Count} UTF-8 compatible feedback strategies");
        }
        
        private void SubscribeToEvents()
        {
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
            Debug.Log($"🎭 Phase changed to: {newPhase}");
            
            isFeedbackPhase = (newPhase == Core.GamePhase.Feedback);
            
            if (newPhase == Core.GamePhase.Feedback)
            {
                StartFeedbackPhase();
            }
            else
            {
                StopFeedbackSequence();
                // DON'T clear feedback - let it accumulate throughout the game
            }
        }
        
        private void StartFeedbackPhase()
        {
            Debug.Log("🎭 Starting Feedback Phase - Preparing community reactions...");
            
            // Generate all feedback first (but don't display yet)
            GenerateFeedbackForImplementedChanges();
            
            // Start sequential display with a small delay
            Invoke(nameof(StartSequentialFeedbackDisplay), 0.5f);
        }
        
        private void StartSequentialFeedbackDisplay()
        {
            if (pendingFeedback.Count == 0)
            {
                Debug.Log("📭 No feedback to display");
                return;
            }
            
            Debug.Log($"🎬 Starting sequential feedback display: {pendingFeedback.Count} items queued");
            
            OnFeedbackSequenceStarted.Invoke();
            
            // Start the sequential display coroutine
            if (currentSequenceCoroutine != null)
            {
                StopCoroutine(currentSequenceCoroutine);
            }
            
            currentSequenceCoroutine = StartCoroutine(DisplayFeedbackSequentially());
        }
        
        private IEnumerator DisplayFeedbackSequentially()
        {
            isDisplayingSequence = true;
            
            Debug.Log($"📺 Sequential display started - {pendingFeedback.Count} feedback items to show");
            
            int itemsShown = 0;
            
            while (pendingFeedback.Count > 0 && isFeedbackPhase && itemsShown < maxFeedbackPerPhase)
            {
                // Check if we should show this feedback (probability based)
                if (Random.value > feedbackShowChance)
                {
                    Debug.Log("🎲 Skipping feedback item due to probability");
                    pendingFeedback.Dequeue(); // Skip this one
                    continue;
                }
                
                var feedback = pendingFeedback.Dequeue();
                
                Debug.Log($"📝 Displaying feedback {itemsShown + 1}: {feedback.author} - '{feedback.content}'");
                
                // Add to active feedback and notify UI
                activeFeedback.Enqueue(feedback);
                OnNewFeedbackAdded.Invoke(feedback);
                
                itemsShown++;
                
                // Wait before showing next feedback
                if (pendingFeedback.Count > 0) // Don't wait after the last item
                {
                    Debug.Log($"⏱️ Waiting {delayBetweenFeedback}s before next feedback...");
                    yield return new WaitForSeconds(delayBetweenFeedback);
                    
                    // Check if we're still in feedback phase
                    if (!isFeedbackPhase)
                    {
                        Debug.Log("🛑 Phase changed - stopping feedback sequence");
                        break;
                    }
                }
            }
            
            Debug.Log($"✅ Sequential feedback display completed. Shown: {itemsShown} items");
            
            isDisplayingSequence = false;
            OnFeedbackSequenceCompleted.Invoke();
        }
        
        private void StopFeedbackSequence()
        {
            if (currentSequenceCoroutine != null)
            {
                StopCoroutine(currentSequenceCoroutine);
                currentSequenceCoroutine = null;
            }
            
            isDisplayingSequence = false;
            
            Debug.Log("🛑 Feedback sequence stopped");
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
                
                // Generate some organic feedback anyway
                var organicFeedback = GenerateOrganicFeedback();
                foreach (var feedback in organicFeedback)
                {
                    pendingFeedback.Enqueue(feedback);
                }
                
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
                    }
                }
            }
            
            // Add some random organic feedback
            newFeedback.AddRange(GenerateOrganicFeedback());
            
            // Shuffle and queue for sequential display
            newFeedback = newFeedback.OrderBy(f => Random.value).ToList();
            
            foreach (var feedback in newFeedback)
            {
                pendingFeedback.Enqueue(feedback);
            }
            
            // Limit pending feedback to prevent overflow
            while (pendingFeedback.Count > maxFeedbackPerPhase * 2)
            {
                pendingFeedback.Dequeue();
            }
            
            // Update community sentiment based on generated feedback
            UpdateCommunitySentimentFromFeedback(newFeedback);
            
            // Notify that feedback was generated (but not yet displayed)
            OnFeedbackGenerated.Invoke(newFeedback);
            
            Debug.Log($"✅ Generated and queued {newFeedback.Count} feedback items for sequential display");
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
            
            // Default UTF-8 compatible templates if none are assigned
            var defaultTemplates = new[]
            {
                new FeedbackTemplate("Great changes! The game feels more balanced now [GOOD]", FeedbackType.BalanceReaction, 0.5f),
                new FeedbackTemplate("Not sure about these changes... [?]", FeedbackType.BalanceReaction, 0f),
                new FeedbackTemplate("This completely ruins my favorite character [SAD]", FeedbackType.BalanceReaction, -0.7f),
                new FeedbackTemplate("The meta is shifting in interesting ways [TARGET]", FeedbackType.MetaAnalysis, 0.2f),
                new FeedbackTemplate("Finally some character diversity! [BALANCE]", FeedbackType.MetaAnalysis, 0.6f),
                new FeedbackTemplate("Everyone's playing the same characters [DOWN]", FeedbackType.PopularityShift, -0.4f),
                new FeedbackTemplate("Stream tonight: Testing the new changes live! [VIDEO]", FeedbackType.ContentCreator, 0.3f),
                new FeedbackTemplate("These changes will shake up tournaments [TROPHY]", FeedbackType.CompetitiveScene, 0.4f)
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
            
            // Replace UTF-8 symbol placeholders
            template = ReplaceUTF8Symbols(template);
            
            return template;
        }
        
        /// <summary>
        /// Replace symbol placeholders with UTF-8 compatible characters
        /// </summary>
        private string ReplaceUTF8Symbols(string text)
        {
            return text
                // Positive symbols
                .Replace("[GOOD]", "✓")
                .Replace("[YES]", "✓")
                .Replace("[FIX]", "✓")
                .Replace("[THANKS]", "★")
                .Replace("[STRONG]", "♦")
                .Replace("[TROPHY]", "◆")
                .Replace("[TARGET]", "●")
                .Replace("[UP]", "↑")
                .Replace("[HAPPY]", ":)")
                .Replace("[BALANCE]", "⚖")
                .Replace("[STAR]", "★")
                
                // Negative symbols
                .Replace("[BROKEN]", "✗")
                .Replace("[SAD]", ":(")
                .Replace("[BAD]", "✗")
                .Replace("[RIP]", "†")
                .Replace("[ANGRY]", "►")
                .Replace("[DOWN]", "↓")
                .Replace("[DEAD]", "✗")
                .Replace("[HARD]", "▲")
                .Replace("[X]", "✗")
                
                // Neutral symbols
                .Replace("[?]", "?")
                .Replace("[CONFUSED]", "?")
                .Replace("[CIRCLE]", "●")
                .Replace("[CLOCK]", "○")
                
                // Content creator symbols
                .Replace("[VIDEO]", "►")
                .Replace("[BELL]", "♪")
                .Replace("[LIST]", "■")
                .Replace("[SMART]", "※")
                
                // Generic symbols
                .Replace("[FIRE]", "▲")
                .Replace("[CHECK]", "✓")
                .Replace("[HEART]", "♥")
                .Replace("[DIAMOND]", "◆")
                .Replace("[SQUARE]", "■");
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
        
        public bool IsDisplayingSequence()
        {
            return isDisplayingSequence;
        }
        
        public int GetPendingFeedbackCount()
        {
            return pendingFeedback.Count;
        }
        
        // Manual controls for testing and debugging
        public void ForceStartSequentialDisplay()
        {
            if (pendingFeedback.Count > 0)
            {
                StartSequentialFeedbackDisplay();
            }
        }
        
        public void ForceStopSequentialDisplay()
        {
            StopFeedbackSequence();
        }
        
        // Debug methods
        [ContextMenu("🧪 Generate Test Feedback")]
        public void DebugGenerateTestFeedback()
        {
            GenerateFeedbackForImplementedChanges();
        }
        
        [ContextMenu("🎬 Force Start Sequential Display")]
        public void DebugForceStartSequence()
        {
            ForceStartSequentialDisplay();
        }
        
        [ContextMenu("🛑 Force Stop Sequential Display")]
        public void DebugForceStopSequence()
        {
            ForceStopSequentialDisplay();
        }
        
        [ContextMenu("📊 Show Community State")]
        public void DebugShowCommunityState()
        {
            Debug.Log("=== 📊 COMMUNITY STATE ===");
            Debug.Log($"Current Phase: {Core.PhaseManager.Instance?.GetCurrentPhase()}");
            Debug.Log($"Is Feedback Phase: {isFeedbackPhase}");
            Debug.Log($"Is Displaying Sequence: {isDisplayingSequence}");
            Debug.Log($"Current Sentiment: {currentCommunitySentiment:F1}%");
            Debug.Log($"Active Feedback Items: {activeFeedback.Count}");
            Debug.Log($"Pending Feedback Items: {pendingFeedback.Count}");
            Debug.Log($"Recent Changes: {recentChanges.Count}");
            Debug.Log($"Community Segments: {communitySegments.Count}");
            Debug.Log($"Delay Between Feedback: {delayBetweenFeedback}s");
            Debug.Log($"Max Feedback Per Phase: {maxFeedbackPerPhase}");
        }
        
        [ContextMenu("🎲 Add Test Feedback to Queue")]
        public void DebugAddTestFeedbackToQueue()
        {
            var testFeedback = new CommunityFeedback
            {
                author = "TestUser123",
                content = "This is a test feedback message for debugging 🧪",
                sentiment = Random.Range(-1f, 1f),
                feedbackType = FeedbackType.BalanceReaction,
                communitySegment = "Casual Players",
                timestamp = System.DateTime.Now,
                upvotes = Random.Range(1, 30),
                replies = Random.Range(0, 10)
            };
            
            pendingFeedback.Enqueue(testFeedback);
            Debug.Log($"✅ Added test feedback to queue. Queue size: {pendingFeedback.Count}");
        }
        
        [ContextMenu("🧹 Clear All Feedback (Debug Only)")]
        public void DebugClearAllQueues()
        {
            pendingFeedback.Clear();
            activeFeedback.Clear();
            Debug.Log("🧹 DEBUG: Cleared all feedback queues (this should not happen during normal gameplay)");
        }
    }
}