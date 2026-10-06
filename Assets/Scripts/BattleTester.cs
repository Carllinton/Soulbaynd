using UnityEngine;

namespace SoulBind
{
    public class BattleTester : MonoBehaviour
    {
        private BattleState battleState;
        private TurnManager turnManager;

        [SerializeField] private int maxTurns = 30;

        private void Start()
        {
            battleState = GetComponent<BattleState>();
            turnManager = GetComponent<TurnManager>();

            if (battleState == null || turnManager == null)
            {
                Debug.LogError("BattleTester: BattleState or TurnManager missing on this GameObject.");
                return;
            }

            if (battleState.players.Count < 2)
            {
                Debug.LogError("BattleTester: BattleState needs at least 2 players assigned.");
                return;
            }

            const int startingHandSize = 5;
            foreach (var p in battleState.players)
            {
                p.ResetForNewGame(startingHandSize);
                Debug.Log($"{p.playerName} drew a hand of {p.hand.Count}: {string.Join(", ", p.hand.ConvertAll(c => c.cardName))}");
            }

            int turnCount = 0;
            while (!battleState.IsGameOver(out _) && turnCount < maxTurns)
            {
                turnManager.RunTurn();
                LogAllStats();
                turnCount++;
            }

            if (turnCount >= maxTurns)
                Debug.LogWarning($"BattleTester: hit the {maxTurns}-turn safety cap without a winner. Either combat is too weak to finish games, or something is looping without progress.");
        

            foreach (var p in battleState.players)
            p.souls = 13; 
        }

        private void LogAllStats()
        {
            foreach (var p in battleState.players)
                Debug.Log($"{p.playerName}: ATK={p.CurrentATK} DEF={p.CurrentDEF} CAP={p.CurrentCAP} HP={p.vesselHP} Corruption={p.corruption} Souls={p.souls}{(p.hasAwakened ? " [AWAKENED]" : "")}");
        }
    }
}