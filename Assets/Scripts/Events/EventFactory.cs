using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Events
{
    /// <summary>
    /// Event Factory - Creates specific events for your game balance scenarios
    /// Completely rewritten to eliminate all CS0111 conflicts
    /// Uses Factory Pattern for clean event creation
    /// </summary>
    [CreateAssetMenu(fileName = "EventFactory", menuName = "MetaBalance/Event Factory")]
    public class EventFactory : ScriptableObject
    {
        [Header("Event Templates")]
        [SerializeField] private List<EventTemplate> crisisTemplates = new List<EventTemplate>();
        [SerializeField] private List<EventTemplate> opportunityTemplates = new List<EventTemplate>();
        [SerializeField] private List<EventTemplate> communityTemplates = new List<EventTemplate>();
        [SerializeField] private List<EventTemplate> technicalTemplates = new List<EventTemplate>();
        [SerializeField] private List<EventTemplate> competitiveTemplates = new List<EventTemplate>();

        #region Core Event Creators

        /// <summary>
        /// Create the game-breaking exploit crisis event
        /// </summary>
        public static EventData CreateGameBreakingExploitEvent()
        {
            var eventData = new EventData(
                "Game-Breaking Exploit Discovered",
                "Players have found a way to stack Support abilities that makes them nearly invincible. The community is in uproar and demanding immediate action. Pro players are threatening to boycott upcoming tournaments.",
                EventType.Crisis,
                EventSeverity.Critical
            );

            eventData.timeRemaining = 60f; // 2 turns remaining
            eventData.expectedImpact = 9.5f;
            eventData.expectedImpacts = new List<string>
            {
                "Support win rate: 58.8% → 70%+",
                "Community satisfaction: -15%",
                "Tournament participation: -25%"
            };

            // Add response options that match your UI design
            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Emergency Hotfix",
                    description = "Deploy immediate patch to fix exploit",
                    responseType = EventResponseType.EmergencyFix,
                    rpCost = 3,
                    cpCost = 0,
                    sentimentChange = 15f,
                    successMessage = "Exploit patched! Community crisis averted.",
                    buttonColor = new Color(0.7f, 0.3f, 0.9f) // Purple
                },
                new EventResponseOption
                {
                    buttonText = "Investigate First",
                    description = "Analyze the exploit before making changes",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 2,
                    cpCost = 2,
                    sentimentChange = 5f,
                    successMessage = "Thorough analysis completed before fix.",
                    buttonColor = new Color(0.5f, 0.5f, 0.5f) // Gray
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a tournament opportunity event
        /// </summary>
        public static EventData CreateTournamentOpportunityEvent()
        {
            var eventData = new EventData(
                "Major Tournament Announcement",
                "A large esports organization just announced a $500K tournament for your game. This is a perfect opportunity to showcase the current balance state and boost competitive engagement across all skill levels.",
                EventType.Opportunity,
                EventSeverity.Medium
            );

            eventData.timeRemaining = 120f; // 4 turns remaining
            eventData.expectedImpact = 7.5f;
            eventData.expectedImpacts = new List<string>
            {
                "Community satisfaction: +15%",
                "Character diversity spotlight",
                "Pro player engagement: +30%"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Tournament Promotion",
                    description = "Actively promote and support the tournament",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 0,
                    cpCost = 4,
                    sentimentChange = 12f,
                    successMessage = "Tournament promotion successful!",
                    buttonColor = new Color(0.2f, 0.6f, 0.9f) // Blue
                },
                new EventResponseOption
                {
                    buttonText = "Pre-Tournament Balance",
                    description = "Make targeted balance adjustments",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 5,
                    cpCost = 0,
                    sentimentChange = 8f,
                    successMessage = "Tournament-ready balance achieved!",
                    buttonColor = new Color(0.9f, 0.6f, 0.2f) // Orange
                },
                new EventResponseOption
                {
                    buttonText = "Observe & Learn",
                    description = "Monitor tournament without intervention",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = 3f,
                    successMessage = "Valuable tournament data collected.",
                    buttonColor = new Color(0.4f, 0.4f, 0.4f) // Gray
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a content creator collaboration event
        /// </summary>
        public static EventData CreateCreatorCollaborationEvent()
        {
            var eventData = new EventData(
                "Content Creator Collaboration",
                "Top streamers want to create educational content about game balance. They're offering to feature your design philosophy and explain recent changes to their combined audience of 2M+ viewers.",
                EventType.Community,
                EventSeverity.Medium
            );

            eventData.timeRemaining = 180f; // 6 turns remaining
            eventData.expectedImpact = 6.5f;
            eventData.expectedImpacts = new List<string>
            {
                "Player education improvement",
                "Community understanding: +20%",
                "New player onboarding: +10%"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Full Collaboration",
                    description = "Work directly with creators on content",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 0,
                    cpCost = 3,
                    sentimentChange = 15f,
                    successMessage = "Amazing collaborative content created!",
                    buttonColor = new Color(0.2f, 0.6f, 0.9f) // Blue
                },
                new EventResponseOption
                {
                    buttonText = "Developer Interview",
                    description = "Provide interview and behind-scenes access",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 0,
                    cpCost = 2,
                    sentimentChange = 10f,
                    successMessage = "Great developer insight shared!",
                    buttonColor = new Color(0.9f, 0.6f, 0.2f) // Orange
                },
                new EventResponseOption
                {
                    buttonText = "Politely Decline",
                    description = "Decline collaboration for now",
                    responseType = EventResponseType.Ignore,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = -2f,
                    successMessage = "Opportunity declined politely.",
                    buttonColor = new Color(0.4f, 0.4f, 0.4f) // Gray
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a championship meta analysis event
        /// </summary>
        public static EventData CreateChampionshipMetaEvent()
        {
            var eventData = new EventData(
                "Championship Meta Analysis",
                "The recent championship revealed that Tank characters dominated with an 87% pick rate. While not game-breaking, this suggests the current meta may be too narrow for optimal competitive diversity.",
                EventType.Competitive,
                EventSeverity.Low
            );

            eventData.timeRemaining = 240f; // 8 turns remaining
            eventData.expectedImpact = 4.0f;
            eventData.expectedImpacts = new List<string>
            {
                "Character diversity: Low",
                "Viewer engagement: Stable",
                "Pro player satisfaction: Mixed"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Meta Adjustment",
                    description = "Make targeted changes to improve diversity",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 4,
                    cpCost = 0,
                    sentimentChange = 8f,
                    successMessage = "Meta diversity improved successfully!",
                    buttonColor = new Color(0.7f, 0.3f, 0.9f) // Purple
                },
                new EventResponseOption
                {
                    buttonText = "Deep Analysis",
                    description = "Conduct thorough meta analysis first",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 2,
                    cpCost = 1,
                    sentimentChange = 4f,
                    successMessage = "Comprehensive meta analysis completed.",
                    buttonColor = new Color(0.9f, 0.6f, 0.2f) // Orange
                },
                new EventResponseOption
                {
                    buttonText = "Continue Monitoring",
                    description = "Wait and see how meta develops naturally",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = 0f,
                    successMessage = "Continued monitoring of meta trends.",
                    buttonColor = new Color(0.4f, 0.4f, 0.4f) // Gray
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a viral moment opportunity event
        /// </summary>
        public static EventData CreateViralMomentEvent()
        {
            var eventData = new EventData(
                "Viral Gameplay Moment",
                "A popular streamer just pulled off an incredible combo that's going viral across social media. This is a perfect opportunity to showcase the current balance state and capitalize on the excitement.",
                EventType.Opportunity,
                EventSeverity.Medium
            );

            eventData.timeRemaining = 90f;
            eventData.expectedImpact = 6.5f;
            eventData.expectedImpacts = new List<string>
            {
                "+25% Social media engagement",
                "Positive community sentiment",
                "New player interest spike",
                "Meta showcase opportunity"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Amplify Moment",
                    description = "Share and promote the viral content",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 1,
                    cpCost = 3,
                    sentimentChange = 12f,
                    successMessage = "Viral moment amplified! Massive positive engagement.",
                    buttonColor = new Color(0.6f, 0.4f, 0.8f)
                },
                new EventResponseOption
                {
                    buttonText = "Balance Spotlight",
                    description = "Use moment to highlight balance philosophy",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 2,
                    cpCost = 2,
                    sentimentChange = 8f,
                    successMessage = "Great education opportunity seized!",
                    buttonColor = new Color(0.4f, 0.6f, 0.8f)
                },
                new EventResponseOption
                {
                    buttonText = "Monitor Impact",
                    description = "Track the viral spread and its effects",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 1,
                    sentimentChange = 3f,
                    successMessage = "Valuable engagement data collected.",
                    buttonColor = new Color(0.3f, 0.4f, 0.8f)
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a server performance crisis event
        /// </summary>
        public static EventData CreateServerCrisisEvent()
        {
            var eventData = new EventData(
                "Server Performance Crisis",
                "Massive lag spikes and connection issues are plaguing ranked matches. Players are losing games due to technical problems, and competitive integrity is being questioned.",
                EventType.Technical,
                EventSeverity.Critical
            );

            eventData.timeRemaining = 30f; // Very urgent
            eventData.expectedImpact = 8.5f;
            eventData.expectedImpacts = new List<string>
            {
                "Competitive integrity compromised",
                "Player frustration at peak",
                "Ranked system credibility damaged",
                "Mass disconnections continuing"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Emergency Maintenance",
                    description = "Take servers offline for immediate fixes",
                    responseType = EventResponseType.EmergencyFix,
                    rpCost = 5,
                    cpCost = 0,
                    sentimentChange = 12f,
                    successMessage = "Servers stabilized! Performance restored.",
                    buttonColor = new Color(0.8f, 0.3f, 0.4f)
                },
                new EventResponseOption
                {
                    buttonText = "Crisis Communication",
                    description = "Keep players informed while fixing issues",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 1,
                    cpCost = 4,
                    sentimentChange = 7f,
                    successMessage = "Players appreciate transparency during crisis.",
                    buttonColor = new Color(0.6f, 0.4f, 0.8f)
                },
                new EventResponseOption
                {
                    buttonText = "Monitor Systems",
                    description = "Continue monitoring without major action",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = -8f,
                    successMessage = "Issues persist. Player exodus begins.",
                    buttonColor = new Color(0.3f, 0.4f, 0.8f)
                }
            };

            return eventData;
        }

        /// <summary>
        /// Create a community feedback surge event
        /// </summary>
        public static EventData CreateFeedbackSurgeEvent()
        {
            var eventData = new EventData(
                "Community Feedback Surge",
                "The latest balance changes have generated intense discussion across all community platforms. Players are split between loving the changes and demanding rollbacks.",
                EventType.Community,
                EventSeverity.Medium
            );

            eventData.timeRemaining = 75f;
            eventData.expectedImpact = 5.5f;
            eventData.expectedImpacts = new List<string>
            {
                "Polarized community opinions",
                "High engagement levels",
                "Sentiment volatility",
                "Meta adaptation phase"
            };

            eventData.responseOptions = new List<EventResponseOption>
            {
                new EventResponseOption
                {
                    buttonText = "Address Concerns",
                    description = "Directly respond to community feedback",
                    responseType = EventResponseType.CommunityManagement,
                    rpCost = 0,
                    cpCost = 5,
                    sentimentChange = 8f,
                    successMessage = "Community feels heard and understood.",
                    buttonColor = new Color(0.6f, 0.4f, 0.8f)
                },
                new EventResponseOption
                {
                    buttonText = "Data Deep Dive",
                    description = "Share detailed balance data and reasoning",
                    responseType = EventResponseType.CustomResponse,
                    rpCost = 3,
                    cpCost = 2,
                    sentimentChange = 6f,
                    successMessage = "Transparency builds trust with community.",
                    buttonColor = new Color(0.4f, 0.6f, 0.8f)
                },
                new EventResponseOption
                {
                    buttonText = "Let It Settle",
                    description = "Allow community to adapt naturally",
                    responseType = EventResponseType.ObserveAndLearn,
                    rpCost = 0,
                    cpCost = 0,
                    sentimentChange = -2f,
                    successMessage = "Community gradually adapts to changes.",
                    buttonColor = new Color(0.3f, 0.4f, 0.8f)
                }
            };

            return eventData;
        }

        #endregion

        #region Contextual Event Creation

        /// <summary>
        /// Create a random event based on current game state
        /// </summary>
        public static EventData CreateContextualEvent(float communitySentiment, int currentWeek, bool hasCriticalIssues)
        {
            if (hasCriticalIssues)
            {
                return CreateGameBreakingExploitEvent();
            }

            if (communitySentiment < 30f)
            {
                return CreateFeedbackSurgeEvent();
            }

            if (communitySentiment > 70f)
            {
                return CreateViralMomentEvent();
            }

            if (currentWeek % 4 == 0) // Every 4 weeks
            {
                return CreateTournamentOpportunityEvent();
            }

            // Default to community event
            return CreateFeedbackSurgeEvent();
        }

        /// <summary>
        /// Get a random crisis event
        /// </summary>
        public static EventData GetRandomCrisisEvent()
        {
            int eventChoice = Random.Range(0, 3);

            switch (eventChoice)
            {
                case 0:
                    return CreateGameBreakingExploitEvent();
                case 1:
                    return CreateServerCrisisEvent();
                case 2:
                    return CreateFeedbackSurgeEvent();
                default:
                    return CreateGameBreakingExploitEvent();
            }
        }

        /// <summary>
        /// Get a random opportunity event
        /// </summary>
        public static EventData GetRandomOpportunityEvent()
        {
            int eventChoice = Random.Range(0, 3);

            switch (eventChoice)
            {
                case 0:
                    return CreateViralMomentEvent();
                case 1:
                    return CreateCreatorCollaborationEvent();
                case 2:
                    return CreateTournamentOpportunityEvent();
                default:
                    return CreateViralMomentEvent();
            }
        }

        /// <summary>
        /// Get a random community event
        /// </summary>
        public static EventData GetRandomCommunityEvent()
        {
            return CreateFeedbackSurgeEvent();
        }

        /// <summary>
        /// Get a random technical event
        /// </summary>
        public static EventData GetRandomTechnicalEvent()
        {
            return CreateServerCrisisEvent();
        }

        /// <summary>
        /// Get a random competitive event
        /// </summary>
        public static EventData GetRandomCompetitiveEvent()
        {
            return CreateChampionshipMetaEvent();
        }

        /// <summary>
        /// Get any random event
        /// </summary>
        public static EventData GetAnyRandomEvent()
        {
            int eventCategory = Random.Range(0, 5);

            switch (eventCategory)
            {
                case 0:
                    return GetRandomCrisisEvent();
                case 1:
                    return GetRandomOpportunityEvent();
                case 2:
                    return GetRandomCommunityEvent();
                case 3:
                    return GetRandomTechnicalEvent();
                case 4:
                    return GetRandomCompetitiveEvent();
                default:
                    return CreateFeedbackSurgeEvent();
            }
        }

        #endregion

        #region Instance Methods (for ScriptableObject usage)

        /// <summary>
        /// Create event from template (if using ScriptableObject templates)
        /// </summary>
        public EventData CreateEventFromTemplate(EventType eventType, int templateIndex = -1)
        {
            List<EventTemplate> templates = eventType switch
            {
                EventType.Crisis => crisisTemplates,
                EventType.Opportunity => opportunityTemplates,
                EventType.Community => communityTemplates,
                EventType.Technical => technicalTemplates,
                EventType.Competitive => competitiveTemplates,
                _ => communityTemplates
            };

            if (templates.Count == 0)
            {
                // Fallback to static methods
                return eventType switch
                {
                    EventType.Crisis => CreateGameBreakingExploitEvent(),
                    EventType.Opportunity => CreateViralMomentEvent(),
                    EventType.Community => CreateFeedbackSurgeEvent(),
                    EventType.Technical => CreateServerCrisisEvent(),
                    EventType.Competitive => CreateChampionshipMetaEvent(),
                    _ => CreateFeedbackSurgeEvent()
                };
            }

            int index = templateIndex >= 0 ? templateIndex : Random.Range(0, templates.Count);
            return templates[index].CreateEvent();
        }

        #endregion
    }

    #region Template Classes

    /// <summary>
    /// Template for creating events from ScriptableObjects
    /// </summary>
    [System.Serializable]
    public class EventTemplate
    {
        [Header("Basic Info")]
        public string title;
        [TextArea(3, 5)]
        public string description;
        public EventType eventType;
        public EventSeverity severity;

        [Header("Timing")]
        public float timeRemaining = 60f;
        public float expectedImpact = 5f;

        [Header("Expected Impacts")]
        public List<string> expectedImpacts = new List<string>();

        [Header("Response Options")]
        public List<EventResponseTemplate> responseTemplates = new List<EventResponseTemplate>();

        public EventData CreateEvent()
        {
            var eventData = new EventData(title, description, eventType, severity)
            {
                timeRemaining = timeRemaining,
                expectedImpact = expectedImpact,
                expectedImpacts = new List<string>(expectedImpacts)
            };

            foreach (var template in responseTemplates)
            {
                eventData.responseOptions.Add(template.CreateResponse());
            }

            return eventData;
        }
    }

    /// <summary>
    /// Template for event response options
    /// </summary>
    [System.Serializable]
    public class EventResponseTemplate
    {
        [Header("Response Info")]
        public string buttonText;
        [TextArea(2, 3)]
        public string description;
        public EventResponseType responseType;

        [Header("Costs")]
        public int rpCost = 0;
        public int cpCost = 0;

        [Header("Effects")]
        public float sentimentChange = 0f;
        public string successMessage = "";
        public Color buttonColor = Color.white;

        public EventResponseOption CreateResponse()
        {
            return new EventResponseOption
            {
                buttonText = buttonText,
                description = description,
                responseType = responseType,
                rpCost = rpCost,
                cpCost = cpCost,
                sentimentChange = sentimentChange,
                successMessage = successMessage,
                buttonColor = buttonColor
            };
        }
    }

    #endregion
}