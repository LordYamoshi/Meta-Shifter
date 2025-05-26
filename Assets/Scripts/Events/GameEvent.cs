using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Events
{
    /// <summary>
    /// ScriptableObject for game events
    /// </summary>
    [CreateAssetMenu(fileName = "New Game Event", menuName = "Meta Balance/Game Event")]
    public class GameEvent : ScriptableObject
    {
        [Header("Event Info")]
        public string eventTitle;
        [TextArea(3, 5)]
        public string eventDescription;
        public EventCategory category;
        public EventSeverity severity;
        
        [Header("Event Effects")]
        [Range(-25f, 25f)]
        public float playerSatisfactionEffect;
        public List<CharacterEffect> characterEffects = new List<CharacterEffect>();
        
        [Header("Resource Effects")]
        public int researchPointsCost;
        public int communityPointsCost;
        public int researchPointsReward;
        public int communityPointsReward;
        
        [Header("Options")]
        public List<EventOption> options = new List<EventOption>();
        
        [Header("Visuals")]
        public Sprite eventImage;
        public AudioClip eventSound;
    }
}