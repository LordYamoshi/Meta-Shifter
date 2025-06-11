    using UnityEngine;
    using System;

    namespace MetaBalance.Cards
    {

        /// <summary>
        /// Balance change cards that modify character stats
        /// </summary>
        [CreateAssetMenu(fileName = "BalanceCard", menuName = "Meta Balance/Balance Card")]
        public class BalanceChangeCard : CardData
        {
            [Header("Balance Change Settings")]
            public Characters.CharacterType targetCharacter;
            public Characters.CharacterStat targetStat;
            [Range(-50f, 50f)]
            public float percentageChange;
        
            public override void PlayCard()
            {
                var characterManager = Characters.CharacterManager.Instance;
                if (characterManager != null)
                {
                    characterManager.ModifyStat(targetCharacter, targetStat, percentageChange);
                    Debug.Log($"Applied {percentageChange:+0.0;-0.0}% to {targetCharacter} {targetStat}");
                }
            }
        }
    }