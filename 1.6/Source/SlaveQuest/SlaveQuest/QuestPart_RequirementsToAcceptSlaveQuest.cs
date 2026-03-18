using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace SlaveQuest
{
    public class QuestPart_RequirementsToAcceptSlaveQuest : QuestPart_RequirementsToAccept
    {
        public RequireOptions requireOptions;

        public override AcceptanceReport CanAccept()
        {
            var selectedPawns = PawnsFinder.AllMaps_FreeColonistsAndPrisonersSpawned;

            foreach (Pawn pawn in selectedPawns)
            {
                if (pawn.Map != null && pawn.Map.IsPlayerHome)
                {
                    if (CanPawnAccept(pawn))
                    {
                        return true;
                    }
                }
            }

            return new AcceptanceReport("SlaveQuest.UI.CantAcceptQuest".Translate());
        }

        public override bool CanPawnAccept(Pawn p)
        {
            if(ModsConfig.BiotechActive && requireOptions.xenotype != null)
            {
                if (requireOptions.xenotype != null && p.genes?.Xenotype != requireOptions.xenotype) return false;
                if (requireOptions.lifeStage != null && p.ageTracker?.CurLifeStage != requireOptions.lifeStage) return false;
            }

            if (requireOptions.traits.Count != 0)
            {
                if(p.story.traits.allTraits.Count == 0) { return false; }

                foreach (Trait req_Trait in requireOptions.traits)
                {
                    if (!p.story.traits.HasTrait(req_Trait.def, req_Trait.Degree)) { return false; }
                }
            }

            if (requireOptions.skills.Count != 0)
            {
                foreach(SQ_Skill req_Skill in requireOptions.skills)
                {
                    foreach(SkillRecord pawn_Skill in p.skills.skills)
                    {
                        if (req_Skill.skillDef == pawn_Skill.def)
                        {
                            if (pawn_Skill.Level >= req_Skill.value) { break; }
                            else { return false; }
                        } 
                    }
                }
            }

            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref requireOptions, "requireOptions");
        }
    }
}
