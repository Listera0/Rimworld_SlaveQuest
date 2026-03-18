using RimWorld;
using RimWorld.QuestGen;
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
    public class QuestNode_SlaveQuest_BreakWill : QuestNode
    {
        public int challengeRating = 1;
        public float challengeValue = 1.0f;
        public float questDuration = 10;

        protected override bool TestRunInt(Slate slate)
        {
            if (!ModsConfig.IdeologyActive && !CheckAnyoneCanBreakWill()) { return false; }
            if (Rand.Range(0, 101) > (int)(SlaveQuest_Config.QuestGenerateRate_BreakWill * 20)) { return false; }

            return true;
        }

        protected override void RunInt()
        {
            // ForTest ->> GeneratePrisonerPawn.requireWill -> 0.2f

            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map = QuestGen_Get.GetMap();

            // <-- Generate Quest Require -->
            GenerateQuestChallengeRating(out challengeRating, out challengeValue);
            GenerateWillValue(challengeRating, out float requireWill);
            GenerateAskerPawn(quest, out Pawn asker);
            GeneratePrisonerPawn(quest, asker, requireWill, out List <Pawn> prisoners);
            GenerateCollectorGroup(quest, map, asker, out List<Pawn> collectors);

            // <-- Slate Part -->
            slate.Set("map", map);
            slate.Set("asker", asker);
            slate.Set("askerFaction", asker.Faction);
            slate.Set("prisoner_count", challengeRating);
            slate.Set("requireWill", requireWill);
            slate.Set("questDuration", questDuration);

            // <-- Signal Serialize -->
            string initiateSignal = quest.InitiateSignal;
            string completeChangeWillSignal = QuestGen.GenerateNewSignal("CompleteChangeWill");
            string collectorArriveSignal = QuestGen.GenerateNewSignal("CollectorArrive");
            string collectorArriveStartSignal = QuestGen.GenerateNewSignal("CollectorArriveStart");
            string collectorArriveFinishSignal = QuestGen.GenerateNewSignal("CollectorArriveFinish");
            string collectorLeaveSIgnal = QuestGen.GenerateNewSignal("CollectorLeaveStart");
            string prisonerSignal = QuestGen.GenerateNewSignal("PrisonerSignal");
            string questSuccessSignal = QuestGen.GenerateNewSignal("QuestSuccess");
            string timeOutSignal = QuestGen.GenerateNewSignal("QuestTimeout");
            string mapRemovedSignal = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
            string askerHostileSignal = "askerFaction.BecameHostileToPlayer";
            string questFailWithPanelty = QuestGen.GenerateNewSignal("QuestFailWithPanelty");

            // <-- QuestPart_PrisonerArrive -->
            QuestPart_PawnsArrive questPart_PrisonerArive = new QuestPart_PawnsArrive();
            questPart_PrisonerArive.inSignal = initiateSignal;
            questPart_PrisonerArive.pawns = prisoners;
            questPart_PrisonerArive.arrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            questPart_PrisonerArive.mapParent = map.Parent;
            questPart_PrisonerArive.customLetterLabel = "SlaveQuest.UI.Label.PrisonerArrive".Translate();
            questPart_PrisonerArive.customLetterText = "SlaveQuest.UI.Text.PrisonerArrive".Translate(asker.Faction);
            quest.AddPart(questPart_PrisonerArive);

            // <-- QuestPart_PrisonerCheck -->
            QuestPart_PrisonerArrive questPart_PrisonerCheck = new QuestPart_PrisonerArrive();
            questPart_PrisonerCheck.inSignalEnable = initiateSignal;
            questPart_PrisonerCheck.inSignalDisable = collectorArriveFinishSignal;
            questPart_PrisonerCheck.outSignal = prisonerSignal;
            questPart_PrisonerCheck.prisoners = prisoners;
            quest.AddPart(questPart_PrisonerCheck);

            // <-- QuestPart_CheckPrisonerWill -->
            QuestPart_CheckPrisonerWill questPart_CheckPrisonerWill = new QuestPart_CheckPrisonerWill();
            questPart_CheckPrisonerWill.inSignalEnable = initiateSignal;
            questPart_CheckPrisonerWill.outSignal = completeChangeWillSignal;
            questPart_CheckPrisonerWill.pawns = prisoners;
            quest.AddPart(questPart_CheckPrisonerWill);

            // <-- QuestPart_CollectorArive -->
            QuestPart_PawnsArrive questPart_CollectorArrive = new QuestPart_PawnsArrive();
            questPart_CollectorArrive.inSignal = completeChangeWillSignal;
            questPart_CollectorArrive.pawns = collectors;
            questPart_CollectorArrive.arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            questPart_CollectorArrive.mapParent = map.Parent;
            questPart_CollectorArrive.customLetterLabel = "SlaveQuest.UI.Label.CollectorArrive".Translate();
            questPart_CollectorArrive.customLetterText = "SlaveQuest.UI.Text.CollectorArrive".Translate(asker.Faction);
            quest.AddPart(questPart_CollectorArrive);
            
            // <-- QuestPart_CollectorJob -->
            QuestPart_CollectorLord questPart_CollectorLordJob = new QuestPart_CollectorLord();
            questPart_CollectorLordJob.inSignal = completeChangeWillSignal;
            questPart_CollectorLordJob.inSignal_Pass = collectorArriveStartSignal;
            questPart_CollectorLordJob.outSignal_Pass = collectorArriveFinishSignal;
            questPart_CollectorLordJob.collectors = collectors;
            quest.AddPart(questPart_CollectorLordJob);

            // <-- QuestPart_CollectSlave -->
            QuestPart_CollectSlave questPart_CollectSlave = new QuestPart_CollectSlave();
            questPart_CollectSlave.inSignal = collectorArriveFinishSignal;
            questPart_CollectSlave.outSignal = prisonerSignal;
            questPart_CollectSlave.successSignal = questSuccessSignal;
            questPart_CollectSlave.collectors = collectors;
            questPart_CollectSlave.prisoners = prisoners;
            quest.AddPart(questPart_CollectSlave);

            // <-- QuestPart_DelayWaitCollectSlave -->
            QuestPart_Delay questPart_Delay = new QuestPart_Delay();
            questPart_Delay.delayTicks = 5000;
            questPart_Delay.inSignalEnable = collectorArriveFinishSignal;
            questPart_Delay.outSignalsCompleted.Add(collectorLeaveSIgnal);
            quest.AddPart(questPart_Delay);

            // <-- QuestPart_LeaveAllPeople -->
            QuestPart_Leave questPart_Leave = new QuestPart_Leave();
            questPart_Leave.inSignal = collectorLeaveSIgnal;
            questPart_Leave.pawns.AddRange(prisoners);
            questPart_Leave.pawns.AddRange(collectors);
            questPart_Leave.wakeUp = true;
            quest.AddPart(questPart_Leave);

            // <-- QuestPart_GiveRewards-->
            RewardsGeneratorParams rewardsGeneratorParams = new RewardsGeneratorParams();
            rewardsGeneratorParams.rewardValue = slate.Get<float>("points") * challengeValue;
            rewardsGeneratorParams.giverFaction = asker.Faction;
            rewardsGeneratorParams.allowRoyalFavor = ModsConfig.RoyaltyActive;
            rewardsGeneratorParams.allowGoodwill = true;
            quest.GiveRewards(rewardsGeneratorParams, inSignal: questSuccessSignal, asker: asker);

            // <-- QuestPart_Timeout -->
            QuestPart_Delay questPart_Timeout = new QuestPart_Delay();
            questPart_Timeout.delayTicks = (int)(questDuration * 60000);
            questPart_Timeout.isBad = true;
            questPart_Timeout.inSignalEnable = initiateSignal;
            questPart_Timeout.expiryInfoPart = "QuestExpiresIn".Translate();
            questPart_Timeout.expiryInfoPartTip = "QuestExpiresOn".Translate();
            if (questPart_Timeout.outSignalsCompleted.NullOrEmpty()) { questPart_Timeout.outSignalsCompleted = new List<string>() { timeOutSignal }; }
            else { questPart_Timeout.outSignalsCompleted.Add(timeOutSignal); }
            quest.AddPart(questPart_Timeout);

            // <-- Quest Link Part -->
            quest.challengeRating = challengeRating;
            quest.Letter(LetterDefOf.NeutralEvent, mapRemovedSignal, text: "SlaveQuest.UI.MapRemoved".Translate(), label: "LetterQuestFailedLabel".Translate());
            quest.End(QuestEndOutcome.Success, inSignal: questSuccessSignal, sendStandardLetter: true, playSound: true);
            quest.End(QuestEndOutcome.InvalidPreAcceptance, inSignal: askerHostileSignal, signalListenMode: QuestPart.SignalListenMode.NotYetAcceptedOnly);
            quest.End(QuestEndOutcome.Fail, inSignal: mapRemovedSignal, playSound: true);
            quest.End(QuestEndOutcome.Fail, inSignal: askerHostileSignal, sendStandardLetter: true, playSound: true);
            quest.End(QuestEndOutcome.Fail, -15, asker.Faction, inSignal: timeOutSignal, sendStandardLetter: true, playSound: true);
            quest.End(QuestEndOutcome.Fail, -30, asker.Faction, questFailWithPanelty, sendStandardLetter: true, playSound: true);
        }

        public bool CheckAnyoneCanBreakWill()
        {
            List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonistsSpawned;
            foreach(Pawn pawn in pawns)
            {
                if (pawn.Ideo.IdeoApprovesOfSlavery()) { return true; }
            }

            return false;
        }

        public void GenerateQuestChallengeRating(out int rate, out float value)
        {
            float wealth = Find.CurrentMap.PlayerWealthForStoryteller;
            int pointValue = Rand.Range(0, 30 + (int)Math.Min(wealth / 2500, 70));

            rate = pointValue > 80 ? 3 : pointValue > 50 ? 2 : 1;
            value = rate == 3 ? 2.5f : rate == 2 ? 1.5f : 1.0f;
        }

        public void GenerateWillValue(int challenge, out float value)
        {
            // 8 ~ 10 | 10 ~ 12 | 14 ~ 20
            if (challenge == 1) { value = Rand.Range(8, 11); }
            else if (challenge == 2) { value = Rand.Range(10, 13); }
            else { value = Rand.Range(14, 21); }

            questDuration = value + (challengeRating == 3 ? 5 : (challengeRating == 2) ? 4 : 3);
        }

        public void GenerateAskerPawn(Quest quest, out Pawn pawn)
        {
            pawn = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
            {
                mustBeFactionLeader = true,
                mustBeNonHostileToPlayer = true,
                allowPermanentEnemyFaction = false,
                minTechLevel = TechLevel.Industrial
            });
        }

        public void GeneratePrisonerPawn(Quest quest, Pawn asker, float requireWill, out List<Pawn> pawns)
        {
            pawns = new List<Pawn>();
            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, faction: asker.Faction, forceGenerateNewPawn: true);

            for (int i = 0; i < challengeRating; i++)
            {
                Pawn newPawn = PawnGenerator.GeneratePawn(request);
                newPawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);
                newPawn.guest.SetExclusiveInteraction(PrisonerInteractionModeDefOf.ReduceWill);
                newPawn.guest.will = requireWill;
                //newPawn.guest.will = 0.2f;
                newPawn.guest.Recruitable = !Find.Storyteller.difficulty.unwaveringPrisoners;
                pawns.Add(newPawn);
            }
        }

        public void GenerateCollectorGroup(Quest quest, Map map, Pawn asker, out List<Pawn> pawns)
        {
            PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
            pawnGroupMakerParms.faction = asker.Faction;
            pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
            pawnGroupMakerParms.points = 500;
            pawnGroupMakerParms.tile = map.Tile;
            pawns = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
        }
    }
}
