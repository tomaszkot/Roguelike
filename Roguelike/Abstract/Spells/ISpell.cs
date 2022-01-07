﻿using Roguelike.Attributes;
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

  public sealed class SpellStatsDescription
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
    public int Range { get; set; }

    public SpellStatsDescription(int level, int? manaCost, int magicRequired, SpellKind kind, int range)
    {
      Level = level;
      ManaCost = manaCost;
      MagicRequired = magicRequired;
      Kind = kind;
      Range = range;
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
      if(Range > 0)
        AddString("Range: " + Range, addIndent);

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

    SpellStatsDescription CreateSpellStatsDescription(bool currentLevel, bool withVariation);
  }

  public interface IProjectileSpell : ISpell, Roguelike.Abstract.Projectiles.IProjectile
  { 
  }
}
