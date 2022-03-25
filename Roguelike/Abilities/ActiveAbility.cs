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
        case AbilityKind.ExplosiveMastering:
        case AbilityKind.PoisonMastering:
        case AbilityKind.ThrowingStoneMastering:
        case AbilityKind.ThrowingKnifeMastering:
        case AbilityKind.Stride:
        case AbilityKind.CauseBleeding:
        case AbilityKind.Rage:
          float fac = CalcFightItemFactor(level);
          factor = fac;

          if (!primary)
          {
            if (kind == AbilityKind.ExplosiveMastering ||
              kind == AbilityKind.PoisonMastering)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == AbilityKind.ExplosiveMastering ||
              kind == AbilityKind.PoisonMastering)
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
        case AbilityKind.ExplosiveMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.PoisonMastering:
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
        case AbilityKind.CauseBleeding:
          desc = "Hitting target with mellee will cause bleeding";
          break;
        case AbilityKind.Rage:
          desc = "Increases the mellee damage of the caster";
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
          case AbilityKind.ExplosiveMastering:
            psk = EntityStatKind.ExlosiveCoctailExtraDamage;
            break;
          case AbilityKind.PoisonMastering:
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
          case AbilityKind.CauseBleeding:
            psk = EntityStatKind.CausedBleedingDuration;
            break;
          case AbilityKind.Rage:
            psk = EntityStatKind.MeleeAttack;
            break;
          default:
            Debug.WriteLine("Unsupported kind:  "+ kind);
            break;
        }

        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
        if (kind == AbilityKind.Stride)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
        }
      }
    }
  }
}
