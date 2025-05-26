using UnityEngine;
using MetaBalance.Characters;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Base class for all card effects (Command pattern)
    /// </summary>
    public abstract class CardEffect
    {
        protected CardData sourceCard;
        
        public CardEffect(CardData source)
        {
            sourceCard = source;
        }
        
        public abstract bool Execute();
        public abstract bool Undo();
        public abstract string GetDescription();
    }
}