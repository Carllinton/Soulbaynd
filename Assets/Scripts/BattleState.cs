using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SoulBind
{
    public class BattleState : MonoBehaviour
    {
        public List<PlayerState> players = new List<PlayerState>();
        public int currentRound = 1;
        public int currentPhase = 1;

        public IEnumerable<PlayerState> GetOpponentsOf(PlayerState self)
        {
            return players.Where(p => p != self);
        }

        public void AdvanceRound()
        {
            currentRound++;
            foreach (var p in players)
            {
                p.TickRoundEnd();
                p.isFrozen = false;

                if (p.corruption >= 10) p.vesselHP -= 4;
                else if (p.corruption >= 5) p.vesselHP -= 2;
            }
        }

        public int currentTurnIndex = 0;

        public PlayerState GetCurrentTurnPlayer()
        {
            return players[currentTurnIndex];
        }

        public void AdvanceTurn()
        {
            int attempts = 0;
            do
            {
                currentTurnIndex = (currentTurnIndex + 1) % players.Count;
                attempts++;
            }
            while (players[currentTurnIndex].isDefeated && attempts < players.Count);
        }

        public bool IsGameOver(out PlayerState winner)
        {
            winner = null;
            int aliveCount = 0;
            foreach (var p in players)
            {
                if (!p.isDefeated)
                {
                    aliveCount++;
                    winner = p;
                }
            }
            return aliveCount <= 1;
        }
    }
}