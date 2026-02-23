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
            List<Pawn> selectedPawns = new List<Pawn>();

            selectedPawns.AddRange(PawnsFinder.AllMaps_PrisonersOfColonySpawned.Where(x => x.Map != null && x.Map.IsPlayerHome));
            if (ModsConfig.IdeologyActive) { selectedPawns.AddRange(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_SlavesOfColony.Where(x => x.Map != null && x.Map.IsPlayerHome)); }

            foreach (Pawn pawn in selectedPawns)
            {
                if (CanPawnAccept(pawn))
                {
                    return true;
                }
            }

            return new AcceptanceReport("SlaveQuest.UI.CantAcceptQuest".Translate());
        }

        public override bool CanPawnAccept(Pawn p)
        {
            if (p.genes.Xenotype != requireOptions.xenotype) { return false; }
            if (p.ageTracker.CurLifeStage != requireOptions.lifeStage) { return false; }

            if (requireOptions.traits.Count != 0 && p.story.traits.allTraits.Count != 0)
            {
                foreach(Trait req_Trait in requireOptions.traits)
                {
                    if (!p.story.traits.allTraits.Contains(req_Trait)) { return false; }
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
