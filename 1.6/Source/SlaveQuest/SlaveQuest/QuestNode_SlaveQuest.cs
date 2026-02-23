using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using Verse;
using Verse.Noise;


namespace SlaveQuest
{
    public class SQ_Skill : IExposable
    {
        public SkillDef skillDef;
        public int value;

        public SQ_Skill() { }

        public SQ_Skill(SkillDef def, int val)
        {
            skillDef = def; value = val;
        }
        public void ExposeData()
        {
            Scribe_Defs.Look(ref skillDef, "skillDef");
            Scribe_Values.Look(ref value, "value");
        }
    }

    public class CustomValues : IExposable
    {
        public List<CustomSlaveCategory> customSlaveCategories = new List<CustomSlaveCategory>();
        public List<CustomTraitValue> customTraitValues = new List<CustomTraitValue>();
        public List<CustomSkillValue> customSkillValues = new List<CustomSkillValue>();
        public List<CustomXenotypeWeight> customXenotypeWeights = new List<CustomXenotypeWeight>();
        public List<CustomAgeWeight> customAgeWeights = new List<CustomAgeWeight>();
        public CustomSlaveCategory category;

        public CustomValues()
        {
            customSlaveCategories = DefDatabase<CustomSlaveCategory>.AllDefsListForReading.ToList();
            customTraitValues = DefDatabase<CustomTraitValue>.AllDefsListForReading.ToList();
            customSkillValues = DefDatabase<CustomSkillValue>.AllDefsListForReading.ToList();
            customXenotypeWeights = DefDatabase<CustomXenotypeWeight>.AllDefsListForReading.ToList();
            customAgeWeights = DefDatabase<CustomAgeWeight>.AllDefsListForReading.ToList();
            SelectSlaveCategory();
        }

        public CustomTraitValue GetTraitValue() { return customTraitValues.Where(x => x.role == category.defName).ToList().First(); }
        public CustomSkillValue GetSkillValue() { return customSkillValues.Where(x => x.role == category.defName).ToList().First(); }

