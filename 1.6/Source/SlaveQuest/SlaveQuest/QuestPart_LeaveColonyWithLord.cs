using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace SlaveQuest
{
    public class QuestPart_LeaveColonyWithLord : QuestPart
    {
        public string inSignal;
        public List<Pawn> pawns = new List<Pawn>();

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);

            if (pawns[0].TryGetLord(out Lord oldLord))
            {
                oldLord.Map.lordManager.RemoveLord(oldLord);

                LordJob_ExitMapBest exitJob = new LordJob_ExitMapBest(LocomotionUrgency.Walk, true, true);
                Lord recoveryLord = LordMaker.MakeNewLord(pawns[0].Faction, exitJob, pawns[0].Map, pawns);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        }
    }
}
