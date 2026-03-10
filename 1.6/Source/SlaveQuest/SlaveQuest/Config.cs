using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SlaveQuest
{
    public class SlaveQuest : Mod
    {
        public SlaveQuest(ModContentPack content) : base(content)
        {
            GetSettings<SlaveQuest_Config>();
        }

        public override string SettingsCategory()
        {
            return "SlaveQuest.Config.Tittle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            SlaveQuest_Config.DoWindowContents(inRect);
        }
    }

    public class SlaveQuest_Config : ModSettings
    {
        public static float QuestGenerateRate_Contract = 1.0f;
        public static float QuestGenerateRate_BreakWill = 1.0f;

        public static void ResetConfig()
        {
            QuestGenerateRate_Contract = 1.0f;
            QuestGenerateRate_BreakWill = 1.0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref QuestGenerateRate_Contract, "QuestGenerateRate_Contract", 1.0f);
            Scribe_Values.Look(ref QuestGenerateRate_BreakWill, "QuestGenerateRate_BreakWill", 1.0f);
        }

        public static void DoWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + 500f);

            Listing_Standard listingStandard = new Listing_Standard(); 
            listingStandard.maxOneColumn = true;
            listingStandard.ColumnWidth = viewRect.width / 2f;
            listingStandard.Begin(viewRect);
            listingStandard.Gap(50f);
            string defaultValueLabel1 = ((QuestGenerateRate_Contract == 1.0f) ? (" (" + "SlaveQuest.Config.DefaultValue.Label".Translate().ToString() + ")") : "");
            listingStandard.Label("SlaveQuest.Config.QuestGenerateRate_Contract.Label".Translate() + " " + (QuestGenerateRate_Contract * 100).ToString("F1") + "%" + defaultValueLabel1, -1.0f, "SlaveQuest.Config.QuestGenerateRate_Contract.Description".Translate());
            listingStandard.Gap(5f);
            QuestGenerateRate_Contract = listingStandard.Slider(QuestGenerateRate_Contract, 0.0f, 5.0f);
            string defaultValueLabel2 = ((QuestGenerateRate_Contract == 1.0f) ? (" (" + "SlaveQuest.Config.DefaultValue.Label".Translate().ToString() + ")") : "");
            listingStandard.Label("SlaveQuest.Config.QuestGenerateRate_BreakWill.Label".Translate() + " " + (QuestGenerateRate_BreakWill * 100).ToString("F1") + "%" + defaultValueLabel2, -1.0f, "SlaveQuest.Config.QuestGenerateRate_BreakWill.Description".Translate());
            listingStandard.Gap(5f);
            QuestGenerateRate_BreakWill = listingStandard.Slider(QuestGenerateRate_BreakWill, 0.0f, 5.0f);
            listingStandard.Gap(15f);
            Rect lineRect = listingStandard.GetRect(30f);
            Rect buttonRect = new Rect(lineRect.x, lineRect.y, 100f, lineRect.height);
            if (Widgets.ButtonText(buttonRect, "SlaveQuest.Config.Reset.Label".Translate())) { ResetConfig(); }
            listingStandard.End();
        }
    }
}
