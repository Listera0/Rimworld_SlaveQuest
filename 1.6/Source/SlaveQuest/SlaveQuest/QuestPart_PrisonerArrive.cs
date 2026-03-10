using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI.Group;
using Verse;
using Verse.AI;
using RimWorld.QuestGen;

namespace SlaveQuest
{
    public class QuestPart_PrisonerArrive : QuestPart
    {
        public string inSignalEnable;
        public string inSignalDisable;
        public string outSignal;
        public string failSignal;

        public bool enable;

        public List<Pawn> prisoners;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (inSignalEnable.NullOrEmpty() || inSignalDisable.NullOrEmpty() || outSignal.NullOrEmpty()) { return; }
            if (signal.tag == inSignalEnable) { enable = true; }
            if (signal.tag == inSignalDisable) { enable = false; }
            if (!enable) { return; }

            if (signal.tag == inSignalEnable)
            {
                foreach (Pawn pawn in prisoners)
                {
                    if (pawn.questTags == null) { pawn.questTags = new List<string>() { outSignal }; }
                    else { if (!pawn.questTags.Contains(outSignal)) { pawn.questTags.Add(outSignal); } }
                }
            }

            if (signal.tag == outSignal + ".LeftMap" || signal.tag == outSignal + ".Killed")
            {
                Find.SignalManager.SendSignal(new Signal(failSignal));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignalEnable, "inSignalEnable");
            Scribe_Values.Look(ref inSignalDisable, "inSignalDisable");
            Scribe_Values.Look(ref outSignal, "outSignal");
            Scribe_Values.Look(ref failSignal, "failSignal");
            Scribe_Values.Look(ref enable, "enable");
            Scribe_Collections.Look(ref prisoners, "prisoners", LookMode.Reference);
        }
    }
}
