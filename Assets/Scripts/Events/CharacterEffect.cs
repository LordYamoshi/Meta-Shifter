using MetaBalance.Characters;

namespace MetaBalance.Events
{
    /// <summary>
    /// Effect on a character
    /// </summary>
    [System.Serializable]
    public class CharacterEffect
    {
        public CharacterType characterType;
        public CharacterStat targetStat;
        [UnityEngine.Range(-25f, 25f)]
        public float percentageChange;
    }
}