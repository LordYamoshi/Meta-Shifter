using UnityEngine;
using System;

namespace MetaBalance.Cards
{
  
    /// <summary>
    /// Community cards that affect popularity
    /// </summary>
    [CreateAssetMenu(fileName = "CommunityCard", menuName = "Meta Balance/Community Card")]
    public class CommunityCard : CardData
    {
        [Header("Community Settings")]
        public bool affectsAllCharacters = true;
        public Characters.CharacterType specificCharacter;
        [Range(-20f, 20f)]
        public float popularityChange = 10f;
        
        public override void PlayCard()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            if (affectsAllCharacters)
            {
                foreach (Characters.CharacterType type in Enum.GetValues(typeof(Characters.CharacterType)))
                {
                    characterManager.ModifyStat(type, Characters.CharacterStat.Popularity, popularityChange);
                }
                Debug.Log($"Applied {popularityChange:+0.0;-0.0}% popularity to all characters");
            }
            else
            {
                characterManager.ModifyStat(specificCharacter, Characters.CharacterStat.Popularity, popularityChange);
                Debug.Log($"Applied {popularityChange:+0.0;-0.0}% popularity to {specificCharacter}");
            }
        }
    }
}