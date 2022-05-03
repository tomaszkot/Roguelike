using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roguelike.Abilities
{
  /// <summary>
  /// ActiveAbility must be used explicitely by a user
  /// </summary>
  public class ActiveAbility : Ability
  {
    public override bool useCustomStatDescription()
    {
      return false;
    }

    public override bool IsPercentageFromKind => kind == AbilityKind.Stride;//|| kind == AbilityKind.CauseBleeding;

    public override float CalcFactor(bool primary, int level)
    {
      AbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case AbilityKind.ExplosiveCocktail:
        case AbilityKind.PoisonCocktail:
        case AbilityKind.ThrowingStone:
        case AbilityKind.ThrowingKnife:
        case AbilityKind.Stride:
                
          float fac = CalcFightItemFactor(level);
          factor = fac;

          if (!primary)
          {
            if (kind == AbilityKind.ExplosiveCocktail ||
              kind == AbilityKind.PoisonCocktail)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == AbilityKind.ExplosiveCocktail ||
              kind == AbilityKind.PoisonCocktail)
            {
              factor *= 2f;
            }
            if (kind == AbilityKind.Stride)
              factor *= 6f;
          }
          break;
        
        case AbilityKind.HunterTrap:
          var multsHunt = new int[] { 0, 10, 20, 30, 40, 50 };
          factor = multsHunt[level];
          break;
        case AbilityKind.PiercingArrow:
          if (primary)
          {
            var percentToDo = new int[] { 0, 80, 85, 90, 95, 100 };
            factor = percentToDo[level];
          }
          else 
          {
            var victims = new int[] { 0, 2, 3, 4, 5, 6 };
            factor = victims[level];
          }
          break;
        case AbilityKind.ArrowVolley:
          if (primary)
          {
            var victims = new int[] { 0, 2, 3, 4, 5, 6 };
            factor = victims[level];
          }
          break;
        case AbilityKind.OpenWound:
          if (primary)
          {
            //duration
            var durations = new int[] { 0, 3, 3, 4, 4, 5 };
            factor = durations[level];
          }
          else
          {
            var extraDamages = new int[] { 0, 5, 10, 15, 20, 25 };
            factor = extraDamages[level];
          }
          break;
        case AbilityKind.Rage:
          if (primary)
          {
            var perc = new int[] { 0, 25, 30, 40, 55, 70 };
            factor = perc[level];
          }
          break;
        case AbilityKind.WeightedNet:
            var vals = new int[] { 0, 1, 2, 3, 4, 5 };
            factor = vals[level];
  
          break;
        case AbilityKind.PerfectHit:
          if (primary)
          {
            var victims = new int[] { 0, 10, 15, 35, 50, 70 };//PerfectHitDamage %
            factor = victims[level];
          }
          else
          {
            var extraDamages = new int[] { 0, 5, 10, 15, 20, 25 };//PerfectHitChanceToHit %
            factor = extraDamages[level];
          }
          break;
        default:
          break;
      }
      return factor;
    }

    public void SetPrimaryStatDescription()
    {
      var desc = "";
      //todo  add map to FightItem AbilityKind/FightItemKind and use here
      switch (kind)
      {
        case AbilityKind.ExplosiveCocktail:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.PoisonCocktail:
          desc = "Bonus to Poison Cocktail damage";
          break;
          
        case AbilityKind.ThrowingStone:
          desc = "Bonus to throwing stones";
          break;
        case AbilityKind.ThrowingKnife:
          desc = "Bonus to throwing knife";
          break;
        case AbilityKind.HunterTrap:
          desc = "Bonus to hunter trap";
          break;
        case AbilityKind.Stride:
          desc = "Hit target with your body causing damage and possibly knocking it back";
          break;
        case AbilityKind.OpenWound:
          desc = "Hit target with a melee weapon to cause bleeding";
          break;
        case AbilityKind.Rage:
          desc = "Increases the melee damage of the caster";
          break;
        case AbilityKind.ArrowVolley:
          desc = "Discharges a number of arrows at one time";
          break;
        case AbilityKind.PiercingArrow:
          desc = "Pierces an enemy, hitting other one behind";
          break;
        case AbilityKind.PerfectHit:
          desc = "Increases chance to hit and damage made by projectile";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }

    public override string ToString()
    {
      return base.ToString() + Kind;
    }

    public override AbilityKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;

        SetName(Kind.ToDescription());
        SetPrimaryStatDescription();
        EntityStatKind psk = EntityStatKind.Unset;
        EntityStatKind ask = EntityStatKind.Unset;
        switch (kind)
        {
          case AbilityKind.ExplosiveCocktail:
            psk = EntityStatKind.ExlosiveCoctailExtraDamage;
            break;
          case AbilityKind.PoisonCocktail:
            psk = EntityStatKind.PoisonCoctailExtraDamage;
            break;
          case AbilityKind.ThrowingKnife:
            psk = EntityStatKind.ThrowingKnifeExtraDamage;
            break;
          case AbilityKind.ThrowingStone:
            psk = EntityStatKind.ThrowingStoneExtraDamage;
            break;
          case AbilityKind.HunterTrap:
            psk = EntityStatKind.HunterTrapExtraDamage;
            break;
          case AbilityKind.Stride:
            psk = EntityStatKind.StrideExtraDamage;
            break;
          case AbilityKind.OpenWound:
            psk = EntityStatKind.BleedingDuration;
            ask = EntityStatKind.BleedingExtraDamage;
            break;
          case AbilityKind.Rage:
            psk = EntityStatKind.MeleeAttack;
            break;
          case AbilityKind.PiercingArrow:
            psk = EntityStatKind.ChanceForPiercing;
            ask = EntityStatKind.NumberOfPiercedVictims;
            break;
          case AbilityKind.ArrowVolley:
            psk = EntityStatKind.ArrowVolleyCount;
            break;

          case AbilityKind.WeightedNet:
            psk = EntityStatKind.WeightedNetDuration;
            ask = EntityStatKind.WeightedNetRange;
            break;
          case AbilityKind.PerfectHit:
            psk = EntityStatKind.PerfectHitDamage;
            ask = EntityStatKind.PerfectHitChanceToHit;
            break;
          default:
            Debug.WriteLine("Unsupported kind:  "+ kind);
            break;
        }

        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
        if (kind == AbilityKind.Stride || kind == AbilityKind.PiercingArrow)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
          if (kind == AbilityKind.PiercingArrow)
            AuxStat.Unit = EntityStatUnit.Absolute;
        }

        if (kind == AbilityKind.ArrowVolley)
          PrimaryStat.Unit = EntityStatUnit.Absolute;

        else if (kind == AbilityKind.OpenWound)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
          AuxStat.Unit = EntityStatUnit.Percentage;
        }

        else if (kind == AbilityKind.Rage)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
        }
        else if (kind == AbilityKind.WeightedNet)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
          AuxStat.Unit = EntityStatUnit.Absolute;
        }
      }
    }

    static Dictionary<AbilityKind, FightItemKind> Ab2Fi = new Dictionary<AbilityKind, FightItemKind>()
    {
      { AbilityKind.ThrowingKnife, FightItemKind.ThrowingKnife},
      { AbilityKind.ThrowingStone, FightItemKind.Stone},
      { AbilityKind.PoisonCocktail, FightItemKind.PoisonCocktail},
      { AbilityKind.ExplosiveCocktail, FightItemKind.ExplosiveCocktail},
      { AbilityKind.HunterTrap, FightItemKind.HunterTrap},
      { AbilityKind.WeightedNet, FightItemKind.WeightedNet},
    };

    public static FightItemKind GetFightItemKind(AbilityKind kind)
    {
      if (Ab2Fi.ContainsKey(kind))
      {
        return Ab2Fi[kind];
      }

      return FightItemKind.Unset;
    }
  }
}
