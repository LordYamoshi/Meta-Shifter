using UnityEngine;
using System;

namespace MetaBalance.Cards
{
    /// <summary>
    /// Runtime instance of a card
    /// </summary>
    [Serializable]
    public class CardInstance
    {
        // Unique ID for this card instance
        public string id;
        
        // Reference to the card data
        public CardData cardData;
        
        // Effect instance
        private CardEffect _effect;
        
        // State
        public bool isPlayed { get; private set; }
        
        public CardInstance(CardData data)
        {
            id = Guid.NewGuid().ToString();
            cardData = data;
            isPlayed = false;
        }
        
        public CardEffect GetEffect()
        {
            if (_effect == null)
                _effect = cardData.CreateEffect();
                
            return _effect;
        }
        
        public bool Play()
        {
            if (isPlayed)
                return false;
                
            if (GetEffect().Execute())
            {
                isPlayed = true;
                return true;
            }
            
            return false;
        }
        
        public bool Undo()
        {
            if (!isPlayed)
                return false;
                
            if (_effect != null && _effect.Undo())
            {
                isPlayed = false;
                return true;
            }
            
            return false;
        }
    }
}