using UnityEngine;

namespace SoulBind
{
    public static class CardEffectExecutor
    {
        public static void Execute(Card card, PlayerState caster, BattleState battle)
        {
            var p = card.effectParams;

            switch (card.effectType)
            {
                case CardEffectType.None:
                    break;

                case CardEffectType.ReduceCorruptionSelfDefPenalty:
                    caster.TakeCorruption(-p.amount);
                    caster.AddModifier(StatType.DEF, -p.secondaryAmount, 1);
                    break;

                case CardEffectType.SelfBuffNextRound:
                    caster.AddModifier(StatType.ATK, p.amount, 1);
                    caster.AddModifier(StatType.DEF, p.secondaryAmount, 1);
                    break;

                case CardEffectType.CorruptAllOpponents:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                        opp.TakeCorruption(p.amount);
                    break;

                case CardEffectType.SkipAndBuffNext:
                    caster.AddModifier(StatType.ATK, p.amount, 2);
                    caster.AddModifier(StatType.DEF, p.secondaryAmount, 2);
                    break;

                case CardEffectType.FreezeOpponents:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                        opp.isFrozen = true;
                    break; 

                case CardEffectType.PhaseATK:
                    int bonus = battle.currentPhase == 1 ? p.amount
                              : battle.currentPhase <= 3 ? p.secondaryAmount
                              : p.threshold;
                    caster.AddModifier(StatType.ATK, bonus, 1);
                    break;

                case CardEffectType.CorruptionReduceCombo:
                    caster.TakeCorruption(-p.amount);
                    break;

                case CardEffectType.CapDebuffOpponents:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                        opp.AddModifier(StatType.CAP, -p.amount, 1);
                    break;

                case CardEffectType.HealAndTempDef: 
                    caster.Heal(p.amount);
                    caster.AddModifier(StatType.DEF, p.secondaryAmount, p.rounds);
                    break;

                case CardEffectType.DrawExtra:
                    for (int i = 0; i < p.amount; i++) caster.Draw();
                    break;

                case CardEffectType.CapBuffRounds:
                    caster.AddModifier(StatType.CAP, p.amount, p.rounds);
                    break;

                case CardEffectType.ConditionalLowestVesselATK:
                    if (IsLowestHP(caster, battle))
                        caster.AddModifier(StatType.ATK, p.amount, p.rounds);
                    break;

                case CardEffectType.ExecuteIfLowVessel:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                        if (opp.vesselHP < p.threshold)
                            opp.isDefeated = true;
                    break;

                case CardEffectType.CorruptOpponentAndSelf:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                        opp.TakeCorruption(p.amount);
                    caster.TakeCorruption(p.secondaryAmount);
                    break;

                case CardEffectType.FlatATKBonus:
                    caster.AddModifier(StatType.ATK, p.amount, 1);
                    break;

                case CardEffectType.HealAndReduceCorruption:
                    caster.Heal(p.amount);
                    caster.TakeCorruption(-p.secondaryAmount);
                    break;

                case CardEffectType.DefBuffHeal:
                    caster.AddModifier(StatType.DEF, p.amount, 1);
                    caster.Heal(p.secondaryAmount);
                    break;

                case CardEffectType.Swap: 
                case CardEffectType.ConditionalCorruptionMultiplyATK:
                    foreach (var opp in battle.GetOpponentsOf(caster))
                    {
                        if (opp.corruption > caster.corruption)
                        {
                            caster.pendingATKMultiplier = 2f;
                            break;
                        }
                    }
                    break;
                case CardEffectType.ConditionalDefBreakCorrupt:
                    caster.pendingCorruptOnDefBreakFail = p.amount;
                    break;
                case CardEffectType.PairBonus: 
                    Debug.LogWarning($"{card.cardName}: effect not implemented yet ({card.effectType})");
                    break;
            }
        }

        private static bool IsLowestHP(PlayerState self, BattleState battle)
        {
            foreach (var p in battle.players)
                if (p != self && p.vesselHP <= self.vesselHP)
                    return false;
            return true;
        }
    }
}