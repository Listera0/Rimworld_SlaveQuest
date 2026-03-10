using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SlaveQuest
{
    public class QuestPart_ArrivedLocation : QuestPartActivable
    {
        public List<Pawn> collectors = new List<Pawn>();
        public IntVec3 targetCell;
        public string outSignal;

        public override void QuestPartTick()
        {
            base.QuestPartTick();

            if (State != QuestPartState.Enabled) { return; }
            if (Find.TickManager.TicksGame % 250 != 0) { return; }
            if (collectors.NullOrEmpty()) { return; }

            foreach (Pawn pawn in collectors)
            {
                if (pawn.Spawned && !pawn.Dead && pawn.Position.InHorDistOf(targetCell, 5.0f))
                {
                    Find.SignalManager.SendSignal(new Signal(outSignal));
                    Complete();
                    break;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref collectors, "collectors", LookMode.Reference);
            Scribe_Values.Look(ref targetCell, "targetCell");
            Scribe_Values.Look(ref outSignal, "outSignal");
        }
    }
}
