using Dungeons.Tiles;
using Roguelike.Abstract.Effects;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Spells
{
  public class PassiveSpell : Spell, ILastingSpell
  {
    protected Tile tile;
    public int TourLasting { get; set; }
    public readonly int BaseFactor = 30;

    public EntityStatKind StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    public EffectiveFactor StatKindEffective { get; set; }

    public PassiveSpell(LivingEntity caller, EntityStatKind statKind, int baseFactor = 30) : base(caller)
    {
      BaseFactor = baseFactor;
      manaCost = (float)(BaseManaCost * 2);
      StatKind = statKind;

      StatKindPercentage = CalcFactor(CurrentLevel);
      StatKindEffective = caller.CalcEffectiveFactor(StatKind, StatKindPercentage.Value);
      TourLasting = CalcTourLasting();
    }

    protected virtual PercentageFactor CalcFactor()
    {
      return CalcFactor(CurrentLevel);
    }

    protected virtual PercentageFactor CalcFactor(int magicLevel)
    {
      return new PercentageFactor(BaseFactor + magicLevel);
    }

    protected void SetHealthFromLevel(LivingEntity spellTarget, float factor = 1)
    {
      var lvl = CurrentLevel;
      var he = GetHealthFromLevel(lvl) * factor;
      spellTarget.Stats.SetNominal(EntityStatKind.Health, he);
    }

    const int baseHealth = 20;
    protected int GetHealthFromLevel(int lvl)
    {
      return CalcHealthFromLevel(lvl);
    }

    public static int CalcHealthFromLevel(int lvl)
    {
      return FactorCalculator.CalcFromLevel(lvl, baseHealth);
      //if (lvl == 0)
      //	return 0;

      //if (lvl == 1)
      //     return baseHealth;

      //int prev = GetHealthFromLevel(lvl - 1);
      //return prev + (int)(prev * 10f/100f);
    }

    public virtual Tile Tile
    {
      get
      {
        return tile;
      }

      set
      {
        tile = value;
      }
    }

    protected int CalcTourLasting(float factor = 1)
    {
      return CalcTourLasting(CurrentLevel, factor);
    }

    protected int CalcTourLasting(int magicLevel, float factor = 1)
    {
      var he = GetHealthFromLevel(magicLevel);
      float baseVal = ((float)he) * factor;
      return (int)(baseVal / 4f);
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if(currentMagicLevel)
        desc.TourLasting = TourLasting;
      else
        desc.TourLasting = CalcTourLasting(CurrentLevel+1);

      if (StatKind != EntityStatKind.Unset && Kind != SpellKind.ManaShield)
      {
        desc.StatKind = StatKind;
        desc.StatKindPercentage = StatKindPercentage;
      }
      return desc;
    }

    //protected override void AppendPrivateFeatures(List<string> fe)
    //{
    //  fe.Add(StatKind.ToDescription() + ": " + StatKindPercentage);
    //  fe.Add(GetTourLasting(TourLasting));
    //}

    //public string GetCoolingDown()
    //{
    //  return "Cooling Down: " + CoolingDown;
    //}

    //public static string GetTourLasting(int tourLasting)
    //{
    //  return "Tour Lasting: " + tourLasting;
    //}

    //protected string GetNextLevelTourLasting()
    //{
    //  return GetNextLevelTourLasting(CalcTourLasting(CurrentLevel() + 1));
    //}

    //protected string GetNextLevelTourLasting(int tourLasting)
    //{
    //  return GetNextLevel(GetTourLasting(tourLasting));
    //}

    //public static string GetNextLevelTourLasting(int tourLasting)
    //{
    //  return "Next Level: Tour Lasting: " + tourLasting;
    //}

    //protected override void AppendNextLevel(List<string> fe)
    //{
    //  base.AppendNextLevel(fe);

    //  var suffix = StatKind.ToDescription() + " " + CalcFactor(GetCurrentLevel() + 1);
    //  fe.Add(GetNextLevel(suffix));
    //  fe.Add(GetNextLevelTourLasting());
    //}
  }
}
