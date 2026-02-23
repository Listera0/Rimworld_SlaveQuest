using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

public class CustomSlaveCategory : Def { }

public class CustomTraitValue : Def
{
    public string role;
    public List<TraitValue> traitPrices = new List<TraitValue>();
}

public class CustomSkillValue : Def
{
    public string role;
    public List<SkillValue> skillPrices = new List<SkillValue>();
}

public class CustomXenotypeWeight : Def
{
    public string xenotypeDef;
    public int weight;
}

public class CustomAgeWeight : Def
{
    public string ageSectionDef;
    public int weight;
}

public class TraitValue
{
    public string traitDef;
    public int degree;
    public float value;
}

public class SkillValue
{
    public string skillDef;
    public int standard;
    public float value;
}

namespace SlaveQuest
{
    internal class CustomValue_SlaveQuest
    {
    }
}
