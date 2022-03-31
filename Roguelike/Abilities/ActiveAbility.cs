using Roguelike.Attributes;
using Roguelike.Extensions;
using System;
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
        case AbilityKind.ExplosiveCocktailMastering:
        case AbilityKind.PoisonCocktailMastering:
        case AbilityKind.ThrowingStoneMastering:
        case AbilityKind.ThrowingKnifeMastering:
        case AbilityKind.Stride:
                
          float fac = CalcFightItemFactor(level);
          factor = fac;

          if (!primary)
          {
            if (kind == AbilityKind.ExplosiveCocktailMastering ||
              kind == AbilityKind.PoisonCocktailMastering)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == AbilityKind.ExplosiveCocktailMastering ||
              kind == AbilityKind.PoisonCocktailMastering)
            {
              factor *= 2f;
            }
            if (kind == AbilityKind.Stride)
              factor *= 5f;
          }
          break;
        
        case AbilityKind.HunterTrapMastering:
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
            var victims = new int[] { 0, 1, 2, 3, 4, 5 };
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
            var victims = new int[] { 0, 3, 3, 4, 4, 5 };
            factor = victims[level];
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
        case AbilityKind.WeightedNetMastering:
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
        case AbilityKind.ExplosiveCocktailMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.PoisonCocktailMastering:
          desc = "Bonus to Poison Cocktail damage";
          break;
          
        case AbilityKind.ThrowingStoneMastering:
          desc = "Bonus to throwing stones";
          break;
        case AbilityKind.ThrowingKnifeMastering:
          desc = "Bonus to throwing knife";
          break;
        case AbilityKind.HunterTrapMastering:
          desc = "Bonus to hunter trap";
          break;
        case AbilityKind.Stride:
          desc = "Hit target with your body causing damage and possibly knocking it back";
          break;
        case AbilityKind.OpenWound:
          desc = "Hitting target with mellee will cause bleeding";
          break;
        case AbilityKind.Rage:
          desc = "Increases the mellee damage of the caster";
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
          case AbilityKind.ExplosiveCocktailMastering:
            psk = EntityStatKind.ExlosiveCoctailExtraDamage;
            break;
          case AbilityKind.PoisonCocktailMastering:
            psk = EntityStatKind.PoisonCoctailExtraDamage;
            break;
          case AbilityKind.ThrowingKnifeMastering:
            psk = EntityStatKind.ThrowingKnifeExtraDamage;
            break;
          case AbilityKind.ThrowingStoneMastering:
            psk = EntityStatKind.ThrowingStoneExtraDamage;
            break;
          case AbilityKind.HunterTrapMastering:
            psk = EntityStatKind.HunterTrapExtraDamage;
            break;
          case AbilityKind.Stride:
            psk = EntityStatKind.MeleeAttack;
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

          case AbilityKind.WeightedNetMastering:
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

        if (kind == AbilityKind.OpenWound)
        {
          AuxStat.Unit = EntityStatUnit.Percentage;
          PrimaryStat.Unit = EntityStatUnit.Absolute;
        }

        if (kind == AbilityKind.Rage)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
        }
        if (kind == AbilityKind.WeightedNetMastering)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
          AuxStat.Unit = EntityStatUnit.Absolute;
        }
      }
    }
  }
}
