using UnityEngine;
using System.Collections.Generic;

namespace MetaBalance.Characters
{
    /// <summary>
    /// ScriptableObject containing base character data
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "Meta Balance/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public CharacterType characterType;
        public string characterName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("Base Stats")]
        public float baseHealth = 50f;
        public float baseDamage = 50f;
        public float baseSpeed = 50f;
        public float baseUtility = 50f;
        public float basePopularity = 50f;
        
        [Header("Visuals")]
        public GameObject characterPrefab;
        public Sprite characterPortrait;
        public Color characterColor = Color.white;
        
        [Header("Sound Effects")]
        public AudioClip selectionSound;
        public AudioClip buffSound;
        public AudioClip nerfSound;
        
        /// <summary>
        /// Gets the base value for a specific stat
        /// </summary>
        public float GetBaseStatValue(CharacterStat stat)
        {
            return stat switch
            {
                CharacterStat.Health => baseHealth,
                CharacterStat.Damage => baseDamage,
                CharacterStat.Speed => baseSpeed,
                CharacterStat.Utility => baseUtility,
                CharacterStat.Popularity => basePopularity,
                _ => 50f // Default for stats like WinRate that don't have base values
            };
        }
    }
}