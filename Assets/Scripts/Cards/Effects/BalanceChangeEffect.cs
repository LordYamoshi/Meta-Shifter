using UnityEngine;
using MetaBalance.Characters;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Effect that changes a character's stat
    /// </summary>
    public class BalanceChangeEffect : CardEffect
    {
        private CharacterType _targetType;
        private CharacterStat _targetStat;
        private float _percentageChange;
        private bool _hasExecuted = false;
        
        public BalanceChangeEffect(CardData source, CharacterType targetType, CharacterStat targetStat, float percentageChange) 
            : base(source)
        {
            _targetType = targetType;
            _targetStat = targetStat;
            _percentageChange = percentageChange;
        }
        
        public override bool Execute()
        {
            // Find character manager
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("Character Manager not found");
                return false;
            }
            
            // Apply stat change
            characterManager.ModifyCharacterStat(_targetType, _targetStat, _percentageChange);
            _hasExecuted = true;
            
            // Log for debugging
            Debug.Log($"Balance Change: {_targetType}'s {_targetStat} changed by {_percentageChange}%");
            
            return true;
        }
        
        public override bool Undo()
        {
            if (!_hasExecuted)
                return false;
                
            // Find character manager
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("Character Manager not found");
                return false;
            }
            
            // Reverse the percentage change
            float reverseChange = -_percentageChange;
            characterManager.ModifyCharacterStat(_targetType, _targetStat, reverseChange);
            
            // Log for debugging
            Debug.Log($"Balance Change Undone: {_targetType}'s {_targetStat} reverted");
            
            _hasExecuted = false;
            return true;
        }
        
        public override string GetDescription()
        {
            string direction = _percentageChange >= 0 ? "Increase" : "Decrease";
            return $"{direction} {_targetType}'s {_targetStat} by {Mathf.Abs(_percentageChange)}%";
        }
    }
}