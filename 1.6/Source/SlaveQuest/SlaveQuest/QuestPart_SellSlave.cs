using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static System.Collections.Specialized.BitVector32;

namespace SlaveQuest
{
    public class QuestPart_SellSlave : QuestPart
    {
        public override bool RequiresAccepter => true;

        public string inSignal;
        public string outSignal;
        public string askerName;
        public Faction askerFaction;
        public Pawn pawn;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (inSignal.NullOrEmpty()) { return; }

            if (signal.tag == inSignal)
            {
                pawn = quest.AccepterPawn;
                if (pawn == null) { signal.args.TryGetArg("CHOSEN", out pawn); }

                if (pawn != null && pawn.Spawned)
                {
                    pawn.SetFaction(askerFaction);
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.ExitMapBest);
                    pawn.jobs.StopAll();

                    if (outSignal.NullOrEmpty()) { Log.Message("Missing QuestPart_SellSlave outSignal!"); return; }

                    if (pawn.questTags == null) { pawn.questTags = new List<string>() { outSignal }; }
                    else { pawn.questTags.Add(outSignal); }
                }
            }

            if (signal.tag == outSignal + ".Arrested")
            {
                Find.LetterStack.ReceiveLetter("LetterQuestFailedLabel".Translate(), "LetterQuestCompletedFail".Translate(quest.name) + "\n"  + "SlaveQuest.UI.TargetArrested".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, lookTargets: pawn, quest: quest, playSound: true);
                quest.End(QuestEndOutcome.Fail, false, false);
            }
            else if (signal.tag == outSignal + ".Killed") 
            { 
                Find.LetterStack.ReceiveLetter("LetterQuestFailedLabel".Translate(), "LetterQuestCompletedFail".Translate(quest.name) + "\n" + "SlaveQuest.UI.TargetKilled".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, lookTargets: pawn, quest: quest, playSound: true);
                quest.End(QuestEndOutcome.Fail, false, false);
            }
        }

        public void InitSetting(string signalInput)
        {
            outSignal = signalInput;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref outSignal, "outSignal");
            Scribe_Values.Look(ref askerName, "askerName");
            Scribe_References.Look(ref askerFaction, "askerFaction");
            Scribe_References.Look(ref pawn, "pawn");
        }
    }
}
