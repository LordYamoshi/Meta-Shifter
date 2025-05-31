using UnityEngine;

namespace MetaBalance.Core
{
    public enum GamePhase
    {
        Planning,       // Select cards to play
        Implementation, // Apply card effects
        Feedback,       // See results
        Event           // Handle events (optional for now)
    }
}

namespace MetaBalance.Characters
{
    public enum CharacterType
    {
        Warrior,
        Mage, 
        Support,
        Tank
    }
    
    public enum CharacterStat
    {
        Health,
        Damage,
        Speed,
        Utility,
        WinRate,
        Popularity
    }
}

namespace MetaBalance.Cards
{
    public enum CardType
    {
        BalanceChange,
        MetaShift,
        Community,
        CrisisResponse,
        Special
    }
    
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Special
    }
}