using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI.Group;
using Verse.Noise;

namespace SlaveQuest
{
    public class QuestPart_CollectorLord : QuestPart
    {
        public string inSignal;
        public string inSignal_Pass;
        public string outSignal_Pass;
        public List<Pawn> collectors;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (inSignal.NullOrEmpty()) { return; }
            
            if (signal.tag == inSignal)
            {
                IntVec3 targetCell = DropCellFinder.TradeDropSpot(collectors[0].Map);
                LordJob_DefendPoint lordJob_VisitColony = new LordJob_DefendPoint(targetCell, 4, 4);
                Lord visitLoard = LordMaker.MakeNewLord(collectors[0].Faction, lordJob_VisitColony, collectors[0].Map, collectors);

                QuestPart_ArrivedLocation questPart_ArrivedLocation = new QuestPart_ArrivedLocation();
                questPart_ArrivedLocation.inSignalEnable = inSignal_Pass;
                questPart_ArrivedLocation.outSignal = outSignal_Pass;
                questPart_ArrivedLocation.collectors = collectors;
                questPart_ArrivedLocation.targetCell = targetCell;
                quest.AddPart(questPart_ArrivedLocation);

                Find.SignalManager.SendSignal(new Signal(inSignal_Pass));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Values.Look(ref inSignal, "inSignal_Pass");
            Scribe_Values.Look(ref inSignal, "outSignal_Pass");
            Scribe_Collections.Look(ref collectors, "collectors", LookMode.Reference);
        }
    }
}
