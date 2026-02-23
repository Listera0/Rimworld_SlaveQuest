using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace SlaveQuest
{
    public class QuestPart_DropRewards : QuestPart
    {
        public string inSignal;
        public string outSignal;

        public int challengeRating;
        public CustomValues customValues;
        public RequireOptions requireOptions;
        public string askerName;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == inSignal + ".LeftMap")
            {
                Map map = QuestGen_Get.GetMap();
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = GetSlavePrice();

                if (map.listerBuildings.allBuildingsColonist.Where(x => x.def == ThingDefOf.OrbitalTradeBeacon).Any())
                {
                    DropPodUtility.DropThingsNear(DropCellFinder.TradeDropSpot(map), map, new List<Thing> { silver });
                }
                else
                {
                    DropPodUtility.DropThingsNear(DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, new IntVec2(2, 2)), map, new List<Thing> { silver });
                }

                if (outSignal.NullOrEmpty()) { Log.Message("Missing QuestPart_DropRewards outSignal!"); return; }

                Find.LetterStack.ReceiveLetter("LetterQuestCompletedLabel".Translate(), "SlaveQuest.UI.SuccessQuest".Translate(askerName, silver.Label), LetterDefOf.PositiveEvent, lookTargets: silver, quest: quest, playSound: true );
                quest?.Notify_SignalReceived(new Signal(outSignal));
            }
        }

        public void InitSetting(int challengeRate, CustomValues customValue, RequireOptions requireOption, string name)
        {
            challengeRating = challengeRate;
            customValues = customValue;
            requireOptions = requireOption;
            askerName = name;
        }

        public int GetSlavePrice()
        {
            if (quest.AccepterPawn == null) return 1;
            Pawn pawn = quest.AccepterPawn;

            float finalValue = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.MarketValueIgnoreHp);

            CustomTraitValue customTraitValue = customValues.GetTraitValue();
            foreach (Trait trait in pawn.story.traits.allTraits.Where(x => !requireOptions.traits.Contains(x)))
            {
                foreach (TraitValue value in customTraitValue.traitPrices)
                {
                    if (value.traitDef == trait.def.defName && value.degree == trait.Degree)
                    {
                        finalValue += value.value * (requireOptions.traits.Contains(trait) ? 2 : 1);
                        break;
                    }
                }
            }

            CustomSkillValue customSkillValue = customValues.GetSkillValue();
            foreach (SQ_Skill skill in requireOptions.skills)
            {
                foreach (SkillValue value in customSkillValue.skillPrices)
                {
                    if (value.skillDef == skill.skillDef.defName)
                    {
                        finalValue += value.value * 10;
                    }
                }
            }

            foreach (SkillRecord skill in pawn.skills.skills)
            {
                foreach (SkillValue value in customSkillValue.skillPrices)
                {
                    if (value.skillDef == skill.def.defName && value.standard < skill.Level)
                    {
                        finalValue += (skill.Level - value.standard) * value.value;
                    }
                }
            }

            float HPFactor = pawn.MarketValue / pawn.GetStatValue(StatDefOf.MarketValueIgnoreHp);
            finalValue *= HPFactor;

            float challengeRatingValue = challengeRating == 3 ? 1.25f : challengeRating == 2 ? 1.1f : 1.0f;
            return (int)(finalValue * challengeRatingValue);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref outSignal, "outSignal");
            Scribe_Values.Look(ref challengeRating, "challengeRating");
            Scribe_Deep.Look(ref customValues, "customValues");
            Scribe_Deep.Look(ref requireOptions, "requireOptions");
            Scribe_Values.Look(ref askerName, "askerName");
        }
    }
}
