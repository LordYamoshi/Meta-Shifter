using UnityEngine;

namespace MetaBalance.Characters
{
    /// <summary>
    /// State for balanced characters (win rate 45-55%)
    /// </summary>
    public class BalancedState : ICharacterState
    {
        protected readonly GameCharacter _character;
        
        public BalancedState(GameCharacter character)
        {
            _character = character;
        }
        
        public virtual void Enter()
        {
            Debug.Log($"{_character.GetCharacterName()} is now balanced");
        }
        
        public virtual void Update()
        {
            // Balanced characters are stable
        }
        
        public virtual void Exit()
        {
            // Nothing specific needed
        }
    }
}