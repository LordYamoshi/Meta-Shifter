using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MetaBalance.Core
{
    /// <summary>
    /// Dedicated Implementation Manager using Command Pattern and Observer Pattern
    /// Follows Single Responsibility Principle
    /// </summary>
    public class ImplementationManager : MonoBehaviour
    {
        public static ImplementationManager Instance { get; private set; }
        
        [Header("Implementation Settings")]
        [SerializeField] private float cardImplementationDelay = 0.5f;
        [SerializeField] private float phaseTransitionDelay = 1.0f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject implementationEffectPrefab;
        [SerializeField] private AudioClip implementationSound;
        [SerializeField] private ParticleSystem globalImplementationEffect;
        
        [Header("Events")]
        public UnityEvent OnImplementationStarted;
        public UnityEvent<Cards.CardData> OnCardImplemented;
        public UnityEvent<int, int> OnResourcesSpent; // RP, CP
        public UnityEvent OnImplementationCompleted;
        public UnityEvent<string> OnImplementationStatusChanged;
        
        private bool isImplementing = false;
        private Queue<IImplementationCommand> implementationQueue = new Queue<IImplementationCommand>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // Subscribe to phase changes
            if (PhaseManager.Instance != null)
            {
                PhaseManager.Instance.OnPhaseChanged.AddListener(OnPhaseChanged);
            }
        }
        
        private void OnPhaseChanged(GamePhase newPhase)
        {
            if (newPhase == GamePhase.Implementation && !isImplementing)
            {
                StartImplementationProcess();
            }
        }
        
        /// <summary>
        /// Main implementation process using Command Pattern
        /// </summary>
        public void StartImplementationProcess()
        {
            if (isImplementing) return;
            
            Debug.Log("🚀 Starting Implementation Phase");
            StartCoroutine(ImplementationSequence());
        }
        
        private IEnumerator ImplementationSequence()
        {
            isImplementing = true;
            OnImplementationStarted.Invoke();
            OnImplementationStatusChanged.Invoke("Preparing implementations...");
            
            // 1. Collect all queued cards from all drop zones
            var allImplementationCommands = CollectImplementationCommands();
            
            if (allImplementationCommands.Count == 0)
            {
                OnImplementationStatusChanged.Invoke("No cards to implement");
                yield return new WaitForSeconds(1f);
                CompleteImplementation();
                yield break;
            }
            
            // 2. Validate resources
            var (totalRP, totalCP) = CalculateTotalCost(allImplementationCommands);
            
            if (!ValidateResources(totalRP, totalCP))
            {
                OnImplementationStatusChanged.Invoke("Insufficient resources!");
                yield return new WaitForSeconds(2f);
                CompleteImplementation();
                yield break;
            }
            
            // 3. Spend resources upfront
            SpendResources(totalRP, totalCP);
            OnImplementationStatusChanged.Invoke($"Spent {totalRP} RP, {totalCP} CP");
            yield return new WaitForSeconds(0.5f);
            
            // 4. Execute each card with visual feedback
            OnImplementationStatusChanged.Invoke($"Implementing {allImplementationCommands.Count} cards...");
            
            for (int i = 0; i < allImplementationCommands.Count; i++)
            {
                var command = allImplementationCommands[i];
                
                OnImplementationStatusChanged.Invoke($"Implementing {command.GetCardName()} ({i + 1}/{allImplementationCommands.Count})");
                
                // Visual effect
                yield return StartCoroutine(PlayImplementationEffect(command));
                
                // Execute the card
                command.Execute();
                OnCardImplemented.Invoke(command.GetCardData());
                
                Debug.Log($"✅ Implemented: {command.GetCardName()}");
                
                // Delay between cards
                yield return new WaitForSeconds(cardImplementationDelay);
            }
            
            // 5. Clean up and complete
            OnImplementationStatusChanged.Invoke("Implementation complete!");
            yield return new WaitForSeconds(phaseTransitionDelay);
            
            CompleteImplementation();
        }
        
        private List<IImplementationCommand> CollectImplementationCommands()
        {
            var commands = new List<IImplementationCommand>();
            
            // Find all drop zones and collect their cards
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            
            foreach (var dropZone in allDropZones)
            {
                var queuedCards = dropZone.GetQueuedCards();
                
                foreach (var card in queuedCards)
                {
                    if (card != null && card.CardData != null)
                    {
                        // Create command for this card using Factory Pattern
                        var command = CardImplementationCommandFactory.CreateCommand(card.CardData, card);
                        if (command != null)
                        {
                            commands.Add(command);
                        }
                    }
                }
            }
            
            Debug.Log($"📋 Collected {commands.Count} implementation commands");
            return commands;
        }
        
        private (int totalRP, int totalCP) CalculateTotalCost(List<IImplementationCommand> commands)
        {
            int totalRP = 0;
            int totalCP = 0;
            
            foreach (var command in commands)
            {
                var cardData = command.GetCardData();
                totalRP += cardData.researchPointCost;
                totalCP += cardData.communityPointCost;
            }
            
            return (totalRP, totalCP);
        }
        
        private bool ValidateResources(int requiredRP, int requiredCP)
        {
            var resourceManager = ResourceManager.Instance;
            if (resourceManager == null) return false;
            
            bool canAfford = resourceManager.CanSpend(requiredRP, requiredCP);
            
            Debug.Log($"💰 Resource validation: Need {requiredRP} RP, {requiredCP} CP | Have {resourceManager.ResearchPoints} RP, {resourceManager.CommunityPoints} CP | Can afford: {canAfford}");
            
            return canAfford;
        }
        
        private void SpendResources(int rp, int cp)
        {
            var resourceManager = ResourceManager.Instance;
            if (resourceManager != null)
            {
                resourceManager.SpendResources(rp, cp);
                OnResourcesSpent.Invoke(rp, cp);
            }
        }
        
        private IEnumerator PlayImplementationEffect(IImplementationCommand command)
        {
            // Visual feedback for card implementation
            if (implementationEffectPrefab != null)
            {
                var effect = Instantiate(implementationEffectPrefab, command.GetCardTransform().position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Audio feedback
            if (implementationSound != null)
            {
                AudioSource.PlayClipAtPoint(implementationSound, Camera.main.transform.position);
            }
            
            // Global particle effect
            if (globalImplementationEffect != null)
            {
                globalImplementationEffect.Play();
            }
            
            yield return new WaitForSeconds(0.3f);
        }
        
        private void CompleteImplementation()
        {
            // Clear all drop zones
            ClearAllDropZones();
            
            isImplementing = false;
            OnImplementationCompleted.Invoke();
            OnImplementationStatusChanged.Invoke("Ready for feedback phase");
            
            Debug.Log("✅ Implementation phase completed");
        }
        
        private void ClearAllDropZones()
        {
            var allDropZones = FindObjectsOfType<Cards.CardDropZone>();
            
            foreach (var dropZone in allDropZones)
            {
                dropZone.ClearQueue();
            }
        }
        
        public bool IsImplementing() => isImplementing;
    }
    
    /// <summary>
    /// Command Pattern for card implementation
    /// </summary>
    public interface IImplementationCommand
    {
        void Execute();
        Cards.CardData GetCardData();
        string GetCardName();
        Transform GetCardTransform();
        bool CanExecute();
    }
    
    /// <summary>
    /// Concrete command for card implementation
    /// </summary>
    public class CardImplementationCommand : IImplementationCommand
    {
        private readonly Cards.CardData cardData;
        private readonly Cards.DraggableCard cardComponent;
        
        public CardImplementationCommand(Cards.CardData data, Cards.DraggableCard component)
        {
            cardData = data;
            cardComponent = component;
        }
        
        public void Execute()
        {
            if (CanExecute())
            {
                cardData.PlayCard();
            }
        }
        
        public Cards.CardData GetCardData() => cardData;
        public string GetCardName() => cardData?.cardName ?? "Unknown Card";
        public Transform GetCardTransform() => cardComponent?.transform;
        
        public bool CanExecute()
        {
            return cardData != null;
        }
    }
    
    /// <summary>
    /// Factory Pattern for creating implementation commands
    /// </summary>
    public static class CardImplementationCommandFactory
    {
        public static IImplementationCommand CreateCommand(Cards.CardData cardData, Cards.DraggableCard cardComponent)
        {
            if (cardData == null) return null;
            
            // Could extend this to create different command types based on card type
            return cardData.cardType switch
            {
                Cards.CardType.BalanceChange => new CardImplementationCommand(cardData, cardComponent),
                Cards.CardType.MetaShift => new CardImplementationCommand(cardData, cardComponent),
                Cards.CardType.Community => new CardImplementationCommand(cardData, cardComponent),
                Cards.CardType.CrisisResponse => new CardImplementationCommand(cardData, cardComponent),
                Cards.CardType.Special => new CardImplementationCommand(cardData, cardComponent),
                _ => new CardImplementationCommand(cardData, cardComponent)
            };
        }
    }
}