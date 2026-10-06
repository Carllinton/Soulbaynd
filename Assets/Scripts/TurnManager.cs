using UnityEngine;

namespace SoulBind
{
    public class TurnManager : MonoBehaviour
    {
        public BattleState battleState;

        public void PlayCard(PlayerState caster, Card card)
        {
            if (caster.isFrozen)
            {
                Debug.Log($"{caster.playerName} is frozen and cannot play cards.");
                return;
            }

            CardEffectExecutor.Execute(card, caster, battleState);
            caster.cardsPlayedThisTurn.Add(card);
            caster.hand.Remove(card);
            caster.discardPile.Add(card);
        }

        public void ResolveCombat(PlayerState attacker, PlayerState defender)
        {
            int atk = Mathf.RoundToInt(attacker.CurrentATK * attacker.pendingATKMultiplier);
            attacker.pendingATKMultiplier = 1f;

            bool brokeDef = atk > defender.CurrentDEF;

            if (brokeDef)
            {
                int damage = atk - defender.CurrentDEF;
                defender.vesselHP -= damage;

                int soulsStolen = defender.souls > 0 ? 1 : 0;
                defender.souls = Mathf.Max(0, defender.souls - soulsStolen);

                if (!attacker.IsSoullocked)
                    attacker.souls += soulsStolen;

                Debug.Log($"{attacker.playerName} breaks {defender.playerName}'s DEF for {damage} damage" +
                    (soulsStolen > 0 ? $", stealing {soulsStolen} soul(s)" : "") +
                    (attacker.IsSoullocked ? " (soul gain locked — corrupted)" : "") + ".");

                defender.CheckDesperation();
                attacker.CheckAwakening();
            }
            else
            {
                Debug.Log($"{attacker.playerName} fails to break {defender.playerName}'s DEF.");
                if (defender.pendingCorruptOnDefBreakFail > 0)
                {
                    attacker.TakeCorruption(defender.pendingCorruptOnDefBreakFail);
                    defender.pendingCorruptOnDefBreakFail = 0;
                }
            }

            if (defender.vesselHP <= 0)
                defender.isDefeated = true;
        }

        public void EndTurn(PlayerState player)
        {
            player.cardsPlayedThisTurn.Clear();
        }

        public void EndRound()
        {
            battleState.AdvanceRound();
            foreach (var p in battleState.players)
                p.isFrozen = false; 
        }

        public void RunTurn()
        {
            var current = battleState.GetCurrentTurnPlayer();
            Debug.Log($"=== {current.playerName}'s turn (Round {battleState.currentRound}) ===");

            if (current.isFrozen)
            {
                Debug.Log($"{current.playerName} is frozen and skips this turn.");
            }
            else
            {
                current.Draw();

                if (current.hand.Count > 0)
                {
                    var card = current.hand[0];
                    PlayCard(current, card);

                    foreach (var opp in battleState.GetOpponentsOf(current))
                    {
                        if (opp.isDefeated) continue;
                        ResolveCombat(current, opp);
                    }
                }
                else
                {
                    Debug.Log($"{current.playerName} has no cards left to play.");
                }
            }

            EndTurn(current);
            int previousIndex = battleState.currentTurnIndex;
            battleState.AdvanceTurn();
            if (battleState.currentTurnIndex <= previousIndex)
            {
                battleState.AdvanceRound();
            }

            if (battleState.IsGameOver(out var winner))
            {
                Debug.Log($"=== GAME OVER — {winner?.playerName ?? "no one"} wins ===");
            }
        }
    }
}