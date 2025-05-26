namespace MetaBalance.Characters
{
    /// <summary>
    /// Interface for character states (State Pattern)
    /// </summary>
    public interface ICharacterState
    {
        void Enter();
        void Update();
        void Exit();
    }
}