using UnityEngine;

namespace MetaBalance.Cards
{
    [System.Serializable]
    public class CardPoolSettings
    {
        [Range(1, 10)] public int commonCardCopies = 4;
        [Range(1, 5)] public int uncommonCardCopies = 3;
        [Range(1, 3)] public int rareCardCopies = 2;
        [Range(1, 2)] public int epicCardCopies = 1;
        [Range(1, 1)] public int specialCardCopies = 1;
    }
}