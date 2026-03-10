using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using Verse.Noise;

namespace SlaveQuest
{
    public class QuestPart_CollectSlave : QuestPart
    {
        public string inSignal;
        public string outSignal;
        public string successSignal;

        public List<Pawn> collectors;
        public List<Pawn> prisoners;

        public int leftCount = 0;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (inSignal.NullOrEmpty()) { return; }

            if(signal.tag == inSignal)
            {
                if (collectors[0].TryGetLord(out Lord oldLord)) 
                {
                    foreach (Pawn pawn in prisoners)
                    {
                        pawn.SetFaction(collectors[0].Faction);
                        pawn.guest.SetGuestStatus(null);
                        oldLord.AddPawn(pawn);
                    }
                }
            }
            
            if(signal.tag == outSignal + ".LeftMap")
            {
                leftCount++;
                if(leftCount >= prisoners.Count) { Find.SignalManager.SendSignal(new Signal(successSignal)); }
            }
            else if (signal.tag == outSignal + ".Arrested")
            {
                //Find.LetterStack.ReceiveLetter("LetterQuestFailedLabel".Translate(), "LetterQuestCompletedFail".Translate(quest.name) + "\n" + "SlaveQuest.UI.TargetArrested".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, lookTargets: pawn, quest: quest, playSound: true);
                quest.End(QuestEndOutcome.Fail, false, false);
            }
            else if(signal.tag == outSignal + ".Killed")
            {
                //Find.LetterStack.ReceiveLetter("LetterQuestFailedLabel".Translate(), "LetterQuestCompletedFail".Translate(quest.name) + "\n" + "SlaveQuest.UI.TargetKilled".Translate(pawn.LabelShort), LetterDefOf.NegativeEvent, lookTargets: pawn, quest: quest, playSound: true);
                quest.End(QuestEndOutcome.Fail, false, false);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref outSignal, "outSignal");
            Scribe_Values.Look(ref successSignal, "successSignal");
            Scribe_Collections.Look(ref collectors, "collectors", LookMode.Reference);
            Scribe_Collections.Look(ref prisoners, "prisoners", LookMode.Reference);
            Scribe_Values.Look(ref leftCount, "leftCount");
        }
    }
}
