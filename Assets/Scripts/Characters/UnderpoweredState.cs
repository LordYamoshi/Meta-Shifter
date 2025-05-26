using UnityEngine;

namespace MetaBalance.Characters
{
    /// <summary>
    /// State for underpowered characters (win rate < 45%)
    /// </summary>
    public class UnderpoweredState : ICharacterState
    {
        protected readonly GameCharacter _character;
        private float _timeInState = 0f;
        
        public UnderpoweredState(GameCharacter character)
        {
            _character = character;
        }
        
        public virtual void Enter()
        {
            Debug.Log($"{_character.GetCharacterName()} is now underpowered!");
            
            // Underpowered characters lose popularity
            _character.ModifyStat(CharacterStat.Popularity, -5f);
        }
        
        public virtual void Update()
        {
            _timeInState += Time.deltaTime;
            
            // The longer a character is underpowered, the less popular they become
            if (_timeInState > 5.0f)
            {
                _timeInState = 0f;
                _character.ModifyStat(CharacterStat.Popularity, -1f);
            }
        }
        
        public virtual void Exit()
        {
            // Nothing specific needed
        }
    }
}