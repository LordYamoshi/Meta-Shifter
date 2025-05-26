using UnityEngine;

namespace MetaBalance.Characters
{
    /// <summary>
    /// State for overpowered characters (win rate > 55%)
    /// </summary>
    public class OverpoweredState : ICharacterState
    {
        protected readonly GameCharacter _character;
        private float _timeInState = 0f;
        
        public OverpoweredState(GameCharacter character)
        {
            _character = character;
        }
        
        public virtual void Enter()
        {
            Debug.Log($"{_character.GetCharacterName()} is now overpowered!");
            
            // Overpowered characters gain popularity
            _character.ModifyStat(CharacterStat.Popularity, 5f);
        }
        
        public virtual void Update()
        {
            _timeInState += Time.deltaTime;
            
            // The longer a character is overpowered, the more popular they become
            if (_timeInState > 5.0f)
            {
                _timeInState = 0f;
                _character.ModifyStat(CharacterStat.Popularity, 1f);
            }
        }
        
        public virtual void Exit()
        {
            // Nothing specific needed
        }
    }
}