        public void SelectSlaveCategory()
        {
            int select = Rand.Range(0, customSlaveCategories.Count);
            category = customSlaveCategories[select];
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref customSlaveCategories, "customSlaveCategories", LookMode.Def);
            Scribe_Collections.Look(ref customTraitValues, "customTraitValues", LookMode.Def);
            Scribe_Collections.Look(ref customSkillValues, "customSkillValues", LookMode.Def);
            Scribe_Collections.Look(ref customXenotypeWeights, "customXenotypeWeights", LookMode.Def);
            Scribe_Collections.Look(ref customAgeWeights, "customAgeWeights", LookMode.Def);
            Scribe_Defs.Look(ref category, "category");
        }
    }

    public class RequireOptions : IExposable
    {
        public List<Trait> traits;
        public List<SQ_Skill> skills;
        public XenotypeDef xenotype;
        public LifeStageDef lifeStage;

        public RequireOptions()
        {
            traits = new List<Trait>();
            skills = new List<SQ_Skill>();
            xenotype = null;
            lifeStage = null;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref traits, "traits", LookMode.Deep);
            Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
            Scribe_Defs.Look(ref xenotype, "xenotype");
            Scribe_Defs.Look(ref lifeStage, "lifeStageAge");
        }
    }

    public class QuestNode_SlaveQuest : QuestNode
    {
        public RequireOptions requireOptions;
        public CustomValues customValues;
        public int challengeRating = 1;
        public int estimatedPrice = 1;

        protected override bool TestRunInt(Slate slate)
        {
            // GenerateValue
            if (Rand.Range(0, 100) >= (int)(SlaveQuest_Config.QuestGenerateRate * 20)) { return false; }

            return true;
        }

        protected override void RunInt()
        {
            Quest quest = QuestGen.quest;
            Slate slate = QuestGen.slate;
            Map map = QuestGen_Get.GetMap();
            requireOptions = new RequireOptions();
            customValues = new CustomValues();
            challengeRating = GenerateQuestChallengeRating(slate.Get<float>("points"));

            Pawn asker = GenerateAskerPawn(quest);

            // <-- Generate Quest Require -->
            GenerateRandomXenotype();
            GenerateRandomRequireOption();
            GenerateSlaveEstimatedPrice();

            // <-- Slate Part -->
            slate.Set("asker", asker);
            slate.Set("askerFaction", asker.Faction);
            slate.Set("quest_requires", GenerateQuestRequireSlate());
            slate.Set("slave_category_def", customValues.category.label);

            // <-- Signal Serialize -->
            string initiateSignal = quest.InitiateSignal;
            string successSignal = QuestGen.GenerateNewSignal("SuccessQuest");
            string finishSignal = QuestGen.GenerateNewSignal("FinishQuest");
            string mapRemovedSignal = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
            string askerHostileSignal = "askerFaction.BecameHostileToPlayer";

            // <-- QuestPart_Choice -->
            QuestPart_Choice questChoice = new QuestPart_Choice();
            QuestPart_Choice.Choice option1 = new QuestPart_Choice.Choice();
            questChoice.inSignalChoiceUsed = quest.InitiateSignal;
            questChoice.choices.Add(option1);

            // <-- QuestPart_Choice Option1 -->
            Reward_DynamicSilver rewardDynamicSilver = new Reward_DynamicSilver();
            RewardsGeneratorParams generatorParams = new RewardsGeneratorParams();
            generatorParams.chosenPawnSignal = quest.InitiateSignal;
            rewardDynamicSilver.InitFromValue(estimatedPrice, generatorParams, out _);
            rewardDynamicSilver.InitSetting(asker.Faction, successSignal);
            LinkRewardQuestPartToQuest(quest, option1, rewardDynamicSilver, generatorParams);
            option1.rewards.Add(rewardDynamicSilver);

            // <-- QuestPart_DropRewards -->
            QuestPart_DropRewards questPart_DropRewards = new QuestPart_DropRewards();
            questPart_DropRewards.InitSetting(challengeRating, customValues, requireOptions, asker.LabelShort);
            questPart_DropRewards.inSignal = successSignal;
            questPart_DropRewards.outSignal = finishSignal;

            // <-- Quest Link Part -->
            quest.challengeRating = challengeRating;
            quest.AddPart(questChoice);
            quest.AddPart(questPart_DropRewards);
            quest.End(QuestEndOutcome.Success, inSignal: finishSignal);
            quest.Letter(LetterDefOf.NeutralEvent, mapRemovedSignal, text: "SlaveQuest.UI.MapRemoved".Translate(), label: "LetterQuestFailedLabel".Translate());
            quest.End(QuestEndOutcome.Fail, inSignal: mapRemovedSignal);
            quest.End(QuestEndOutcome.InvalidPreAcceptance, inSignal: askerHostileSignal, signalListenMode: QuestPart.SignalListenMode.NotYetAcceptedOnly);
            quest.End(QuestEndOutcome.Fail, inSignal: askerHostileSignal, sendStandardLetter: true, playSound: true);
        }

        public void LinkRewardQuestPartToQuest(Quest quest, QuestPart_Choice.Choice choice, Reward reward, RewardsGeneratorParams parms)
        {
            var reward_QuestParts = reward.GenerateQuestParts(0, parms, null, null, null, null);
            if (reward_QuestParts == null) return;

            foreach(QuestPart part in reward_QuestParts)
            {
                quest.AddPart(part);
                choice.questParts.Add(part);
            }
        }

        // Total Generate System
        public void GenerateRandomRequireOption()
        {
            for (int i = 0; i < challengeRating + 1; i++)
            {
                List<int> ableToSelect = new List<int>();
                for(int j = 0; j < 2; j++) 
                { 
                    if (CheckAbleToAddRequire(j)) ableToSelect.Add(j); 
                }
                if (ableToSelect.Count == 0) break;
                int select = ableToSelect.RandomElement();
                GenerateRequireOptionWithIndex(select);
            }

            QuestGen.slate.Set("RequireOptions", requireOptions);
        }

        public bool CheckAbleToAddRequire(int index)
        {
            switch (index)
            {
                case 0:
                    if (requireOptions.lifeStage == LifeStageDefOf.HumanlikeChild && requireOptions.traits.Count == 1) break;
                    if (requireOptions.traits.Count == 2) break;
                    List<TraitValue> traitCandidates = customValues.GetTraitValue().traitPrices.Where(tp => tp.value > 0 && !requireOptions.traits.Any(t => t.def.defName == tp.traitDef)).ToList();
                    if (traitCandidates.Any()) return true;
                    break;
                case 1:
                    List<SkillValue> skillCandidates = customValues.GetSkillValue().skillPrices.Where(sp => sp.value >= 10 && !requireOptions.skills.Any(sd => sd.skillDef.defName == sp.skillDef)).ToList();
                    if (skillCandidates.Any()) return true;
                    break;
            }

            return false;
        }

        public void GenerateRequireOptionWithIndex(int index)
        {
            switch (index)
            {
                case 0: GenerateRandomTraitPositive(); break;
                case 1: GenerateRandomSkillPositive(); break;
            }
        }

        public int GenerateQuestChallengeRating(float point)
        {
            // 20000 over : allow rank 2 | 50000 over : allow rank 3 & maximum rank2 | 70000 over : maximum rank3
            int pointValue = Rand.Range(0, 30 + (int)Math.Min(point / 1000, 70));

            if (pointValue < 50) return 1;
            else if (pointValue < 80) return 2;
            return 3;
        }

        public Pawn GenerateAskerPawn(Quest quest)
        {
            return quest.GetPawn(new QuestGen_Pawns.GetPawnParms
            {
                mustBeFactionLeader = true,
                mustBeNonHostileToPlayer = true,
                allowPermanentEnemyFaction = false
            });
        }

        public string GenerateQuestRequireSlate()
        {
            string result = "";
            bool first = true;

            if (ModsConfig.BiotechActive && requireOptions.xenotype != null)
            {
                if (!first) result += "\n";
                else first = false;
                result += $" - {requireOptions.xenotype.label} ({requireOptions.lifeStage.label})";
            }

            foreach (Trait trait in requireOptions.traits)
            {
                if (!first) result += "\n";
                else first = false;
                result += $" - {trait.Label}";
            }

            foreach (SQ_Skill skill in requireOptions.skills)
            {
                if (!first) result += "\n";
                else first = false;
                result += $" - {skill.skillDef.label} " + "SlaveQuest.UI.RequireSkillValue".Translate(skill.value);
            }

            return result;
        }

        // Generate Require Options
        public void GenerateRandomXenotype()
        {
            if (!ModsConfig.BiotechActive) { return; }

            List<CustomAgeWeight> ageCandidates = customValues.customAgeWeights;
            CustomAgeWeight randomSelAge = ageCandidates[0];

            if (ageCandidates.Any())
            {
                int totalWeight = ageCandidates.Sum(x => x.weight);
                int selectWeight = Rand.Range(0, totalWeight);
                int currentSum = 0;
                foreach (CustomAgeWeight candidate in ageCandidates)
                {
                    currentSum += candidate.weight;
                    if (selectWeight < currentSum)
                    {
                        randomSelAge = candidate;
                        break;
                    }
                }
            }

            List<CustomXenotypeWeight> xenoCandidates = customValues.customXenotypeWeights.Where(x => ModsConfig.OdysseyActive || x.xenotypeDef != "Starjack").ToList();
            CustomXenotypeWeight randomSelXeno = xenoCandidates[0];

            if (xenoCandidates.Any())
            {
                int totalWeight = xenoCandidates.Sum(x => x.weight);
                int selectWeight = Rand.Range(0, totalWeight);
                int currentSum = 0;
                foreach (CustomXenotypeWeight candidate in xenoCandidates)
                {
                    currentSum += candidate.weight;
                    if (selectWeight < currentSum)
                    {
                        randomSelXeno = candidate; 
                        break;
                    } 
                }
            }

            requireOptions.xenotype = DefDatabase<XenotypeDef>.GetNamed(randomSelXeno.xenotypeDef, false);
            requireOptions.lifeStage = ThingDefOf.Human.race.lifeStageAges.FirstOrDefault(x => x.def.defName == randomSelAge.ageSectionDef).def;
        }

        public void GenerateRandomTraitPositive()
        {
            List<TraitValue> candidates = customValues.GetTraitValue().traitPrices.Where(tp => tp.value > 0 && !requireOptions.traits.Any(t => t.def.defName == tp.traitDef)).ToList();
            TraitValue randomSel = customValues.GetTraitValue().traitPrices[0];

            if (candidates.Any())
            {
                randomSel = candidates.RandomElement();
            }

            Trait trait = new Trait(TraitDef.Named(randomSel.traitDef), degree: randomSel.degree);
            requireOptions.traits.Add(trait);
        }

        public void GenerateRandomSkillPositive() 
        {
            List<SkillValue> candidates = customValues.GetSkillValue().skillPrices.Where(sp => sp.value >= 10 && !requireOptions.skills.Any(sd => sd.skillDef.defName == sp.skillDef)).ToList();
            SkillValue randomSel = customValues.GetSkillValue().skillPrices[0];

            if (candidates.Any()) 
            {
                randomSel = candidates.RandomElement();
            }

            int skillRequireOffset = challengeRating - 1;
            if (requireOptions.lifeStage == LifeStageDefOf.HumanlikeChild) skillRequireOffset -= 3;

            SkillDef skillDef = DefDatabase<SkillDef>.GetNamed(randomSel.skillDef);
                SQ_Skill skill = new SQ_Skill(skillDef, randomSel.standard + Rand.Range(skillRequireOffset, skillRequireOffset + 3));
            requireOptions.skills.Add(skill);
        }

        public void GenerateSlaveEstimatedPrice()
        {
            float baseValue = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.MarketValueIgnoreHp);

            CustomTraitValue customTraitValue = customValues.GetTraitValue();
            foreach (Trait trait in requireOptions.traits)
            {
                foreach (TraitValue value in customTraitValue.traitPrices)
                {
                    if (value.traitDef == trait.def.defName && value.degree == trait.Degree)
                    {
                        baseValue += value.value * 2;
                        break;
                    }
                }
            }

            CustomSkillValue customSkillValue = customValues.GetSkillValue();
            foreach (SQ_Skill skill in requireOptions.skills)
            { 
                foreach(SkillValue value in customSkillValue.skillPrices)
                {
                    if (value.skillDef == skill.skillDef.defName)
                    {
                        baseValue += value.value * 10;
                    }
                }
            }

            float challengeRatingValue = challengeRating == 3 ? 1.25f : challengeRating == 2 ? 1.1f : 1.0f;
            estimatedPrice = (int)(baseValue * challengeRatingValue);
        }
    }
}
