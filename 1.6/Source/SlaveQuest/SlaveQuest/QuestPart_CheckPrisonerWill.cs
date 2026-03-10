using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SlaveQuest
{
    public class QuestPart_CheckPrisonerWill : QuestPartActivable
    {
        public List<Pawn> pawns = new List<Pawn>();
        public string outSignal;

        public override void QuestPartTick()
        {
            base.QuestPartTick();

            if (State != QuestPartState.Enabled) { return; }
            if (Find.TickManager.TicksGame % 250 != 0) { return; }
            if (pawns.NullOrEmpty()) { return; }

            foreach (Pawn pawn in pawns)
            {
                if (pawn != null && pawn.guest != null && pawn.guest.will > 0) { return; }
            }

            Find.SignalManager.SendSignal(new Signal(outSignal));
            Complete();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
            Scribe_Values.Look(ref outSignal, "outSignal");
        }
    }
}
