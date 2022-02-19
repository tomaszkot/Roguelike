using Roguelike.Attributes;
using Roguelike.Factors;
using System.Collections.Generic;

namespace Roguelike.Spells
{
  public sealed class SpellStatsDescription
  {
    public int Level { get; set; }
    public int? ManaCost { get; set; }
    public int? MagicRequired { get; set; }

    public float? Damage
    {
      get => damage;
      set
      {
        damage = value;
      }
    }
    public int? TourLasting { get; set; }
    public EntityStatKind? StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    List<string> extraStatDescription = new List<string>();
    private float? damage;

    SpellKind Kind { get; set; }
    public int Range { get; set; }
    List<EntityStat> entityStats;

    public SpellStatsDescription(int level, int? manaCost, int? magicRequired, SpellKind kind, int range)
    {
      Level = level;
      ManaCost = manaCost;
      MagicRequired = magicRequired;
      Kind = kind;
      Range = range;
           
    }

    public EntityStat[] GetEntityStats()
    {
      if (entityStats == null)
      {
        entityStats = new List<EntityStat>();
        if (ManaCost.HasValue)
        {
          entityStats.Add(new EntityStat() { Kind = EntityStatKind.Mana, Value = new StatValue() { Nominal = ManaCost.Value } });
        }

        if(Range > 0)
          entityStats.Add(new EntityStat() { Kind = EntityStatKind.ElementalProjectilesRange, Value = new StatValue() { Nominal = Range } });

        if (Damage.HasValue)
          entityStats.Add(new EntityStat() { Kind = EntityStatKind.ElementalSpellProjectilesAttack, Value = new StatValue() { Nominal = Damage.Value } });
      }
      return entityStats.ToArray();
    }

    public void AddString(string str, bool addIndent = true)
    {
      if (addIndent)
        str = "  " + str;
      extraStatDescription.Add(str);
    }

    public string[] GetDescription(bool addIndent = true)
    {
      extraStatDescription.Clear();

      AddString("Level: " + Level, addIndent);

      if (ManaCost != null)
        AddString("Mana Cost: " + ManaCost, addIndent);
      if (Damage != null)
        AddString(Kind + " Damage: " + Damage, addIndent);
      if (TourLasting != null)
        AddString("Duration: " + TourLasting);
      if (StatKind != null)
        AddString(StatKind + " " + StatKindPercentage.ToString(), addIndent);
      if (Range > 0)
        AddString("Range: " + Range, addIndent);

      return extraStatDescription.ToArray();
    }
  }
}
