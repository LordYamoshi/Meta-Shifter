using UnityEngine;
using System;

namespace MetaBalance.Cards
{

  /// <summary>
    /// Meta shift cards that affect multiple characters
    /// </summary>
    [CreateAssetMenu(fileName = "MetaShiftCard", menuName = "Meta Balance/Meta Shift Card")]
    public class MetaShiftCard : CardData
    {
        [Header("Meta Shift Settings")]
        public MetaShiftType shiftType;
        [Range(1, 10)]
        public int effectPower = 5;
        
        public override void PlayCard()
        {
            var characterManager = Characters.CharacterManager.Instance;
            if (characterManager == null) return;
            
            switch (shiftType)
            {
                case MetaShiftType.MapPool:
                    // Favor tanks and supports
                    characterManager.ModifyStat(Characters.CharacterType.Tank, Characters.CharacterStat.WinRate, effectPower);
                    characterManager.ModifyStat(Characters.CharacterType.Support, Characters.CharacterStat.WinRate, effectPower * 0.8f);
                    characterManager.ModifyStat(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, -effectPower * 0.5f);
                    break;
                    
                case MetaShiftType.GameMode:
                    // Favor damage dealers
                    characterManager.ModifyStat(Characters.CharacterType.Mage, Characters.CharacterStat.WinRate, effectPower);
                    characterManager.ModifyStat(Characters.CharacterType.Warrior, Characters.CharacterStat.WinRate, effectPower * 0.7f);
                    break;
                    
                case MetaShiftType.ItemBalance:
                    // Small changes to all characters
                    foreach (Characters.CharacterType type in Enum.GetValues(typeof(Characters.CharacterType)))
                    {
                        float change = UnityEngine.Random.Range(-effectPower * 0.5f, effectPower * 0.5f);
                        characterManager.ModifyStat(type, Characters.CharacterStat.WinRate, change);
                    }
                    break;
            }
            
            Debug.Log($"Applied {shiftType} meta shift with power {effectPower}");
        }
    }
    
}