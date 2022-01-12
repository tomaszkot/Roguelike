﻿using Roguelike.Attributes;
using Roguelike.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  /// <summary>
  /// ActiveAbility must be used explicitely by a user
  /// </summary>
  public class ActiveAbility : Ability
  {
    public override bool useCustomStatDescription()
    {
      return Kind == AbilityKind.ExplosiveMastering ||
        Kind == AbilityKind.PoisonMastering;
    }


    public override bool IsPercentageFromKind()
    {
      return false;
    }

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
          float fac = CalcFightItemFactor(level);
          //List<float> facs = new List<float>();
          //for (int i = 0; i < 5; i++)
          //{
          //  facs.Add(CalcExplFactor(i));
          //}
          factor = fac;

          if (!primary)
          {
            //if (kind == AbilityKind.HuntingMastering)
            //  factor = level;
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
            //else if (kind == PassiveAbilityKind.ThrowingWeaponsMastering)
            //{
            //  factor *= 23f;
            //}
            //else if (kind == AbilityKind.HuntingMastering)
            //  factor *= 18f;
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
          default:

            break;
        }
        //if (PrimaryStat == null)
        //  return;
        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
      }
    }
  }
}
