using Roguelike.Attributes;
using Roguelike.Factors;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Abstract.Spells
{
  public interface IDamagingSpell
  {
    float Damage { get; }
  }

  public class SpellStatsDescription
  {
    public int Level { get; set; }
    public int? ManaCost { get; set; }
    public int MagicRequired { get; set; }

    public float? Damage
    {
      get => damage;
      set => damage = value;
    }
    public int? TourLasting { get; set; }
    public EntityStatKind? StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    List<string> extraStatDescription = new List<string>();
    private float? damage;

    SpellKind Kind { get; set; }


    public SpellStatsDescription(int level, int? manaCost, int magicRequired, SpellKind kind)
    {
      Level = level;
      ManaCost = manaCost;
      MagicRequired = magicRequired;
      Kind = kind;
    }

    public void AddString(string str, bool addIndent = true)
    {
      if (addIndent)
        str = "  " + str;
      extraStatDescription.Add(str);
    }

    public string[] GetDescription(bool addIndent = true, bool includeLevel = false)
    {
      extraStatDescription.Clear();

      if (includeLevel)
        AddString("Level: " + Level, addIndent);

      if (ManaCost != null)
        AddString("Mana Cost: " + ManaCost, addIndent);
      if (Damage != null)
        AddString(Kind + " Damage: " + Damage, addIndent);
      if (TourLasting != null)
        AddString("TourLasting: " + TourLasting);
      if (StatKind != null)
        AddString(StatKind + " " + StatKindPercentage.ToString(), addIndent);
      return extraStatDescription.ToArray();
    }

  }

  public interface ISpell : Dungeons.Tiles.Abstract.ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
    int ManaCost { get; }
    bool Utylized { get; set; }
    int CurrentLevel { get; }
    SpellKind Kind { get;}

    SpellStatsDescription CreateSpellStatsDescription(bool currentLevel);
  }

  public interface IProjectileSpell : ISpell, Roguelike.Abstract.Projectiles.IProjectile
  { 
  }
}
