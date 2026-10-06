using System.Collections.Generic;
using UnityEngine;

namespace SoulBind
{
    public enum StatType { ATK, DEF, CAP }

    [System.Serializable]
    public class StatModifier
    {
        public StatType stat;
        public int amount;
        public int roundsRemaining; 

        public StatModifier(StatType stat, int amount, int rounds)
        {
            this.stat = stat;
            this.amount = amount;
            this.roundsRemaining = rounds;
        }
    }

    public class PlayerState : MonoBehaviour
    {
        [Header("Identity")]
        public string playerName;

        [Header("Base Stats")]
        public int baseATK;
        public int baseDEF;
        public int baseCAP;

        [Header("Vitals")]
        public int vesselHP;
        public int corruption;

        [Header("Cards")]
        public List<Card> deck = new List<Card>();
        public List<Card> hand = new List<Card>();
        public List<Card> discardPile = new List<Card>();

        private List<StatModifier> activeModifiers = new List<StatModifier>();

        public int CurrentATK => baseATK + SumModifier(StatType.ATK);
        public int CurrentDEF => baseDEF + SumModifier(StatType.DEF);
        public int CurrentCAP => baseCAP + SumModifier(StatType.CAP);

        private int SumModifier(StatType stat)
        {
            int total = 0;
            foreach (var m in activeModifiers)
                if (m.stat == stat) total += m.amount;
            return total;
        }

        public void AddModifier(StatType stat, int amount, int rounds)
        {
            activeModifiers.Add(new StatModifier(stat, amount, rounds));
        }

        public void TakeCorruption(int amount)
        {
            corruption = Mathf.Max(0, corruption + amount);
        }

        public void Heal(int amount)
        {
            vesselHP += amount;
        }

        public void TickRoundEnd()
        {
            for (int i = activeModifiers.Count - 1; i >= 0; i--)
            {
                activeModifiers[i].roundsRemaining--;
                if (activeModifiers[i].roundsRemaining <= 0)
                    activeModifiers.RemoveAt(i);
            }
        }
        public bool isFrozen = false;
        public bool isDefeated = false;
        public void Draw()
        {
            if (deck.Count == 0 && discardPile.Count > 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck();
                Debug.Log($"{playerName}: deck empty, reshuffled discard pile back in.");
            }

            if (deck.Count == 0) return;

            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }

        public System.Collections.Generic.List<Card> cardsPlayedThisTurn = new System.Collections.Generic.List<Card>();
        public float pendingATKMultiplier = 1f;
        public int pendingCorruptOnDefBreakFail = 0;

        public void ShuffleDeck()
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (deck[i], deck[j]) = (deck[j], deck[i]);
            }
        }

        public void DrawUpTo(int handSize)
        {
            while (hand.Count < handSize && deck.Count > 0)
                Draw();

            if (deck.Count == 0 && hand.Count < handSize)
                Debug.LogWarning($"{playerName}: deck is empty, could not fill hand to {handSize}.");
        }

        public void ResetForNewGame(int startingHandSize)
        {
            hand.Clear();
            discardPile.Clear();
            ShuffleDeck();
            DrawUpTo(startingHandSize);
        }

        public bool hasAwakened = false;
        public const int SOULS_TO_AWAKEN = 25;

        public int souls;
        public bool isDesperate = false;

        public bool IsSoullocked => corruption >= 5;

        public void CheckDesperation()
        {
            if (souls <= 0 && !isDesperate)
            {
                isDesperate = true;
                TakeCorruption(5);
                Debug.Log($"{playerName} falls to 0 souls — Desperation triggers!");
            }
        }

        public void CheckAwakening()
        {
            if (souls >= SOULS_TO_AWAKEN && !hasAwakened)
            {
                hasAwakened = true;
                Debug.Log($"{playerName} reaches {souls} souls — Awakened! Must travel to the Ascension Hex to win.");
            }
        }
    }
}