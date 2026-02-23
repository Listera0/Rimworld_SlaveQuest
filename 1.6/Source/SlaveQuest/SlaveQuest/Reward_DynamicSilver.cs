using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using static System.Collections.Specialized.BitVector32;

namespace SlaveQuest
{
    public class Reward_DynamicSilver : Reward
    {
        public int estimatedPrice = 1000;
        public string signals_sellslave;
        public Faction faction_sellslave_askerfaction;

        public override bool MakesUseOfChosenPawnSignal => true;

        public override IEnumerable<GenUI.AnonymousStackElement> StackElements
        {
            get
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = estimatedPrice;

                foreach (var element in QuestPartUtility.GetRewardStackElementsForThings(new List<Thing> { silver }))
                {
                    yield return element;
                }
            }
        }

        public override void Notify_Used()
        {
            base.Notify_Used();
        }

        public override IEnumerable<QuestPart> GenerateQuestParts(int index, RewardsGeneratorParams parms, string customLetterLabel, string customLetterText, RulePack customLetterLabelRules, RulePack customLetterTextRules)
        {
            QuestPart_SellSlave questPart_SellSlave = new QuestPart_SellSlave();
            if (!parms.chosenPawnSignal.NullOrEmpty())
            {
                if (signals_sellslave.NullOrEmpty()) { Log.Message("Missing Reward_DynamicSilver passing signals!"); Log.Message(signals_sellslave); }
                questPart_SellSlave.InitSetting(signals_sellslave);
                questPart_SellSlave.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(parms.chosenPawnSignal);
                questPart_SellSlave.askerFaction = faction_sellslave_askerfaction;
                questPart_SellSlave.signalListenMode = QuestPart.SignalListenMode.OngoingOnly;
            }

            QuestPart_RequirementsToAcceptSlaveQuest questPart_requireMent = new QuestPart_RequirementsToAcceptSlaveQuest();
            questPart_requireMent.requireOptions = QuestGen.slate.Get<RequireOptions>("RequireOptions");

            yield return questPart_SellSlave;
            yield return questPart_requireMent;
        }

        public override string GetDescription(RewardsGeneratorParams parms)
        {
            Pawn chosenPawn = QuestGen.slate.Get<Pawn>("chosenPawn");

            if (chosenPawn != null)
            {
                return $"Sell {chosenPawn.LabelShort}";
            }

            return $"Sell 'Error'";
        }

        public override void InitFromValue(float rewardValue, RewardsGeneratorParams parms, out float valueActuallyUsed)
        {
            estimatedPrice = (int)rewardValue;
            valueActuallyUsed = estimatedPrice;
        }

        public void InitSetting(Faction faction, string pass_signal)
        {
            faction_sellslave_askerfaction = faction;
            signals_sellslave = pass_signal;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref estimatedPrice, "estimatedPrice");
            Scribe_Values.Look(ref signals_sellslave, "signals_sellslave");
            Scribe_References.Look(ref faction_sellslave_askerfaction, "faction_sellslave_askerfaction");
        }
    }
}